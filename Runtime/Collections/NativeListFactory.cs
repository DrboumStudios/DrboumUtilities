using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Drboum.Utilities.Collections
{
    public unsafe struct NativeListFactory<TData> : IFactory<NativeList<TData>>, IEqualityComparer<NativeList<TData>>, INativeDisposableWrapper<NativeList<TData>>
        where TData : unmanaged
    {
        public Allocator Allocator;
        public int InitialInnerCapacity;

        public NativeList<TData> CreateNew()
        {
            return new NativeList<TData>(InitialInnerCapacity, Allocator);
        }

        public FixedString128Bytes TypeName => nameof(NativeList<TData>);

        public bool Equals(NativeList<TData> x, NativeList<TData> y)
        {
            return (IntPtr)x.GetUnsafeList()->Ptr == (IntPtr)y.GetUnsafeList()->Ptr;
        }

        public int GetHashCode(NativeList<TData> obj)
        {
            return ((IntPtr)obj.GetUnsafeList()->Ptr).GetHashCode();
        }

        public void Dispose(ref NativeList<TData> disposable)
        {
            disposable.DisposeIfCreated();
        }

        public JobHandle Dispose(ref NativeList<TData> disposable, in JobHandle inputDependencies)
        {
            return disposable.DisposeIfCreated(inputDependencies);
        }
    }
}