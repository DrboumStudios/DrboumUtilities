using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Drboum.Utilities.Runtime.Pool {
    public class Pool<T> where T : class {
        private readonly Stack<T>            _objects;
        private          IInstanceCreator<T> _instanceCreator;
        private          PoolSettings        _poolSettings;
        public Pool(PoolSettings poolSettings)
        {
            _poolSettings = poolSettings;
            _objects      = new Stack<T>(_poolSettings.InitialPoolCapacity);
        }
        public Type         ConcreteType    => _instanceCreator?.ConcreteType;
        public PoolSettings Settings        => _poolSettings;
        public int          FreeObjectCount => _objects.Count;
        public bool         IsEmpty         => _objects.Count == 0;
        public void SetInstanceCreator<U>(U creator, string poolName, int poolId = -1)
            where U : struct, IInstanceCreator<T>
        {
#if UNITY_EDITOR || DEBUG
            _poolName = poolName;
            _poolId   = poolId;
#endif
            _instanceCreator = creator;
        }

        public T GetInstance()
        {
            if ( IsEmpty ) {
                int oldCount = FreeObjectCount;
                ExpandPool();
#if UNITY_EDITOR || DEBUG
                MemoryManager.LogExpansion<T>(_poolName, _poolId, oldCount, FreeObjectCount);
#endif
            }
            return _objects.Pop();
        }

        public void Release(T instance)
        {
            if ( instance == null ) {
                Debug.LogError(
                    $"[{nameof(Pool<T>)}.{nameof(Release)}] the parameter {nameof(instance)} of type {typeof(T).Name} is null");
                return;
            }
            _objects.Push(instance);
        }

        internal T Peek()
        {
            if ( _objects.Count == 0 ) {
                return null;
            }

            return _objects.Peek();
        }
        private void ExpandPoolSize()
        {
            _poolSettings.PoolLength =
                (_poolSettings.PoolLength + _poolSettings.ExpandOffset) * _poolSettings.ExpandFactor;
        }

        public void FillNewPool()
        {
            for ( var i = 0; i < _poolSettings.PoolLength; i++ ) {
                InstantiateNew();
            }
        }

        private void ExpandPool(uint length)
        {
            for ( var i = 0; i < length; i++ ) {
                InstantiateNew();
            }
        }

        public void ExpandPool()
        {
            if ( _poolSettings.PoolLength == 0 ) {
                _poolSettings.PoolLength = 1;
            }
            ExpandPool(_poolSettings.PoolLength);
            ExpandPoolSize();
        }

        public T InstantiateNew()
        {
            T newObj = _instanceCreator.Instantiate();
            _objects.Push(newObj);
            return newObj;
        }
#if UNITY_EDITOR||DEBUG
        private string _poolName = typeof(T).Name;
        private int    _poolId   = 1;
#endif
    }
    public interface IInstanceCreator<T> {
        Type ConcreteType { get; }
        T    Instantiate();
    }
    public struct GameObjectInstanceCreator : IInstanceCreator<Object> {

        public GameObject Prototype  { get; }
        public Transform  PoolParent { get; }

        public Type ConcreteType => typeof(GameObject);

        public GameObjectInstanceCreator(GameObject prototype, Transform poolParent)
        {
            Prototype  = prototype;
            PoolParent = poolParent;
        }

        public Object Instantiate()
        {
            GameObject gobj = Object.Instantiate(Prototype, PoolParent);
            gobj.SetActive(false);
            return gobj;
        }
    }

    public struct UObjectInstanceCreator<T> : IInstanceCreator<T> where T : Object {
        public Type ConcreteType => typeof(T);
        public UObjectInstanceCreator(T prototype)
        {
            Prototype = prototype;
        }

        public T Prototype { get; }
        public T Instantiate()
        {
            return Object.Instantiate(Prototype);
        }
    }
    public struct InstanceCreator<T> : IInstanceCreator<T> where T : new() {
        public Type ConcreteType => typeof(T);
        public T Instantiate()
        {
            return new T();
        }
    }
    public struct ArrayInstanceCreator<T> : IInstanceCreator<T[]> {
        public Type ConcreteType => typeof(T[]);
        public long Length;
        public T[] Instantiate()
        {
            return new T[Length];
        }
    }
    public struct InstanceCreator<T, ReturnType> : IInstanceCreator<ReturnType> where T : class, ReturnType, new() {
        public Type ConcreteType => typeof(T);
        public ReturnType Instantiate()
        {
            return new T();
        }
    }
    public struct PoolSettings {
        public uint PoolLength;
        public int  InitialPoolCapacity;
        public uint ExpandFactor;
        public uint ExpandOffset;

        public static readonly PoolSettings Default = new PoolSettings {
            ExpandFactor        = 2,
            InitialPoolCapacity = 5,
            ExpandOffset        = 2
        };
    }
}