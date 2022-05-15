using System;
using System.Runtime.CompilerServices;
using Drboum.Utilities.Runtime.NativeLinq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static partial class NativeLinqExtensions {
    public static EquatablePredicate<T> CreateEquatablePredicate<T>(T lookupValue)
        where T : struct, IEquatable<T>
    {
        return new EquatablePredicate<T>(lookupValue);
    }
    public static EquatablePredicate<T, TEquatable> CreateEquatablePredicate<T, TEquatable>(TEquatable lookupValue)
        where T : struct
        where TEquatable : IEquatable<T>
    {
        return new EquatablePredicate<T, TEquatable>(lookupValue);
    }
    public static NativeArray<T> CreateCopy<T>(this ref NativeArray<T> buffer, Allocator allocator = Allocator.Temp)
        where T : struct
    {
        return new NativeArray<T>(buffer, allocator);
    }
    /// <summary>
    /// return the results matched by the <see cref="TNativePredicate"/> instance in a ,smaller, temporary array, effectively reusing the same memory
    /// </summary>
    /// <param name="source">the source array that will be modified</param>
    /// <param name="predicate">predicate to filter in results</param>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// <see cref="CollectionCustomHelper.Shrink{T}"/>
    /// </remarks>
    /// <example>
    /// NativePredicate implementation of IEquatable => <see cref="EquatablePredicate{T,TEquatable}"/>
    /// <seealso cref="CreateEquatablePredicate{T,TEquatable}"/>
    /// </example>
    /// <returns>a modified version of the <paramref name="source"/> which the number of matching results equals the length on the returned array,
    /// </returns>
    public static NativeArray<T> WhereNative<TNativePredicate, T>(this NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        var resultStartIndex = 0;
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                source[resultStartIndex++] = element;
            }
        }
        return source.Shrink(resultStartIndex);
    }

    /// <summary>
    /// Allocate a new array containing the <typeparamref name="TNativePredicate"/> matched results
    /// </summary>
    /// <param name="source">the source array to query against</param>
    /// <param name="predicate">predicate to filter in results</param>
    /// <param name="allocator"></param>
    /// <param name="expectedMatches">expected size of the matching array, will be used to set the initial capacity of the returned collection</param>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <example> NativePredicate implementation of IEquatable => <see cref="EquatablePredicate{T,TEquatable}"/></example>
    /// <returns>a modified version of the same array source with the result from the query, the returned array share the same memory as the <paramref name="source"/> disposing the source will invalid the returned array too, therefore its not necessary to dispose the returned array, disposing the returned array will not affect the <paramref name="source"/> array</returns>
    public static NativeArray<T> WhereNative<TNativePredicate, T>(this in NativeArray<T> source, in TNativePredicate predicate, Allocator allocator,
        int expectedMatches = 2)
        where T : unmanaged
        where TNativePredicate : struct, INativePredicate<T>
    {
        var nativeList = new NativeList<T>(expectedMatches, allocator);
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                nativeList.Add(element);
            }
        }
        return nativeList.AsArray();
    }

    /// <summary>
    /// linq equivalent of Select
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <param name="converter"></param>
    /// <param name="allocator"></param>
    /// <param name="dummyConvertValueTypeForGenerics">avoid typing all generics</param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TConvertResult"></typeparam>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <typeparam name="TConverter"></typeparam>
    /// <returns></returns>
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TNativePredicate, TConverter>(
        this in NativeArray<T> source,
        in TNativePredicate predicate,
        in TConverter converter,
        Allocator allocator,
        in TConvertResult dummyConvertValueTypeForGenerics = default)
        where T : struct
        where TConvertResult : unmanaged
        where TNativePredicate : struct, INativePredicate<T>
        where TConverter : struct, IConverter<T, TConvertResult>
    {
        var results = new NativeList<TConvertResult>(source.Length, allocator);
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                results.Add(converter.ConvertValue(in element));
            }
        }
        return results;
    }
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TConverter>(
        this in NativeArray<T> source,
        in TConverter converter,
        Allocator allocator,
        in TConvertResult dummyConvertValueTypeForGenerics)
        where T : struct
        where TConvertResult : unmanaged
        where TConverter : struct, IConverter<T, TConvertResult>
    {
        return source.SelectNative(default(AlwaysTruePredicate<T>), in converter, allocator, dummyConvertValueTypeForGenerics);
    }

    /// <summary>
    /// linq equivalent of Select
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <param name="converter"></param>
    /// <param name="allocator"></param>
    /// <param name="resultListInitialCapacity"></param>
    /// <param name="dummyConvertValueTypeForGenerics">avoid typing all generics</param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TConvertResult"></typeparam>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <typeparam name="TConverter"></typeparam>
    /// <typeparam name="TNativeList"></typeparam>
    /// <returns></returns>
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TNativePredicate, TConverter, TNativeList>(
        this ref TNativeList source,
        in TNativePredicate predicate,
        in TConverter converter,
        Allocator allocator,
        TConvertResult dummyConvertValueTypeForGenerics = default,
        int resultListInitialCapacity = -1)
        where T : struct
        where TConvertResult : unmanaged
        where TNativePredicate : struct, INativePredicate<T>
        where TNativeList : struct, INativeList<T>
        where TConverter : struct, IConverter<T, TConvertResult>
    {
        if ( resultListInitialCapacity < 0 )
        {
            resultListInitialCapacity = source.Length;
        }
        var results = new NativeList<TConvertResult>(resultListInitialCapacity, allocator);
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                results.Add(converter.ConvertValue(in element));
            }
        }
        return results;
    }
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TConverter, TNativeList>(
        this ref TNativeList source,
        in TConverter converter,
        Allocator allocator,
        in TConvertResult dummyConvertValueTypeForGenerics,
        int resultListInitialCapacity = -1)
        where T : struct
        where TConvertResult : unmanaged
        where TNativeList : struct, INativeList<T>
        where TConverter : struct, IConverter<T, TConvertResult>
    {
        return
            source.SelectNative
                <T, TConvertResult, AlwaysTruePredicate<T>, TConverter, TNativeList>
                (default, in converter, allocator, dummyConvertValueTypeForGenerics, source.Length);
    }
    /// <summary>
    /// linq equivalent of Any
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <returns></returns>
    public static bool AnyNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        return FindFirstIndex(in source, in predicate) != -1;
    }
    public static int FindFirstIndex<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct where TNativePredicate : struct, INativePredicate<T>
    {
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                return index;
            }
        }
        return -1;
    }
    public static int FindLastIndex<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct where TNativePredicate : struct, INativePredicate<T>
    {
        for ( var index = source.Length - 1; index >= 0; index-- )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                return index;
            }
        }
        return -1;
    }
    /// <summary>
    /// linq equivalent of All
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <returns></returns>
    public static bool AllNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( !predicate.EvaluatePredicate(in element) )
            {
                return false;
            }
        }
        return true;
    }
    public static T FirstOrDefaultNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        int foundIndex = source.FindFirstIndex(in predicate);
        return foundIndex != -1 ? source[foundIndex] : default;
    }
    public static T LastOrDefaultNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        int foundIndex = source.FindLastIndex(in predicate);
        return foundIndex != -1 ? source[foundIndex] : default;
    }
    public static NativeArray<T> TakeNative<T>(this ref NativeArray<T> source, int count) where T : struct => source.Shrink(count);
    public static NativeArray<T> TakeWhileNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        int count = 0;
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( !predicate.EvaluatePredicate(in element) )
            {
                break;
            }
            count++;
        }
        return source.Shrink(count);
    }
    public static NativeArray<T> SkipNative<T>(this NativeArray<T> source, int count) where T : struct => source.Shrink(source.Length - count, count);
    public static unsafe NativeArray<T> SkipWhileNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        int count = GetSkipCount<T, TNativePredicate>(source.GetUnsafeReadOnlyPtr(), source.Length, predicate);
        return source.SkipNative(count);
    }
    private static unsafe int GetSkipCount<T, TNativePredicate>(void* source, int sourceLength, in TNativePredicate predicate)
        where T : struct
        where TNativePredicate : struct, INativePredicate<T>
    {
        int count = 0;
        for ( var index = 0; index < sourceLength; index++ )
        {
            var element = UnsafeUtility.ReadArrayElement<T>(source, index);
            if ( !predicate.EvaluatePredicate(in element) )
            {
                break;
            }
            count++;
        }
        return count;
    }
    public static unsafe int SumNative(this in NativeArray<int> source) => SumNativePtr((int*)source.GetUnsafeReadOnlyPtr(), source.Length,
        default(AlwaysTruePredicate<int>));
    public static unsafe int SumNative<TNativePredicate>(this in NativeArray<int> source, in TNativePredicate predicate)
        where TNativePredicate : struct, INativePredicate<int>
    {
        return SumNativePtr((int*)source.GetUnsafeReadOnlyPtr(), source.Length, predicate);
    }
    public static unsafe int SumNative<TNativeList>(this ref TNativeList source)
        where TNativeList : struct, INativeList<int>
    {
        return SumNativePtr((int*)source.GetUnsafePtr<int, TNativeList>(), source.Length, default(AlwaysTruePredicate<int>));
    }
    public static unsafe int SumNative<TNativeList, TNativePredicate>(this ref TNativeList source, in TNativePredicate predicate)
        where TNativeList : struct, INativeList<int>
        where TNativePredicate : struct, INativePredicate<int>
    {
        return SumNativePtr((int*)source.GetUnsafePtr<int, TNativeList>(), source.Length, predicate);
    }
    public static unsafe float SumNative(this in NativeArray<float> source) => SumNativePtr((float*)source.GetUnsafeReadOnlyPtr(), source.Length,
        default(AlwaysTruePredicate<float>));

    internal static unsafe int SumNativePtr<TNativePredicate>(int* source, int length, in TNativePredicate predicate)
        where TNativePredicate : struct, INativePredicate<int>
    {
        var count = 0;
        for ( int i = 0; i < length; i++ )
        {
            var element = source[i];
            if ( predicate.EvaluatePredicate(in element) )
            {
                count += element;
            }
        }
        return count;
    }
    internal static unsafe float SumNativePtr<TNativePredicate>(float* source, int length, in TNativePredicate predicate)
        where TNativePredicate : struct, INativePredicate<float>
    {
        var count = 0f;
        for ( int i = 0; i < length; i++ )
        {
            var element = source[i];
            if ( predicate.EvaluatePredicate(in element) )
            {
                count += element;
            }
        }
        return count;
    }
    public static unsafe float SumNative<TNativePredicate>(this in NativeArray<float> source, in TNativePredicate predicate)
        where TNativePredicate : struct, INativePredicate<float>
    {
        return SumNativePtr((float*)source.GetUnsafeReadOnlyPtr(), source.Length, in predicate);
    }
    public static NativeArray<T> Aggregate<T>(this NativeArray<T> source1, NativeArray<T> source2, Allocator allocator)
        where T : struct
    {
        var aggregatedArray = new NativeArray<T>(source1.Length + source2.Length, allocator);
        var offset = 0;

        aggregatedArray.AggregateInternal(ref source1, ref offset);
        aggregatedArray.AggregateInternal(ref source2, ref offset);
        return aggregatedArray;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> Aggregate<T>(this NativeArray<T> source1, NativeArray<T> source2, NativeArray<T> source3, Allocator allocator)
        where T : struct
    {
        var aggregatedArray = new NativeArray<T>(source1.Length + source2.Length + source3.Length, allocator);
        var offset = 0;

        aggregatedArray.AggregateInternal(ref source1, ref offset);
        aggregatedArray.AggregateInternal(ref source2, ref offset);
        aggregatedArray.AggregateInternal(ref source3, ref offset);

        return aggregatedArray;
    }
    private static void AggregateInternal<T>(this ref NativeArray<T> aggregatedArray, ref NativeArray<T> nativeArray, ref int offset) where T : struct
    {
        NativeArray<T>.Copy(nativeArray, 0, aggregatedArray, offset, nativeArray.Length);
        offset += nativeArray.Length;
    }
    internal static unsafe NativeArray<T> Aggregate<T>(void** nativeArrays, int length, int aggregateArrayLength, Allocator allocator)
        where T : struct
    {
        var aggregatedArray = new NativeArray<T>(aggregateArrayLength, allocator);
        var offset = 0;

        int sizeOf = UnsafeUtility.SizeOf<T>();
        for ( int i = 0; i < length * sizeOf; i += sizeOf )
        {
            UnsafeUtility.CopyPtrToStructure(nativeArrays[i], out NativeArray<T> nativeArray);
            NativeArray<T>.Copy(nativeArray, 0, aggregatedArray, offset, nativeArray.Length);
            offset += nativeArray.Length;
        }
        return aggregatedArray;
    }
    public static AggregatedPredicate<T, TPredicate1, TPredicate2> Aggregate<T, TPredicate1, TPredicate2>(this TPredicate1 predicate1, in TPredicate2 predicate2)
        where T : struct
        where TPredicate1 : struct, INativePredicate<T>
        where TPredicate2 : struct, INativePredicate<T>
    {
        return new AggregatedPredicate<T, TPredicate1, TPredicate2>(predicate1, predicate2);
    }
    public static AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3> Aggregate<T, TPredicate1, TPredicate2, TPredicate3>(this TPredicate1 predicate1,
        in TPredicate2 predicate2, in TPredicate3 predicate3)
        where T : struct
        where TPredicate1 : struct, INativePredicate<T>
        where TPredicate2 : struct, INativePredicate<T>
        where TPredicate3 : struct, INativePredicate<T>
    {
        return new AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3>(predicate1, predicate2, predicate3);
    }
    public static AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4> Aggregate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4>(
        this TPredicate1 predicate1, in TPredicate2 predicate2, in TPredicate3 predicate3, in TPredicate4 predicate4)
        where T : struct
        where TPredicate1 : struct, INativePredicate<T>
        where TPredicate2 : struct, INativePredicate<T>
        where TPredicate3 : struct, INativePredicate<T>
        where TPredicate4 : struct, INativePredicate<T>
    {
        return new AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4>(predicate1, predicate2, predicate3, predicate4);
    }
}