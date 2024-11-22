using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Drboum.Utilities.Collections
{
    public interface ITypeNameBurstProvider
    {
        FixedString128Bytes TypeName {
            get;
        }
    }

    public interface INativeDisposableWrapper<TDisposable>
        where TDisposable : unmanaged
    {
        public void Dispose(ref TDisposable disposable);

        public JobHandle Dispose(ref TDisposable disposable, in JobHandle inputDependencies);
    }

    public interface IFactory<out T> : ITypeNameBurstProvider
    {
        public T CreateNew();
    }

    public unsafe struct NativeCollectionPool<TNativeCollection, TFactory> : INativeDisposable
        where TNativeCollection : unmanaged
        where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>, INativeDisposableWrapper<TNativeCollection>
    {
        private NativeCollectionPool<TNativeCollection> _nativeCollectionPoolImpl;
        private readonly TFactory _factory;

        public NativeCollectionPool(int initialCapacity, Allocator allocator, in TFactory factory = default)
        {
            _nativeCollectionPoolImpl = new(initialCapacity, allocator);
            _factory = factory;
        }

        public int PoolCapacity => _nativeCollectionPoolImpl.PoolCapacity;

        public void PreAllocate()
        {
            _nativeCollectionPoolImpl.PreAllocate(in _factory);
        }

        public void PreAllocate(int count)
        {
            _nativeCollectionPoolImpl.PreAllocate(count, in _factory);
        }


        public TNativeCollection GetOrThrow()
        {
            return _nativeCollectionPoolImpl.GetOrThrow(_factory.TypeName);
        }

        public TNativeCollection GetOrCreate()
        {
            return _nativeCollectionPoolImpl.GetOrCreate(in _factory);
        }

        public void Return(in TNativeCollection nativeContainer)
        {
            _nativeCollectionPoolImpl.Return(in nativeContainer, in _factory);
        }

        public NativeCollectionPool<TNativeCollection>.ReadOnly AsReadOnly() => new(_nativeCollectionPoolImpl, _factory.TypeName);

        public void Dispose()
        {
            _nativeCollectionPoolImpl.Dispose(in _factory);
        }

        public JobHandle Dispose(JobHandle inputDependencies)
        {
            return _nativeCollectionPoolImpl.Dispose(in _factory, in inputDependencies);
        }
    }

    public unsafe struct NativeCollectionPool<TNativeCollection>
        where TNativeCollection : unmanaged
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeList<TNativeCollection> _collectionCache;

        [NativeDisableContainerSafetyRestriction]
        private NativeQueue<TNativeCollection> _freeCollectionList;

        public int PoolCapacity => _collectionCache.Length;

        public NativeCollectionPool(int initialCapacity, Allocator allocator)
        {
            _collectionCache = new(initialCapacity, allocator);
            _freeCollectionList = new(allocator);
        }

        public void PreAllocate<TFactory>(in TFactory factory)
            where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>
        {
            PreAllocate(_collectionCache.Capacity, in factory);
        }

        public void PreAllocate<TFactory>(int count, in TFactory factory)
            where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>
        {
            for ( int i = 0; i < count; i++ )
            {
                CreateNewInstance(in factory);
            }
        }

        private void CreateNewInstance<TFactory>(in TFactory factory)
            where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>
        {
            _freeCollectionList.Enqueue(CreateAndRegisterToCache(in factory));
        }

        private TNativeCollection CreateAndRegisterToCache<TFactory>(in TFactory factory)
            where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>
        {
            var newCollection = factory.CreateNew();
            _collectionCache.Add(in newCollection);
            return newCollection;
        }

        public TNativeCollection GetOrThrow(in FixedString128Bytes collectionTypeNameProvider)
        {
            if ( !_freeCollectionList.TryDequeue(out var container) )
            {
                Debug.LogError($"Cannot Allocate a {collectionTypeNameProvider} using this method.");
            }
            return container;
        }

        public TNativeCollection GetOrCreate<TFactory>(in TFactory factory)
            where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>
        {
            if ( !_freeCollectionList.TryDequeue(out var container) )
            {
                container = CreateAndRegisterToCache(in factory);
            }
            return container;
        }

        public void Return<TFactory>(in TNativeCollection nativeContainer, in TFactory factory)
            where TFactory : unmanaged, IFactory<TNativeCollection>, IEqualityComparer<TNativeCollection>
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var isOwnedByThePool = false;
            for ( int i = 0; i < _collectionCache.Length && !isOwnedByThePool; i++ )
            {
                var collection = _collectionCache[i];
                isOwnedByThePool = factory.Equals(collection, nativeContainer);
            }

            if ( isOwnedByThePool )
            {
                _freeCollectionList.Enqueue(nativeContainer);
            }
            else
            {
                Debug.LogError($"this collection is not owned by the pool and cannot be returned");
            }
#else
            _freeCollectionList.Enqueue(nativeContainer);
#endif
        }

        public void Dispose<TFactory>(in TFactory factory)
            where TFactory : unmanaged, INativeDisposableWrapper<TNativeCollection>
        {
            if ( _collectionCache.IsCreated )
            {
                for ( var index = 0; index < _collectionCache.Length; index++ )
                {
                    factory.Dispose(ref _collectionCache.ElementAt(index));
                }
                _collectionCache.Dispose();
            }
            _freeCollectionList.Dispose();
        }

        public JobHandle Dispose<TFactory>(in TFactory factory, in JobHandle inputDependencies)
            where TFactory : unmanaged, INativeDisposableWrapper<TNativeCollection>
        {
            using var dependencyList = new NativeList<JobHandle>(Allocator.Temp);
            if ( _collectionCache.IsCreated )
            {
                for ( var index = 0; index < _collectionCache.Length; index++ )
                {
                    ref TNativeCollection nativeContainer = ref _collectionCache.ElementAt(index);
                    dependencyList.Add(factory.Dispose(ref nativeContainer, in inputDependencies));
                }
            }

            dependencyList.Add(_collectionCache.Dispose(inputDependencies));
            dependencyList.Add(_freeCollectionList.Dispose(inputDependencies));
            return JobHandle.CombineDependencies(dependencyList.AsArray());
        }

        /// <summary>
        /// Readonly as in, it prevent creating new instance
        /// </summary>
        public readonly struct ReadOnly
        {
            private readonly NativeCollectionPool<TNativeCollection> _nativeCollectionPool;
            private readonly FixedString128Bytes _collectionTypeName;

            public ReadOnly(NativeCollectionPool<TNativeCollection> nativeCollectionPool, in FixedString128Bytes collectionTypeName)
            {
                _nativeCollectionPool = nativeCollectionPool;
                _collectionTypeName = collectionTypeName;
            }

            public TNativeCollection GetOrThrow()
            {
                return _nativeCollectionPool.GetOrThrow(in _collectionTypeName);
            }
        }
    }
}