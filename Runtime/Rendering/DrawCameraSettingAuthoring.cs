using Unity.Entities;
using UnityEngine;

namespace Drboum.Utilities.Rendering
{
    public class DrawCameraSettingAuthoring : MonoBehaviour
    {
        public Camera Camera;

        public class DrawCameraSettingBaker : Baker<DrawCameraSettingAuthoring>
        {
            public override void Bake(DrawCameraSettingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new RenderPropsCameraSetting { Camera = authoring.Camera });
            }
        }
    }
    
    public class RenderPropsCameraSetting : IComponentData
    {
        public Camera Camera;
    }
}