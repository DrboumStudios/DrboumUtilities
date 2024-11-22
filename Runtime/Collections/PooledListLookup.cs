using System;
using Unity.Collections;
using Unity.Jobs;

namespace Drboum.Utilities.Collections
{
    public struct PooledListLookup<TKey, TData> : INativeDisposable
        where TData : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
    {
        private NativeHashMap<TKey, NativeList<TData>> _collectionLookup;
        private NativeCollectionPool<NativeList<TData>, NativeListFactory<TData>> _collectionBufferPool;

        public PooledListLookup(int poolInitialCollectionCapacity, int lookupInitialCapacity, int initialCollectionInstanceCapacity, Allocator allocator)
        {
            var factory = new NativeListFactory<TData> {
                InitialInnerCapacity = initialCollectionInstanceCapacity,
                Allocator = allocator
            };
            _collectionBufferPool = new(poolInitialCollectionCapacity, allocator, in factory);
            _collectionLookup = new(lookupInitialCapacity, allocator);
        }

        public PooledListLookup(NativeCollectionPool<NativeList<TData>, NativeListFactory<TData>> sharedNativeCollectionPool, int lookupInitialCapacity, Allocator allocator)
        {
            _collectionBufferPool = sharedNativeCollectionPool;
            _collectionLookup = new(lookupInitialCapacity, allocator);
        }

        public bool IsEmpty => _collectionLookup.IsEmpty;

        public bool TryGetValue(in TKey key, out NativeList<TData> buffer)
        {
            return _collectionLookup.TryGetValue(key, out buffer);
        }

        /// <summary>
        /// try to get or give access to a collection if it does not exist for the current key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool GetOrCreateCollection(in TKey key, out NativeList<TData> list)
        {
            return _collectionLookup.TryGetValue(key, out list) || _collectionLookup.TryAdd(key, list = _collectionBufferPool.GetOrCreate());
        }

        public NativeKeyValueArrays<TKey, NativeList<TData>> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
        {
            return _collectionLookup.GetKeyValueArrays(allocator);
        }

        public void PreAllocatePoolCollectionInstances(int capacityTarget)
        {
            if ( capacityTarget > _collectionBufferPool.PoolCapacity )
            {
                _collectionBufferPool.PreAllocate(capacityTarget);
            }
        }

        public void RecycleSpawnDataBuffers()
        {
            foreach ( var dataList in _collectionLookup )
            {
                dataList.Value.Clear();
                _collectionBufferPool.Return(in dataList.Value);
            }
            _collectionLookup.Clear();
        }

        public void RemoveCollection(in TKey key)
        {
            if ( TryGetValue(in key, out var list) )
            {
                _collectionLookup.Remove(key);
                list.Clear();
                _collectionBufferPool.Return(list);
            }
        }

        public void Dispose()
        {
            _collectionLookup.Dispose();
            _collectionBufferPool.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return JobHandle.CombineDependencies(_collectionBufferPool.Dispose(inputDeps), _collectionLookup.Dispose(inputDeps));
        }
    }
}