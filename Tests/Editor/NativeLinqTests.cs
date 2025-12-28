using System;
using System.Linq;
using Drboum.Utilities.NativeLinq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using static Drboum.Utilities.NativeLinq.NativeLinqExtensions;
using static Drboum.Utilities.Collections.CollectionCustomHelper;
namespace Drboum.Utilities.Tests.Editor {
    public class NativeLinqTests {
        private const int INITIAL_DEFAULT_LENGTH = 10;
        internal static void CreateFloat3TestData(out NativeArray<float3> nativeArray, Allocator allocator = Allocator.Temp, int length = INITIAL_DEFAULT_LENGTH)
        {
            nativeArray = new NativeArray<float3>(length, allocator, NativeArrayOptions.UninitializedMemory);
            for ( var index = 0; index < nativeArray.Length; index++ )
            {
                nativeArray[index] = GetGeneratedFloat3Value(index);
            }
        }
        internal static float3 GetGeneratedFloat3Value(int index)
        {
            return new float3(index);
        }
        private static void CreateTestData<T>(out NativeArray<T> nativeArray, T fixedValue, Allocator allocator = Allocator.Temp, int length = INITIAL_DEFAULT_LENGTH)
            where T : struct
        {
            nativeArray = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
            for ( var index = 0; index < nativeArray.Length; index++ )
            {
                nativeArray[index] = fixedValue;
            }
        }
        [Test]
        public void WhereNativeNativeArrayReturnExpectedData()
        {
            var lookupValue = new float3(0f);
            CreateFloat3TestData(out var data, Allocator.Temp);
            var predicateFloat3 = CreateEquatablePredicate(lookupValue);
            var results         = data.WhereNative(predicateFloat3, Allocator.Temp);
            var list            = data.Where((x) => predicateFloat3.EvaluatePredicate(x)).ToList();
            Assert.AreEqual(list.Count, results.Length);
            Assert.AreEqual(results[0], lookupValue);
            results.Dispose();
            data.Dispose();
        }
        [Test]
        public void WhereNativeNativeArrayDoesNotAllocateNewArrayAndReturnExpectedData()
        {
            CreateFloat3TestData(out var data);
            var                        float3All2      = new float3(2);
            EquatablePredicate<float3> predicateFloat3 = CreateEquatablePredicate(float3All2);
            var                        dataCpy         = data.CreateCopy();
            var                        results         = data.WhereNative(predicateFloat3);
            var                        list            = dataCpy.Where((x) => predicateFloat3.EvaluatePredicate(x)).ToList();
            Assert.AreEqual(list.Count, results.Length);
            Assert.AreEqual(data[0],    float3All2);
            data.Dispose();
        }
        public struct ConvertFloatToInt : IConverter<float, int> {

            public int ConvertValue(in float element)
            {
                return (int)element;
            }
        }
        [Test]
        public void SelectNativeNativeArrayReturnExpectedValues()
        {
            var fixedDataValue = 1f;
            CreateTestData(out var data, fixedDataValue);
            ConvertFloatToInt       converter                        = new ConvertFloatToInt();
            int                     dummyConvertValueTypeForGenerics = 0;
            NativeList<int>         result                           = data.SelectNative(in converter, Allocator.Temp, dummyConvertValueTypeForGenerics);
            Assert.AreEqual(result.Length, data.Length);
            EquatablePredicate<int> equatablePredicate               = CreateEquatablePredicate(1);
            Assert.IsTrue(result.AsArray().AllNative(in equatablePredicate));
        }
        [Test]
        public void AllNativeIsReturningExpectedResults()
        {
            CreateFloat3TestData(out var data);
            EquatablePredicate<float3> equatablePredicate = CreateEquatablePredicate(new float3(0));
            Assert.IsFalse(data.AllNative(in equatablePredicate));
            CreateTestData(out data,0);
            Assert.IsTrue(data.AllNative(in equatablePredicate));
            data[data.Length - 1] = new float3(1);
            Assert.IsFalse(data.AllNative(in equatablePredicate));
        }
        [Test]
        public void AnyNativeIsReturningExpectedResults()
        {
            CreateFloat3TestData(out var data);
            EquatablePredicate<float3> equatablePredicate = CreateEquatablePredicate(new float3(0));
            Assert.IsFalse(data.AnyNative(in equatablePredicate));
            data[data.Length - 1] = new float3(1);
            Assert.IsFalse(data.AllNative(in equatablePredicate));
            CreateTestData(out data, 0);
            Assert.IsTrue(data.AnyNative(in equatablePredicate));
        }
        [Test]
        public void Take2ElementNativeArrayReturnExpectedValues()
        {
            CreateFloat3TestData(out var data);
            var results = data.TakeNative(2);
            Assert.AreEqual(2, results.Length);
            for ( int i = 0; i < results.Length; i++ )
            {
                Assert.AreEqual(GetGeneratedFloat3Value(i), results[i]);
            }
        }
        [Test]
        public void TakeWhileNativeArrayReturnExpectedValues()
        {
            var fixedValue = new float3(5f);
            CreateTestData(out var data, fixedValue, length: INITIAL_DEFAULT_LENGTH);
            var                 predicate = CreateEquatablePredicate(fixedValue);
            NativeArray<float3> results   = default;

            results = data.TakeWhileNative(predicate);

            Assert.AreEqual(INITIAL_DEFAULT_LENGTH, results.Length);
            for ( int i = 0; i < results.Length; i++ )
            {
                Assert.AreEqual(fixedValue, results[i]);
            }
            data.Dispose();
        }
        [Test]
        public void SkipNativeArray2ElementReturnExpectedValues()
        {
            CreateFloat3TestData(out var data, length: INITIAL_DEFAULT_LENGTH);
            const int count          = 2;
            var       originalLength = data.Length;
            var       results        = data.SkipNative(count);
            Assert.AreEqual(originalLength - count, results.Length);
            for ( int i = 0; i < results.Length; i++ )
            {
                Assert.AreEqual(GetGeneratedFloat3Value(i + count), results[i]);
            }
            data.Dispose();
        }
        [Test]
        public void SkipWhileNativeArrayReturnExpectedValues()
        {
            var fixedValue = new float3(0f);
            CreateTestData(out var data, fixedValue);
            var predicate = CreateEquatablePredicate(fixedValue);
            SkipWhileTest(data, predicate);
            CreateFloat3TestData(out data);
            SkipWhileTest(data, predicate);

        }
        private static void SkipWhileTest<T, TNativePredicate>(NativeArray<T> data, TNativePredicate predicate)
            where T : unmanaged
            where TNativePredicate : unmanaged, INativePredicate<T>
        {
            var results  = data.CreateCopy().SkipWhileNative<T, TNativePredicate>(in predicate);
            var listLinq = data.SkipWhile((x) => predicate.EvaluatePredicate(x)).ToList();
            Assert.AreEqual(listLinq.Count, results.Length);
        }
        [Test]
        public void SumNativeArrayWithPredicateIsBursted()
        {
            var lookupValue = 1;
            CreateTestData(out var data, lookupValue, Allocator.TempJob);
            var predicate = CreateEquatablePredicate(lookupValue);
            var burstPredicate = new EquatablePredicateBurstTest<int> {
                Predicate = predicate
            };
            var resultSum = new NativeReference<int>(Allocator.TempJob);
            var job1 = new BurstTestPredicateSumJob {
                Source = data,
                Result = resultSum
            };
            job1.Run();
            var job2 = new BurstTestPredicateSumJob<AlwaysTruePredicate<int>> {
                Predicate = default,
                Source    = data,
                Result    = resultSum
            };
            job2.Run();
            resultSum.Dispose();
            data.Dispose();
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct BurstTestJob<T, TNativePredicate> : IJob
            where T : unmanaged
            where TNativePredicate : struct, INativePredicate<T> {

            public NativeArray<T>   Source;
            public TNativePredicate Predicate;
            public void Execute()
            { }
        }
        [BurstCompile(CompileSynchronously = true)]
        private struct BurstTestPredicateSumJob<TNativePredicate> : IJob
            where TNativePredicate : struct, INativePredicate<int> {
            public NativeArray<int>     Source;
            public TNativePredicate     Predicate;
            public NativeReference<int> Result;
            public unsafe void Execute()
            {
                Result.Value = SumNativePtr((int*)Source.GetUnsafeReadOnlyPtr(), Source.Length, Predicate);
            }
        }
        [BurstCompile(CompileSynchronously = true)]
        private struct BurstTestPredicateSumJob : IJob {
            public NativeArray<int>     Source;
            public NativeReference<int> Result;
            public unsafe void Execute()
            {
                Result.Value = Source.SumNative();
            }
        }

        private struct EquatablePredicateBurstTest<T> : INativePredicate<T>
            where T : unmanaged, IEquatable<T> {
            public  EquatablePredicate<T> Predicate;
            private bool                  _runOnce;
            public bool EvaluatePredicate(in T element)
            {
                LogErrorIfJobIsNotBurstCompile(ref _runOnce);
                return Predicate.EvaluatePredicate(in element);
            }

        }
    }

}