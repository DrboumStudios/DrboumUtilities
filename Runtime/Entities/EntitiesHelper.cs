using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Drboum.Utilities.Entities
{
    public static partial class EntitiesHelper
    {

        public static EntityGuid GenerateEntityGuid(Hash128 guid, uint serial, ComponentType componentType, int originatingId = 0)
        {
            return new EntityGuid(originatingId, guid.GetHashCode(), (uint)componentType.TypeIndex.Index, serial);
        }

        public static EntityGuid GenerateEntityGuid(int subId, uint serial, ComponentType componentType, int originatingId = 0)
        {
            return new EntityGuid(originatingId, subId, (uint)componentType.TypeIndex.Index, serial);
        }

        public static bool RegisterEntityToBakingIfValid(this IBaker baker, Transform childTransform, ref NativeHashSet<Entity> deduplicator, IReadOnlyList<Type> bakingTypes, IReadOnlyList<Type> excludingTypes)
        {
            var skipGameObject = false;

            for ( var typeIndex = 0; typeIndex < excludingTypes.Count; typeIndex++ )
            {
                Type excludingType = excludingTypes[typeIndex];
                if ( childTransform.TryGetComponent(excludingType, out _) )
                {
                    skipGameObject = true;
                    break;
                }
            }

            if ( skipGameObject )
                return false;

            for ( var typeIndex = 0; typeIndex < bakingTypes.Count; typeIndex++ )
            {
                Type bakingType = bakingTypes[typeIndex];
                if ( childTransform.TryGetComponent(bakingType, out var foundComponent) )
                {
                    AddToDeduplicator(baker, ref deduplicator, foundComponent);
                    break;
                }
            }
            return true;
        }

        public static void RegisterChildrenEntityFromAnyMatchingGameObject(this IBaker baker, Transform targetTransform, ref NativeHashSet<Entity> deduplicator, IReadOnlyList<Type> bakingTypes, IReadOnlyList<Type> excludingTypes = null)
        {
            if ( excludingTypes == null )
            {
                excludingTypes = Array.Empty<Type>();
            }

            RegisterChildrenEntityFromAnyMatchingGameObjectImpl(baker, targetTransform, ref deduplicator, bakingTypes, excludingTypes);
        }

        private static void RegisterChildrenEntityFromAnyMatchingGameObjectImpl(IBaker baker, Transform targetTransform, ref NativeHashSet<Entity> deduplicator, IReadOnlyList<Type> bakingTypes, IReadOnlyList<Type> excludingTypes)
        {
            foreach ( Transform childTransform in targetTransform.transform )
            {
                if ( !RegisterEntityToBakingIfValid(baker, childTransform, ref deduplicator, bakingTypes, excludingTypes) )
                    continue;

                RegisterChildrenEntityFromAnyMatchingGameObjectImpl(baker, childTransform, ref deduplicator, bakingTypes, excludingTypes);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetRequiredComponentDataPtrROAsT<T>(this ArchetypeChunk chunk, ref ComponentTypeHandle<T> heatMaterialSettingsHandle)
            where T : unmanaged
        {
            return (T*)chunk.GetRequiredComponentDataPtrRO(ref heatMaterialSettingsHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T* GetRequiredComponentDataPtrRWAsT<T>(this ArchetypeChunk chunk, ref ComponentTypeHandle<T> heatMaterialSettingsHandle)
            where T : unmanaged
        {
            return (T*)chunk.GetRequiredComponentDataPtrRW(ref heatMaterialSettingsHandle);
        }
        public static uint GetNameSpaceId(this in EntityGuid guid)
        {
            return (uint)(guid.b >> 32);
        }
        public static void SetSerialAndNameSpace(this ref EntityGuid guid, uint nameSpaceId, uint serial)
        {
            guid.b = serial | ((ulong)nameSpaceId << 32);
        }
        public static void SetSerial(this ref EntityGuid guid, uint serial)
        {
            guid.SetSerialAndNameSpace(guid.GetNameSpaceId(), serial);
        }

        public static Entity GetSystemEntity<TSystemTag>(this ref SystemState state)
            where TSystemTag : IComponentData
        {
            return new EntityQueryBuilder(Allocator.Temp)
                .WithPresent<TSystemTag>()
                .WithOptions(EntityQueryOptions.IncludeSystems)
                .Build(ref state)
                .GetSingletonEntity();
        }

        public static Entity AddTagToGetSystemEntity<TSystemTag>(this ref SystemState state)
            where TSystemTag : IComponentData
        {
            state.EntityManager.AddComponent<TSystemTag>(state.SystemHandle);
            return state.GetSystemEntity<TSystemTag>();
        }

        public static void SetColliderGroupIndex(this PhysicsCollider collider, int collisionFilterGroupIndex)
        {
#if UNITY_EDITOR
            if ( !collider.Value.Value.IsUnique )
            {
                Debug.LogWarning($"modifying non unique collider will affect all colliders that shares it, make sure this is intended.");
            }
#endif
            var collisionFilter = collider.Value.Value.GetCollisionFilter();
            collisionFilter.GroupIndex = collisionFilterGroupIndex;
            collider.Value.Value.SetCollisionFilter(collisionFilter);
        }

        public static void AddChildrenWith<TComponent>(this IBaker baker, GameObject root, ref NativeHashSet<Entity> deduplicator)
            where TComponent : Component
        {
            var particlesInChildren = root.GetComponentsInChildren<TComponent>();
            foreach ( var component in particlesInChildren )
            {
                AddToDeduplicator(baker, ref deduplicator, component);
            }
        }

        public static void AddChildrenWith<TComponent>(this IBaker baker, GameObject root, ref NativeHashSet<Entity> deduplicator, Func<TComponent, bool> addCondition)
            where TComponent : Component
        {
            var particlesInChildren = root.GetComponentsInChildren<TComponent>();
            foreach ( var component in particlesInChildren )
            {
                if ( addCondition.Invoke(component) )
                {
                    AddToDeduplicator(baker, ref deduplicator, component);
                }
            }
        }

        private static void AddToDeduplicator<TComponent>(IBaker baker, ref NativeHashSet<Entity> deduplicator, TComponent component)
            where TComponent : Component
        {
            Entity child = baker.GetEntity(component, TransformUsageFlags.None);
            deduplicator.Add(child);
        }

        public static void SetComponentEnabledIfExist<TEnableable>(this ref SystemState state, Entity entity, bool enabled)
            where TEnableable : unmanaged, IEnableableComponent
        {
            if ( state.EntityManager.HasComponent<TEnableable>(entity) )
            {
                state.EntityManager.SetComponentEnabled<TEnableable>(entity, enabled);
            }
        }

        public static bool TryGetComponent<TComponent>(this EntityManager entityManager, Entity entity, ref TComponent component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = entityManager.HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = entityManager.GetComponentData<TComponent>(entity);
            return hasComponent;
        }

        public static bool TryGetComponent<TComponent>(this EntityManager entityManager, Entity entity, ref DynamicBuffer<TComponent> component)
            where TComponent : unmanaged, IBufferElementData
        {
            bool hasComponent = entityManager.HasComponent<TComponent>(entity);
            component = hasComponent ? entityManager.GetBuffer<TComponent>(entity) : default;
            return hasComponent;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this ref LocalToWorld matrix, in float3 position)
        {
            matrix.Value.SetPosition(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this ref float4x4 matrix, in float3 position)
        {
            matrix.c3.xyz = position;
        }
    }
}