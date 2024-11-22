using Unity.Entities;

namespace Drboum.Utilities.Entities
{
    public struct EntityReference : IComponentData
    {
        public Entity Value;
        
        public static implicit operator Entity(EntityReference value) => value.Value;

        public static implicit operator EntityReference(Entity value)
        {
            return new EntityReference {
                Value = value
            };
        }
    }
}