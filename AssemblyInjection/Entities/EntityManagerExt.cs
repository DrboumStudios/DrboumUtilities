using System;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        // From https://forum.unity.com/threads/really-hoped-for-refrw-refro-getcomponentrw-ro-entity.1369275/
        /// <summary>
        /// Gets the value of a component for an entity associated with a system.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>A <see cref="RefRW{T}"/> struct of type T containing access to the component value.</returns>
        /// <exception cref="ArgumentException">Thrown if the component type has no fields.</exception>
        [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(BurstCompatibleComponentData) })]
        public RefRW<T> GetComponentDataRW<T>(Entity entity)
            where T : unmanaged, IComponentData
        {
            var access = GetUncheckedEntityDataAccess();

            var typeIndex = TypeManager.GetTypeIndex<T>();
            var data = access->GetComponentDataRW_AsBytePointer(entity, typeIndex);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            return new RefRW<T>(data, access->DependencyManager->Safety.GetSafetyHandle(typeIndex, false));
#else
            return new RefRW<T>(data);
#endif
        }

        public bool TryGetComponent<TComponent>(Entity entity, out TComponent component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = GetComponentData<TComponent>(entity);
            else
                component = default;
            return hasComponent;
        }
        public bool TryGetComponentIfExist<TComponent>(Entity entity, ref TComponent component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = GetComponentData<TComponent>(entity);
            return hasComponent;
        }

        public bool TryGetComponentRW<TComponent>(Entity entity, out RefRW<TComponent> component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = GetComponentDataRW<TComponent>(entity);
            else
                component = default;
            return hasComponent;
        }

        public void CreateNewLinkedGroupRootFrom(Entity oldRoot, Entity newRoot)
        {
            var entityBufferList = new NativeList<Entity>(8, AllocatorManager.Temp);
            entityBufferList.Add(in newRoot);
            entityBufferList.AddRange(GetBuffer<LinkedEntityGroup>(oldRoot).Reinterpret<Entity>().AsNativeArray());
            AddBuffer<LinkedEntityGroup>(newRoot).Reinterpret<Entity>().AddRange(entityBufferList.AsArray());
        }

        public bool TrySetComponentEnabled<TEnableable>(Entity entity, bool enabled)
            where TEnableable : unmanaged, IEnableableComponent
        {
            if ( !HasComponent<TEnableable>(entity) )
                return false;

            SetComponentEnabled<TEnableable>(entity, enabled);
            return true;
        }

        public bool TryGetComponent<TBuffer>(Entity entity, out DynamicBuffer<TBuffer> component)
            where TBuffer : unmanaged, IBufferElementData
        {
            bool hasComponent = HasComponent<TBuffer>(entity);
            component = hasComponent ? GetBuffer<TBuffer>(entity) : default;
            return hasComponent;
        }

        [ExcludeFromBurstCompatTesting("Takes managed object")]
        public void SetComponentObject<T>(Entity entity, T componentObject)
            where T : class
        {
            SetComponentObject(entity, typeof(T), componentObject);
        }

        public void AddComponent<TComponent1, TComponent2>(Entity entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3>(Entity entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2, TComponent3>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4>(Entity entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(Entity entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>());
        }

        public void AddComponent<TComponent1, TComponent2>(NativeArray<Entity> entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3>(NativeArray<Entity> entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2, TComponent3>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4>(NativeArray<Entity> entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(NativeArray<Entity> entity)
        {
            AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>());
        }

        public void AddComponent<TComponent1, TComponent2>(EntityQuery entityQuery)
        {
            AddComponent(entityQuery, Create<TComponent1, TComponent2>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3>(EntityQuery entityQuery)
        {
            AddComponent(entityQuery, Create<TComponent1, TComponent2, TComponent3>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4>(EntityQuery entityQuery)
        {
            AddComponent(entityQuery, Create<TComponent1, TComponent2, TComponent3, TComponent4>());
        }

        public void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(EntityQuery entityQuery)
        {
            AddComponent(entityQuery, Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>());
        }

        public static ComponentTypeSet Create<TComponent1, TComponent2>()
        {
            return new ComponentTypeSet(ComponentType.ReadWrite<TComponent1>(), ComponentType.ReadWrite<TComponent2>());
        }

        public static ComponentTypeSet Create<TComponent1, TComponent2, TComponent3>()
        {
            return new ComponentTypeSet(
                ComponentType.ReadWrite<TComponent1>(),
                ComponentType.ReadWrite<TComponent2>(),
                ComponentType.ReadWrite<TComponent3>()
            );
        }

        public static ComponentTypeSet Create<TComponent1, TComponent2, TComponent3, TComponent4>()
        {
            return new ComponentTypeSet(
                ComponentType.ReadWrite<TComponent1>(),
                ComponentType.ReadWrite<TComponent2>(),
                ComponentType.ReadWrite<TComponent3>(),
                ComponentType.ReadWrite<TComponent4>()
            );
        }

        public static ComponentTypeSet Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>()
        {
            return new ComponentTypeSet(
                ComponentType.ReadWrite<TComponent1>(),
                ComponentType.ReadWrite<TComponent2>(),
                ComponentType.ReadWrite<TComponent3>(),
                ComponentType.ReadWrite<TComponent4>(),
                ComponentType.ReadWrite<TComponent5>()
            );
        }
    }
}