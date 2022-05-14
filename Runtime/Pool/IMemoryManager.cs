using DrboumLibrary.Interfaces;
using UnityEngine;
namespace DrboumLibrary.Pool {

    public interface IMemoryManager {
        ReusablePoolManager ReusablePoolManager { get; }
        GameObject          GetInstanceOf(GameObject prefab);
        GameObject GetInstanceOf(GameObject prefab, Transform parent, bool setActive = true,
            bool                            staysWorldPosition = false);
        GameObject GetInstanceOf(GameObject prefab, Vector3 position, bool       setActive               = true);
        GameObject GetInstanceOf(GameObject prefab, Vector3 position, Quaternion rotation, bool activate = true);
        GameObject GetInstanceOf(GameObject prefab,          Vector3 position, Quaternion rotation, Transform parent,
            bool                            activate = true, bool    worldPositionStays = false);
        /// <summary>
        ///     initialize an instance to its run state.
        /// </summary>
        /// <typeparam name="T">
        ///     the type to lookup for enabling this instance, for enabling a gameobject use a
        ///     <see cref="IHandleGameObjectLifeCycle" /> type specifically
        /// </typeparam>
        /// <param name="prefab"></param>
        /// <param name="setActive"></param>
        /// <returns></returns>
        T GetInstanceOf<T>(GameObject prefab, bool setActive = true) where T : Component, IEnableInstance;
        T GetInstanceOf<T>(T                   prefab) where T : Object;
        T GetInstanceOfComponent<T>(T          prefab, bool setActive = true) where T : Component;
        T GetInstanceOfComponent<T>(GameObject prefab, bool setActive = true) where T : Component;
        T GetInstanceOfComponent<T>(T prefab, Transform parent, bool setActive = true, bool worldPositionStays = false)
            where T : Component;
        T GetInstanceOfComponent<T>(GameObject prefab, Transform parent, bool setActive = true,
            bool                               worldPositionStays = false) where T : Component;
        T GetInstanceOfComponent<T>(T prefab, Vector3 position, Quaternion rotation, bool setActive = true)
            where T : Component;
        T GetInstanceOfComponent<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
            where T : Component;
        T GetInstanceOfComponent<T>(T prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                      setActive = true, bool    worldPositionStays = false) where T : Component;
        T GetInstanceOfComponent<T>(GameObject prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                               setActive = true, bool    worldPositionStays = false) where T : Component;
        void        ReleaseInstanceOf(GameObject    prefab, GameObject   instance);
        void        ReleaseInstanceOf<T>(T          prefab, T            instance) where T : Object;
        void        ReleaseInstanceOf<T>(GameObject prefab, T            instance) where T : Component, IDisableInstance;
        public void CreateNewPool<T>(T              prefab, PoolSettings settings) where T : Object;
        T GetInstanceOf<T>(GameObject prefab,           Vector3 position, Quaternion rotation, Transform parent,
            bool                      setActive = true, bool    worldPositionStays = false) where T : Component, IEnableInstance;
        T GetInstanceOf<T>(GameObject prefab, Transform parent, bool setActive = true, bool worldPositionStays = false)
            where T : Component, IEnableInstance;
        T GetInstanceOf<T>(GameObject prefab, Vector3 position, bool setActive = true)
            where T : Component, IEnableInstance;
        T GetInstanceOf<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
            where T : Component, IEnableInstance;
    }
}