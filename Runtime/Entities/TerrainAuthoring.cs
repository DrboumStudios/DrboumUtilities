using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using TerrainCollider = Unity.Physics.TerrainCollider;

namespace Drboum.Utilities.Entities
{
    public class TerrainAuthoring : MonoBehaviour
    {
        [SerializeField, Tooltip("A bit mask describing which layers this object belong to.")]
        private PhysicsCategoryTags belongTo = PhysicsCategoryTags.Everything;
        [SerializeField, Tooltip("A bit mask describing which layers this object can collide with.")]
        private PhysicsCategoryTags collidesWith = PhysicsCategoryTags.Everything;

        [SerializeField, Tooltip("An optional override for the bit mask checks. If the value in both objects is equal and positive, the objects always collide. If the value in both objects is equal and negative, the objects never collide.")]
        private int groupIndex;

        [SerializeField, Tooltip("Triangles works like a mesh, is more accurate but more expensive. VertexSamples works like a terrain, much less accurate but very fast.")]
        private TerrainCollider.CollisionMethod collisionMethod = TerrainCollider.CollisionMethod.Triangles;

        class Baker : Baker<TerrainAuthoring>
        {
            public override void Bake(TerrainAuthoring authoring)
            {
                // fetch the terrain monobehaviour
                if ( !authoring.TryGetComponent<Terrain>(out var terrain) )
                {
                    Debug.LogWarning("Terrain game object not found!");
                    return;
                }

                // This keeps the Terrain game-object synchronized with the baked mesh.
                // But it doesn't really work like it's supposed to. Often I have to Reimport the scene anyway.
                DependsOn(terrain);

                // setup a collision filter using the parameters the user specified.
                var collisionFilter = new CollisionFilter {
                    BelongsTo = authoring.belongTo.Value,
                    CollidesWith = authoring.collidesWith.Value,
                    GroupIndex = authoring.groupIndex
                };

                // create the physics terrain collider and add it to the baked entity
                // see https://forum.unity.com/threads/using-unity-terrain-with-dots-workflow.755105
                var collider = CreateTerrainColliderV2(terrain.terrainData, collisionFilter, authoring.collisionMethod);
                if ( !collider.IsValid )
                {
                    Debug.LogWarning("Collider is invalid!");
                    return;
                }

                var ent = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(ent, collider);
                // This is needed for the collider to be registered properly in the physics world
                AddSharedComponent(ent, new PhysicsWorldIndex());
            }
        }

        private static PhysicsCollider CreateTerrainColliderV2(TerrainData terrainData, CollisionFilter filter, TerrainCollider.CollisionMethod method)
        {
            var physicsCollider = new PhysicsCollider();
            var scale = terrainData.heightmapScale;

            var colliderHeights = new NativeArray<float>(terrainData.heightmapResolution * terrainData.heightmapResolution, Allocator.TempJob);
            var terrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            // NOTE: Solves an issue with perfectly flat terrain failing to collide with objects.
            var heightmapScale = terrainData.size.z;
            var smallestOffset = 0.01f * (2048f / terrainData.heightmapResolution); // 1 cm offset, works with 2048 resolution terrain
            var heightmapValuePerMeterInWorldSpace = 0.5f / heightmapScale;
            var inHeightMapUnits = smallestOffset * heightmapValuePerMeterInWorldSpace;

            for ( var j = 0; j < terrainData.heightmapResolution; j++ )
            {
                for ( var i = 0; i < terrainData.heightmapResolution; i++ )
                {
                    var checkerboard = (i + j) % 2;
                    colliderHeights[j + (i * terrainData.heightmapResolution)] = terrainHeights[i, j] + inHeightMapUnits * checkerboard; // Note: assumes terrain neighboars are never 1 cm difference from eachother
                }
            }

            // Note: Heightmap is between 0 and 0.5f (https://forum.unity.com/threads/terraindata-heightmaptexture-float-value-range.672421/)
            physicsCollider.Value = TerrainCollider.Create(colliderHeights, new int2(terrainData.heightmapResolution, terrainData.heightmapResolution), scale, method, filter);

            colliderHeights.Dispose();
            return physicsCollider;
        }
    }
}