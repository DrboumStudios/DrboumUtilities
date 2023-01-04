using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.Collections;
using Drboum.Utilities.Runtime.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public static class CollectionCustomHelper {

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    public static void CheckCapacity<TDest, TSource>(TDest destinationBuffer, TSource textStream)
        where TDest : unmanaged, INativeList<byte>
        where TSource : unmanaged, INativeList<byte>
    {
        if ( textStream.Length > destinationBuffer.Capacity )
        {
            Debug.LogError($"The destination buffer of capacity: [{destinationBuffer.Capacity}] cannot contains the source buffer of Length: {textStream.Length}");
        }
    }
    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    public static void CheckElementAccess(int index, int length)
    {
        if ( index < 0 || index >= length )
        {
            throw new IndexOutOfRangeException($"Index {index} is out of range of '{length}' Length.");
        }
    }
    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    public static void CheckResize(int newLength, int maxCapacity)
    {
        if ( newLength < 0 || newLength > maxCapacity )
        {
            throw new IndexOutOfRangeException($"NewLength {newLength} is out of range of '{maxCapacity}' Capacity.");
        }
    }
    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    public static void CheckIndexIsPositive(int number)
    {
        if ( number < 0 )
        {
            throw new IndexOutOfRangeException($"the number with value {number} is negative and is therefore OutOfRange of the collection");
        }
    }
    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private static void CheckStartIndexIsValidIfResultArrayIsNotEmpty(int length, int newLength, int startIndex)
    {
        if ( startIndex != length )
        {
            CheckElementAccess(startIndex, length);
            CheckResize(startIndex + newLength, length);
        }
    }
    public static NativeArray<T> Flatten2DArray<T>(this T[,] arrayToFlatten, int sizeX, int sizeY,
        Allocator allocator = Allocator.Temp) where T : unmanaged
    {
        var colliderHeights = new NativeArray<T>(sizeX * sizeY, allocator, NativeArrayOptions.UninitializedMemory);
        Flatten2DArray(arrayToFlatten, sizeX, sizeY, ref colliderHeights);
        return colliderHeights;
    }

    public static NativeArray<Out> Flatten2DArray<In, Out>(this In[,] arrayToFlatten, int sizeX, int sizeY,
        Allocator allocator = Allocator.Temp)
        where Out : unmanaged, IConvertStruct<In, Out>
        where In : unmanaged
    {
        var colliderHeights = new NativeArray<Out>(sizeX * sizeY, allocator, NativeArrayOptions.UninitializedMemory);
        Flatten2DArray(arrayToFlatten, sizeX, sizeY, ref colliderHeights);
        return colliderHeights;
    }

    public static void Flatten2DArray<In, Out>(this In[,] arrayToFlatten, int sizeX, int sizeY,
        ref NativeArray<Out> colliderHeights)
        where Out : unmanaged, IConvertStruct<In, Out>
        where In : unmanaged
    {
        for ( var j = 0; j < sizeY; j++ )
        {
            for ( var i = 0; i < sizeX; i++ )
            {
                In h = arrayToFlatten[j, i];
                colliderHeights[j + i * sizeX] = new Out().Convert(h);
            }
        }
    }

    public static void Flatten2DArray<T>(this T[,] arrayToFlatten, int sizeX, int sizeY,
        ref NativeArray<T> colliderHeights) where T : struct
    {
        for ( var j = 0; j < sizeY; j++ )
        {
            for ( var i = 0; i < sizeX; i++ )
            {
                T h = arrayToFlatten[j, i];
                colliderHeights[j + i * sizeX] = h;
            }
        }
    }
#if UNITY_TERRAIN_EXIST
    public static NativeArray<float> ConvertHeightMap(this TerrainData terrainData, int sizeX, int sizeY,
        Allocator allocator = Allocator.TempJob)
    {
        var colliderHeights = new NativeArray<float>(sizeX * sizeY, allocator, NativeArrayOptions.UninitializedMemory);
        ConvertHeightMap(terrainData, sizeX, sizeY, ref colliderHeights);
        return colliderHeights;
    }

    public static void ConvertHeightMap(this TerrainData terrainData, int sizeX, int sizeY,
        ref NativeArray<float> colliderHeights)
    {
        for ( var y = 0; y < sizeY; y++ )
        {
            for ( var x = 0; x < sizeX; x++ )
            {
                float h = terrainData.GetHeight(y, x) / terrainData.heightmapScale.y;
                colliderHeights[y + x * sizeX] = h;
            }
        }
    }
#endif


    public static bool Contains<TNativeList, T>(this ref TNativeList collection, T element)
        where TNativeList : unmanaged, INativeList<T>
        where T : unmanaged, IEquatable<T>
    {
        return collection.FindFirstIndexOf(element) > -1;
    }
    public static bool Contains<TNativeList, T, U>(this ref TNativeList collection, U element)
        where TNativeList : unmanaged, INativeList<T>
        where T : unmanaged
        where U : unmanaged, IEquatable<T>
    {
        return collection.FindFirstIndexOf<TNativeList, T, U>(element) > -1;
    }
    public static int FindFirstIndexOf<TCollection, T>(this ref TCollection list, T value)
        where TCollection : unmanaged, INativeList<T>
        where T : unmanaged, IEquatable<T>
    {
        for ( var i = 0; i < list.Length; i++ )
        {
            if ( list[i].Equals(value) )
            {
                return i;
            }
        }
        return -1;
    }

    public static int FindFirstIndexOf<TCollection, T, U>(this ref TCollection list, U value)
        where TCollection : unmanaged, INativeList<T>
        where T : unmanaged
        where U : unmanaged, IEquatable<T>
    {
        for ( var i = 0; i < list.Length; i++ )
        {
            if ( value.Equals(list[i]) )
            {
                return i;
            }
        }
        return -1;
    }
    public static unsafe NativeArray<T> ToNativeArray<T>(this in NativeSlice<T> source, Allocator allocator)
        where T : unmanaged
    {
        var dstArray = new NativeArray<T>(source.Length, allocator, NativeArrayOptions.UninitializedMemory);
        var srcPtr = source.GetUnsafeReadOnlyPtr();
        source.CopyTo(dstArray);
        return dstArray;
    }

    public static unsafe NativeArray<T> ToNativeArray<T>(this in NativeSlice<T> source)
        where T : unmanaged
    {
        var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(source.GetUnsafeReadOnlyPtr(), source.Length, Allocator.None);
#if UNITY_EDITOR
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeSliceUnsafeUtility.GetAtomicSafetyHandle(source));
#endif
        return array;
    }
    /// <summary>
    /// create a new NativeList that is a copy of <paramref name="source"/> array
    /// </summary>
    /// <param name="source"></param>
    /// <param name="allocator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static NativeList<T> ToNativeList<T>(this in NativeArray<T> source, Allocator allocator)
        where T : unmanaged
    {
        var nativeList = new NativeList<T>();
        nativeList.CopyFrom(source);
        return nativeList;
    }

    public static void DisposeIfCreated<TKey, TValue>(this ref NativeHashMap<TKey, TValue> buffer)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose();
        }
    }
    public static void DisposeIfCreated<TKey, TValue>(this ref NativeHashMap<TKey, TValue> buffer, JobHandle dependencies)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose(dependencies);
        }
    }
    public static void DisposeIfCreated<T>(this ref NativeArray<T> buffer) where T : unmanaged
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose();
        }
    }
    public static void DisposeIfCreated<T>(this ref NativeArray<T> buffer, JobHandle dependencies) where T : unmanaged
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose(dependencies);
        }
    }
    public static void DisposeIfCreated<T>(this ref UnsafeList<T> buffer) where T : unmanaged
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose();
        }
    }
    public static JobHandle DisposeIfCreated<T>(this ref UnsafeList<T> buffer, JobHandle dependencies) where T : unmanaged
    {
        return buffer.IsCreated ? buffer.Dispose(dependencies) : default;
    }
    public static void DisposeIfCreated<T>(this ref NativeList<T> buffer) where T : unmanaged
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose();
        }
    }
    public static JobHandle DisposeIfCreated<T>(this ref NativeList<T> buffer, JobHandle dependencies) where T : unmanaged
    {
        return buffer.IsCreated ? buffer.Dispose(dependencies) : default;
    }
    public static void DisposeIfCreated<T>(this ref NativeHashSet<T> buffer)
        where T : unmanaged, IEquatable<T>
    {
        if ( buffer.IsCreated )
        {
            buffer.Dispose();
        }
    }
    public static JobHandle DisposeIfCreated<T>(this ref NativeHashSet<T> buffer, JobHandle dependencies)
        where T : unmanaged, IEquatable<T>
    {
        if ( buffer.IsCreated )
        {
            return buffer.Dispose(dependencies);
        }
        return default;
    }

    public static void DisposeIfCreated(this ref NativeText nativeText)
    {
        if ( nativeText.IsCreated )
            nativeText.Dispose();
    }
    public static void SwapElements<T>(this ref NativeArray<T> arrayElement, int indexLhs, int indexRhs)
        where T : unmanaged
    {
        (arrayElement[indexLhs], arrayElement[indexRhs]) = (arrayElement[indexRhs], arrayElement[indexLhs]);
    }
    public static void SwapElements<T>(this T[] arrayElement, int indexLhs, int indexRhs)
    {
        (arrayElement[indexLhs], arrayElement[indexRhs]) = (arrayElement[indexRhs], arrayElement[indexLhs]);
    }
    public static bool ByteArrayGuidIsEqual(byte[] lhs, byte[] rhs)
    {

        return lhs[0] == rhs[0] &&
               lhs[1] == rhs[1] &&
               lhs[2] == rhs[2] &&
               lhs[3] == rhs[3] &&
               lhs[4] == rhs[4] &&
               lhs[5] == rhs[5] &&
               lhs[6] == rhs[6] &&
               lhs[7] == rhs[7] &&
               lhs[8] == rhs[8] &&
               lhs[9] == rhs[9] &&
               lhs[10] == rhs[10] &&
               lhs[11] == rhs[11] &&
               lhs[12] == rhs[12] &&
               lhs[13] == rhs[13] &&
               lhs[14] == rhs[14] &&
               lhs[15] == rhs[15];
    }
    public static bool IsValidIndex(int id, int length)
    {
        return id >= 0 && id < length;
    }
    public static string ToContentString<T>(this in NativeArray<T> flatNativeArray, char valueSeparator = ',')
        where T : unmanaged
    {
        flatNativeArray.ToContentFixedString(out FixedString4096Bytes toString, valueSeparator);
        return toString.ToString();
    }
    public static void ToContentFixedString<T, TFixedString>(this in NativeArray<T> source,
        out TFixedString fixedStringContent, char valueSeparator = ',')
        where T : unmanaged
        where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
    {
        fixedStringContent = new TFixedString();
        fixedStringContent.Append("Array content: ");
        for ( var i = 0; i < source.Length; i++ )
        {
            fixedStringContent.Append(source[i].ToString());
            fixedStringContent.Append(valueSeparator);
        }
    }

    public static T[] MergeArrays<T>(this T[] array1, T[] array2)
    {
        var mergedArr = new T[array1.Length + array2.Length];
        for ( var i = 0; i < array1.Length; i++ )
        {
            mergedArr[i] = array1[i];
        }

        for ( var i = 0; i < array2.Length; i++ )
        {
            mergedArr[i + array1.Length] = array2[i];
        }
        return mergedArr;
    }
    public static void MergeArrays<T>(this T[] array1, T[] array2, T[] resultArray)
    {
        if ( resultArray.Length < array1.Length + array2.Length )
        {
            Debug.LogError("the length of the result array passed as a parameter is too small to be merged");
            return;
        }

        for ( var i = 0; i < array1.Length; i++ )
        {
            resultArray[i] = array1[i];
        }

        for ( var i = 0; i < array2.Length; i++ )
        {
            resultArray[i + array1.Length] = array2[i];
        }
    }
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if ( dictionary.ContainsKey(key) )
        {
            return false;
        }
        dictionary.Add(key, value);
        return true;
    }
    public static NativeHashMap<GuidWrapper, TValue> ToNativeContainer<TValue>(
        this Dictionary<FixedBytes16, TValue> dictionnary, Allocator allocator) where TValue : unmanaged
    {
        var hashMap = new NativeHashMap<GuidWrapper, TValue>(dictionnary.Count, allocator);
        foreach ( KeyValuePair<FixedBytes16, TValue> item in dictionnary )
        {
            hashMap.Add(item.Key, item.Value);
        }
        return hashMap;
    }
    public static T[] AddInNewArray<T>(this T[] sourceArray, params T[] elementsToAdd)
    {
        if ( sourceArray.Length == 0 || sourceArray.Length + elementsToAdd.Length == 0 )
        {
            return elementsToAdd;
        }

        Array.Resize(ref sourceArray, sourceArray.Length + elementsToAdd.Length);
        Array.Copy(elementsToAdd, 0, sourceArray, sourceArray.Length, elementsToAdd.Length);
        return sourceArray;
    }

    public static T[] AddInNewArray<T>(this T[] sourceArray, T element1, T element2)
    {
        int i = sourceArray.Length;
        Array.Resize(ref sourceArray, sourceArray.Length + 2);
        sourceArray[i++] = element1;
        sourceArray[i++] = element2;
        return sourceArray;
    }
    public static T[] AddInNewArray<T>(this T[] sourceArray, T element)
    {
        Append(ref sourceArray, element);
        return sourceArray;
    }
    public static void Append<T>(ref T[] sourceArray, T element)
    {
        Array.Resize(ref sourceArray, sourceArray.Length + 1);
        sourceArray[sourceArray.Length - 1] = element;
    }
    /// <summary>
    ///     lookup the array and return the index if found otherwise -1
    /// </summary>
    /// <param name="arrayToLookup"></param>
    /// <param name="elementToLookFor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int FindFirstIndex<T>(this T[] arrayToLookup, T elementToLookFor)
        where T : IEquatable<T>
    {

        for ( var i = 0; i < arrayToLookup.Length; i++ )
        {
            T current = arrayToLookup[i];
            if ( current.Equals(elementToLookFor) )
            {
                return i;
            }
        }
        return -1;
    }
    public static T[] AddInNewArray<T>(this ReadOnlyArray<T> readOnlyArray, params T[] array)
    {
        if ( readOnlyArray.Count == 0 || readOnlyArray.Count + array.Length == 0 )
        {
            return array;
        }

        var allComponents = new T[readOnlyArray.Count + array.Length];

        Array.Copy(readOnlyArray.m_Array, 0, allComponents, 0, readOnlyArray.Count);
        Array.Copy(array, 0, allComponents, readOnlyArray.Count, array.Length);
        return allComponents;
    }
    public static T[] AddInNewArray<T>(this ReadOnlyArray<T> readOnlyArray, T element)
    {
        if ( element == null )
        {
            return readOnlyArray.ToArray();
        }
        if ( readOnlyArray.Count == 0 )
        {
            return new[] {
                element
            };
        }
        return readOnlyArray.m_Array.AddInNewArray(element);
    }
    /// <summary>
    /// create a new temporary <see cref="NativeArray{T}"/> of a smaller length without allocating new memory, therefore sharing the memory with the one that allocated it
    /// </summary>
    /// <remarks>
    /// this function does NOT perform any memory allocation, the returned array share the same memory with the <paramref name="source"/>.
    /// disposing the array that allocated the memory will free memory for the returned array too, in that case, it is not necessary to dispose the returned array,
    /// in contrast, disposing the returned array will not affect the <paramref name="source"/> array though it will remove informations about the original data"/>
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="startIndex"></param>
    /// <param name="newLength"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static unsafe NativeArray<T> Shrink<T>(this in NativeArray<T> source, int newLength, int startIndex = 0)
        where T : struct
    {
        bool newArrayIsEmpty = startIndex >= source.Length;
        var index = GetSafeIndex(startIndex, newArrayIsEmpty);
        CheckShrinkSafeGuard(source.Length, newLength, index);

        long unsafeReadOnlyPtr = PointerAsLong(source.GetUnsafeReadOnlyPtr());
        var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)GetArrayElementPtr<T>(unsafeReadOnlyPtr, index), newLength, Allocator.None);
#if UNITY_EDITOR
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(source));
#endif
        return array;
    }
    /// <summary>
    /// create starting at <paramref name="startIndex"/> with a length of <paramref name="newLength"/>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="newLength"></param>
    /// <param name="allocator"></param>
    /// <param name="startIndex"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static NativeArray<T> Shrink<T>(this in NativeArray<T> source, int newLength, Allocator allocator, int startIndex = 0)
        where T : struct
    {
        bool newArrayIsEmpty = startIndex >= source.Length;
        var index = GetSafeIndex(startIndex, newArrayIsEmpty);
        CheckShrinkSafeGuard(source.Length, newLength, index);
        var result = new NativeArray<T>(newLength, allocator, NativeArrayOptions.UninitializedMemory);
        NativeArray<T>.Copy(source, startIndex, result, 0, result.Length);
        return result;
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private static void CheckShrinkSafeGuard(int sourceLength, int newLength, int index)
    {
        CheckElementAccess(index, sourceLength);
        CheckResize(index + newLength, sourceLength);
    }
    public static unsafe ref TNativeList Shrink<T, TNativeList>(this ref TNativeList source, int newLength, int startIndex)
        where T : unmanaged
        where TNativeList : unmanaged, INativeList<T>
    {
        bool newArrayIsEmpty = startIndex >= source.Length;

        var index = GetSafeIndex(startIndex, newArrayIsEmpty);
        CheckShrinkSafeGuard(source.Length, newLength, index);
        var dstPtr = source.GetUnsafePtr<T, TNativeList>();
        var srcPtr = GetArrayElementPtr<T>(dstPtr, startIndex);
        UnsafeUtility.MemMove(dstPtr, srcPtr, newLength);
        return ref Shrink<T, TNativeList>(ref source, newLength);
    }
    private static int GetSafeIndex(int startIndex, bool newArrayIsEmpty)
    {
        return math.select(startIndex, 0, newArrayIsEmpty);
    }
    public static ref TNativeList Shrink<T, TNativeList>(this ref TNativeList source, int newLength)
        where T : unmanaged
        where TNativeList : unmanaged, INativeList<T>
    {
        CheckResize(newLength, source.Capacity);
        source.Length = newLength;
        return ref source;
    }
    public static unsafe void* GetUnsafePtr<T, TNativeList>(this ref TNativeList source)
        where T : unmanaged
        where TNativeList : unmanaged, INativeList<T>
    {
        return UnsafeUtility.AddressOf(ref source.ElementAt(0));
    }
    public static unsafe void* GetArrayElementPtr<T>(void* sourcePtr, long index)
        where T : unmanaged
    {
        return (void*)GetArrayElementPtr<T>(PointerAsLong(sourcePtr), index);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long PointerAsLong(void* ptr)
    {
        return ((IntPtr)ptr).ToInt64();
    }
    public static long GetArrayElementPtr<T>(long sourcePtr, long index)
        where T : unmanaged
    {
        return (sourcePtr + (UnsafeUtility.SizeOf<T>() * index));
    }
    [BurstDiscard]
    public static void LogErrorIfJobIsNotBurstCompile(ref bool runOnce)
    {
        if ( runOnce )
        {
            return;
        }
        runOnce = true;
        Debug.LogError($"a job was not bursted when it was expected to");
    }
}