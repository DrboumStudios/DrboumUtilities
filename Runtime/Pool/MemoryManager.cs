using System.Collections.Generic;
using Drboum.Utilities.Runtime.Interfaces;
using UnityEngine;
namespace Drboum.Utilities.Runtime.Pool {
    public class MemoryManager : IMemoryManager {
        private const int DefaultPoolLength = 5;
        public static readonly PoolSettings DefaultSettings = new PoolSettings {
            ExpandFactor        = 2,
            InitialPoolCapacity = DefaultPoolLength * 4,
            PoolLength          = DefaultPoolLength,
            ExpandOffset        = 1
        };
        private readonly Dictionary<int, Pool<Object>> _unityObjectPools;
        public MemoryManager()
        {
            ReusablePoolManager = new ReusablePoolManager();
            _unityObjectPools   = new Dictionary<int, Pool<Object>>();
        }
        public MemoryManager(int unityPoolObjectCapacity, int reusablePoolObjectCapacity)
        {
            _unityObjectPools   = new Dictionary<int, Pool<Object>>(unityPoolObjectCapacity);
            ReusablePoolManager = new ReusablePoolManager(reusablePoolObjectCapacity);
        }
        public static bool                DebugLog            { get; set; } = true;
        public        Transform           PoolParentTransform { get; set; }
        public        ReusablePoolManager ReusablePoolManager { get; }

        public T GetInstanceOf<T>(T prefab) where T : Object
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(GetInstanceOf));
                return null;
            }
            Object obj;
            int    instanceId = prefab.GetInstanceID();
            if ( _unityObjectPools.TryGetValue(instanceId, out Pool<Object> poolObj) ) {
                obj = poolObj.GetInstance();
                if ( obj == null ) {
                    Debug.LogError(
                        $"{nameof(MemoryManager)}.{nameof(GetInstanceOf)}({nameof(prefab)} {prefab.name}) is null after being pulled from an existing objectpool");
                }
            }
            else {
                poolObj = CreateNewPoolInternal(prefab, instanceId, DefaultSettings);
                obj     = poolObj.GetInstance();
                if ( obj == null ) {
                    Debug.LogError(
                        $"{nameof(MemoryManager)}.{nameof(GetInstanceOf)}({nameof(prefab)} {prefab.name}) is null after being pulled from an freshly created objectpool");
                }
            }

            return obj as T;
        }
        public GameObject GetInstanceOf(GameObject prefab)
        {
            var obj = GetInstanceOf<GameObject>(prefab);
            obj.transform.SetParent(null, false);
            SetActive(true, obj);
            return obj;
        }
        public GameObject GetInstanceOf(GameObject prefab, Transform parent, bool setActive = true,
            bool                                   worldPositionStays = false)
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(GetInstanceOf));
                return null;
            }
            return GetInstanceOf(prefab, prefab.transform.position, prefab.transform.rotation, parent, setActive,
                worldPositionStays);
        }
        public GameObject GetInstanceOf(GameObject prefab, Vector3 position, bool setActive = true)
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(GetInstanceOf));
                return null;
            }
            return GetInstanceOf(prefab, position, prefab.transform.rotation, setActive);
        }
        public GameObject GetInstanceOf(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(GetInstanceOf));
                return null;
            }

            return GetInstanceOf(prefab, position, rotation, null, setActive);
        }
        public GameObject GetInstanceOf(GameObject prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                                   setActive = true, bool    worldPositionStays = false)
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(GetInstanceOf));
                return null;
            }
            GameObject obj = GetInstanceOf(prefab);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent, worldPositionStays);
            SetActive(setActive, obj);
            return obj;
        }

        public T GetInstanceOf<T>(GameObject prefab, bool setActive = true) where T : Component, IEnableInstance
        {
            GameObject instance = GetInstanceOf(prefab);
            if ( instance == null ) {
                return null;
            }

            var component = EnableInstance<T>(instance);
            SetActive(setActive, instance);
            return component;
        }

        public T GetInstanceOf<T>(GameObject prefab, Transform parent, bool setActive = true,
            bool                             worldPositionStays = false) where T : Component, IEnableInstance
        {
            GameObject obj = GetInstanceOf(prefab, parent, setActive, worldPositionStays);
            if ( obj == null ) {
                return null;
            }

            return EnableInstance<T>(obj);
        }
        public T GetInstanceOf<T>(GameObject prefab, Vector3 position, bool setActive = true)
            where T : Component, IEnableInstance
        {
            GameObject obj = GetInstanceOf(prefab, position, setActive);
            if ( obj == null ) {
                return null;
            }

            return EnableInstance<T>(obj);
        }
        public T GetInstanceOf<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
            where T : Component, IEnableInstance
        {
            GameObject obj = GetInstanceOf(prefab, position, rotation, null, setActive);
            if ( obj == null ) {
                return null;
            }

            return EnableInstance<T>(obj);
        }
        public T GetInstanceOf<T>(GameObject prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                             setActive = true, bool    worldPositionStays = false)
            where T : Component, IEnableInstance
        {
            GameObject obj = GetInstanceOf(prefab, position, rotation, parent, setActive, worldPositionStays);
            if ( obj == null ) {
                return null;
            }

            return EnableInstance<T>(obj);
        }
        public T GetInstanceOfComponent<T>(T prefab, bool setActive = true) where T : Component
        {
            return GetInstanceOfComponent<T>(prefab.gameObject, setActive);
        }
        public T GetInstanceOfComponent<T>(GameObject prefab, bool setActive = true) where T : Component
        {
            GameObject obj = GetInstanceOf(prefab);
            if ( obj == null ) {
                return null;
            }
            obj.SetActive(setActive);
            var component = obj.GetComponent<T>();
            LogIfComponentIsNull(prefab, component);
            return component;
        }
        public T GetInstanceOfComponent<T>(T prefab, Transform parent, bool setActive = true,
            bool                             worldPositionStays = false) where T : Component
        {
            return GetInstanceOfComponent<T>(prefab.gameObject, parent, setActive, worldPositionStays);
        }
        public T GetInstanceOfComponent<T>(GameObject prefab, Transform parent, bool setActive = true,
            bool                                      worldPositionStays = false) where T : Component
        {
            GameObject obj = GetInstanceOf(prefab, parent, setActive, worldPositionStays);
            if ( obj == null ) {
                return null;
            }

            var component = obj.GetComponent<T>();
            LogIfComponentIsNull(prefab, component);
            return component;
        }
        public T GetInstanceOfComponent<T>(T prefab, Vector3 position, Quaternion rotation, bool setActive = true)
            where T : Component
        {
            return GetInstanceOfComponent<T>(prefab.gameObject, position, rotation, setActive);
        }
        public T GetInstanceOfComponent<T>(GameObject prefab, Vector3 position, Quaternion rotation,
            bool                                      setActive = true) where T : Component
        {
            GameObject obj = GetInstanceOf(prefab, position, rotation, setActive);
            if ( obj == null ) {
                return null;
            }

            var component = obj.GetComponent<T>();
            LogIfComponentIsNull(prefab, component);
            return component;
        }
        public T GetInstanceOfComponent<T>(T prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                             setActive = true, bool    worldPositionStays = false)
            where T : Component
        {
            return GetInstanceOfComponent<T>(prefab.gameObject, position, rotation, parent, setActive,
                worldPositionStays);
        }
        public T GetInstanceOfComponent<T>(GameObject prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                                      setActive = true, bool    worldPositionStays = false)
            where T : Component
        {
            GameObject obj = GetInstanceOf(prefab, position, rotation, parent, setActive, worldPositionStays);
            if ( obj == null ) {
                return null;
            }

            var component = obj.GetComponent<T>();
            LogIfComponentIsNull(prefab, component);
            return component;
        }

        public void ReleaseInstanceOf<T>(T prefab, T instance) where T : Object
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(ReleaseInstanceOf));
                return;
            }
            if ( instance == null ) {
                LogNullParameterErrorMessage(nameof(instance), nameof(ReleaseInstanceOf));
                return;
            }

            int instanceId = prefab.GetInstanceID();
            if ( _unityObjectPools.TryGetValue(instanceId, out Pool<Object> objQueue) ) {
                objQueue.Release(instance);
            }
            else {
                var          settings = PoolSettings.Default;
                Pool<Object> poolObj  = CreateNewPoolInternal(prefab, instanceId, settings);
                Debug.LogWarning(
                    $"The parameter {nameof(prefab)} '{prefab.name}' is not referenced by the memory {nameof(MemoryManager)}, because this is not an efficient use of a pool, consider creating a pool ahead of time with a size that never run out of instances");
                LogExpansion<T>(prefab.name, instanceId, 0, poolObj.FreeObjectCount);
                poolObj.Release(instance);
            }
        }
        public void ReleaseInstanceOf(GameObject prefab, GameObject instance)
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(ReleaseInstanceOf),
                    instance != null ? $"instance.name= {instance.name}" : null);
                return;
            }

            ReleaseInstanceOf(prefab, instance, prefab.transform.position, prefab.transform.rotation);
        }
        public void ReleaseInstanceOf<T>(GameObject prefab, T instance) where T : Component, IDisableInstance
        {
            if ( instance == null ) {
                LogNullParameterErrorMessage(nameof(instance), nameof(ReleaseInstanceOf) + $"<{typeof(T)}>",
                    "from IDisableInstance");
                return;
            }

            instance.ResetReusableInstanceState();
            ReleaseInstanceOf(prefab, instance.gameObject);
        }
        public void CreateNewPool<T>(T prefab, PoolSettings settings) where T : Object
        {
            int id = prefab.GetInstanceID();
            if ( !_unityObjectPools.ContainsKey(id) ) {
                CreateNewPoolInternal(prefab, id, settings);
            }
        }
        private static T EnableInstance<T>(GameObject instance) where T : Component, IEnableInstance
        {
            var lifeObj = instance.GetComponent<T>();
            if ( lifeObj == null ) {
                LogComponentIsNull(instance);
                return null;
            }
            lifeObj.InitReusableInstanceState();
            return lifeObj;
        }
        private void ReleaseInstanceOf(GameObject prefab, GameObject instance, Vector3 position, Quaternion rotation)
        {
            if ( prefab == null ) {
                LogNullParameterErrorMessage(nameof(prefab), nameof(ReleaseInstanceOf));
                return;
            }
            if ( instance == null ) {
                LogNullParameterErrorMessage(nameof(instance), nameof(ReleaseInstanceOf));
                return;
            }

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(PoolParentTransform, false);
            instanceTransform.position   = position;
            instanceTransform.rotation   = rotation;
            instanceTransform.localScale = prefab.transform.localScale;
            instance.SetActive(false);
            ReleaseInstanceOf<Object>(prefab, instance);
        }
        private Pool<Object> CreateNewPoolInternal<T>(T prefab, int instanceId, PoolSettings settings) where T : Object
        {
            Pool<Object> objPool;
            if ( prefab is GameObject prefabGameObject ) {
                objPool = new Pool<Object>(settings);
                objPool.SetInstanceCreator(new GameObjectInstanceCreator(prefabGameObject, PoolParentTransform),
                    prefab.name, instanceId);
            }
            else {
                objPool = new Pool<Object>(settings);
                objPool.SetInstanceCreator(new UObjectInstanceCreator<Object>(prefab), prefab.name, instanceId);
            }
            objPool.FillNewPool();

            _unityObjectPools.Add(instanceId, objPool);
            return objPool;
        }
        public static void LogExpansion<T>(string name, int Id, int Count, int newCount)
        {
            if ( DebugLog ) {
                Debug.LogWarning(
                    $"Pool expanded from {Count} to {newCount}, objectInfo=> prefabGameobjectID: [{Id}] prefabName: '{name}'");
            }
        }
        private static void LogIfComponentIsNull(GameObject prefab, Component component)
        {
            if ( component == null ) {
                LogComponentIsNull(prefab);
            }
        }
        private static void LogNullParameterErrorMessage(string parametername, string methodName,
            string                                              additionalMessage = null)
        {
            LogHelper.LogNullParameterErrorMessage(parametername, methodName, additionalMessage);
        }
        private static void LogComponentIsNull(GameObject prefab)
        {
            if ( DebugLog ) {
                Debug.LogError(
                    $"[{nameof(MemoryManager)}.{nameof(GetInstanceOfComponent)}] the {prefab.name} does not contains the requested component");
            }
        }
        private static void SetActive(bool setActive, GameObject obj)
        {
            if ( setActive ) {
                obj.SetActive(true);
            }
        }
    }
}