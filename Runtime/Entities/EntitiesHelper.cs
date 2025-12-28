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

public static unsafe partial class EntitiesHelper
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

    public static EntityQuery SetChangedVersionFilter<TComponent>(this ref EntityQuery query)
    {
        query.SetChangedVersionFilter(ComponentType.ReadWrite<TComponent>());
        return query;
    }

    public static EntityQuery AddChangedVersionFilter<TComponent>(this ref EntityQuery query)
    {
        query.AddChangedVersionFilter(ComponentType.ReadWrite<TComponent>());
        return query;
    }

    public static void GetBufferLookup<TBuffer>(this ref SystemState state, out BufferLookup<TBuffer> typeHandle, bool isReadOnly = false)
        where TBuffer : unmanaged, IBufferElementData
    {
        typeHandle = state.GetBufferLookup<TBuffer>(isReadOnly);
    }

    public static void GetComponentLookup<TBuffer>(this ref SystemState state, out ComponentLookup<TBuffer> typeHandle, bool isReadOnly = false)
        where TBuffer : unmanaged, IComponentData
    {
        typeHandle = state.GetComponentLookup<TBuffer>(isReadOnly);
    }

    public static void GetBufferTypeHandle<TBuffer>(this ref SystemState state, out BufferTypeHandle<TBuffer> typeHandle, bool isReadOnly = false)
        where TBuffer : unmanaged, IBufferElementData
    {
        typeHandle = state.GetBufferTypeHandle<TBuffer>(isReadOnly);
    }

    public static void GetComponentTypeHandle<TBuffer>(this ref SystemState state, out ComponentTypeHandle<TBuffer> typeHandle, bool isReadOnly = false)
        where TBuffer : unmanaged, IComponentData
    {
        typeHandle = state.GetComponentTypeHandle<TBuffer>(isReadOnly);
    }

    public static Type GetSystemType(this ref SystemHandle handle, WorldUnmanaged worldUnmanaged)
    {
        return worldUnmanaged.GetTypeOfSystem(handle);
    }
#pragma warning disable EA0016
    public static unsafe T GetManagedSystemInstance<T>(this ref SystemHandle handle, WorldUnmanaged worldUnmanaged, out SystemState systemState)
        where T : ComponentSystemBase
    {
        SystemState* s = worldUnmanaged.ResolveSystemState(handle);
        systemState = *s;
        if ( s != null )
        {
            if ( s->m_ManagedSystem.IsAllocated )
            {
                return s->m_ManagedSystem.Target as T;
            }
        }
        return null;
    }
#pragma warning restore EA0016
    public static void* GetDynamicComponentPtr(in ArchetypeChunk chunk, ref DynamicComponentTypeHandle componentTypeHandle)
    {
        return chunk.GetDynamicComponentDataPtr(ref componentTypeHandle);
    }

    public static void CopyEntities(EntityManager entityManager, NativeArray<Entity> srcEntities, NativeArray<Entity> outputEntities)
    {
        entityManager.CopyEntitiesInternal(srcEntities, outputEntities);
    }

    public static void RequireForUpdate<TComponent1, TComponent2>(this ref SystemState state)
    {
        const int count = 2;
        var componentTypes = stackalloc ComponentType[count] {
            ComponentType.ReadWrite<TComponent1>(),
            ComponentType.ReadWrite<TComponent2>()
        };
        RequireComponentsForUpdate(ref state, componentTypes, count);
    }

    public static void RequireForUpdate<TComponent1, TComponent2, TComponent3>(this ref SystemState state)
    {
        const int count = 3;
        var componentTypes = stackalloc ComponentType[count] {
            ComponentType.ReadWrite<TComponent1>(),
            ComponentType.ReadWrite<TComponent2>(),
            ComponentType.ReadWrite<TComponent3>(),
        };
        RequireComponentsForUpdate(ref state, componentTypes, count);
    }

    public static void RequireForUpdate<TComponent1, TComponent2, TComponent3, TComponent4>(this ref SystemState state)
    {
        const int count = 4;
        var componentTypes = stackalloc ComponentType[count] {
            ComponentType.ReadWrite<TComponent1>(),
            ComponentType.ReadWrite<TComponent2>(),
            ComponentType.ReadWrite<TComponent3>(),
            ComponentType.ReadWrite<TComponent4>(),
        };
        RequireComponentsForUpdate(ref state, componentTypes, count);
    }

    public static void RequireForUpdate<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(this ref SystemState state)
    {
        const int count = 5;
        var componentTypes = stackalloc ComponentType[count] {
            ComponentType.ReadWrite<TComponent1>(),
            ComponentType.ReadWrite<TComponent2>(),
            ComponentType.ReadWrite<TComponent3>(),
            ComponentType.ReadWrite<TComponent4>(),
            ComponentType.ReadWrite<TComponent5>(),
        };
        RequireComponentsForUpdate(ref state, componentTypes, count);
    }

    private static void RequireComponentsForUpdate(ref SystemState state, ComponentType* componentTypes, int count)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp, componentTypes, count).WithOptions(EntityQueryOptions.IncludeSystems);
        state.RequireForUpdate(state.GetEntityQueryInternal(builder));
    }
}