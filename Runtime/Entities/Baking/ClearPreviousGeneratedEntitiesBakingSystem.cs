using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Drboum.Utilities.Entities.Baking
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct ClearPreviousGeneratedEntitiesBakingSystem : ISystem
    {
        private EntityQuery _clearQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _clearQuery = SystemAPI.QueryBuilder()
                .WithAll<OriginBakerSharedKey>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _clearQuery.ResetFilter();
            var allBakingSpawnersQuery = SystemAPI.QueryBuilder()
                .WithAll<OriginBakerIdentity, FreshlyBakedTag>()
                .Build();
            
            var allBakingSpawnersIds = allBakingSpawnersQuery.ToComponentDataArray<OriginBakerIdentity>(Allocator.Temp);
            for ( var index = 0; index < allBakingSpawnersIds.Length; index++ )
            {
                var originBakerIdentity = allBakingSpawnersIds[index];
                _clearQuery.SetSharedComponentFilter(originBakerIdentity.ToSharedKey());
                state.EntityManager.DestroyEntity(_clearQuery);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        { }
    }
}