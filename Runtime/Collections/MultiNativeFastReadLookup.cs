using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace Drboum.Utilities.Collections
{
    public unsafe struct MultiNativeFastReadLookup<TKey, TData1, TData2, TData3> : IMultiNativeFastReadLookup<TKey>
        where TKey : unmanaged, IEquatable<TKey>
        where TData1 : unmanaged
        where TData2 : unmanaged
        where TData3 : unmanaged
    {
        private MultiNativeFastReadLookup<TKey> _collection;

        public MultiNativeFastReadLookup(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            ReadOnlySpan<TypeDescriptor> typeDescriptors = stackalloc TypeDescriptor[3] {
                new TypeDescriptor(sizeof(TData1)),
                new TypeDescriptor(sizeof(TData2)),
                new TypeDescriptor(sizeof(TData3)),
            };
            _collection = new(typeDescriptors, initialCapacity, allocator);
        }

        public void Dispose()
        {
            _collection.Dispose();
        }

        public bool IsCreated => _collection.IsCreated;

        public int Length => _collection.Length;

        public int Capacity {
            get => _collection.Capacity;
            set => _collection.Capacity = value;
        }

        public bool TryGetValue<TInstance>(in TKey key, int valueTypeIndex, out TInstance value)
            where TInstance : unmanaged
        {
            return _collection.TryGetValue(in key, valueTypeIndex, out value);
        }

        public ref TInstance ElementAt<TInstance>(in TKey key, int typeIndex)
            where TInstance : unmanaged
        {
            return ref _collection.ElementAt<TInstance>(in key, typeIndex);
        }

        public bool Contains(in TKey key)
        {
            return _collection.Contains(in key);
        }

        public void AssertArraysSizeMatch()
        {
            _collection.AssertArraysSizeMatch();
        }

        public bool TryAdd(in TKey key, CompactInstance instanceData)
        {
            return _collection.TryAdd(in key, instanceData);
        }

        public void AddRange(in NativeArray<TKey> keys, in NativeArray<NativeArray<byte>> instances)
        {
            _collection.AddRange(in keys, in instances);
        }

        public bool TryRemove(in TKey key)
        {
            return _collection.TryRemove(in key);
        }

        public ReadOnlySpan<TKey> AsKeysArray()
        {
            return _collection.AsKeysArray();
        }

        public ReadOnlySpan<TInstance> AsValuesArray<TInstance>(int typeIndex)
            where TInstance : unmanaged
        {
            return _collection.AsValuesArray<TInstance>(typeIndex);
        }

        public void Dispose(JobHandle dependencies, NativeList<JobHandle> disposeHandles)
        {
            _collection.Dispose(dependencies, disposeHandles);
        }

        public void Clear()
        {
            _collection.Clear();
        }
    }

    internal interface IMultiNativeFastReadLookup<TKey> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
    {
        bool IsCreated {
            get;
        }
        int Length {
            get;
        }
        int Capacity {
            get;
            set;
        }

        bool TryGetValue<TInstance>(in TKey key, int valueTypeIndex, out TInstance value)
            where TInstance : unmanaged;

        ref TInstance ElementAt<TInstance>(in TKey key, int typeIndex)
            where TInstance : unmanaged;

        bool Contains(in TKey key);
        unsafe bool TryAdd(in TKey key, CompactInstance instanceData);
        void AddRange(in NativeArray<TKey> keys, in NativeArray<NativeArray<byte>> instances);
        bool TryRemove(in TKey key);
        ReadOnlySpan<TKey> AsKeysArray();

        unsafe ReadOnlySpan<TInstance> AsValuesArray<TInstance>(int typeIndex)
            where TInstance : unmanaged;

        void Dispose(JobHandle dependencies, NativeList<JobHandle> disposeHandles);
        void Clear();
    }

    internal unsafe struct MultiNativeFastReadLookup<TKey> : IMultiNativeFastReadLookup<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeList<TKey> _referencesKeys;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<(TypeDescriptor Type, NativeList<byte> Collection)> _referencesValues;
        [NativeDisableContainerSafetyRestriction]
        private NativeHashMap<TKey, int> _indexLookup;

        private readonly int _totalInstanceByteLength;

        public MultiNativeFastReadLookup(in ReadOnlySpan<TypeDescriptor> types, int initialElementCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            _referencesKeys = new(initialElementCapacity, allocator);
            _indexLookup = new(initialElementCapacity, allocator);
            _referencesValues = CollectionHelper.CreateNativeArray<(TypeDescriptor, NativeList<byte>)>(types.Length, allocator, NativeArrayOptions.UninitializedMemory);
            _totalInstanceByteLength = 0;
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                var typeDescriptor = types[index];
                _referencesValues[index] = new(typeDescriptor, new NativeList<byte>(initialElementCapacity * typeDescriptor.Size, allocator));
                _totalInstanceByteLength += typeDescriptor.Size;
            }
        }

        public bool IsCreated => _referencesKeys.IsCreated;
        public int Length => _referencesKeys.Length;

        public int Capacity {
            get => _referencesKeys.Capacity;
            set {
                _referencesKeys.Capacity = value;
                for ( int i = 0; i < _referencesValues.Length; i++ )
                {
                    var values = _referencesValues.ReadElementAsRef(i);
                    values.Collection.Capacity = value * values.Type.Size;
                }
                _indexLookup.Capacity = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue<TInstance>(in TKey key, int valueTypeIndex, out TInstance value)
            where TInstance : unmanaged
        {
            if ( _indexLookup.TryGetValue(key, out var elementIndex) )
            {
                value = ElementAt<TInstance>(elementIndex, valueTypeIndex);
                return true;
            }
            value = default;
            return false;
        }

        public ref TInstance ElementAt<TInstance>(in TKey key, int typeIndex)
            where TInstance : unmanaged
        {
            var exist = _indexLookup.TryGetValue(key, out var elementIndex);
            Assert.IsTrue(exist);
            return ref ElementAt<TInstance>(elementIndex, typeIndex);
        }

        private ref TInstance ElementAt<TInstance>(int elementIndex, int typeIndex)
            where TInstance : unmanaged
        {
            ref var untypedCollection = ref _referencesValues.ReadElementAsRef(typeIndex);
            CollectionCustomHelper.CheckEstimatedSizeMatchActualSize(sizeof(TInstance), untypedCollection.Type.Size);
            CollectionCustomHelper.CheckElementAccess(elementIndex, _referencesKeys.Length);
            return ref UnsafeUtility.ArrayElementAsRef<TInstance>(untypedCollection.Collection.GetUnsafePtr(), elementIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in TKey key)
        {
            return _indexLookup.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddOrReplace(in TKey key, CompactInstance instanceData)
        {
            if ( _indexLookup.TryGetValue(key, out var elementIndex) )
            {
                var reader = instanceData.AsReader();
                for ( var index = 0; index < _referencesValues.Length; index++ )
                {
                    ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                    UnsafeUtility.MemCpy(typeValues.Collection.GetUnsafePtr() + (elementIndex * typeValues.Type.Size), reader.ReadNext(typeValues.Type.Size), typeValues.Type.Size);
                }
            }
            else
            {
                TryAdd(in key, instanceData);
            }
            AssertArraysSizeMatch();
        }

        [Conditional("UNITY_ASSERTIONS")]
        public void AssertArraysSizeMatch()
        {
            var values = _referencesValues[0];
            Assert.AreEqual(_referencesKeys.Length, values.Collection.Length / values.Type.Size, "[PANIC] the length of values and keys collections are different.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in TKey key, CompactInstance instanceData)
        {
            if ( !_indexLookup.TryAdd(key, _referencesKeys.Length) )
            {
                CollectionCustomHelper.CheckEstimatedSizeMatchActualSize(instanceData.Length, _totalInstanceByteLength);
                var reader = instanceData.AsReader();
                for ( var index = 0; index < _referencesValues.Length; index++ )
                {
                    ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                    typeValues.Collection.AddRange(reader.ReadNext(typeValues.Type.Size), typeValues.Type.Size);
                }

                _referencesKeys.Add(in key);
                AssertArraysSizeMatch();

                return true;
            }
            return false;
        }

        public void AddRange(in NativeArray<TKey> keys, in NativeArray<NativeArray<byte>> instances)
        {
            var startIndex = _referencesKeys.Length;
            for ( int i = 0; i < keys.Length; i++ )
            {
                var key = keys[i];
                if ( TryRemove(key) )
                {
                    i--;
                }
                _indexLookup.TryAdd(key, startIndex + i);
            }
            _referencesKeys.AddRange(keys);
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                typeValues.Collection.AddRange(instances[index]);
            }
            AssertArraysSizeMatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryRemove(TKey instanceID, out int instanceArrayIndex)
        {
            if ( !_indexLookup.TryGetValue(instanceID, out instanceArrayIndex) )
                return false;

            RemoveSwapBack(instanceID, instanceArrayIndex);
            return true;
        }

        public bool TryRemove(in TKey key) => TryRemove(key, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TKey> AsKeysArray()
        {
            return _referencesKeys.AsArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TInstance> AsValuesArray<TInstance>(int typeIndex)
            where TInstance : unmanaged
        {
            ref readonly var referencesValue = ref _referencesValues.ReadElementAsRef(typeIndex);
            CollectionCustomHelper.CheckEstimatedSizeMatchActualSize(UnsafeUtility.SizeOf<TInstance>(), referencesValue.Type.Size);
            return new ReadOnlySpan<TInstance>(referencesValue.Collection.GetUnsafePtr(), Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveSwapBack(TKey instanceToRemove, int arrayIndexToSwapAt)
        {
            int lastIndex = _referencesKeys.Length - 1;
            _indexLookup[_referencesKeys.ElementAt(lastIndex)] = arrayIndexToSwapAt;

            _indexLookup.Remove(instanceToRemove);
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                typeValues.Collection.RemoveAtSwapBack(arrayIndexToSwapAt);
            }
            _referencesKeys.RemoveAtSwapBack(arrayIndexToSwapAt);
            AssertArraysSizeMatch();
        }

        public void Dispose()
        {
            _referencesKeys.Dispose();
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                typeValues.Collection.Dispose();
            }
            _referencesValues.Dispose();
            _indexLookup.Dispose();
        }

        public void Dispose(JobHandle dependencies, NativeList<JobHandle> disposeHandles)
        {
            disposeHandles.Add(_referencesKeys.Dispose(dependencies));
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                disposeHandles.Add(typeValues.Collection.Dispose(dependencies));
            }
            disposeHandles.Add(_indexLookup.Dispose(dependencies));
        }

        public void Clear()
        {
            _referencesKeys.Clear();
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                typeValues.Collection.Clear();
            }
            _indexLookup.Clear();
        }
    }

    public readonly struct TypeDescriptor
    {
        public readonly int Size;

        public TypeDescriptor(int size)
        {
            Size = size;
        }

        public static TypeDescriptor Create<T>()
            where T : unmanaged
        {
            return new TypeDescriptor(UnsafeUtility.SizeOf<T>());
        }
    }

    public unsafe struct CompactInstance
    {
        private NativeList<byte> _data;

        public int Length => _data.Length;

        public CompactInstance(int totalInstanceDataSize)
        {
            _data = new(totalInstanceDataSize, Allocator.Temp);
        }

        public CompactInstance(NativeList<byte> data)
        {
            _data = data;
            _data.Clear();
        }

        public void AddData<T>(T value)
            where T : unmanaged
        {
            _data.AddRange(&value, sizeof(T));
        }

        public Reader AsReader() => new(in this);

        public ref struct Reader
        {
            private readonly byte* _data;
            private int _positionRead;

            public Reader(in CompactInstance compactInstance)
            {
                _data = compactInstance._data.GetUnsafePtr();
                _positionRead = 0;
            }

            internal void* ReadNext(int size)
            {
                var ptr = _data + _positionRead;
                _positionRead += size;
                return ptr;
            }
        }
    }
}