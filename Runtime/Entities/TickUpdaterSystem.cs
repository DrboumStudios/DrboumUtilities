using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;

namespace Drboum.Utilities.Entities
{
    public interface ITickData
    {
        double ElapsedTime { get; set; }
        uint TickValue { get; set; }
    }

    [DisableAutoCreation]
    public partial struct TickUpdaterSystem<T> : ISystem,ISystemStartStop
        where T : unmanaged, ITickData, IComponentData
    {

        private uint _tick;
        private T _tickData;
        private Entity _tickSingletonEntity;

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            using var builder = new EntityQueryBuilder(Allocator.Temp);
            _tickData.TickValue = 1;
            _tickData.ElapsedTime = 0;
            if ( !builder.WithPresent<T>().Build(ref state).TryGetSingletonEntity<T>(out _tickSingletonEntity) )
            {
                _tickSingletonEntity = state.EntityManager.CreateSingleton<T>();
            }
            state.EntityManager.SetComponentData(_tickSingletonEntity, _tickData);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state, in TimeData timeData)
        {
            _tick++;
            _tickData.TickValue = _tick;
            _tickData.ElapsedTime = timeData.ElapsedTime;
            state.EntityManager.SetComponentData(_tickSingletonEntity, _tickData);
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }
    }

    public struct RawTick : IComponentData, ITickData
    {
        public double ElapsedTime {
            get;
            private set;
        }
        public uint Tick {
            get;
            private set;
        }
        double ITickData.ElapsedTime { get => ElapsedTime; set => ElapsedTime = value; }
        uint ITickData.TickValue { get => Tick; set => Tick = value; }
    }
}