using Unity.Entities;
using Unity.Entities.Hybrid.Baking;

namespace GameProject.EntitiesInjected.Hybrid
{
    public static class EntityProxyBridge
    {
        public static EntityManager GetEntityManager(this IBaker baker)
        {
            return baker._State.World.EntityManager;
        }

        public static void BakeAuthoringWith<TOtherBaker>(this IBaker sourceBaker)
            where TOtherBaker : IBaker, new()
        {
            BakeAuthoringWith(sourceBaker, new TOtherBaker());
        }

        public static void BakeAuthoringWith<TOtherBaker>(this IBaker sourceBaker, TOtherBaker baker)
            where TOtherBaker : IBaker
        {
            baker.InvokeBake(sourceBaker._State);
        }

        public static bool TryGetLinkedGroupEntityBakingData(this EntityManager entityManager, Entity entity, out DynamicBuffer<Entity> linkedGroupEntityBakingDataAsEntity)
        {
            bool hasBuffer = entityManager.HasBuffer<LinkedEntityGroupBakingData>(entity);
            linkedGroupEntityBakingDataAsEntity = hasBuffer
                ? entityManager.GetBuffer<LinkedEntityGroupBakingData>(entity, true).Reinterpret<Entity>()
                : default;
            return hasBuffer;
        }
    }
}