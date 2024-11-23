using Unity.Burst;
using Unity.Entities;

namespace Drboum.Utilities.Entities
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct RawTickUpdaterSystem : ISystem,ISystemStartStop
    {
        private TickUpdaterSystem<RawTick> _tickUpdaterSystem;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _tickUpdaterSystem.OnUpdate(ref state,in SystemAPI.Time);
        }
        
        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            _tickUpdaterSystem.OnStartRunning(ref state);
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            _tickUpdaterSystem.OnStopRunning(ref state);
        }
    }
}