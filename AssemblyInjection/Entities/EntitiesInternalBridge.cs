using System;
using Unity.Collections;
using Unity.Entities;

public static unsafe class EntitiesInternalBridge
{
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