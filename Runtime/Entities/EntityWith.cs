using Unity.Entities;
namespace Drboum.Utilities.Entities
{
    /// <summary>
    /// Represents an entity that has the expected component or Buffer type on it
    /// </summary>
    /// <typeparam name="TExpectedComponent"></typeparam>
    public struct EntityWith<TExpectedComponent> : IComponentData
    {
        public Entity Value;

        public static implicit operator Entity(EntityWith<TExpectedComponent> entityWith)
        {
            return entityWith.Value;
        }

        public static implicit operator EntityWith<TExpectedComponent>(Entity entity)
        {
            return new EntityWith<TExpectedComponent> {
                Value = entity
            };
        }
    }

    public static class EntityWithExtensions
    {
        public static bool IsComponentEnabled<TComponent>(this in EntityWith<TComponent> refEntity, EntityManager entityManager)
            where TComponent : unmanaged, IEnableableComponent
        {
            return entityManager.IsComponentEnabled<TComponent>(refEntity.Value);
        }

        public static bool IsComponentEnabled<TComponent>(this in EntityWith<TComponent> refEntity, ComponentLookup<TComponent> entityManager)
            where TComponent : unmanaged, IComponentData, IEnableableComponent
        {
            return entityManager.IsComponentEnabled(refEntity.Value);
        }

        public static bool IsComponentEnabled<TComponent>(this in EntityWith<TComponent> refEntity, BufferLookup<TComponent> entityManager)
            where TComponent : unmanaged, IBufferElementData, IEnableableComponent
        {
            return entityManager.IsBufferEnabled(refEntity.Value);
        }

        public static RefRW<TComponent> GetComponentRW<TComponent>(this in EntityWith<TComponent> refEntity, ref ComponentLookup<TComponent> entityManager)
            where TComponent : unmanaged, IComponentData
        {
            return entityManager.GetRefRW(refEntity.Value);
        }

        public static TComponent GetComponent<TComponent>(this in EntityWith<TComponent> refEntity, EntityManager entityManager)
            where TComponent : unmanaged, IComponentData
        {
            return entityManager.GetComponentData<TComponent>(refEntity.Value);
        }

        public static TComponent GetComponent<TComponent>(this in EntityWith<TComponent> refEntity, ref ComponentLookup<TComponent> entityManager)
            where TComponent : unmanaged, IComponentData
        {
            return entityManager[refEntity.Value];
        }

        public static DynamicBuffer<TComponent> GetBuffer<TComponent>(this in EntityWith<TComponent> refEntity, EntityManager entityManager)
            where TComponent : unmanaged, IBufferElementData
        {
            return entityManager.GetBuffer<TComponent>(refEntity.Value);
        }

        public static DynamicBuffer<TComponent> GetBuffer<TComponent>(this in EntityWith<TComponent> refEntity, ref BufferLookup<TComponent> entityManager)
            where TComponent : unmanaged, IBufferElementData
        {
            return entityManager[refEntity.Value];
        }
    }
}