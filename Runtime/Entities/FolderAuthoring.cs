using Unity.Entities;
using UnityEngine;

namespace Drboum.Utilities.Entities
{
    public class FolderAuthoring : MonoBehaviour
    {
        public class Baker : Baker<FolderAuthoring>
        {
            public override void Bake(FolderAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent<BakingOnlyEntity>(entity);
                UnParentChildrenAuthoring.Baker.BakeRemoveParentImpl(this, authoring, entity,default);
            }

        }
    }
}