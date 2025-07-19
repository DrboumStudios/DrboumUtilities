namespace Unity.Physics.Authoring
{
    public static class PhysicsInternals
    {
        public static CapsuleGeometry GetBakedCapsuleRuntime(this PhysicsShapeAuthoring shapeAuthoring)
        {
            return shapeAuthoring.GetBakedCapsuleProperties().ToRuntime();
        }
    }
}