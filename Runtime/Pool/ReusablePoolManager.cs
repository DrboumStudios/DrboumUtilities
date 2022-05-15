using System.Collections.Generic;
using Drboum.Utilities.Runtime.Interfaces;
using UnityEngine;
namespace Drboum.Utilities.Runtime.Pool {
    public class ReusablePoolManager {
        private readonly Dictionary<int, Pool<IReusableInstance>> _objectPools;
        public ReusablePoolManager()
        {
            _objectPools = new Dictionary<int, Pool<IReusableInstance>>();
        }
        public ReusablePoolManager(int initialCapacity)
        {
            _objectPools = new Dictionary<int, Pool<IReusableInstance>>(initialCapacity);
        }

        public T GetInstanceOf<T>() where T : class, IReusableInstance, new()
        {
            int typeHash = GetTypeId<T>();
            if ( !_objectPools.TryGetValue(typeHash, out Pool<IReusableInstance> poolobj) ) {
                poolobj = CreateNewPoolInternal<T>(typeHash, MemoryManager.DefaultSettings);
            }
            IReusableInstance obj = poolobj.GetInstance();
            obj.InitReusableInstanceState();
            return obj as T;
        }
        public void CreateNewPool<T>(PoolSettings settings) where T : class, IReusableInstance, new()
        {
            int typeId = GetTypeId<T>();
            if ( _objectPools.ContainsKey(typeId) ) {
                string typetext = _objectPools[typeId].FreeObjectCount > 0
                    ? "by the type " + _objectPools[typeId].ConcreteType?.Name : "";
                Debug.LogWarning(
                    $" A pool of type {typeof(T)} with the same key has already been added {typetext}. Key: {typeId}");
                return;
            }

            CreateNewPoolInternal<T>(typeId, settings);
        }
        private Pool<IReusableInstance> CreateNewPoolInternal<T>(int typeId, PoolSettings settings)
            where T : class, IReusableInstance, new()
        {
            var objPool = new Pool<IReusableInstance>(settings);
            objPool.SetInstanceCreator(new InstanceCreator<T, IReusableInstance>(), typeof(T).Name, GetTypeId<T>());
            objPool.FillNewPool();

            _objectPools.Add(typeId, objPool);
            return objPool;
        }
        public void ReleaseInstanceOf<T>(T instance) where T : class, IReusableInstance, new()
        {
            if ( _objectPools.TryGetValue(GetTypeId<T>(), out Pool<IReusableInstance> objQueue) ) {
                objQueue.Release(instance);
                instance?.ResetReusableInstanceState();
            }
        }

        private static int GetTypeId<T>() where T : class, IReusableInstance, new()
        {
            return typeof(T).GetHashCode();
        }
    }
}