﻿using UnityEditor;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class UnityObjectEditorHelper
{

#if UNITY_EDITOR
    static Component[] copiedComponents;

    [MenuItem("GameObject/Copy all components %&C")]
    static void Copy()
    {
        if ( UnityEditor.Selection.activeGameObject == null )
            return;

        copiedComponents = UnityEditor.Selection.activeGameObject.GetComponents<Component>();
    }

    [MenuItem("GameObject/Paste all components %&P")]
    static void Paste()
    {
        if ( copiedComponents == null )
        {
            Debug.LogError("Nothing is copied!");
            return;
        }

        foreach ( var targetGameObject in UnityEditor.Selection.gameObjects )
        {
            if ( !targetGameObject )
                continue;

            Undo.RegisterCompleteObjectUndo(targetGameObject,
                targetGameObject.name + ": Paste All Components"); // sadly does not record PasteComponentValues, i guess

            foreach ( var copiedComponent in copiedComponents )
            {
                if ( !copiedComponent )
                    continue;

                UnityEditorInternal.ComponentUtility.CopyComponent(copiedComponent);

                var targetComponent = targetGameObject.GetComponent(copiedComponent.GetType());

                if ( targetComponent ) // if gameObject already contains the component
                {
                    if ( UnityEditorInternal.ComponentUtility.PasteComponentValues(targetComponent) )
                    {
                        Debug.Log("Successfully pasted: " + copiedComponent.GetType());
                    }
                    else
                    {
                        Debug.LogError("Failed to copy: " + copiedComponent.GetType());
                    }
                }
                else // if gameObject does not contain the component
                {
                    if ( UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGameObject) )
                    {
                        Debug.Log("Successfully pasted: " + copiedComponent.GetType());
                    }
                    else
                    {
                        Debug.LogError("Failed to copy: " + copiedComponent.GetType());
                    }
                }
            }
        }
        copiedComponents = null; // to prevent wrong pastes in future
    }

    [MenuItem("GameObject/Reset prefab Instance to parent prefab")]
    static void RevertPrefabInstanceToBase()
    {
        var selection = Selection.gameObjects;

        if ( selection.Length > 0 )
        {
            for ( var i = 0; i < selection.Length; i++ )
            {
                PrefabUtility.RevertPrefabInstance(selection[i], InteractionMode.AutomatedAction);
            }
        }
        else
        {
            Debug.Log("Cannot revert to prefab - nothing selected");
        }
    }

    [MenuItem("CONTEXT/Component/Revert prefab overrides to closest parent prefab")]
    static void Revert(MenuCommand command)
    {
        var selection = command.context as Component;
        if ( selection != null )
        {
            PrefabUtility.RevertObjectOverride(selection, InteractionMode.AutomatedAction);
        }
        else
        {
            Debug.Log("Cannot revert to prefab");
        }
    }

    [MenuItem("Tools/" + nameof(SaveAssetsDataBase))]
    private static void SaveAssetsDataBase()
    {
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Tools/" + nameof(RefreshAssetDataBase))]
    private static void RefreshAssetDataBase()
    {
        AssetDatabase.Refresh();
    }
    public static bool IsPrefabAssetEditor(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.IsPartOfPrefabAsset(gameObject);
#else
			return PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab;
#endif
    }

    public static bool IsPrefabAssetOrOpenInPrefabStage(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.IsPartOfPrefabAsset(gameObject) || UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null;
#else
			return PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab;
#endif
    }

    public static bool IsPrefabAssetOrInstance(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab;
#else
			var prefabType = PrefabUtility.GetPrefabType(gameObject);
			return prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance;
#endif
    }

    public static bool IsConnectedPrefabInstance(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.Connected;
#else
			return PrefabUtility.GetPrefabType(gameObject) == PrefabType.PrefabInstance;
#endif
    }

    public static bool IsDisconnectedPrefabInstance(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.Disconnected;
#else
			return PrefabUtility.GetPrefabType(gameObject) == PrefabType.DisconnectedPrefabInstance;
#endif
    }

    public static bool IsPartOfInstantiatedPrefabInstance(this GameObject gameObject)
    {
        for ( var transform = gameObject.transform; transform != null; transform = transform.parent )
        {
            if ( transform.name.EndsWith("(Clone)", StringComparison.Ordinal) )
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsPartOfPrefabVariant(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.IsPartOfVariantPrefab(gameObject);
#else
			return false;
#endif
    }

    public static bool IsConnectedOrDisconnectedPrefabInstance(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
        return prefabStatus == PrefabInstanceStatus.Connected || prefabStatus == PrefabInstanceStatus.Disconnected;
#else
			var prefabType = PrefabUtility.GetPrefabType(gameObject);
			return prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance;
#endif
    }

    public static bool IsPrefabInstanceRoot(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
#else
			return PrefabUtility.GetPrefabType(gameObject) == PrefabType.PrefabInstance && PrefabUtility.FindPrefabRoot(gameObject) == gameObject;
#endif
    }

    public static GameObject GetOutermostPrefabInstanceRoot(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
#else
			return PrefabUtility.FindPrefabRoot(gameObject);
#endif
    }

    public static bool IsOpenInPrefabStage(this GameObject gameObject)
    {
#if UNITY_2018_3_OR_NEWER
        return UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null;
#else
			return false;
#endif
    }

    [MenuItem("CONTEXT/Component/" + nameof(FindAllPrefabWithComponentType))]
    public static void FindAllPrefabWithComponentType(MenuCommand command)
    {
        var component = command.context as Component;
        var targetType = component.GetType();
        var matches = new List<Object>(200);
        var assetGuids = AssetDatabase.FindAssets($"t:Prefab");
        foreach ( var assetGuid in assetGuids )
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGuid));
            if ( prefab != null )
            {
                if ( prefab.GetComponentInChildren(targetType, true) )
                {
                    matches.Add(prefab);
                }
            }
        }

        Selection.objects = matches.ToArray();
    }
#endif
    public static void CheckAndSetActive(this GameObject gameObject, bool active)
    {
        if ( gameObject.activeSelf != active )
            gameObject.SetActive(active);
    }
    public static void CheckAndSetActive(this GameObject[] gameObjects, bool active)
    {
        for ( var index = 0; index < gameObjects.Length; index++ )
        {
            gameObjects[index].CheckAndSetActive(active);
        }
    }

    public static bool IsInCurrentPrefabStage(this GameObject gameObject)
    {
        return gameObject.IsInCurrentPrefabStage(out var _);
    }
    public static bool IsInCurrentPrefabStage(this GameObject gameObject, out UnityEditor.SceneManagement.PrefabStage currentPrefabStage)
    {
        currentPrefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        return currentPrefabStage != null && currentPrefabStage.IsPartOfPrefabContents(gameObject);
    }
    public static bool IsPrefabAsset(this GameObject gameObject)
    {
        return EditorUtility.IsPersistent(gameObject) && PrefabUtility.IsPartOfPrefabAsset(gameObject) && gameObject.transform.parent.IsNull();
    }

    /// <summary>
    ///     Editor Only
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static bool TryGetAssetGuid(this Object asset, out Guid guid)
    {
        if ( asset.IsNull() )
        {
            guid = default;
            return false;
        }
        string path = AssetDatabase.GetAssetPath(asset);
        if ( !string.IsNullOrEmpty(path) )
        {
            guid = Guid.Parse(AssetDatabase.AssetPathToGUID(path));
            return true;
        }
        guid = default;
        return false;
    }
    public static bool TryLoadAsset<T>(string assetGuid, out string path, out T asset)
        where T : Object
    {
        asset = default;
        path = AssetDatabase.GUIDToAssetPath(assetGuid);
        bool nothingAtPathOrIsInvalid = string.IsNullOrEmpty(path);
        if ( nothingAtPathOrIsInvalid )
        {
            return false;
        }
        //check if there is actually something at this path to make sure the cache is valid
        asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return !asset.IsNull();
    }
    public static bool TryLoadAsset<T>(Guid assetGuid, out string path, out T asset)
        where T : Object
    {
        return TryLoadAsset(assetGuid.ToString("n"), out path, out asset);
    }
    public static T GetSingletonAssetInstance<T>(string folderPath)
        where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] {
            folderPath
        });
        if ( guids.Length > 1 )
        {
            Debug.LogError($"SingletonInstance of type {typeof(T).Name} is not unique path: {folderPath}");
        }

        if ( guids.Length == 0 )
        {
            Debug.LogError($"SingletonInstance of type {typeof(T).Name} has not been found on path: {folderPath}");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));

    }
    public static T[] FindAllAssetInstances<T>()
        where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        var a = new T[guids.Length];
        AssignInstances(guids, a);

        return a;
    }
    public static T[] FindAllAssetInstances<T>(string[] folderPaths)
        where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, folderPaths);
        var a = new T[guids.Length];
        AssignInstances(guids, a);

        return a;
    }
    private static void AssignInstances<T>(string[] guids, T[] a)
        where T : Object
    {
        for ( var i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }

    public static void FindAllAssetInstances<T>(List<Object> scriptableObjectsResult,
        Type abstractType,
        string[] lookupFolders)
        where T : Object
    {
        //string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name, lookupFolders);
        FindAllAssetInstances(scriptableObjectsResult, lookupFolders,
            (guid, path, inst) => abstractType.IsAssignableFrom(inst.GetType()));
    }

    public static string[] GetPrefabGuids(string[] folders)
    {
        return AssetDatabase.FindAssets("t:Prefab", folders);
    }
    public static void GetPrefabs(List<GameObject> gameObjects, string[] folders = null)
    {
        GetPrefabsAsComponents<GameObject>(gameObjects, folders);
    }
    public static void GetPrefabsAsComponents<T>(List<T> gameObjects, string[] folders = null)
        where T : Object
    {
        string[] prefabGuids = GetPrefabGuids(folders);
        for ( var i = 0; i < prefabGuids.Length; ++i )
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            var loadedObject = AssetDatabase.LoadAssetAtPath<T>(path);
            gameObjects.Add(loadedObject);
        }
    }
    public static void FindAllAssetInstances<T>(List<T> buffer)
        where T : Object
    {
        FindAllAssetInstances(buffer, null);
    }
    public static void FindAllAssetInstances<T>(List<T> buffer, string[] lookupFolders)
        where T : Object
    {
        FindAllAssetInstances(buffer, lookupFolders, null);
    }
    public static void FindAllAssetInstances<T>(List<T> buffer, string[] lookupFolders, AssetSearchPredicate<T> predicate)
        where T : Object
    {

        if ( buffer == null )
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name,
            lookupFolders); //FindAssets uses tags check documentation for more info
        FillBufferAndEnsureCapacity(buffer, guids);
        if ( predicate != null )
        {
            GetAssets(buffer, guids, predicate);
        }
        else
        {
            GetAssets(buffer, guids);
        }
    }
    public delegate bool AssetSearchPredicate<T>(string guid, string path, T instance)
        where T : Object;

    [Conditional("UNITY_EDITOR")]
    public static void OverWriteGuidInMetaFile(this Object assetObject, string assetGuid, ref string path)
    {
        path = AssetDatabase.GetAssetPath(assetObject);
        string pathMeta = AssetDatabase.GetTextMetaFilePathFromAssetPath(path);

        string[] allLines = File.ReadAllLines(pathMeta);
        allLines[1] = $"guid: {assetGuid}";
        File.WriteAllLines(pathMeta, allLines);
    }

    private static void FillBufferAndEnsureCapacity<T>(List<T> buffer, string[] guids)
        where T : Object
    {
        buffer.Clear();
        if ( buffer.Capacity < guids.Length )
        {
            buffer.Capacity = guids.Length;
        }
    }
    private static void GetAssets<T>(List<T> buffer, string[] guids)
        where T : Object
    {
        for ( var i = 0; i < guids.Length; i++ )
        {
            string guid = guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            buffer.Add(AssetDatabase.LoadAssetAtPath<T>(path));

        }
    }
    private static void GetAssets<T>(List<T> buffer, string[] guids, AssetSearchPredicate<T> predicate)
        where T : Object
    {
        for ( var i = 0; i < guids.Length; i++ )
        {
            string guid = guids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var assetInstance = AssetDatabase.LoadAssetAtPath<T>(path);
            if ( predicate(guid, path, assetInstance) )
            {
                buffer.Add(assetInstance);
            }
        }
    }
    /// <summary>
    ///     look up on the active scene
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> FindAllInstancesInScene<T>()
        where T : Component
    {
        var list = new List<T>();
        FindAllInstancesInScene(list, SceneManager.GetActiveScene());
        return list;
    }
    public static void FindAllInstancesInScene<T>(List<T> instancesBuffer, Scene scene)
        where T : Component
    {
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        foreach ( GameObject rootGameObject in rootGameObjects )
        {
            rootGameObject.GetComponentsInChildren(true, instancesBuffer);
        }
    }
    /// <summary>
    ///     look up on the active scene
    /// </summary>
    public static List<Component> FindAllInstancesInScene(Type interfaceType)
    {
        var instancesBuffer = new List<Component>();
        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach ( GameObject rootGameObject in rootGameObjects )
        {
            Component[] childrenInterfaces = rootGameObject.GetComponentsInChildren(interfaceType);
            foreach ( Component childInterface in childrenInterfaces )
            {
                instancesBuffer.Add(childInterface);
            }
        }
        return instancesBuffer;
    }
    /// <summary>
    ///     look up on the active scene
    /// </summary>
    public static List<int> FindAllInstancesIdsInScene(Type interfaceType)
    {
        var instancesBuffer = new List<int>();
        FindAllInstancesInScene(instancesBuffer, interfaceType, SceneManager.GetActiveScene());
        return instancesBuffer;
    }
    public static void FindAllInstancesInActiveScene(List<Object> lookupResult, Type interfaceType)
    {
        FindAllInstancesInScene(new List<GameObject>(SceneManager.GetActiveScene().rootCount), lookupResult,
            interfaceType, SceneManager.GetActiveScene());
    }
    public static void FindAllInstancesInScene(List<GameObject> rootGameObjects,
        List<Object> lookupResult,
        Type interfaceType,
        Scene scene)
    {
        scene.GetRootGameObjects(rootGameObjects);
        foreach ( GameObject rootGameObject in rootGameObjects )
        {
            Component[] childrenInterfaces = rootGameObject.GetComponentsInChildren(interfaceType);
            foreach ( Component childInterface in childrenInterfaces )
            {
                lookupResult.Add(childInterface);
            }
        }
    }
    public static void FindAllInstancesInScene(List<int> instancesBuffer, Type interfaceType, Scene scene)
    {
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        foreach ( GameObject rootGameObject in rootGameObjects )
        {
            Component[] childrenInterfaces = rootGameObject.GetComponentsInChildren(interfaceType);
            foreach ( Component childInterface in childrenInterfaces )
            {
                instancesBuffer.Add(childInterface.GetInstanceID());
            }
        }
    }
    public static void EnsureFolderCreation(string folder)
    {
        Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
    }
}