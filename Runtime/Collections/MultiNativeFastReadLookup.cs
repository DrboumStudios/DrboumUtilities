using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Drboum.Utilities.Collections
{
    public unsafe struct MultiNativeFastReadLookup<TKey, TData1, TData2, TData3> : IMultiNativeFastReadLookup<TKey>
        where TKey : unmanaged, IEquatable<TKey>
        where TData1 : unmanaged
        where TData2 : unmanaged
        where TData3 : unmanaged
    {
        private const int DATAPROPERTIES_COUNT = 3;
        private MultiNativeFastReadLookup<TKey> _collection;

        public MultiNativeFastReadLookup(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            ReadOnlySpan<TypeDescriptor> typeDescriptors = stackalloc TypeDescriptor[DATAPROPERTIES_COUNT] {
                new TypeDescriptor(sizeof(TData1)),
                new TypeDescriptor(sizeof(TData2)),
                new TypeDescriptor(sizeof(TData3)),
            };
            _collection = new(typeDescriptors, initialCapacity, allocator);
        }

        public bool IsCreated => _collection.IsCreated;
        public int Length => _collection.Length;

        public int Capacity {
            get => _collection.Capacity;
            set => _collection.Capacity = value;
        }

        public bool TryGetValue(in TKey key, out TData1 value1, out TData2 value2, out TData3 value3)
        {
            value1 = default;
            value2 = default;
            value3 = default;
            return TryGetValueAsRef(in key, ref value1, ref value2, ref value3);
        }

        public bool TryGetValueAsRef(in TKey key, ref TData1 value1, ref TData2 value2, ref TData3 value3)
        {
            if ( _collection.TryGetValue(in key, out var elementIndex) )
            {
                value1 = _collection.ElementAt<TData1>(elementIndex, 0);
                value2 = _collection.ElementAt<TData2>(elementIndex, 1);
                value3 = _collection.ElementAt<TData3>(elementIndex, 2);
                return true;
            }
            return false;
        }

        public bool TryGetValue(in TKey key, out TData1 value)
        {
            return _collection.TryGetValue(in key, 0, out value);
        }

        public bool TryGetValue(in TKey key, out TData2 value)
        {
            return _collection.TryGetValue(in key, 1, out value);
        }

        public bool TryGetValue(in TKey key, out TData3 value)
        {
            return _collection.TryGetValue(in key, 2, out value);
        }

        public bool Contains(in TKey key)
        {
            return _collection.Contains(in key);
        }

        public void AssertArraysSizeMatch()
        {
            _collection.AssertArraysSizeMatch();
        }

        public void AddRangeFromZero(in NativeArray<TKey> keys, NativeArray<TData1> data1, NativeArray<TData2> data2, NativeArray<TData3> data3)
        {
            _collection.AddRangeFromZero(in keys, PackNativeArraysData(stackalloc NativeArray<byte>[DATAPROPERTIES_COUNT], ref data1, ref data2, ref data3));
        }

        public void AddRange(in NativeArray<TKey> keys, NativeArray<TData1> data1, NativeArray<TData2> data2, NativeArray<TData3> data3)
        {
            _collection.AddRange(in keys, PackNativeArraysData(stackalloc NativeArray<byte>[DATAPROPERTIES_COUNT], ref data1, ref data2, ref data3));
        }

        public void AddOrReplace(in TKey key, TData1 data1, TData2 data2, TData3 data3)
        {
            var compactData = stackalloc byte*[DATAPROPERTIES_COUNT];
            _collection.AddOrUpdate(in key, PackInstanceData(compactData, ref data1, ref data2, ref data3));
        }

        public bool TryAdd(in TKey key, TData1 data1, TData2 data2, TData3 data3)
        {
            var compactData = stackalloc byte*[DATAPROPERTIES_COUNT];
            return _collection.TryAdd(in key, PackInstanceData(compactData, ref data1, ref data2, ref data3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static InstanceData PackInstanceData(byte** compactData, ref TData1 data1, ref TData2 data2, ref TData3 data3)
        {
            compactData[0] = (byte*)UnsafeUtility.AddressOf(ref data1);
            compactData[1] = (byte*)UnsafeUtility.AddressOf(ref data2);
            compactData[2] = (byte*)UnsafeUtility.AddressOf(ref data3);
            return new InstanceData(compactData, DATAPROPERTIES_COUNT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<NativeArray<byte>> PackNativeArraysData(Span<NativeArray<byte>> allDatas, ref NativeArray<TData1> data1, ref NativeArray<TData2> data2, ref NativeArray<TData3> data3)
        {
            allDatas[0] = data1.Reinterpret<byte>(sizeof(TData1));
            allDatas[1] = data2.Reinterpret<byte>(sizeof(TData2));
            allDatas[2] = data3.Reinterpret<byte>(sizeof(TData3));
            return allDatas;
        }

        public bool TryRemove(in TKey key)
        {
            return _collection.TryRemove(in key);
        }

        public ReadOnlySpan<TKey> AsKeysArray()
        {
            return _collection.AsKeysArray();
        }

        public void AsValuesArray(out ReadOnlySpan<TData1> dataArray)
        {
            dataArray = _collection.AsValuesArray<TData1>(0);
        }

        public void AsValuesArray(out ReadOnlySpan<TData2> dataArray)
        {
            dataArray = _collection.AsValuesArray<TData2>(1);
        }

        public void AsValuesArray(out ReadOnlySpan<TData3> dataArray)
        {
            dataArray = _collection.AsValuesArray<TData3>(2);
        }

        public void AsValuesArray(out ReadOnlySpan<TData1> dataArray1, out ReadOnlySpan<TData2> dataArray2, out ReadOnlySpan<TData3> dataArray3)
        {
            dataArray1 = _collection.AsValuesArray<TData1>(0);
            dataArray2 = _collection.AsValuesArray<TData2>(1);
            dataArray3 = _collection.AsValuesArray<TData3>(2);
        }

        public void Dispose(JobHandle dependencies, NativeList<JobHandle> disposeHandles)
        {
            _collection.Dispose(dependencies, disposeHandles);
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public void Dispose()
        {
            _collection.Dispose();
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

        bool Contains(in TKey key);
        bool TryRemove(in TKey key);
        ReadOnlySpan<TKey> AsKeysArray();
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

        public bool TryGetValue(in TKey key, out int elementIndex)
        {
            return _indexLookup.TryGetValue(key, out elementIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue<TInstance>(in TKey key, int typeIndex, out TInstance value)
            where TInstance : unmanaged
        {
            if ( _indexLookup.TryGetValue(key, out var elementIndex) )
            {
                value = ElementAt<TInstance>(elementIndex, typeIndex);
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

        public ref TInstance ElementAt<TInstance>(int elementIndex, int typeIndex)
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

        [Conditional("UNITY_ASSERTIONS")]
        public void AssertArraysSizeMatch()
        {
            var values = _referencesValues[0];
            Assert.AreEqual(_referencesKeys.Length, values.Collection.Length / values.Type.Size, "[PANIC] the length of values and keys collections are different.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in TKey key, InstanceData instanceData)
        {
            if ( _indexLookup.TryAdd(key, _referencesKeys.Length) )
            {
                AddCollectionsNoCheckImpl(key, instanceData);

                return true;
            }
            return false;
        }

        private void AddCollectionsNoCheckImpl(in TKey key, InstanceData instanceData)
        {
            CollectionCustomHelper.CheckEstimatedSizeMatchActualSize(instanceData.PropertiesCount, _referencesValues.Length);
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                byte* inputData = instanceData.Read(index);
                typeValues.Collection.AddRange(inputData, typeValues.Type.Size);
            }

            _referencesKeys.Add(in key);
            AssertArraysSizeMatch();
        }

        public void AddRangeFromZero(in NativeArray<TKey> keys, in ReadOnlySpan<NativeArray<byte>> instancesData)
        {
            Assert.IsTrue(_referencesKeys.IsEmpty);
            for ( int i = 0; i < keys.Length; i++ )
            {
                _indexLookup.TryAdd(keys[i], i);
            }
            AddRangeAfterIndexAddImpl(keys, instancesData);
            AssertArraysSizeMatch();
        }

        public void AddRange(in NativeArray<TKey> keys, in ReadOnlySpan<NativeArray<byte>> instances)
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
            AddRangeAfterIndexAddImpl(keys, instances);
            AssertArraysSizeMatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddRangeAfterIndexAddImpl(NativeArray<TKey> keys, ReadOnlySpan<NativeArray<byte>> instances)
        {
            _referencesKeys.AddRange(keys);
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                typeValues.Collection.AddRange(instances[index]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddOrUpdate(in TKey key, InstanceData instanceData)
        {
            int findMapIndex = _indexLookup.FindMapIndex(in key);
            if ( findMapIndex != -1 )
            {
                int elementIndex = _indexLookup.GetValueFromMapIndex(findMapIndex);
                for ( var index = 0; index < _referencesValues.Length; index++ )
                {
                    ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                    UnsafeUtility.MemCpy(typeValues.Collection.GetUnsafePtr() + (elementIndex * typeValues.Type.Size), instanceData.Read(index), typeValues.Type.Size);
                }
            }
            else
            {
                _indexLookup.AddNoExistCheckImpl(in key, _referencesKeys.Length);
                AddCollectionsNoCheckImpl(in key, instanceData);
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

        public void Clear()
        {
            for ( var index = 0; index < _referencesValues.Length; index++ )
            {
                ref var typeValues = ref _referencesValues.ReadElementAsRef(index);
                typeValues.Collection.Clear();
            }
            _referencesKeys.Clear();
            _indexLookup.Clear();
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

    public readonly unsafe ref struct InstanceData
    {
        private readonly byte** _data;

        public int PropertiesCount {
            get;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InstanceData(byte** data, int propertiesCount)
        {
            _data = data;
            PropertiesCount = propertiesCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* Read(int index)
        {
            return _data[index];
        }
    }
}