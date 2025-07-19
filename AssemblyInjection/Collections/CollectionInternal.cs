using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static unsafe class CollectionInternal
{
    public static ref T AsRef<T>(this ref NativeReference<T> nativeReference)
        where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(nativeReference.m_Safety);
#endif
        return ref UnsafeUtility.AsRef<T>(nativeReference.m_Data);
    }

    public static TValue GetValueFromMapIndex<TKey, TValue>(this ref NativeHashMap<TKey, TValue> hashMap, int mapIndex)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        CheckRead(ref hashMap);
        return UnsafeUtility.ReadArrayElement<TValue>(hashMap.m_Data->Ptr, mapIndex);
    }

    public static int FindMapIndex<TKey, TValue>(this ref NativeHashMap<TKey, TValue> hashMap, in TKey key)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        CheckRead(ref hashMap);
        return hashMap.m_Data->Find(key);
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void CheckRead<TKey, TValue>(ref NativeHashMap<TKey, TValue> hashMap)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(hashMap.m_Safety);
#endif
    }

    public static void AddOrUpdate<TKey, TValue>(this ref NativeHashMap<TKey, TValue> hashMap, in TKey key, in TValue value)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        var unsafeMap = hashMap.m_Data;
        var idx = unsafeMap->Find(key);
        if ( idx == -1 )
        {
            idx = AddKeyNoExistCheckImpl(unsafeMap, in key);
        }

        UnsafeUtility.WriteArrayElement(unsafeMap->Ptr, idx, value);
    }

    internal static void AddNoExistCheckImpl<TKey, TValue>(this ref NativeHashMap<TKey, TValue> hashMap, in TKey key, in TValue value)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        var idx = AddKeyNoExistCheckImpl(hashMap.m_Data, in key);
        UnsafeUtility.WriteArrayElement(hashMap.m_Data->Ptr, idx, value);
    }

    /// <summary>
    /// copied from <see cref="HashMapHelper{TKey}.TryAdd(in TKey)"/>
    /// </summary>
    /// <returns> the hashmap index in the keys and the values arrays</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static int AddKeyNoExistCheckImpl<TKey>(HashMapHelper<TKey>* unsafeMap, in TKey key)
        where TKey : unmanaged, IEquatable<TKey>
    {
        if ( unsafeMap->AllocatedIndex >= unsafeMap->Capacity && unsafeMap->FirstFreeIdx < 0 )
        {
            int newCap = unsafeMap->CalcCapacityCeilPow2(unsafeMap->Capacity + (1 << unsafeMap->Log2MinGrowth));
            unsafeMap->Resize(newCap);
        }

        int idx = unsafeMap->FirstFreeIdx;

        if ( idx >= 0 )
        {
            unsafeMap->FirstFreeIdx = unsafeMap->Next[idx];
        }
        else
        {
            idx = unsafeMap->AllocatedIndex++;
        }

        CheckIndexOutOfBounds(unsafeMap, idx);

        UnsafeUtility.WriteArrayElement(unsafeMap->Keys, idx, key);
        var bucket = GetBucket(unsafeMap, in key);

        // Add the index to the hash-map
        int* next = unsafeMap->Next;
        next[idx] = unsafeMap->Buckets[bucket];
        unsafeMap->Buckets[bucket] = idx;
        unsafeMap->Count++;

        return idx;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckIndexOutOfBounds(HashMapHelper<TKey>* unsafeHashMap, int idx)
        {
            if ( (uint)idx >= (uint)unsafeHashMap->Capacity )
            {
                throw new InvalidOperationException($"Internal HashMap error. idx {idx}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetBucket(HashMapHelper<TKey>* unsafeHashMap, in TKey key)
        {
            return (int)((uint)key.GetHashCode() & (unsafeHashMap->BucketCapacity - 1));
        }
    }
}