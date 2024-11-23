using Drboum.Utilities.Rendering;
using Drboum.Utilities.Runtime.EditorHybrid;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Drboum.Utilities.Entities.Baking
{
#if HAS_SHAPE_ASSET
         [RequireComponent(typeof(WiredCubeDrawer))]
#endif
    public class EntitySpawnerAuthoring : EditorObjectTracker
    {
        public GameObject SpawnPrefab;
        public uint SpawnCount;
        public AABB SpawnBounds = new() {
            Extents = new float3(50, 0.1f, 50)
        };
        public Vector3 InstanceSpacing;
#if HAS_SHAPE_ASSET
             [SerializeField] private WiredCubeDrawer m_wiredCubeDrawer;
#endif

        protected override void OnValidate()
        {
            base.OnValidate();
            if ( SpawnPrefab != null )
                SpawnBounds.Extents = math.max(SpawnBounds.Extents, SpawnPrefab.GetComponentInChildren<Renderer>().bounds.extents);

#if HAS_SHAPE_ASSET
           if ( !m_wiredCubeDrawer && !TryGetComponent(out m_wiredCubeDrawer) )
                return;

            m_wiredCubeDrawer.SetDrawingData(SpawnBounds);
#endif
        }

        class Baker : Baker<EntitySpawnerAuthoring>
        {
            public override void Bake(EntitySpawnerAuthoring authoring)
            {
                if ( !authoring.SpawnPrefab )
                    return;

                DependsOn(authoring.transform);
                Entity prefabEntity = GetEntity(authoring.SpawnPrefab, TransformUsageFlags.Renderable);
                Entity entity = GetEntity(TransformUsageFlags.WorldSpace);
                var entitySpawnRequest = new EntitySpawnRequest {
                    Prefab = prefabEntity,
                    SpawnCount = authoring.SpawnCount,
                    InstanceSpacing = authoring.SpawnPrefab.GetComponentInChildren<Renderer>().bounds.size + authoring.InstanceSpacing
                };
                AddComponent(entity, entitySpawnRequest);
                AddComponent<FreshlyBakedTag>(entity);
                AddComponent(entity, new OriginBakerIdentity {
                    Value = authoring.AssetInstanceGuid.GetHashCode()
                });
                AddComponent<AABBComponent>(entity, authoring.SpawnBounds);
            }
        }
    }

    public struct SpawnChildTracker : IBufferElementData
    {
        public Entity Value;
    }

    public struct EntitySpawnRequest : IComponentData
    {
        public uint SpawnCount;
        public Entity Prefab;
        public float3 InstanceSpacing;
    }
}