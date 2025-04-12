using Unity.Burst;
using Unity.Entities;

namespace Drboum.Utilities.Entities
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginInitializationEntityCommandBufferSystem))]
    internal partial struct InitializeEntityRemovalSystem : ISystem
    {
        [BurstCompile]
        void ISystem.OnUpdate(ref SystemState state)
        {
            var allJustSpawnedQuery = SystemAPI.QueryBuilder()
                .WithAll<InitializeEntityTag>()
                .Build();
            state.EntityManager.RemoveComponent<InitializeEntityTag>(allJustSpawnedQuery);
        }
    }

    public struct InitializeEntityTag : IComponentData
    { }
}