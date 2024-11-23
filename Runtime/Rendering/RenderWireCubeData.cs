using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Drboum.Utilities.Rendering
{
    public struct RenderWireCubeData : IComponentData, IEnableableComponent
    {
        public float3 CenterPosition;
        public float3 HalfSize;
        public Color DrawColor;
        public float LineThickness;

        public static RenderWireCubeData GetDefault(float3 centerPosition, float3 halfSize, float lineThickness = .5f) => new RenderWireCubeData {
            CenterPosition = centerPosition,
            HalfSize = halfSize,
            LineThickness = lineThickness,
            DrawColor = new Color(1, 0, 1)
        };
    }
}