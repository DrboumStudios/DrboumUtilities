using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static unsafe class CollectionInternalExtensions
{
    public static ref T AsRef<T>(this ref NativeReference<T> nativeReference)
        where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(nativeReference.m_Safety);
#endif
        return ref UnsafeUtility.AsRef<T>(nativeReference.m_Data);
    }
}