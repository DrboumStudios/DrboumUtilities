using Unity.Entities;

namespace Drboum.Utilities.Entities
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct SystemRunnableEnablerSystem : ISystem
    {
        private EntityQuery _runnableOnlyOnceQuery;

        public void OnCreate(ref SystemState state)
        {
            _runnableOnlyOnceQuery = SystemAPI.QueryBuilder()
                .WithAll<RunnableOncePerFrame>()
                .WithOptions(EntityQueryOptions.IncludeSystems | EntityQueryOptions.IgnoreComponentEnabledState | EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            state.RequireForUpdate(_runnableOnlyOnceQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.SetComponentEnabled<RunnableOncePerFrame>(_runnableOnlyOnceQuery, true);
        }

        public static bool IsSystemRunnableThisFrame(ref SystemState state)
        {
            var canRun = state.EntityManager.IsComponentEnabled<RunnableOncePerFrame>(state.SystemHandle);
            if ( canRun )
            {
                state.EntityManager.SetComponentEnabled<RunnableOncePerFrame>(state.SystemHandle, false);
            }
            return canRun;
        }
    }

    public struct RunnableOncePerFrame : IComponentData, IEnableableComponent
    { }
}