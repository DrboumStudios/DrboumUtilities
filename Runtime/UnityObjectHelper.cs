using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class UnityObjectHelper
{
    private static readonly List<GameObject> _rootGameObjectsBuffer = new(20);
    private static Dictionary<Type, IEnumerable> _dictionaryListBuffer;

    public static bool IsPrefabInSubSceneContext(this Object @this) =>
        @this is GameObject go && IsPrefabInSubSceneContext(go);

    public static bool IsPrefabInSubSceneContext(this GameObject @this) =>
        !@this.scene.IsValid();

    public static void RemoveComponent<T>(this T component)
        where T : Component
    {
        if ( !Application.isPlaying )
        {
            Object.DestroyImmediate(component, true);
        }
        else
        {
            Object.Destroy(component);
        }

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

    public static T GetSingletonComponent<T>(bool includeInactive = false)
        where T : MonoBehaviour
    {
        T[] componentInScenes = Object.FindObjectsOfType<T>(includeInactive);
        T singletonInScene = null;
        CheckMultipleObjectOfType(ref singletonInScene, componentInScenes);
        return singletonInScene;
    }

    /// <summary>
    /// <see cref="FillNullMonobehaviourField{ReturnType,LookupType}"/>
    /// </summary>
    /// <param name="component"></param>
    /// <param name="includeInactive"></param>
    /// <typeparam name="T"></typeparam>
    public static void FillNullMonobehaviourField<T>(ref T component, bool includeInactive = false)
        where T : Component
    {
        FillNullMonobehaviourField<T, T>(ref component, includeInactive);
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
    ///  will throw an exception if the <typeparamref name="LookupType"/> is not found, will log if there is more than a single instance available in the scene
    /// </remarks>
    /// <param name="resultReference"></param>
    /// <param name="includeInactive"></param>
    /// <typeparam name="ReturnType"></typeparam>
    /// <typeparam name="LookupType"></typeparam>
    public static void FillNullMonobehaviourField<ReturnType, LookupType>(ref ReturnType resultReference, bool includeInactive = false)
        where LookupType : Component, ReturnType
    {
        if ( resultReference == null )
        {
#if UNITY_EDITOR
            LookupType[] foundArray = Object.FindObjectsOfType<LookupType>(includeInactive);
            CheckMultipleObjectOfType(ref resultReference, foundArray);

#else
                resultReference = Object.FindObjectOfType<LookupType>();
#endif
            if ( resultReference == null )
            {
                LogErrorMessageUnassignedFieldAndAbsentofScene<LookupType>();
            }
        }
    }

    private static void CheckMultipleObjectOfType<ReturnType, LookupType>(ref ReturnType resultReference,
        LookupType[] foundArray)
        where LookupType : Component, ReturnType
    {
        CheckSceneSingletonComponent<ReturnType, LookupType>(foundArray);
        resultReference = foundArray[0];
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEBUG")]
    private static void CheckSceneSingletonComponent<ReturnType, LookupType>(LookupType[] foundArray)
        where LookupType : Component, ReturnType
    {
        if ( foundArray.Length == 0 )
        {
            throw new InvalidOperationException(
                $"the parameter of type {typeof(LookupType)} is not assigned on the component field and can't be found in the scene.");
        }
        if ( foundArray.Length > 1 )
        {
            Debug.LogWarning(
                $"there is more than 1 {typeof(LookupType)} in the scene. this might have unexpected effects");
        }
    }

    public static ReturnType FillNullMonobehaviourField<ReturnType, LookupType, ParameterType>(
        ParameterType parameterToNullCheck = null,
        bool includeInactive = false)
        where LookupType : Component, ReturnType
        where ParameterType : class
    {
        ReturnType resultReference = default;
        if ( parameterToNullCheck == null )
        {
#if UNITY_EDITOR
            LookupType[] foundArray = Object.FindObjectsOfType<LookupType>(includeInactive);
            CheckMultipleObjectOfType(ref resultReference, foundArray);
#else
                resultReference = Object.FindObjectOfType<LookupType>();
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
        if ( !_dictionaryListBuffer.TryGetValue(type, out var Listobj) )
        {
            buffer = new List<T>();
            _dictionaryListBuffer.Add(type, buffer);
        }
        return buffer;
    }

    public static List<T> FindAllInstancesInScene<T>(this Scene scene)
        where T : Component
    {
        var list = new List<T>();
        FindAllInstancesInScene(scene, list);
        return list;
    }

    public static T FindFirstInstancesInScene<T>(this Scene scene)
        where T : Component
    {
        scene.GetRootGameObjects(_rootGameObjectsBuffer);
        T firstComponent = default;
        for ( var index = 0; index < _rootGameObjectsBuffer.Count; index++ )
        {
            GameObject rootGameObject = _rootGameObjectsBuffer[index];
            firstComponent = rootGameObject.GetComponentInChildren<T>();
            if ( firstComponent )
                break;
        }
        return firstComponent;
    }

    public static void FindAllInstancesInScene<T>(this Scene scene, List<T> instancesBuffer)
        where T : Component
    {
        scene.GetRootGameObjects(_rootGameObjectsBuffer);
        var buffer = GetListFromPool<T>();
        foreach ( GameObject rootGameObject in _rootGameObjectsBuffer )
        {
            rootGameObject.GetComponentsInChildren(true, buffer);
            instancesBuffer.AddRange(buffer);
        }
    }


    public static List<int> FindAllInstancesIdsInActiveScene(Type lookupType)
    {
        var instancesBuffer = new List<int>();
        FindAllInstancesIdInScene(instancesBuffer, lookupType, SceneManager.GetActiveScene());
        return instancesBuffer;
    }

    public static void FindAllInstancesInActiveScene(List<Object> lookupResult, Type interfaceType)
    {
        FindAllInstancesInScene(SceneManager.GetActiveScene(), lookupResult, interfaceType);
    }

    public static void FindAllInstancesInScene(this Scene scene,List<Object> lookupResult,Type lookupType)
    {
        scene.GetRootGameObjects(_rootGameObjectsBuffer);
        foreach ( GameObject rootGameObject in _rootGameObjectsBuffer )
        {
            Component[] childrenInterfaces = rootGameObject.GetComponentsInChildren(lookupType);
            for ( var index = 0; index < childrenInterfaces.Length; index++ )
            {
                Component childInterface = childrenInterfaces[index];
                lookupResult.Add(childInterface);
            }
        }
    }

    public static void FindAllInstancesIdInScene(List<int> instancesBuffer, Type lookupType, Scene scene)
    {
        scene.GetRootGameObjects(_rootGameObjectsBuffer);
        foreach ( GameObject rootGameObject in _rootGameObjectsBuffer )
        {
            Component[] childrenInterfaces = rootGameObject.GetComponentsInChildren(lookupType);
            for ( var index = 0; index < childrenInterfaces.Length; index++ )
            {
                Component childInterface = childrenInterfaces[index];
                instancesBuffer.Add(childInterface.GetInstanceID());
            }
        }
    }
}