using System;
using Unity.Collections;
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

        public bool TryGetComponent<TComponent>(Entity entity, ref TComponent component)
            where TComponent : unmanaged, IComponentData
        {
            bool hasComponent = HasComponent<TComponent>(entity);
            if ( hasComponent )
                component = GetComponentData<TComponent>(entity);
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
            {
                return false;
            }
            SetComponentEnabled<TEnableable>(entity, enabled);
            return true;
        }

        public bool TryGetComponent<TComponent>(Entity entity, ref DynamicBuffer<TComponent> component)
            where TComponent : unmanaged, IBufferElementData
        {
            bool hasComponent = HasComponent<TComponent>(entity);
            component = hasComponent ? GetBuffer<TComponent>(entity) : default;
            return hasComponent;
        }
    }
}