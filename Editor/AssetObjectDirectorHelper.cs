using System;
using System.IO;
using Drboum.Utilities.Runtime.EditorHybrid;
using Drboum.Utilities.Runtime.Interfaces;
using UnityEditor;
using UnityEngine;
namespace Drboum.Utilities.Editor {

    public abstract class AssetObjectDirectorManager<TAssetObjectDirectorInstance, TAssetInstance>
        where TAssetObjectDirectorInstance : AssetObjectDirector<TAssetInstance>
        where TAssetInstance : AssetReferenceID, IInitializable<IAuthoring> {

        public const    string                                                                   ASSETID_SEPARATOR = "#";
        protected static  AssetObjectDirectorManager<TAssetObjectDirectorInstance, TAssetInstance> _instance;
        protected static void CreateStaticInstance<T>()
            where T : AssetObjectDirectorManager<TAssetObjectDirectorInstance, TAssetInstance>, new()
        {
            _instance = new T();
            _instance.Initialize();
        }
        protected AssetObjectDirectorManager() { }

        protected virtual void Initialize()
        {
            InitializeCallBacks();
        }
        protected virtual void InitializeCallBacks()
        {
            EditorObjectsEventCallBacks<TAssetObjectDirectorInstance>.RegisterOnStart += _instance.Awake;
            EditorObjectsEventCallBacks<TAssetObjectDirectorInstance>.RegisterOnValidate += _instance.OnValidate;
            EditorObjectsEventCallBacks<TAssetObjectDirectorInstance>.RegisterOnDestroy += _instance.OnDestroy;
        }


        public string GeneratedName(TAssetObjectDirectorInstance instance)
            => !string.IsNullOrEmpty(AssetInstanceID(instance)) ?
                instance.name + ASSETID_SEPARATOR + AssetInstanceID(instance).Substring(0, 4) :
                $"{instance.name}{ASSETID_SEPARATOR}{(uint)instance.GetInstanceID()}";

        public abstract string GeneratedFolder(TAssetObjectDirectorInstance instance);
        public string AssetInstanceID(TAssetObjectDirectorInstance instance)
        {

            InitializeDeps(instance);
            return instance._editorObjectTracker.AssetInstanceGuid;

        }
        protected virtual void Awake(TAssetObjectDirectorInstance instance)
        {
            InitializeDeps(instance);
            SubscribeEvents(instance);
        }
        protected virtual void OnValidate(TAssetObjectDirectorInstance instance)
        {
            InitializeDeps(instance);
            SubscribeEvents(instance);
        }

        private void InitializeDeps(TAssetObjectDirectorInstance instance)
        {
            if ( instance._editorObjectTracker.IsNull() )
            {
                instance._editorObjectTracker = instance.GetComponent<EditorObjectTracker>();
            }
        }
        protected virtual void SubscribeEvents(TAssetObjectDirectorInstance instance)
        {
            if ( instance._subscribed || instance._editorObjectTracker.IsNull() )
            {
                return;
            }
            instance._subscribed = true;
            instance._editorObjectTracker.RegisterOnCreateComponentEvent(instance, ConstructAction(OnCreateComponent));
            instance._editorObjectTracker.RegisterOnDuplicateEvent(instance, ConstructAction(OnDuplicate));
            instance._editorObjectTracker.RegisterOnGameObjectNameChangedEvent(instance, (c, oldName) => OnGameObjectNameChanged(c as TAssetObjectDirectorInstance, oldName));
        }
        private Action<Component> ConstructAction(Action<TAssetObjectDirectorInstance> methodToExecute)
        {
            return (c) => methodToExecute(c as TAssetObjectDirectorInstance);
        }

        protected virtual void UnsubscribeEvents(TAssetObjectDirectorInstance instance)
        {
            instance._subscribed = false;
            instance._editorObjectTracker.UnRegisterOnCreateComponentEvent(instance);
            instance._editorObjectTracker.UnRegisterOnDuplicateEvent(instance);
            instance._editorObjectTracker.UnRegisterOnGameObjectNameChangedEvent(instance);
        }
        public virtual bool IsValidAssetInstance(TAssetObjectDirectorInstance instance) => !instance.AssetObject.IsNull() && instance.AssetObject.IsValidAsset;

        public virtual void FixAssetInstanceIfInvalid(TAssetObjectDirectorInstance instance, bool saveAssets = true)
        {
            if ( IsValidAssetInstance(instance) || EditorObjectTracker.IsInPrefabMode(instance.gameObject) )
            {
                return;
            }

            bool assetExist = TryLoadAsset(instance, out string path, out instance.AssetObject);

            if ( !assetExist )
            {
                CreateAssetAndOverrideGuid(instance, saveAssets);
            }
            else
            {
                instance.SetDirtySafe();
                instance.AssetObject.FixAssetIDIfInvalid();
            }
        }

        protected void CreateAssetAndOverrideGuid(TAssetObjectDirectorInstance instance, bool saveAssets = true)
        {
            CreateAssetInstance(instance, out instance.AssetObject, saveAssets);
            string path = default;
            instance.AssetObject.OverWriteGuidInMetaFile(instance._editorObjectTracker.AssetInstanceGuid, ref path);

            if ( saveAssets )
            {
                PublishAssetChanges(instance, path);
            }
            if ( TryLoadAsset(instance, out path, out instance.AssetObject) )
            {
                instance.SetDirtySafe();
            }
        }

        protected bool TryLoadAsset(TAssetObjectDirectorInstance instance, out string path, out TAssetInstance assetInstance)
        {
            return UnityObjectEditorHelper.TryLoadAsset(instance._editorObjectTracker.AssetInstanceGuid, out path, out assetInstance);
        }

        protected virtual void PublishAssetChanges(TAssetObjectDirectorInstance instance, string linkedAssetPath)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        protected virtual void OnDestroy(TAssetObjectDirectorInstance instance)
        {
            UnsubscribeEvents(instance);

            if ( !instance.gameObject.scene.isLoaded )
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath(instance.AssetObject);
            if ( !string.IsNullOrEmpty(path) )
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
        internal void OnGameObjectNameChanged(TAssetObjectDirectorInstance instance, string oldName)
        {
            if ( instance.AssetObject.IsNull() )
            {
                return;
            }
            ApplyGeneratedName(instance);
        }
        private void ApplyGeneratedName(TAssetObjectDirectorInstance instance)
        {
            string currentPathName = AssetDatabase.GetAssetPath(instance.AssetObject);
            string newName = GeneratedName(instance);

            string errorMessage = AssetDatabase.RenameAsset(currentPathName, newName);
            if ( !string.IsNullOrEmpty(errorMessage) )
            {
                Debug.LogWarning($"Rename Asset failed  with name: '{newName}' at path: '{currentPathName}'  with error message: {errorMessage}", instance);
            }
        }
        protected virtual void OnDuplicate(TAssetObjectDirectorInstance instance)
        {
            CreateAssetInstance(instance);
        }
        protected virtual void OnCreateComponent(TAssetObjectDirectorInstance instance)
        {
            CreateAssetInstance(instance);
        }
        private void CreateAssetInstance(TAssetObjectDirectorInstance instance)
        {
            CreateAssetInstance(instance, out instance.AssetObject);
            instance._editorObjectTracker.SetAssetGuid(instance.AssetObject);
            ApplyGeneratedName(instance); //to apply the new guid to the name
        }
        public void CreateAssetInstance(TAssetObjectDirectorInstance assetAuthoring, out TAssetInstance newInstance, bool saveAssetsImmediately = true)
        {
            newInstance = ScriptableObject.CreateInstance<TAssetInstance>();
            newInstance.name = GeneratedName(assetAuthoring);
            newInstance.Initialize(assetAuthoring);
            UnityObjectEditorHelper.EnsureFolderCreation(GeneratedFolder(assetAuthoring));
            AssetDatabase.CreateAsset(newInstance, GetAssetPath(assetAuthoring));
            assetAuthoring.AssetObject = newInstance;
            EditorUtility.SetDirty(assetAuthoring);
            if ( saveAssetsImmediately )
            {
                AssetDatabase.SaveAssets();
                assetAuthoring.AssetObject.FixAssetIDIfInvalid();
            }
        }
        public string GetAssetPath(TAssetObjectDirectorInstance waypointAuthoring)
        {
            return $"{GeneratedFolder(waypointAuthoring)}/{GeneratedName(waypointAuthoring)}.asset";
        }
    }
    public static class AssetObjectDirectorHelper {

        /// <summary>
        ///     this method will discard all current change on the asset at path and force serialization from disk
        /// </summary>
        /// <param name="path"></param>
        public static void ForceAssetImport(string path)
        {
            string[] text = File.ReadAllLines(path);
            string firstLine = text[0];
            text[0] = firstLine + " ";
            File.WriteAllLines(path, text);
            AssetDatabase.ImportAsset(path);
        }
    }
}