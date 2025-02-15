#if UNITY_SPLINES_EXIST
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace Drboum.Utilities.Entities.Baking
{
    public struct SplineBlobAssetComponent : IComponentData
    {
        public BlobAssetReference<NativeSplineBlob> BlobReference;
    }

    public struct NativeSplineBlob
    {
        public BlobArray<BezierKnot> Knots;
        public bool Closed;
        public float4x4 TransformMatrix;

        /// <summary>
        /// NativeSpline must be disposed on caller site.
        /// </summary>
        public NativeSpline CreateNativeSpline(Allocator allocator)
        {

            using var nativeList = new NativeList<BezierKnot>(initialCapacity: Knots.Length, Allocator.Temp);

            for ( int i = 0; i < Knots.Length; i++ )
            {
                nativeList.Add(Knots[i]);
            }

            var readonlyKnots = new KnotsReadonlyCollection(nativeList);

            return new NativeSpline(readonlyKnots, Closed, TransformMatrix, allocator);
        }

        public static BlobAssetReference<NativeSplineBlob> CreateNativeSplineBlobAssetRef(NativeSpline nativeSpline, bool isClosed, float4x4 transformMatrix)
        {
            // Riping values
            var knots = nativeSpline.Knots;

            // Constructing blob
            using var nativeSplineBuilder = new BlobBuilder(Allocator.Temp);
            ref var nativeSplineRoot = ref nativeSplineBuilder.ConstructRoot<NativeSplineBlob>();

            var knotsBuilder = nativeSplineBuilder.Allocate(ref nativeSplineRoot.Knots, knots.Length);
            for ( int i = 0; i < knots.Length; i++ )
            {
                knotsBuilder[i] = knots[i];
            }

            nativeSplineRoot.Closed = isClosed;
            nativeSplineRoot.TransformMatrix = transformMatrix;

            return nativeSplineBuilder.CreateBlobAssetReference<NativeSplineBlob>(Allocator.Persistent);
        }
    }

    public readonly struct KnotsReadonlyCollection : IReadOnlyList<BezierKnot>
    {
        private readonly NativeList<BezierKnot> _knots;

        public KnotsReadonlyCollection(NativeList<BezierKnot> knots)
        {
            _knots = knots;
        }

        public IEnumerator<BezierKnot> GetEnumerator()
        {
            for ( int i = 0; i < _knots.Length; i++ )
            {
                yield return _knots[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public BezierKnot this[int index] => _knots[index];
        public int Count => _knots.Length;
    }

    public class SplineContainerBaker : Baker<SplineContainer>
    {
        public override void Bake(SplineContainer authoring)
        {
            var spline = authoring.Spline;
            float4x4 transformationMatrix = authoring.transform.localToWorldMatrix;
            using var nativeSpline = new NativeSpline(spline);

            var nativeSplineBlobAssetRef = NativeSplineBlob.CreateNativeSplineBlobAssetRef(
                nativeSpline,
                spline.Closed,
                transformationMatrix);

            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBlobAsset(ref nativeSplineBlobAssetRef, out _);

            AddComponent(entity, new SplineBlobAssetComponent {
                BlobReference = nativeSplineBlobAssetRef,
            });
        }
    }
}
#endif