using System;
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
    public struct NativeIndexTracker<TKey, TInstance> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TInstance : unmanaged
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeList<TKey> _referencesKeys;
        [NativeDisableContainerSafetyRestriction]
        private NativeList<TInstance> _referencesValues;
        [NativeDisableContainerSafetyRestriction]
        private NativeHashMap<TKey, int> _indexLookup;

        public NativeIndexTracker(int initialCapacity, Allocator allocator)
        {
            _referencesKeys = new(initialCapacity, allocator);
            _referencesValues = new(initialCapacity, allocator);
            _indexLookup = new(initialCapacity, allocator);
        }

        public bool IsCreated => _referencesKeys.IsCreated;

        public TInstance this[TKey key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AddOrReplace(in key, in value);
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
        private void AddOrReplace(in TKey key, in TInstance instance)
        {
            if ( _indexLookup.TryGetValue(key, out var index) )
            {
                _referencesValues.ElementAt(index) = instance;
            }
            else
            {
                Add(in key, in instance);
            }
            AssertArraySizeMatch();
        }

        [Conditional("UNITY_ASSERTIONS")]
        public void AssertArraySizeMatch()
        {
            Assert.AreEqual(_referencesKeys.Length, _referencesValues.Length, "[PANIC] the length of values and keys collections are different.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(in TKey key, in TInstance instance)
        {
            _indexLookup.Add(key, _referencesKeys.Length);
            _referencesValues.Add(in instance);
            _referencesKeys.Add(in key);
            AssertArraySizeMatch();
        }

        public void AddRange(in NativeArray<TKey> keys, in NativeArray<TInstance> instances)
        {
#if UNITY_EDITOR
            if ( keys.Length != instances.Length )
            {
                Debug.LogError($"The number of keys and instance arrays are different. aborting...");
                return;
            }
#endif
            var startIndex = _referencesKeys.Length;
            for ( int i = 0; i < keys.Length; i++ )
            {
                var key = keys[i];
                if(TryRemove(key))
                {
                    i--;
                }
                _indexLookup.TryAdd(key, startIndex + i);
            }
            _referencesKeys.AddRange(keys);
            _referencesValues.AddRange(instances);
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

        public NativeArray<TKey>.ReadOnly AsKeysArray()
        {
            return _referencesKeys.AsArray().AsReadOnly();
        }

        public NativeArray<TInstance>.ReadOnly AsValuesArray()
        {
            return _referencesValues.AsArray().AsReadOnly();
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