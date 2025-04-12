using System;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    { }

    public static unsafe class EntitiesExtensions
    {
        // From https://forum.unity.com/threads/really-hoped-for-refrw-refro-getcomponentrw-ro-entity.1369275/
        /// <summary>
        /// Gets the value of a component for an entity associated with a system.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>A <see cref="RefRW{T}"/> struct of type T containing access to the component value.</returns>
        /// <exception cref="ArgumentException">Thrown if the component type has no fields.</exception>
        [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(BurstCompatibleComponentData) })]
        public static RefRW<T> GetComponentDataRW<T>(this EntityManager em, Entity entity)
            where T : unmanaged, IComponentData
        {
            var access = em.GetUncheckedEntityDataAccess();

            var typeIndex = TypeManager.GetTypeIndex<T>();
            var data = access->GetComponentDataRW_AsBytePointer(entity, typeIndex);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            return new RefRW<T>(data, access->DependencyManager->Safety.GetSafetyHandle(typeIndex, false));
#else
            return new RefRW<T>(data);
#endif
        }

        public static bool TryGetComponent<TComponent>(this EntityManager em, Entity entity, out TComponent component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = em.HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = em.GetComponentData<TComponent>(entity);
            else
                component = default;
            return hasComponent;
        }

        /// <summary>
        /// will only set <see cref="component"/> ref if the component exists
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public static bool TryGetComponentAndSetRefValueIfExist<TComponent>(this EntityManager em, Entity entity, ref TComponent component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = em.HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = em.GetComponentData<TComponent>(entity);
            return hasComponent;
        }

        public static bool TryGetComponentRW<TComponent>(this EntityManager em, Entity entity, out RefRW<TComponent> component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = em.HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = em.GetComponentDataRW<TComponent>(entity);
            else
                component = default;
            return hasComponent;
        }

        public static void CreateNewLinkedGroupRootFrom(this EntityManager em, Entity oldRoot, Entity newRoot)
        {
#if UNITY_EDITOR
            if ( !em.HasComponent<LinkedEntityGroup>(oldRoot) )
            {
                Debug.LogError($"Did not create a linked group root from {oldRoot} because there was no existing group. ");
                return;
            }
#endif
            var entityBufferList = new NativeList<Entity>(8, AllocatorManager.Temp);
            entityBufferList.Add(in newRoot);
            entityBufferList.AddRange(em.GetBuffer<LinkedEntityGroup>(oldRoot).Reinterpret<Entity>().AsNativeArray());
            var newLinkedGroup = em.AddBuffer<LinkedEntityGroup>(newRoot).Reinterpret<Entity>();
            newLinkedGroup.Clear();
            newLinkedGroup.AddRange(entityBufferList.AsArray());
        }

        public static bool TrySetComponentEnabled<TEnableable>(this EntityManager em, Entity entity, bool enabled)
            where TEnableable : unmanaged, IEnableableComponent
        {
            if ( !em.HasComponent<TEnableable>(entity) )
                return false;

            em.SetComponentEnabled<TEnableable>(entity, enabled);
            return true;
        }

        public static bool TryGetComponent<TBuffer>(this EntityManager em, Entity entity, out DynamicBuffer<TBuffer> component)
            where TBuffer : unmanaged, IBufferElementData
        {
            bool hasComponent = em.HasComponent<TBuffer>(entity);
            component = hasComponent ? em.GetBuffer<TBuffer>(entity) : default;
            return hasComponent;
        }

        [ExcludeFromBurstCompatTesting("Takes managed object")]
        public static void SetComponentObject<T>(this EntityManager em, Entity entity, T componentObject)
            where T : class
        {
            em.SetComponentObject(entity, typeof(T), componentObject);
        }

        public static void AddComponent<TComponent1, TComponent2>(this EntityManager em, Entity entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3>(this EntityManager em, Entity entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2, TComponent3>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4>(this EntityManager em, Entity entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(this EntityManager em, Entity entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>());
        }

        public static void AddComponent<TComponent1, TComponent2>(this EntityManager em, NativeArray<Entity> entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3>(this EntityManager em, NativeArray<Entity> entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2, TComponent3>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4>(this EntityManager em, NativeArray<Entity> entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(this EntityManager em, NativeArray<Entity> entity)
        {
            em.AddComponent(entity, Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>());
        }

        public static void AddComponent<TComponent1, TComponent2>(this EntityManager em, EntityQuery entityQuery)
        {
            em.AddComponent(entityQuery, Create<TComponent1, TComponent2>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3>(this EntityManager em, EntityQuery entityQuery)
        {
            em.AddComponent(entityQuery, Create<TComponent1, TComponent2, TComponent3>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4>(this EntityManager em, EntityQuery entityQuery)
        {
            em.AddComponent(entityQuery, Create<TComponent1, TComponent2, TComponent3, TComponent4>());
        }

        public static void AddComponent<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(this EntityManager em, EntityQuery entityQuery)
        {
            em.AddComponent(entityQuery, Create<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>());
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