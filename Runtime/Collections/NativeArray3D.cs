using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
namespace DrboumLibrary {
    public struct NativeArray3D<T> : INativeDisposable, IEquatable<NativeArray<T>> where T : struct {
        internal NativeArray<T> _flatNativeArray;

        public int DimensionOneLength {
            get;
        }
        public int DimensionTwoLength {
            get;
        }
        public int DimensionThreeLength {
            get;
        }
        public NativeArray3D(int dimensionOneLength, int dimensionTwoLength, int dimensionThreeLength, Allocator allocator,
            NativeArrayOptions   nativeArrayOptions = NativeArrayOptions.ClearMemory)
        {
            DimensionOneLength   = dimensionOneLength;
            DimensionTwoLength   = dimensionTwoLength;
            DimensionThreeLength = dimensionThreeLength;
            _flatNativeArray = new NativeArray<T>(dimensionOneLength * dimensionTwoLength * dimensionThreeLength, allocator,
                nativeArrayOptions);
        }
        public T this[int i, int ii, int iii] {
            get => _flatNativeArray[GetFlatIndex(i, ii, iii)];
            set => _flatNativeArray[GetFlatIndex(i, ii, iii)] = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetFlatIndex(int i, int ii, int iii)
        {
            CollectionCustomHelper.CheckElementAccess(i,   DimensionOneLength);
            CollectionCustomHelper.CheckElementAccess(ii,  DimensionTwoLength);
            CollectionCustomHelper.CheckElementAccess(iii, DimensionThreeLength);
            return (i * DimensionTwoLength + ii) * DimensionThreeLength + iii;
        }

        public void Dispose()
        {
            _flatNativeArray.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return _flatNativeArray.Dispose(inputDeps);
        }

        public bool Equals(NativeArray<T> other)
        {
            return _flatNativeArray.Equals(other);
        }

        public override string ToString()
        {
            return _flatNativeArray.ToContentString();
        }
    }
}