using Unity.Mathematics;

namespace Unity.Physics.Authoring
{
    public static class PhysicsInternals
    {
        public static CapsuleGeometry GetBakedCapsuleRuntime(this PhysicsShapeAuthoring shapeAuthoring)
        {
            return shapeAuthoring.GetBakedCapsuleProperties().ToRuntime();
        }
        public static SphereGeometry GetBakedSphereRuntime(this PhysicsShapeAuthoring shapeAuthoring)
        {
            return shapeAuthoring.GetBakedSphereProperties(out _);
        }
        public static BoxGeometry GetBakedBoxRuntime(this PhysicsShapeAuthoring shapeAuthoring)
        {
            return shapeAuthoring.GetBakedBoxProperties();
        }
        public static CylinderGeometry GetBakedCylinderRuntime(this PhysicsShapeAuthoring shapeAuthoring)
        {
            return shapeAuthoring.GetBakedCylinderProperties();
        }
    }
}