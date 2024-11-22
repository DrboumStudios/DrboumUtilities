using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using AABBCustomType = Unity.Physics.Aabb;
namespace Drboum.Utilities.Entities
{
    [DebuggerDisplay("{MinPoint} - {MaxPoint}")]
    [Serializable]
    public struct AABBComponent : IComponentData
    {
        public AABBCustomType Value;

        /// <summary>
        /// <inheritdoc cref="AABB.Center"/>
        /// </summary>
        public float3 Center {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.Center;
        }

        /// <summary>
        /// <inheritdoc cref="AABB.Size"/>
        /// </summary>
        public float3 Size {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.Extents;
        }

        /// <summary>
        /// <inheritdoc cref="AABB.Min"/>
        /// </summary>
        public float3 MinPoint => Value.Min;

        /// <summary>
        /// <inheritdoc cref="AABB.Max"/>
        /// </summary>
        public float3 MaxPoint => Value.Max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCenterAndSize(in float3 center, in float3 size)
        {
            var halfExtents = size * 0.5f;
            Value.Min = center - halfExtents;
            Value.Max = center + halfExtents;
        }

        /// <summary>
        /// <inheritdoc cref="NUnit.Framework.Contains"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in float3 point)
        {
            return Value.Contains(point);
        }

        /// <summary>
        /// return if the aabb component contains completely the aabb passed in the parameters 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in AABBCustomType aabb)
        {
            return Value.Contains(aabb);
        }

        /// <summary>
        /// return if the aabb component contains completely the aabb passed in the parameters 
        /// </summary>
        /// <param name="aabb"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in AABBComponent aabb)
        {
            return Value.Contains(aabb);
        }

        /// <summary>
        /// <inheritdoc cref="Aabb.Overlaps"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in AABBComponent aabb)
        {
            return Overlaps(in aabb.Value);
        }
    
        /// <summary>
        /// <inheritdoc cref="Aabb.Overlaps"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in AABBCustomType aabb)
        {
            return Value.Overlaps(aabb);
        }

        /// <summary>
        /// <inheritdoc cref="Aabb.Overlaps"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in AABB aabb)
        {
            return Value.Overlaps(new() {
                Min = aabb.Min,
                Max =  aabb.Max,
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMinMax(float3 min, float3 max)
        {
            Value.Min = min;
            Value.Max = max;
        }

        /// <summary>
        ///   <para>Grows the Bounds to include the point.</para>
        /// </summary>
        /// <param name="point"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(float3 point) => this.SetMinMax(math.min(MinPoint, point), math.max(MaxPoint, point));

        /// <summary>
        ///   <para>Grow the bounds to encapsulate the bounds.</para>
        /// </summary>
        /// <param name="bounds"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(AABBComponent bounds)
        {
            float3 halfExtent = bounds.Size * .5f;
            Encapsulate(bounds.Center - halfExtent);
            Encapsulate(bounds.Center + halfExtent);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AABBCustomType(AABBComponent component)
        {
            return component.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AABBComponent(Aabb component)
        {
            return Create(component.Center, component.Extents * 2f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AABBComponent(AABB component)
        {
            return Create(component.Center, component.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AABBComponent Create(in float3 center, in float3 size)
        {
            var halfExtents = size * 0.5f;
            AABBComponent newAABBComponent = new AABBComponent {
                Value = new AABBCustomType { Max = center + halfExtents, Min = center - halfExtents }
            };
            return newAABBComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AABBComponent EncapsulateDefault()
        {
            var newAABBComponent = new AABBComponent {
                Value = new AABBCustomType { Max = new float3(float.MinValue), Min = new float3(float.MaxValue) }
            };
            return newAABBComponent;
        }
    }
}