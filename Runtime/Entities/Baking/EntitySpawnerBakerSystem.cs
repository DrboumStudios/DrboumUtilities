using System.Runtime.CompilerServices;
using GameProject;
using GameProject.EntitiesInjected.Hybrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

namespace Drboum.Utilities.Entities.Baking
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct EntitySpawnerBakerSystem : ISystem
    {
        private static readonly ProfilerMarker _EntitySpawnerBakerSystemOnUpdateMarker = new(nameof(EntitySpawnerBakerSystem._EntitySpawnerBakerSystemOnUpdateMarker));
        private NativeReference<uint> _spawnedBySystemTotal;
        private EntityQuery _spawnerRequestEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            _spawnedBySystemTotal = new NativeReference<uint>(0, Allocator.Persistent);
            _spawnerRequestEntityQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllEntitySpawner()
                .WithAll<FreshlyBakedTag>()
                .Build(ref state);

            state.RequireForUpdate(_spawnerRequestEntityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using ( _EntitySpawnerBakerSystemOnUpdateMarker.Auto() )
            {
                var newInstancesRootQuery = SystemAPI.QueryBuilder()
                    .WithAll<OriginBakerSharedKey, LocalTransform, LocalToWorld, SpawnRootTag>()
                    .Build();

                using NativeArray<Entity> spawnerEntities = _spawnerRequestEntityQuery.ToEntityArray(Allocator.TempJob);
                new HandleSpawnQueriesJob {
                    SpawnerEntities = spawnerEntities,
                    EntityManager = state.EntityManager,
                    NewInstancesQuery = newInstancesRootQuery
                }.Execute();
            }
        }

        // [BurstCompile]
        private partial struct HandleSpawnQueriesJob
        {
            [NativeDisableUnsafePtrRestriction] public EntityQuery NewInstancesQuery;
            [DeallocateOnJobCompletion] public NativeArray<Entity> SpawnerEntities;
            public EntityManager EntityManager;

            public void Execute()
            {
                var entityBuffer = new NativeList<Entity>(256, Allocator.Temp);
                for ( var spawnerIndex = 0; spawnerIndex < SpawnerEntities.Length; spawnerIndex++ )
                {
                    Entity spawnerEntity = SpawnerEntities[spawnerIndex];
                    var spawnRequest = EntityManager.GetComponentData<EntitySpawnRequest>(spawnerEntity);
                    var spawnBounds = EntityManager.GetComponentData<AABBComponent>(spawnerEntity);
                    var spawnerWorldTransform = EntityManager.GetComponentData<LocalToWorld>(spawnerEntity);
                    var originBakerKey = EntityManager.GetComponentData<OriginBakerIdentity>(spawnerEntity);
                    Execute(
                        ref spawnRequest,
                        spawnBounds,
                        spawnerWorldTransform,
                        originBakerKey, NewInstancesQuery, EntityManager,
                        entityBuffer);
                    EntityManager.SetComponentData(spawnerEntity, spawnRequest);
                }
            }

            public static void Execute(ref EntitySpawnRequest spawnRequest, in AABBComponent spawnBounds, in LocalToWorld spawnerWorldTransform, in OriginBakerIdentity originBakerKey, EntityQuery newInstancesQuery, EntityManager entityManager, NativeList<Entity> entityTempBuffer)
            {
                float3 spawnerWorldPosition = CalculateSpawnRequestInfo(spawnerWorldTransform.Position, spawnBounds, spawnRequest, out float3 maxInstancesXYZ, out float maxPossibleInstances);
                Debug.Log($"[EntitySpawnerBakerSystem] spawned {maxPossibleInstances} instances!");
                if ( maxPossibleInstances <= 0 )
                    return;

                var prefabCopy = entityManager.Instantiate(spawnRequest.Prefab);
#if UNITY_EDITOR
                if ( entityManager.TryGetLinkedGroupEntityBakingData(prefabCopy, out var linkedGroupEntityBakingDataAsEntity) )
                {
                    entityTempBuffer.AddRange(linkedGroupEntityBakingDataAsEntity.AsNativeArray());
                }
#endif
                if ( entityManager.HasBuffer<LinkedEntityGroup>(prefabCopy) )
                {
                    var runtimeLinkedEntityGroups = entityManager.GetBuffer<LinkedEntityGroup>(prefabCopy, true);
                    entityTempBuffer.AddRange(runtimeLinkedEntityGroups.Reinterpret<Entity>().AsNativeArray());
                }

                if ( entityTempBuffer.IsEmpty )
                {
                    entityTempBuffer.Add(prefabCopy);
                }

                entityManager.AddSharedComponent(entityTempBuffer.AsArray(), new OriginBakerSharedKey {
                    Value = originBakerKey.Value
                });
                entityManager.AddComponent(prefabCopy, ComponentType.ReadWrite<SpawnRootTag>());

                //requires the full count of spawned entities including the entity that we use as prototype
                var localTransforms = new NativeArray<LocalTransform>((int)maxPossibleInstances, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var localToWorlds = new NativeArray<LocalToWorld>((int)maxPossibleInstances, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                maxPossibleInstances--;

                var counterXYZ = uint3.zero;
                var localTransform = entityManager.GetComponentData<LocalTransform>(spawnRequest.Prefab);
                localTransform.Rotation = math.mul(localTransform.Rotation, spawnerWorldTransform.Rotation);

                ProcessSpawnedInstance(ref localTransform, ref counterXYZ, in spawnRequest, in prefabCopy, in spawnerWorldPosition, in maxInstancesXYZ, localTransforms, localToWorlds, 0, spawnerWorldTransform);

                if ( maxPossibleInstances > 0 )
                {
                    var spawnedEntities = new NativeArray<Entity>((int)maxPossibleInstances, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    entityManager.Instantiate(prefabCopy, spawnedEntities);
                    for ( int i = 0; i < maxPossibleInstances; i++ )
                    {
                        Entity spawnedEntity = spawnedEntities[i];
                        ProcessSpawnedInstance(ref localTransform, ref counterXYZ, in spawnRequest, in spawnedEntity, in spawnerWorldPosition, in maxInstancesXYZ, localTransforms, localToWorlds, i + 1, spawnerWorldTransform);
                    }
                }
                newInstancesQuery.CopyFromComponentDataArray(localToWorlds);
                newInstancesQuery.CopyFromComponentDataArray(localTransforms);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ProcessSpawnedInstance(ref LocalTransform localTransform, ref uint3 counterXYZ, in EntitySpawnRequest spawnRequest, in Entity spawnedEntity, in float3 spawnerWorldPosition, in float3 maxInstancesXYZ, NativeArray<LocalTransform> localTransforms, NativeArray<LocalToWorld> localToWorlds, int indexInTransforms, LocalToWorld spawnerWorldTransform)
            {
                float3 flatPosition = spawnerWorldPosition + (counterXYZ * spawnRequest.InstanceSpacing);
                float3 spawnerCenter = spawnerWorldTransform.Position;
                var offsetFromCenter = flatPosition - spawnerCenter;
                localTransform.Position = spawnerCenter + math.mul(spawnerWorldTransform.Rotation, offsetFromCenter);
                var localToWorld = new LocalToWorld {
                    Value = localTransform.ToMatrix()
                };
                localTransforms[indexInTransforms] = localTransform;
                localToWorlds[indexInTransforms] = localToWorld;
                counterXYZ.x++;
                if ( counterXYZ.x >= maxInstancesXYZ.x )
                {
                    counterXYZ.x = 0;
                    counterXYZ.z++;
                }
                if ( counterXYZ.z >= maxInstancesXYZ.z )
                {
                    counterXYZ.x = 0;
                    counterXYZ.z = 0;
                    counterXYZ.y++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalculateSpawnRequestInfo(float3 spawnerPosition, AABBComponent spawnBounds, EntitySpawnRequest spawnRequest, out float3 maxInstancesXYZ, out float maxPossibleInstances)
        {
            float3 spawnerWorldPosition = (spawnerPosition + (spawnRequest.InstanceSpacing / 2) + spawnBounds.Value.Center) - (spawnBounds.Size / 2);
            maxInstancesXYZ = math.floor(spawnBounds.Size / spawnRequest.InstanceSpacing);
            maxPossibleInstances = math.min(maxInstancesXYZ.x * maxInstancesXYZ.y * maxInstancesXYZ.z, spawnRequest.SpawnCount);
            return spawnerWorldPosition;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            state.Dependency.Complete();
            _spawnedBySystemTotal.Dispose();
        }
    }

    [TemporaryBakingType]
    public struct SpawnRootTag : IComponentData
    { }

    public static partial class EntityQueryExtension
    {
        public static EntityQueryBuilder WithAllEntitySpawner(this EntityQueryBuilder builder)
        {
            return builder
                .WithAll<AABBComponent, LocalToWorld, EntityGuid, OriginBakerIdentity>()
                .WithAllRW<EntitySpawnRequest>();
        }
    }
}