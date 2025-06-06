﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace Drboum.Utilities.Collections
{
    /// <summary>
    /// A lookup which order is non-deterministic and enforce key/value unicity with 2 backing collection for fast read-only processing
    /// </summary>
    /// <remarks>not thread safe</remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TInstance"></typeparam>
    public unsafe struct NativeFastReadHashMap<TKey, TInstance> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TInstance : unmanaged
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeList<TKey> _referencesKeys;
        [NativeDisableContainerSafetyRestriction]
        private NativeList<TInstance> _referencesValues;
        [NativeDisableContainerSafetyRestriction]
        private NativeHashMap<TKey, int> _indexLookup;

        public NativeFastReadHashMap(int initialCapacity, Allocator allocator)
        {
            _referencesKeys = new(initialCapacity, allocator);
            _referencesValues = new(initialCapacity, allocator);
            _indexLookup = new(initialCapacity, allocator);
        }

        public bool IsCreated => _referencesKeys.IsCreated;

        public TInstance this[TKey key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AddOrUpdate(in key, in value);
        }

        public ref TInstance ElementAt(in TKey key)
        {
            var exist = _indexLookup.TryGetValue(key, out var index);
            Unity.Assertions.Assert.IsTrue(exist);
            return ref _referencesValues.ElementAt(index);
        }

        public int Length => _referencesKeys.Length;
        public int Capacity => _referencesKeys.Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacity(int capacityToAdd)
        {
            _referencesKeys.Capacity += capacityToAdd;
            _referencesValues.Capacity += capacityToAdd;
            _indexLookup.Capacity += capacityToAdd;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TInstance value)
        {
            if ( _indexLookup.TryGetValue(key, out var index) )
            {
                value = _referencesValues.ElementAt(index);
                return true;
            }
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in TKey key)
        {
            return _indexLookup.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddOrUpdate(in TKey key, in TInstance instance)
        {
            int hashMapIndex = _indexLookup.FindMapIndex(in key);
            if ( hashMapIndex != -1 )
            {
                _referencesValues.ElementAt(_indexLookup.GetValueFromMapIndex(hashMapIndex)) = instance;
            }
            else
            {
                AddNoCheckImpl(in key, in instance);
            }
            AssertArraySizeMatch();
        }

        [Conditional("UNITY_ASSERTIONS")]
        public void AssertArraySizeMatch()
        {
            Assert.AreEqual(_referencesKeys.Length, _referencesValues.Length, "[PANIC] the length of values and keys collections are different.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddNoCheckImpl(in TKey key, in TInstance instance)
        {
            _indexLookup.AddNoExistCheckImpl(in key, _referencesKeys.Length);
            _referencesValues.Add(in instance);
            _referencesKeys.Add(in key);
            AssertArraySizeMatch();
        }

        public bool TryAdd(in TKey key, in TInstance instance)
        {
            if ( _indexLookup.TryAdd(key, _referencesKeys.Length) )
            {
                _referencesValues.Add(in instance);
                _referencesKeys.Add(in key);
                AssertArraySizeMatch();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Add to this collection with as little of a cost as possible
        /// <remarks>the collection must be cleared before running this method</remarks> 
        /// </summary>
        public void AddRangeFromZero(in NativeArray<TKey> keys, in NativeArray<TInstance> instances)
        {
            AddRangeFromZero((TKey*)keys.GetUnsafeReadOnlyPtr(), keys.Length, (TInstance*)instances.GetUnsafeReadOnlyPtr(), instances.Length);
        }

        /// <inheritdoc cref="AddRangeFromZero(in Unity.Collections.NativeArray{TKey},in Unity.Collections.NativeArray{TInstance})"/>
        public void AddRangeFromZero(TKey* keys, int keyCount, TInstance* instances, int instanceCount)
        {
            Assert.IsTrue(_referencesKeys.IsEmpty);
            CollectionCustomHelper.CheckCopyLengths(keyCount, instanceCount);

            for ( int i = 0; i < keyCount; i++ )
            {
                //we cannot have duplicate key as they would store an instance that would never be used,
                //so we rather alert the user (Add will throw if the key exist)
                _indexLookup.Add(keys[i], i);
            }
            _referencesKeys.AddRange(keys, keyCount);
            _referencesValues.AddRange(instances, instanceCount);
        }

        public void AddRange(in NativeArray<TKey> keys, in NativeArray<TInstance> instances)
        {
            AddRange((TKey*)keys.GetUnsafeReadOnlyPtr(), keys.Length, (TInstance*)instances.GetUnsafeReadOnlyPtr(), instances.Length);
        }

        public void AddRange(TKey* keys, int keyCount, TInstance* instances, int instanceCount)
        {
            CollectionCustomHelper.CheckCopyLengths(keyCount, instanceCount);

            var startIndex = _referencesKeys.Length;
            for ( int i = 0; i < keyCount; i++ )
            {
                var key = keys[i];
                if ( TryRemove(key) )
                {
                    i--;
                }
                _indexLookup.TryAdd(key, startIndex + i);
            }
            _referencesKeys.AddRange(keys, keyCount);
            _referencesValues.AddRange(instances, instanceCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryRemove(TKey instanceID, out int instanceArrayIndex)
        {
            if ( !_indexLookup.TryGetValue(instanceID, out instanceArrayIndex) )
                return false;

            RemoveSwapBack(instanceID, instanceArrayIndex);
            return true;
        }

        public bool TryRemove(in TKey key)
        {
            return TryRemove(key, out _);
        }

        public ReadOnlySpan<TKey> AsKeysArray()
        {
            return _referencesKeys.AsArray();
        }

        public ReadOnlySpan<TInstance> AsValuesArray()
        {
            return _referencesValues.AsArray();
        }

        public void ToNativeArrays(Allocator allocator, out NativeArray<TKey> keys, out NativeArray<TInstance> values)
        {
            keys = _referencesKeys.ToArray(allocator);
            values = _referencesValues.ToArray(allocator);
        }

        private void RemoveSwapBack(TKey instanceToRemove, int arrayIndexToSwapAt)
        {
            int lastIndex = _referencesKeys.Length - 1;
            _indexLookup[_referencesKeys.ElementAt(lastIndex)] = arrayIndexToSwapAt;

            _indexLookup.Remove(instanceToRemove);
            _referencesValues.RemoveAtSwapBack(arrayIndexToSwapAt);
            _referencesKeys.RemoveAtSwapBack(arrayIndexToSwapAt);
            AssertArraySizeMatch();
        }

        public void Dispose()
        {
            _referencesKeys.Dispose();
            _referencesValues.Dispose();
            _indexLookup.Dispose();
        }

        public void Dispose(JobHandle dependencies, NativeList<JobHandle> disposeHandles)
        {
            disposeHandles.Add(_referencesValues.Dispose(dependencies));
            disposeHandles.Add(_referencesKeys.Dispose(dependencies));
            disposeHandles.Add(_indexLookup.Dispose(dependencies));
        }

        public void Clear()
        {
            _referencesKeys.Clear();
            _referencesValues.Clear();
            _indexLookup.Clear();
        }
    }
}