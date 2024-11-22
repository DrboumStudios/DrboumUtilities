using System;
using System.Runtime.CompilerServices;
using Drboum.Utilities.Collections;
using Drboum.Utilities.Runtime.NativeLinq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public static partial class NativeLinqExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EquatablePredicate<T> CreateEquatablePredicate<T>(T lookupValue)
        where T : unmanaged, IEquatable<T>
    {
        return new EquatablePredicate<T>(lookupValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EquatablePredicate<T, TEquatable> CreateEquatablePredicate<T, TEquatable>(TEquatable lookupValue)
        where T : unmanaged
        where TEquatable : IEquatable<T>
    {
        return new EquatablePredicate<T, TEquatable>(lookupValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> CreateCopy<T>(this ref NativeArray<T> buffer, Allocator allocator = Allocator.Temp)
        where T : unmanaged
    {
        return new NativeArray<T>(buffer, allocator);
    }

    /// <summary>
    /// return the results matched by the <see cref="TNativePredicate"/> instance in a smaller, temporary array, effectively reusing the same memory
    /// </summary>
    /// <param name="source">the source array that will be modified</param>
    /// <param name="predicate">predicate to filter in results</param>
    /// <typeparam name="TNativePredicate"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// NativePredicate implementation of IEquatable => <see cref="EquatablePredicate{T,TEquatable}"/>
    /// <seealso cref="CreateEquatablePredicate{T,TEquatable}"/>
    /// </example>
    /// <returns>a modified version of the <paramref name="source"/> which the number of matching results equals the length on the returned array,
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> WhereNative<TNativePredicate, T>(this NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
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
        return source.GetSubArray(0, resultStartIndex);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> WhereNative<TNativePredicate, T>(this in NativeArray<T> source, in TNativePredicate predicate, Allocator allocator,
        int expectedMatches = 2)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
    {
        var nativeList = new NativeList<T>(math.min(expectedMatches, source.Length), allocator);
        for ( var index = 0; index < source.Length; index++ )
        {
            T element = source[index];
            if ( predicate.EvaluatePredicate(in element) )
            {
                nativeList.Add(in element);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TNativePredicate, TConverter>(
        this in NativeArray<T> source,
        in TNativePredicate predicate,
        in TConverter converter,
        Allocator allocator,
        in TConvertResult dummyConvertValueTypeForGenerics = default)
        where T : unmanaged
        where TConvertResult : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
        where TConverter : unmanaged, IConverter<T, TConvertResult>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TConverter>(
        this in NativeArray<T> source,
        in TConverter converter,
        Allocator allocator,
        in TConvertResult dummyConvertValueTypeForGenerics)
        where T : unmanaged
        where TConvertResult : unmanaged
        where TConverter : unmanaged, IConverter<T, TConvertResult>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TNativePredicate, TConverter, TNativeList>(
        this ref TNativeList source,
        in TNativePredicate predicate,
        in TConverter converter,
        Allocator allocator,
        TConvertResult dummyConvertValueTypeForGenerics = default,
        int resultListInitialCapacity = -1)
        where T : unmanaged
        where TConvertResult : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
        where TNativeList : unmanaged, INativeList<T>
        where TConverter : unmanaged, IConverter<T, TConvertResult>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeList<TConvertResult> SelectNative<T, TConvertResult, TConverter, TNativeList>(
        this ref TNativeList source,
        in TConverter converter,
        Allocator allocator,
        in TConvertResult dummyConvertValueTypeForGenerics,
        int resultListInitialCapacity = -1)
        where T : unmanaged
        where TConvertResult : unmanaged
        where TNativeList : unmanaged, INativeList<T>
        where TConverter : unmanaged, IConverter<T, TConvertResult>
    {
        return
            source.SelectNative<T, TConvertResult, AlwaysTruePredicate<T>, TConverter, TNativeList>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
    {
        return FindFirstIndex(in source, in predicate) != -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindFirstIndex<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindLastIndex<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FirstOrDefaultNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
    {
        int foundIndex = source.FindFirstIndex(in predicate);
        return foundIndex != -1 ? source[foundIndex] : default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T LastOrDefaultNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
    {
        int foundIndex = source.FindLastIndex(in predicate);
        return foundIndex != -1 ? source[foundIndex] : default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> TakeNative<T>(this ref NativeArray<T> source, int count)
        where T : unmanaged => source.GetSubArray(0, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> TakeWhileNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
    {
        int count = 0;
        int index;
        for ( index = 0; index < source.Length && count == 0; index++ )
        {
            T element = source[index];
            while ( predicate.EvaluatePredicate(in element) )
            {
                count++;
            }
        }
        return source.GetSubArray(index, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> SkipNative<T>(this NativeArray<T> source, int count)
        where T : unmanaged => source.GetSubArray(source.Length - count, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe NativeArray<T> SkipWhileNative<T, TNativePredicate>(this in NativeArray<T> source, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
    {
        int count = GetSkipCount<T, TNativePredicate>(source.GetUnsafeReadOnlyPtr(), source.Length, predicate);
        return source.SkipNative(count);
    }

    private static unsafe int GetSkipCount<T, TNativePredicate>(void* source, int sourceLength, in TNativePredicate predicate)
        where T : unmanaged
        where TNativePredicate : unmanaged, INativePredicate<T>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int SumNative(this in NativeArray<int> source) => SumNativePtr((int*)source.GetUnsafeReadOnlyPtr(), source.Length, default(AlwaysTruePredicate<int>));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int SumNative<TNativePredicate>(this in NativeArray<int> source, in TNativePredicate predicate)
        where TNativePredicate : unmanaged, INativePredicate<int>
    {
        return SumNativePtr((int*)source.GetUnsafeReadOnlyPtr(), source.Length, predicate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int SumNative<TNativeList>(this ref TNativeList source)
        where TNativeList : unmanaged, INativeList<int>
    {
        return SumNativePtr((int*)source.GetUnsafePtr<int, TNativeList>(), source.Length, default(AlwaysTruePredicate<int>));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int SumNative<TNativeList, TNativePredicate>(this ref TNativeList source, in TNativePredicate predicate)
        where TNativeList : unmanaged, INativeList<int>
        where TNativePredicate : unmanaged, INativePredicate<int>
    {
        return SumNativePtr((int*)source.GetUnsafePtr<int, TNativeList>(), source.Length, predicate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float SumNative(this in NativeArray<float> source) => SumNativePtr((float*)source.GetUnsafeReadOnlyPtr(), source.Length, default(AlwaysTruePredicate<float>));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe float SumNativePtr<TNativePredicate>(float* source, int length, in TNativePredicate predicate)
        where TNativePredicate : unmanaged, INativePredicate<float>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float SumNative<TNativePredicate>(this in NativeArray<float> source, in TNativePredicate predicate)
        where TNativePredicate : unmanaged, INativePredicate<float>
    {
        return SumNativePtr((float*)source.GetUnsafeReadOnlyPtr(), source.Length, in predicate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NativeArray<T> Aggregate<T>(this NativeArray<T> source1, NativeArray<T> source2, Allocator allocator)
        where T : unmanaged
    {
        var aggregatedArray = new NativeArray<T>(source1.Length + source2.Length, allocator);
        var offset = 0;

        aggregatedArray.AggregateInternal(ref source1, ref offset);
        aggregatedArray.AggregateInternal(ref source2, ref offset);
        return aggregatedArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe NativeArray<T> Aggregate<T>(this NativeArray<T> source1, NativeArray<T> source2, NativeArray<T> source3, Allocator allocator)
        where T : unmanaged
    {
        var arrays = stackalloc NativeArray<T>[] { source1, source2, source3 };
        return Aggregate(arrays, 3, source1.Length + source2.Length + source3.Length, allocator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AggregateInternal<T>(this ref NativeArray<T> aggregatedArray, ref NativeArray<T> nativeArray, ref int offset)
        where T : unmanaged
    {
        NativeArray<T>.Copy(nativeArray, 0, aggregatedArray, offset, nativeArray.Length);
        offset += nativeArray.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe NativeArray<T> Aggregate<T>(NativeArray<T>* nativeArrays, int length, int aggregateArrayLength, Allocator allocator)
        where T : unmanaged
    {
        var aggregatedArray = new NativeArray<T>(aggregateArrayLength, allocator);
        var offset = 0;

        for ( int i = 0; i < length; i++ )
        {
            NativeArray<T> nativeArray = nativeArrays[i];
            AggregateInternal(ref aggregatedArray, ref nativeArray, ref offset);
        }
        return aggregatedArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AggregatedPredicate<T, TPredicate1, TPredicate2> Aggregate<T, TPredicate1, TPredicate2>(this TPredicate1 predicate1, in TPredicate2 predicate2)
        where T : unmanaged
        where TPredicate1 : unmanaged, INativePredicate<T>
        where TPredicate2 : unmanaged, INativePredicate<T>
    {
        return new AggregatedPredicate<T, TPredicate1, TPredicate2>(predicate1, predicate2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3> Aggregate<T, TPredicate1, TPredicate2, TPredicate3>(this TPredicate1 predicate1,
        in TPredicate2 predicate2, in TPredicate3 predicate3)
        where T : unmanaged
        where TPredicate1 : unmanaged, INativePredicate<T>
        where TPredicate2 : unmanaged, INativePredicate<T>
        where TPredicate3 : unmanaged, INativePredicate<T>
    {
        return new AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3>(predicate1, predicate2, predicate3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4> Aggregate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4>(
        this TPredicate1 predicate1, in TPredicate2 predicate2, in TPredicate3 predicate3, in TPredicate4 predicate4)
        where T : unmanaged
        where TPredicate1 : unmanaged, INativePredicate<T>
        where TPredicate2 : unmanaged, INativePredicate<T>
        where TPredicate3 : unmanaged, INativePredicate<T>
        where TPredicate4 : unmanaged, INativePredicate<T>
    {
        return new AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4>(predicate1, predicate2, predicate3, predicate4);
    }
}