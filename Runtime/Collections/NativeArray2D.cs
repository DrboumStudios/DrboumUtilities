using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
namespace Drboum.Utilities.Runtime.Collections {
    public struct NativeArray2D<T> : INativeDisposable, IEquatable<NativeArray<T>> where T : struct {
        internal NativeArray<T> _flatNativeArray;

        public int DimensionOneLength {
            get;
        }
        public int DimensionTwoLength {
            get;
        }

        public NativeArray2D(int dimensionOneLength, int dimensionTwoLength, Allocator allocator,
            NativeArrayOptions   nativeArrayOptions = NativeArrayOptions.ClearMemory)
        {
            DimensionOneLength = dimensionOneLength;
            DimensionTwoLength = dimensionTwoLength;
            _flatNativeArray   = new NativeArray<T>(dimensionOneLength * dimensionTwoLength, allocator, nativeArrayOptions);
        }
        public T this[int i, int ii] {
            get => _flatNativeArray[GetFlatIndex(i, ii)];
            set => _flatNativeArray[GetFlatIndex(i, ii)] = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetFlatIndex(int i, int ii)
        {
            return i * DimensionTwoLength + ii;
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