using Unity.Entities;
using UnityEngine;

namespace Drboum.Utilities.Entities
{
    public class UnParentEntityAuthoring : MonoBehaviour
    {
        public class Baker : Baker<UnParentEntityAuthoring>
        {
            public override void Bake(UnParentEntityAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent<RemoveParentBakingTag>(entity);
            }
        }
    }
}