using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Extensions;
using UnityEngine;

namespace Drboum.Utilities.Entities
{
    public class UnParentChildrenAuthoring : MonoBehaviour
    {
        public GameObject RemapChildrenOn;
        public bool BakeOnly = false;

        private void OnValidate()
        {
            if ( RemapChildrenOn == gameObject )
            {
                RemapChildrenOn = null;
                this.SetDirtySafe();
                LogHelper.LogErrorMessage($"remap on itself is useless and not allowed, the property {nameof(RemapChildrenOn)} is optional", $"Validation", this);
            }
        }

        public class Baker : Baker<UnParentChildrenAuthoring>
        {
            public override void Bake(UnParentChildrenAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                BakeRemoveParentImpl(this, authoring, entity, GetEntity(authoring.RemapChildrenOn, TransformUsageFlags.Dynamic));
                if ( authoring.BakeOnly )
                {
                    AddComponent<BakingOnlyEntity>(entity);
                }
            }

            public static void BakeRemoveParentImpl(IBaker baker, MonoBehaviour authoring, Entity entity, Entity remappingEntity)
            {
                if ( remappingEntity != Entity.Null )
                {
                    baker.AddComponent(entity, new RemapToParent {
                        NewParent = remappingEntity
                    });
                }
                baker.AddComponent<RemoveChildrenParentTag>(entity);
                var children = baker.AddBuffer<BakedChildren>(entity);
                var childrenGameObject = baker.GetChildren();

                baker.DependsOn(authoring.transform);
                foreach ( var childGameObject in childrenGameObject )
                {
                    baker.DependsOn(childGameObject);
                    children.Add(new BakedChildren {
                        Value = baker.GetEntity(childGameObject, TransformUsageFlags.None)
                    });
                }
            }
        }
    }

    [BakingType]
    public struct BakedChildren : IBufferElementData
    {
        public Entity Value;
    }

    [BakingType]
    public struct RemapToParent : IComponentData
    {
        public Entity NewParent;
    }

    [BakingType]
    public struct RemoveParentBakingTag : IComponentData
    { }

    [BakingType]
    public struct RemoveChildrenParentTag : IComponentData
    { }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(TransformBakingSystemGroup), OrderFirst = true)]
    public partial struct RemoveParentChildrenBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var allChildren = new NativeList<Entity>(Allocator.TempJob);
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach ( var (children, transformAuthoring, entity)
                     in SystemAPI.Query<DynamicBuffer<BakedChildren>, TransformAuthoring>()
                         .WithEntityAccess()
                         .WithAll<RemoveChildrenParentTag>()
                         .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities) )
            {
                var childrenEntities = children.Reinterpret<Entity>().AsNativeArray();
                RemapToParent remapToParent = default;
                bool hasComponent = state.EntityManager.TryGetComponentAndSetRefValueIfExist(entity, ref remapToParent);
                if ( hasComponent )
                {
                    while ( state.EntityManager.TryGetComponentAndSetRefValueIfExist(remapToParent.NewParent, ref remapToParent) )
                    { }
                    ecb.AddComponent(childrenEntities, remapToParent);
                }
                else
                {
                    allChildren.AddRange(childrenEntities);
                }
            }
            ecb.Playback(state.EntityManager);
            state.EntityManager.AddComponent<RemoveParentBakingTag>(allChildren.AsArray());

            foreach ( var transformAuthoringRW
                     in SystemAPI.Query<RefRW<TransformAuthoring>>()
                         .WithNone<RemapToParent>()
                         .WithAll<RemoveParentBakingTag>()
                         .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities) )
            {
                ref var transformAuthoring = ref transformAuthoringRW.ValueRW;
                transformAuthoring.RuntimeParent = Entity.Null;
                transformAuthoring.RuntimeTransformUsage &= ~RuntimeTransformComponentFlags.RequestParent;
            }

            foreach ( var (transformAuthoringRW, remapToParent)
                     in SystemAPI.Query<RefRW<TransformAuthoring>, RemapToParent>()
                         .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities) )
            {
                var newParent = remapToParent;
                while ( state.EntityManager.TryGetComponentAndSetRefValueIfExist(newParent.NewParent, ref newParent) )
                { }
                transformAuthoringRW.ValueRW.RuntimeParent = newParent.NewParent;
                if ( newParent.NewParent == Entity.Null )
                {
                    transformAuthoringRW.ValueRW.RuntimeTransformUsage &= ~RuntimeTransformComponentFlags.RequestParent;
                }
            }
        }
    }
}