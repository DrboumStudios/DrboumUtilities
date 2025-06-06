﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Drboum.Utilities.Collections {
    /// <summary>
    ///     Read-only access to an array or to a slice of an array.
    /// </summary>
    /// <typeparam name="TValue">Type of values stored in the array.</typeparam>
    /// <remarks>
    ///     The purpose of this struct is to allow exposing internal arrays directly such that no
    ///     boxing and no going through interfaces is required but at the same time not allowing
    ///     the internal arrays to be modified.
    ///     It differs from <c>ReadOnlySpan&lt;T&gt;</c> in that it can be stored on the heap and differs
    ///     from <c>ReadOnlyCollection&lt;T&gt;</c> in that it supports slices directly without needing
    ///     an intermediate object representing the slice.
    ///     Note that in most cases, the ReadOnlyArray instance should be treated as a <em>temporary</em>.
    ///     The actual array referred to by a ReadOnlyArray instance is usually owned and probably mutated
    ///     by another piece of code. When that code makes changes to the array, the ReadOnlyArray
    ///     instance will not get updated.
    /// </remarks>
    public struct ReadOnlyArray<TValue> : IReadOnlyList<TValue> {
        internal TValue[] m_Array;
        internal int      m_StartIndex;
        internal int      m_Length;

        /// <summary>
        ///     Construct a read-only array covering all of the given array.
        /// </summary>
        /// <param name="array">Array to index.</param>
        public ReadOnlyArray(TValue[] array)
        {
            m_Array      = array;
            m_StartIndex = 0;
            m_Length     = array?.Length ?? 0;
        }

        /// <summary>
        ///     Construct a read-only array that covers only the given slice of <paramref name="array" />.
        /// </summary>
        /// <param name="array">Array to index.</param>
        /// <param name="index">
        ///     Index at which to start indexing <paramref name="array" />. The given element
        ///     becomes index #0 for the read-only array.
        /// </param>
        /// <param name="length">Length of the slice to index from <paramref name="array" />.</param>
        public ReadOnlyArray(TValue[] array, int index, int length)
        {
            m_Array      = array;
            m_StartIndex = index;
            m_Length     = length;
        }

        /// <summary>
        ///     Convert to array.
        /// </summary>
        /// <returns>A new array containing a copy of the contents of the read-only array.</returns>
        public TValue[] ToArray()
        {
            var result = new TValue[m_Length];
            if ( m_Length > 0 ) {
                Array.Copy(m_Array, m_StartIndex, result, 0, m_Length);
            }
            return result;
        }

        public int IndexOf(Predicate<TValue> predicate)
        {
            if ( predicate == null ) {
                throw new ArgumentNullException(nameof(predicate));
            }

            for ( var i = 0; i < m_Length; ++i ) {
                if ( predicate(m_Array[m_StartIndex + i]) ) {
                    return i;
                }
            }

            return -1;
        }
        /// <summary>
        ///     Returns an enumerator that iterates through the read-only array.
        ///     <returns>
        ///         <see cref="ReadOnlyArray{TValue}.Enumerator" />
        ///         An enumerator for the read-only array.
        ///     </returns>
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_Array, m_StartIndex, m_Length);
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification =
                "`ToXXX` message only really makes sense as static, which is not recommended for generic types.")]
        public static implicit operator ReadOnlyArray<TValue>(TValue[] array)
        {
            return new ReadOnlyArray<TValue>(array);
        }

        /// <summary>
        ///     Number of elements in the array.
        /// </summary>
        public int Count => m_Length;

        /// <summary>
        ///     Return the element at the given index.
        /// </summary>
        /// <param name="index">Index into the array.</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than
        ///     <see cref="Count" />.
        /// </exception>
        /// <exception cref="InvalidOperationException"></exception>
        public TValue this[int index] {
            get {
                if ( index < 0 || index >= m_Length ) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                // We allow array to be null as we are patching up ReadOnlyArrays in a separate
                // path in several places.
                if ( m_Array == null ) {
                    throw new InvalidOperationException();
                }
                return m_Array[m_StartIndex + index];
            }
        }

        /// <summary>
        ///     <see cref="ReadOnlyArray{TValue}" />
        ///     Enumerates the elements of a read-only array.
        /// </summary>
        public struct Enumerator : IEnumerator<TValue> {
            private readonly TValue[] m_Array;
            private readonly int      m_IndexStart;
            private readonly int      m_IndexEnd;
            private          int      m_Index;

            internal Enumerator(TValue[] array, int index, int length)
            {
                m_Array      = array;
                m_IndexStart = index - 1; // First call to MoveNext() moves us to first valid index.
                m_IndexEnd   = index + length;
                m_Index      = m_IndexStart;
            }

            /// <inheritdoc />
            public void Dispose()
            { }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if ( m_Index < m_IndexEnd ) {
                    ++m_Index;
                }
                return m_Index != m_IndexEnd;
            }

            /// <inheritdoc />
            public void Reset()
            {
                m_Index = m_IndexStart;
            }

            /// <inheritdoc />
            public TValue Current {
                get {
                    if ( m_Index == m_IndexEnd ) {
                        throw new InvalidOperationException("Iterated beyond end");
                    }
                    return m_Array[m_Index];
                }
            }

            object IEnumerator.Current => Current;
        }
    }

    /// <summary>
    ///     Extension methods to help with <see cref="ReadOnlyArrayExtensions" /> contents.
    /// </summary>
    public static class ReadOnlyArrayExtensions {
        public static bool Contains<TValue>(this ReadOnlyArray<TValue> array, TValue value)
            where TValue : IComparable<TValue>
        {
            for ( var i = 0; i < array.m_Length; ++i ) {
                if ( array.m_Array[array.m_StartIndex + i].CompareTo(value) == 0 ) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsReference<TValue>(this ReadOnlyArray<TValue> array, TValue value)
            where TValue : class
        {
            return IndexOfReference(array, value) != -1;
        }

        public static int IndexOfReference<TValue>(this ReadOnlyArray<TValue> array, TValue value)
            where TValue : class
        {
            for ( var i = 0; i < array.m_Length; ++i ) {
                if ( ReferenceEquals(array.m_Array[array.m_StartIndex + i], value) ) {
                    return i;
                }
            }
            return -1;
        }

        internal static bool HaveEqualReferences<TValue>(this ReadOnlyArray<TValue> array1,
            IReadOnlyList<TValue>                                                   array2, int count = int.MaxValue)
        {
            int length1 = Math.Min(array1.Count, count);
            int length2 = Math.Min(array2.Count, count);

            if ( length1 != length2 ) {
                return false;
            }

            for ( var i = 0; i < length1; ++i ) {
                if ( !ReferenceEquals(array1[i], array2[i]) ) {
                    return false;
                }
            }

            return true;
        }
    }
}