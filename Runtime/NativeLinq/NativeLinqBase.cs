﻿using System;
using JetBrains.Annotations;
namespace DrboumLibrary.NativeLinq {

    public interface INativePredicate<T>
        where T : struct {
        [Pure]
        bool EvaluatePredicate(in T element);
    }
    public interface IConverter<T, out TResult>
        where T : struct
        where TResult : struct {
        [Pure]
        TResult ConvertValue(in T element);
    }
    public readonly struct EquatablePredicate<T, TEquatable> : INativePredicate<T>
        where T : struct
        where TEquatable : IEquatable<T> {
        private readonly TEquatable _lookupValue;
        public EquatablePredicate(TEquatable lookupValue)
        {
            _lookupValue = lookupValue;
        }
        [Pure]
        public readonly bool EvaluatePredicate(in T element)
        {
            return _lookupValue.Equals(element);
        }
    }
    public readonly struct EquatablePredicate<T> : INativePredicate<T>
        where T : struct, IEquatable<T> {
        private readonly T _lookupValue;
        public EquatablePredicate(T lookupValue)
        {
            _lookupValue = lookupValue;
        }
        [Pure]
        public bool EvaluatePredicate(in T element)
        {
            return _lookupValue.Equals(element);
        }
    }
    
    public struct AlwaysTruePredicate<T> : INativePredicate<T>
        where T : struct {
        public readonly bool EvaluatePredicate(in T x) => true;
    }
    
    public readonly struct AggregatedPredicate<T, TPredicate1, TPredicate2> : INativePredicate<T>
        where T : struct
        where TPredicate1 : struct, INativePredicate<T>
        where TPredicate2 : struct, INativePredicate<T> {

        public readonly TPredicate1 Predicate1;
        public readonly TPredicate2 Predicate2;
        public AggregatedPredicate(in TPredicate1 predicate1, in TPredicate2 predicate2)
        {
            Predicate1 = predicate1;
            Predicate2 = predicate2;
        }
        public bool EvaluatePredicate(in T element)
        {
            return Predicate1.EvaluatePredicate(in element) && Predicate2.EvaluatePredicate(in element);
        }
    }
    public readonly struct AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3> : INativePredicate<T>
        where T : struct
        where TPredicate1 : struct, INativePredicate<T>
        where TPredicate2 : struct, INativePredicate<T>
        where TPredicate3 : struct, INativePredicate<T> {

        public readonly TPredicate1 Predicate1;
        public readonly TPredicate2 Predicate2;
        public readonly TPredicate3 Predicate3;
        public AggregatedPredicate(in TPredicate1 predicate1, in TPredicate2 predicate2, in TPredicate3 predicate3)
        {
            Predicate1 = predicate1;
            Predicate2 = predicate2;
            Predicate3 = predicate3;
        }
        public bool EvaluatePredicate(in T element)
        {
            return Predicate1.EvaluatePredicate(in element) && Predicate2.EvaluatePredicate(in element) && Predicate3.EvaluatePredicate(in element);
        }
    }
    public readonly struct AggregatedPredicate<T, TPredicate1, TPredicate2, TPredicate3, TPredicate4> : INativePredicate<T>
        where T : struct
        where TPredicate1 : struct, INativePredicate<T>
        where TPredicate2 : struct, INativePredicate<T>
        where TPredicate3 : struct, INativePredicate<T>
        where TPredicate4 : struct, INativePredicate<T> {

        public readonly TPredicate1 Predicate1;
        public readonly TPredicate2 Predicate2;
        public readonly TPredicate3 Predicate3;
        public readonly TPredicate4 Predicate4;
        public AggregatedPredicate(in TPredicate1 predicate1, in TPredicate2 predicate2, in TPredicate3 predicate3, in TPredicate4 predicate4)
        {
            Predicate1 = predicate1;
            Predicate2 = predicate2;
            Predicate3 = predicate3;
            Predicate4 = predicate4;
        }
        public bool EvaluatePredicate(in T element)
        {
            return Predicate1.EvaluatePredicate(in element) && Predicate2.EvaluatePredicate(in element) && Predicate3.EvaluatePredicate(in element) &&
                   Predicate4.EvaluatePredicate(in element);
        }
    }
}