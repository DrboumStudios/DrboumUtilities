using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Drboum.Utilities.Entities
{
    public static class PhysicsHelper
    {
        
        public static unsafe BoxGeometry GetBoxGeometry(this in PhysicsCollider p)
        {
            return ((Unity.Physics.BoxCollider*)p.ColliderPtr)->Geometry;
        }

        public static unsafe float3 GetCenterOffset(Unity.Physics.Collider* collider)
        {
            return collider->MassProperties.MassDistribution.Transform.pos;
        }

        public static unsafe TCollider* AsCollider<TCollider>(Unity.Physics.Collider* collider)
            where TCollider : unmanaged, ICollider
        {
            return (TCollider*)collider;
        }

        public static unsafe BoxGeometry SetGeometry(this in PhysicsCollider p, BoxGeometry geometry)
        {
            return ((Unity.Physics.BoxCollider*)p.ColliderPtr)->Geometry = geometry;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoxGeometry DecomposeBoxColliderQuery(BoxGeometry originalBoxGeometry, float cubeQueryStep, LocalTransform transform, out float3 colliderPosition, out float3 endColliderPosition)
        {
            var boxGeometryCpy = originalBoxGeometry;
            boxGeometryCpy.Size = new float3(originalBoxGeometry.Size.x, originalBoxGeometry.Size.y, cubeQueryStep);
            boxGeometryCpy.Center = new float3(originalBoxGeometry.Center.x, originalBoxGeometry.Center.y, cubeQueryStep * .5f);
            colliderPosition = transform.Position + math.mul(transform.Rotation, boxGeometryCpy.Center);
            endColliderPosition = transform.Position + math.mul(transform.Rotation, new float3(boxGeometryCpy.Center.x, boxGeometryCpy.Center.y, originalBoxGeometry.Size.z - boxGeometryCpy.Center.z));
            return boxGeometryCpy;
        }
    }
}