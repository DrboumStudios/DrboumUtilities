using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Drboum.Utilities.Collections
{
    [DebuggerDisplay("Length = {_container.Length}")]
    public unsafe struct AggregatedMapLookup<TKey, TElement> : INativeDisposable
        where TElement : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
    {
        private static readonly int _MaximumAllowedElementCapacity = int.MaxValue / sizeof(TElement);
        
        private NativeList<NativeHashMap<TKey, TElement>> _container;
        private NativeReference<int> _freeContainerIndex;
        private readonly int _initialInnerCapacity;
        private readonly Allocator _allocator;

        public bool IsCreated => _container.IsCreated;

        public AggregatedMapLookup(uint innerInitialCapacity, Allocator allocator, uint rootContainerInitialCapacity = 2)
            : this()
        {
            _container = new((int)rootContainerInitialCapacity, allocator);
            _initialInnerCapacity = (int)innerInitialCapacity;
            _allocator = allocator;
            _freeContainerIndex = new NativeReference<int>(0, allocator);
            _container.Add(CreateNewCollection());
        }


        public bool TryGetValue(in TKey index, out TElement element)
        {
            for ( int i = 0; i < _container.Length; i++ )
            {
                if ( _container[i].TryGetValue(index, out element) )
                {
                    return true;
                }
            }

            element = default;
            return false;
        }

        public bool ContainsKey(in TKey index)
        {
            return FindCollectionIndexOfKey(index) != -1;
        }

        private int FindCollectionIndexOfKey(in TKey index)
        {
            for ( int i = 0; i < _container.Length; i++ )
            {
                if ( _container[i].ContainsKey(index) )
                {
                    return i;
                }
            }
            return -1;
        }

        public TElement this[in TKey key] {
            get {
                TryGetValue(in key, out var element);
                return element;
            }
            set {
                int findIndexOfKey = FindCollectionIndexOfKey(in key);
                if ( findIndexOfKey != -1 )
                {
                    var map = _container[findIndexOfKey];
                    map[key] = value;
                    return;
                }
                Add(in key, in value);
            }
        }

        public NativeArray<NativeKeyValueArrays<TKey, TElement>> ToNativeKeyValueArrays(Allocator allocator)
        {
            var array = new NativeArray<NativeKeyValueArrays<TKey, TElement>>(_container.Length, allocator);
            for ( var index = 0; index < _container.Length; index++ )
            {
                NativeHashMap<TKey, TElement> map = _container[index];
                array[index] = map.GetKeyValueArrays(allocator);
            }
            return array;
        }

        public NativeArray<NativeArray<TElement>> ToNativeValueArrays(Allocator allocator)
        {
            var array = new NativeArray<NativeArray<TElement>>(_container.Length, allocator);
            for ( var index = 0; index < _container.Length; index++ )
            {
                NativeHashMap<TKey, TElement> map = _container[index];
                array[index] = map.GetValueArray(allocator);
            }
            return array;
        }

        public bool TryAdd(in TKey index, in TElement element)
        {
            int indexOfKey = FindCollectionIndexOfKey(in index);
            if ( indexOfKey != -1 )
                return false;

            return Add(index, element);
        }

        private bool Add(in TKey index, in TElement element)
        {
            
            var lastContainer = _container[_freeContainerIndex.Value];
            while ( lastContainer.Count + 1 >= _MaximumAllowedElementCapacity )
            {
                if ( _freeContainerIndex.Value + 1 >= _container.Length )
                {
                    _container.Add(CreateNewCollection());
                }
                lastContainer = _container[++_freeContainerIndex.Value];
            }
           
            return lastContainer.TryAdd(index, element) ;
        }

        private NativeHashMap<TKey, TElement> CreateNewCollection()
        {
            return new NativeHashMap<TKey, TElement>(_initialInnerCapacity, _allocator);
        }

        public void Remove(TKey index)
        {
            for ( int i = 0; i < _container.Length; i++ )
            {
                if ( _container[i].Remove(index) )
                {
                    _freeContainerIndex.Value = math.min(i, _freeContainerIndex.Value);
                    return;
                }
            }
        }

        public void Dispose()
        {
            if ( !_container.IsCreated )
                return;

            for ( var index = 0; index < _container.Length; index++ )
            {
                _container.ElementAt(index).Dispose();
            }
            _container.Dispose();
            _freeContainerIndex.Dispose();
        }

        public JobHandle Dispose(JobHandle jobDependency)
        {
            if ( !IsCreated )
                return jobDependency;

            var dependencyList = new NativeArray<JobHandle>(_container.Length + 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for ( var index = 0; index < _container.Length; index++ )
            {
                dependencyList[index] = _container.ElementAt(index).Dispose(jobDependency);
            }

            dependencyList[_container.Length] = _container.Dispose(jobDependency);
            dependencyList[_container.Length+1] = _freeContainerIndex.Dispose(jobDependency);
            return JobHandle.CombineDependencies(dependencyList);
        }
    }
}