using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

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
    public static BoxGeometry DecomposeBoxColliderQuery(this BoxGeometry originalBoxGeometry, float cubeQueryStep, LocalTransform transform, out float3 colliderPosition, out float3 endColliderPosition)
    {
        originalBoxGeometry.Size = new float3(originalBoxGeometry.Size.x, originalBoxGeometry.Size.y, cubeQueryStep);
        originalBoxGeometry.Center = new float3(originalBoxGeometry.Center.x, originalBoxGeometry.Center.y, cubeQueryStep * .5f);
        colliderPosition = transform.Position + math.mul(transform.Rotation, originalBoxGeometry.Center);
        endColliderPosition = transform.Position + math.mul(transform.Rotation, new float3(originalBoxGeometry.Center.x, originalBoxGeometry.Center.y, originalBoxGeometry.Size.z - originalBoxGeometry.Center.z));
        return originalBoxGeometry;
    }
    
    public static unsafe Entity GetAccurateHitEntity<THitResult>(this in PhysicsWorld physicsWorld, ref BufferLookup<PhysicsColliderKeyEntityPair> bufferLookup,in THitResult collectorClosestHit, out Collider* targetCollider)
        where THitResult : unmanaged, IQueryResult
    {
        Entity closestHitEntity = collectorClosestHit.Entity;
        RigidBody physicsWorldBody = physicsWorld.Bodies[collectorClosestHit.RigidBodyIndex];
        targetCollider = (Collider*)physicsWorldBody.Collider.GetUnsafePtr();
        ColliderKey closestHitColliderKey = collectorClosestHit.ColliderKey;
        if ( closestHitColliderKey.Value != ColliderKey.Empty.Value
             && physicsWorldBody.Collider.Value.Type == ColliderType.Compound )
        {
            var closestHitColliderKeyCopy = closestHitColliderKey;
            if ( targetCollider->GetLeaf(closestHitColliderKeyCopy, out var childCollider) )
            {
                targetCollider = childCollider.Collider;
            }
            var physicsColliderKeyEntityPairs = bufferLookup[collectorClosestHit.Entity].AsNativeArray();
            for ( var index = 0; index < physicsColliderKeyEntityPairs.Length; index++ )
            {
                var colliderKeyEntityPair = physicsColliderKeyEntityPairs[index];
                if ( colliderKeyEntityPair.Key == closestHitColliderKey )
                {
                    closestHitEntity = colliderKeyEntityPair.Entity;
                }
            }
        }
        return closestHitEntity;
    }
}