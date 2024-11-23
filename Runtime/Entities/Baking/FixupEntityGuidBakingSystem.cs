using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;

namespace Drboum.Utilities.Entities.Baking
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndBakingCommandBufferSystem))]
    public partial struct FixupEntityGuidBakingSystem : ISystem
    {

        public void OnCreate(ref SystemState state)
        { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var allSpawnedEntitiesQuery = SystemAPI.QueryBuilder()
                .WithAll<EntityGuid, OriginBakerSharedKey>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();

            var originBakerSharedKeys = new NativeList<OriginBakerSharedKey>(Allocator.Temp);
            foreach ( var originBakerIdentity in SystemAPI.Query<OriginBakerIdentity>().WithAll<FreshlyBakedTag>() )
            {
                originBakerSharedKeys.Add(originBakerIdentity);
            }

            int originBakerKeysCount = originBakerSharedKeys.Length;

            if ( originBakerKeysCount != 0 )
            {
                var deps = new NativeArray<JobHandle>(originBakerKeysCount, Allocator.TempJob);
                var counterArray = new NativeArray<NativeReference<uint>>(originBakerKeysCount, Allocator.TempJob);
                for ( var index = 0; index < originBakerKeysCount; index++ )
                {
                    allSpawnedEntitiesQuery.SetSharedComponentFilter(originBakerSharedKeys[index]);
                    var reference = new NativeReference<uint>(1, Allocator.TempJob);

                    var fixupEntityGuidSourceJob = new FixupEntityGuidSourceJob {
                        Counter = reference,
                        EntityGuidHandle = SystemAPI.GetComponentTypeHandle<EntityGuid>(),
                        OriginSharedKeyHandle = SystemAPI.GetSharedComponentTypeHandle<OriginBakerSharedKey>()
                    };
                    counterArray[index] = reference;
                    deps[index] = fixupEntityGuidSourceJob.Schedule(allSpawnedEntitiesQuery, state.Dependency);
                }
                originBakerSharedKeys.Dispose();

                state.Dependency = JobHandle.CombineDependencies(deps);
                state.Dependency.Complete();
                for ( var index = 0; index < originBakerKeysCount; index++ )
                {
                    NativeReference<uint> counterRef = counterArray[index];
                    counterRef.Dispose();
                }
                deps.Dispose();
                counterArray.Dispose();
            }
            var allEntityGuids = SystemAPI.QueryBuilder()
                .WithAll<EntityGuid>()
                .WithNone<OriginBakerSharedKey>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();

            var correctDuplicateEntityGuidJob = new PatchDuplicateEntityGUIDJob {
                Register = new(allEntityGuids.CalculateEntityCount(), Allocator.TempJob)
            };
            correctDuplicateEntityGuidJob.Run(allEntityGuids);
            correctDuplicateEntityGuidJob.Register.Dispose();
        }

        [BurstCompile]
        public partial struct PatchDuplicateEntityGUIDJob : IJobEntity
        {
            private static readonly ProfilerMarker ExecuteMarker = new("CorrectDuplicateEntityGUIDJobMarker");

            public NativeHashMap<EntityGuid, uint> Register;

            public void Execute(ref EntityGuid entityGuid)
            {
                ExecuteMarker.Begin();
                var cpy = entityGuid;
                cpy.SetSerial(0);
                Register.TryGetValue(cpy, out var count);
                entityGuid.SetSerial(count);
                count++;
                Register[cpy] = count;
                ExecuteMarker.End();
            }
        }

        [BurstCompile]
        private unsafe struct FixupEntityGuidSourceJob : IJobChunk
        {
            private static readonly ProfilerMarker _ExecuteChunkMarker = new("FixupEntityGuidSourceJobMarker");
            public NativeReference<uint> Counter;
            [NativeDisableContainerSafetyRestriction] public ComponentTypeHandle<EntityGuid> EntityGuidHandle;
            [ReadOnly] public SharedComponentTypeHandle<OriginBakerSharedKey> OriginSharedKeyHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                _ExecuteChunkMarker.Begin();
                var entityChunkCount = chunk.Count;
                var sharedKey = chunk.GetSharedComponent(OriginSharedKeyHandle);
                var entityGuids = chunk.GetRequiredComponentDataPtrRWAsT(ref EntityGuidHandle);
                for ( var entityIndexInChunk = 0; entityIndexInChunk < entityChunkCount; entityIndexInChunk++ )
                {
                    Execute(in sharedKey, ref entityGuids[entityIndexInChunk]);
                }
                _ExecuteChunkMarker.End();
            }

            private void Execute(in OriginBakerSharedKey originBakerSharedKey, ref EntityGuid entityGuid)
            {
                entityGuid.SetSerialAndNameSpace((uint)originBakerSharedKey.Value, Counter.Value);
                Counter.Value = Counter.Value + 1;
            }
        }
    }
}