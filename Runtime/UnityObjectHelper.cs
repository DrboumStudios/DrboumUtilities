#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Drboum.Utilities
{
    public static class UnityObjectHelper
    {
        private static List<GameObject> RootGameObjectsBuffer => _rootGameObjectsBuffer ??= new(20);

        private static readonly Dictionary<Type, IEnumerable> _DictionaryListBuffer = new();
        private static List<GameObject> _rootGameObjectsBuffer;

        public static bool IsPrefabInSubSceneContext(this Object @this) =>
            @this is GameObject go && IsPrefabInSubSceneContext(go);

        public static void CheckSetActive(this GameObject @this, bool active)
        {
            if ( @this.activeSelf != active )
            {
                @this.SetActive(active);
            }
        }

        public static bool IsPrefabInSubSceneContext(this GameObject @this) =>
            !@this.scene.IsValid();

        public static void RemoveComponent<T>(this T component)
            where T : Component
        {
            component.Destroy();
        }

        public static void Destroy<T>(this T uObject)
            where T : Object
        {
#if UNITY_EDITOR
            if ( Application.isPlaying )
            {
#endif
                Object.Destroy(uObject);
#if UNITY_EDITOR
            }
            else
            {
                Object.DestroyImmediate(uObject, true);
            }
#endif

        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDirtySafe(this Object uObject)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(uObject);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void RecordObjectForUndo(this Object uObject, string actionName)
        {
#if UNITY_EDITOR
            Undo.RecordObject(uObject, actionName);
#endif
        }

        /// <summary>
        /// return true only if the component has been removed
        /// </summary>
        /// <param name="go"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveComponentIfExists<T>(this GameObject go)
            where T : Component
        {
            if ( !go.TryGetComponent(out T component) )
            {
                return false;
            }
            component.RemoveComponent();
            go.SetDirtySafe();
            return true;
        }

        /// <summary>
        /// return true only if a component has been added
        /// </summary>
        /// <param name="go"></param>
        /// <param name="component"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool AddComponentIfNotExists<T>(this GameObject go, out T component)
            where T : Component
        {
            bool added = !go.TryGetComponent(out component);
            if ( added )
            {
                component = go.AddComponent<T>();
                go.SetDirtySafe();
            }
            return added;
        }

        /// <summary>
        /// return true only if a component has been added
        /// </summary>
        /// <param name="go"></param>
        /// <param name="component"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool AddComponentIfNotExists<T>(this GameObject go)
            where T : Component
        {
            return go.AddComponentIfNotExists<T>(out var _);
        }

        public static void CopyLocalData(this Transform to, Transform source)
        {
            if ( to != null && source != null )
            {
                to.localRotation = source.localRotation;
                to.localPosition = source.localPosition;
            }
        }

        public static bool EnsureObjectIsNotNull<TExecutor>(this TExecutor _, Object objectToCheck, Object objectHolder, string invalidFieldName, string prefixCategory)
        {
            if ( objectToCheck.IsNull() )
            {
                _.LogInvalidRequiredField(objectHolder, invalidFieldName, prefixCategory);
                return true;
            }
            return false;
        }

        public static T GetSingletonComponent<T>(FindObjectsInactive includeInactive = FindObjectsInactive.Exclude, FindObjectsSortMode findObjectsSortMode = FindObjectsSortMode.None)
            where T : MonoBehaviour
        {
            T[] componentInScenes = Object.FindObjectsByType<T>(includeInactive, findObjectsSortMode);
            T singletonInScene = null;
            CheckMultipleObjectOfType(ref singletonInScene, componentInScenes);
            return singletonInScene;
        }

        /// <summary>
        /// <see cref="FillNullMonoBehaviourField{TReturnType,TLookupType}"/>
        /// </summary>
        /// <param name="component"></param>
        /// <param name="includeInactive"></param>
        /// <typeparam name="T"></typeparam>
        public static void FillNullMonoBehaviourField<T>(ref T component, FindObjectsInactive includeInactive = FindObjectsInactive.Exclude, FindObjectsSortMode findObjectsSortMode = FindObjectsSortMode.None)
            where T : Component
        {
            FillNullMonoBehaviourField<T, T>(ref component, includeInactive);
        }

        private static void LogErrorMessageUnassignedFieldAndAbsentofScene<T>()
        {
            Debug.LogError(
                $"the parameter of type {typeof(T)} is not assigned on the component field and can't be find in the scene.");
        }

        /// <summary>
        /// fill resultReference only if its null
        /// </summary>
        /// <remarks>
        ///  will throw an exception if the <typeparamref name="TLookupType"/> is not found, will log if there is more than a single instance available in the scene
        /// </remarks>
        public static void FillNullMonoBehaviourField<TReturnType, TLookupType>(ref TReturnType resultReference, FindObjectsInactive includeInactive = FindObjectsInactive.Exclude, FindObjectsSortMode findObjectsSortMode = FindObjectsSortMode.None)
            where TLookupType : Component, TReturnType
        {
            if ( resultReference == null )
            {
#if UNITY_EDITOR
                TLookupType[] foundArray = Object.FindObjectsByType<TLookupType>(includeInactive, findObjectsSortMode);
                CheckMultipleObjectOfType(ref resultReference, foundArray);

#else
                resultReference = Object.FindObjectOfType<TLookupType>();
#endif
                if ( resultReference == null )
                {
                    LogErrorMessageUnassignedFieldAndAbsentofScene<TLookupType>();
                }
            }
        }

        private static void CheckMultipleObjectOfType<TReturnType, TLookupType>(ref TReturnType resultReference,
            IReadOnlyList<TLookupType> foundArray)
            where TLookupType : Component, TReturnType
        {
            CheckSceneSingletonComponent<TReturnType, TLookupType>(foundArray);
            resultReference = foundArray[0];
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEBUG")]
        private static void CheckSceneSingletonComponent<TReturnType, TLookupType>(IReadOnlyList<TLookupType> foundArray)
            where TLookupType : Component, TReturnType
        {
            if ( foundArray.Count == 0 )
            {
                throw new InvalidOperationException(
                    $"the parameter of type {typeof(TLookupType)} is not assigned on the component field and can't be found in the scene.");
            }
            if ( foundArray.Count > 1 )
            {
                Debug.LogWarning(
                    $"there is more than 1 {typeof(TLookupType)} in the scene. this might have unexpected effects");
            }
        }

        public static TReturnType FillNullMonoBehaviourField<TReturnType, TLookupType, TParameterType>(
            TParameterType parameterToNullCheck = null, FindObjectsInactive includeInactive = FindObjectsInactive.Exclude, FindObjectsSortMode findObjectsSortMode = FindObjectsSortMode.None)
            where TLookupType : Component, TReturnType
            where TParameterType : class
        {
            TReturnType resultReference = default;
            if ( parameterToNullCheck == null )
            {
#if UNITY_EDITOR
                TLookupType[] foundArray = Object.FindObjectsByType<TLookupType>(includeInactive, findObjectsSortMode);
                CheckMultipleObjectOfType(ref resultReference, foundArray);
#else
                resultReference = Object.FindObjectOfType<TLookupType>();
#endif
            }
            return resultReference;
        }

        public static bool IsNull(this Object @object)
        {
            return @object is null || @object == null;
        }

        public static void GetComponentInDirectChildren<T>(this Transform transform, List<T> childrenComponent)
            where T : Component
        {
            for ( int i = 0; i < transform.childCount; i++ )
            {
                Transform childTransform = transform.GetChild(i);
                if ( childTransform.TryGetComponent(out T childNode) )
                    childrenComponent.Add(childNode);
            }
        }

        private static List<T> GetListFromPool<T>()
            where T : Component
        {
            Type type = typeof(T);
            List<T> buffer = null;
            if ( _DictionaryListBuffer.TryGetValue(type, out var listAsEnumerable) )
            {
                buffer = (List<T>)listAsEnumerable;
                buffer.Clear();
            }
            else
            {
                buffer = new List<T>();
                _DictionaryListBuffer.Add(type, buffer);
            }
            return buffer;
        }

        /// <summary>
        /// not Thread safe use internal buffer
        /// </summary>
        public static T FindFirstInstancesInScene<T>(this Scene scene, int layerMask = int.MaxValue)
            where T : Component
        {
            scene.GetRootGameObjects(RootGameObjectsBuffer);
            T firstComponent = default;
            for ( var index = 0; index < RootGameObjectsBuffer.Count; index++ )
            {
                GameObject rootGameObject = RootGameObjectsBuffer[index];
                if ( !rootGameObject.ContainsBitMask(layerMask) )
                    continue;

                firstComponent = rootGameObject.GetComponentInChildren<T>();
                if ( firstComponent )
                    break;
            }
            return firstComponent;
        }

        /// <inheritdoc cref="FindAllInstancesInScene{T}(Scene,List{T},int)"/>
        public static List<T> FindAllInstancesInScene<T>(this Scene scene, int layerMask = int.MaxValue)
            where T : Component
        {
            var list = new List<T>();
            FindAllInstancesInScene(scene, list, layerMask);
            return list;
        }

        /// <summary>
        /// Find all instances in a scene
        /// </summary>
        /// <remarks>this method is NOT Thread safe as it uses cached static collections and must be run in the main thread. use the override <see cref="FindAllInstancesInScene{T}(Scene,List{T},List{GameObject})"/> to run this method in another thread</remarks>
        public static void FindAllInstancesInScene<T>(this Scene scene, List<T> instancesBuffer, int layerMask = int.MaxValue)
            where T : Component
        {
            FindAllInstancesInScene(scene, instancesBuffer, RootGameObjectsBuffer, GetListFromPool<T>(), layerMask);
        }

        /// <summary>
        /// Find all instances in a scene with provided buffers
        /// <param name="combinedLayerMask">optional layer mask on the root scene gameobject that will be included in this query, default to include all</param>
        /// </summary>
        public static void FindAllInstancesInScene<T>(this Scene scene, List<T> resultInstances, List<GameObject> rootGameObjectsBuffer, List<T> componentBuffer, int combinedLayerMask = int.MaxValue)
            where T : Component
        {
            scene.GetRootGameObjects(rootGameObjectsBuffer);
            if ( combinedLayerMask == int.MaxValue )
            {
                for ( var index = 0; index < rootGameObjectsBuffer.Count; index++ )
                {
                    GameObject rootGameObject = rootGameObjectsBuffer[index];
                    rootGameObject.GetComponentsInChildren(true, componentBuffer);
                    resultInstances.AddRange(componentBuffer);
                }
            }
            else
            {
                for ( var index = 0; index < rootGameObjectsBuffer.Count; index++ )
                {
                    GameObject rootGameObject = rootGameObjectsBuffer[index];
                    if ( !rootGameObject.ContainsBitMask(combinedLayerMask) )
                        continue;

                    rootGameObject.GetComponentsInChildren(true, componentBuffer);
                    resultInstances.AddRange(componentBuffer);
                }
            }
        }

        /// <inheritdoc cref="FindAllInstancesInScene{T}(Scene,List{T},int)"/>
        public static List<int> FindAllInstancesIdsInActiveScene(Type lookupType)
        {
            var instancesBuffer = new List<int>();
            FindAllInstancesIdInScene(instancesBuffer, lookupType, SceneManager.GetActiveScene());
            return instancesBuffer;
        }

        /// <inheritdoc cref="FindAllInstancesInScene{T}(Scene,List{T},int)"/>
        public static void FindAllInstancesInActiveScene(List<Object> lookupResult, Type interfaceType)
        {
            FindAllInstancesInScene(SceneManager.GetActiveScene(), lookupResult, interfaceType);
        }

        /// <inheritdoc cref="FindAllInstancesInScene{T}(Scene,List{T},int)"/>
        public static void FindAllInstancesInScene(this Scene scene, List<Object> lookupResult, Type lookupType, int layerMask = int.MaxValue)
        {
            scene.GetRootGameObjects(RootGameObjectsBuffer);
            if ( layerMask == int.MaxValue )
            {
                for ( var rootIndex = 0; rootIndex < RootGameObjectsBuffer.Count; rootIndex++ )
                {
                    GameObject rootGameObject = RootGameObjectsBuffer[rootIndex];
                    GetComponentOfTypeInChildren(lookupResult, lookupType, rootGameObject);
                }
            }
            else
            {
                for ( var rootIndex = 0; rootIndex < RootGameObjectsBuffer.Count; rootIndex++ )
                {
                    GameObject rootGameObject = RootGameObjectsBuffer[rootIndex];
                    if ( !rootGameObject.ContainsBitMask(layerMask) )
                        continue;

                    GetComponentOfTypeInChildren(lookupResult, lookupType, rootGameObject);
                }
            }
        }

        private static void GetComponentOfTypeInChildren(List<Object> lookupResult, Type lookupType, GameObject rootGameObject)
        {
            lookupResult.AddRange(rootGameObject.GetComponentsInChildren(lookupType, true));
        }

        /// <inheritdoc cref="FindAllInstancesInScene{T}(Scene,List{T},int)"/>
        public static void FindAllInstancesIdInScene(List<int> instancesBuffer, Type lookupType, Scene scene)
        {
            scene.GetRootGameObjects(RootGameObjectsBuffer);
            for ( var rootIndex = 0; rootIndex < RootGameObjectsBuffer.Count; rootIndex++ )
            {
                GameObject rootGameObject = RootGameObjectsBuffer[rootIndex];
                Component[] childrenInterfaces = rootGameObject.GetComponentsInChildren(lookupType);
                for ( var index = 0; index < childrenInterfaces.Length; index++ )
                {
                    Component childInterface = childrenInterfaces[index];
                    instancesBuffer.Add(childInterface.GetInstanceID());
                }
            }
        }

        public static float3 GetScaledPoint(Transform relativePoint)
        {
            return new float3(
                relativePoint.localPosition.x * relativePoint.lossyScale.x,
                relativePoint.localPosition.y * relativePoint.lossyScale.y,
                relativePoint.localPosition.z * relativePoint.lossyScale.z
            );
        }
    }
}