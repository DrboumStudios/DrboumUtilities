using System.IO;
using DrboumLibrary;
using DrboumLibrary.Animation;
using DrboumLibrary.Interfaces;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace GameProject.Utils {
    [RequireComponent(typeof(EditorObjectTracker))]
    [ExecuteInEditMode]
    public abstract class AssetObjectDirector : MonoBehaviour
#if UNITY_EDITOR
        , IAssetDirector
#endif
        , IAuthoring {
#if UNITY_EDITOR
        public const string ASSETID_SEPARATOR = "#";

        public string GeneratedName => !string.IsNullOrEmpty(AssetInstanceID) ?
            name + ASSETID_SEPARATOR + AssetInstanceID.Substring(0, 4) :
            $"{name}{ASSETID_SEPARATOR}{(uint)GetInstanceID()}";

        public abstract bool                IsValidAssetInstance { get; }
        public abstract string              GeneratedFolder      { get; protected set; }
        protected       EditorObjectTracker _editorObjectTracker;
        private         bool                _subscribed;
        public string AssetInstanceID {
            get {
                InitializeDeps();
                return _editorObjectTracker.AssetGuid;
            }
        }
        protected virtual void Awake()
        {
            InitializeDeps();
            SubscribeEvents();
        }
        protected virtual void OnValidate()
        {
            InitializeDeps();
            SubscribeEvents();
        }

        private void InitializeDeps()
        {
            if ( _editorObjectTracker.IsNull() )
            {
                _editorObjectTracker = GetComponent<EditorObjectTracker>();
            }
        }
        protected virtual void SubscribeEvents()
        {
            if ( _subscribed || _editorObjectTracker.IsNull() )
            {
                return;
            }
            _subscribed = true;

            _editorObjectTracker.OnCreateComponent       += OnCreateComponent;
            _editorObjectTracker.OnDuplicate             += OnDuplicate;
            _editorObjectTracker.OnGameObjectNameChanged += OnGameObjectNameChanged;
        }

        protected virtual void UnsubscribeEvents()
        {
            _subscribed                                  =  false;
            _editorObjectTracker.OnCreateComponent       -= OnCreateComponent;
            _editorObjectTracker.OnDuplicate             -= OnDuplicate;
            _editorObjectTracker.OnGameObjectNameChanged -= OnGameObjectNameChanged;

        }
        protected virtual void OnCreateComponent()
        { }
        protected virtual void OnGameObjectNameChanged(string oldName) { }
        protected virtual void OnDuplicate()                           { }
        protected virtual void OnDestroy()
        {
            UnsubscribeEvents();
        }

        public static void CreateAssetInstance<TAuthoring, TScriptable>(TAuthoring assetAuthoring, out TScriptable newInstance, bool saveAssetsImmediately = true)
            where TAuthoring : AssetObjectDirector<TScriptable>
            where TScriptable : ScriptableObject, IInitializable<IAuthoring>, IAssetReferenceID
        {
            newInstance      = ScriptableObject.CreateInstance<TScriptable>();
            newInstance.name = assetAuthoring.GeneratedName;
            newInstance.Initialize(assetAuthoring);
            UnityObjectEditorHelper.EnsureFolderCreation(assetAuthoring.GeneratedFolder);
            AssetDatabase.CreateAsset(newInstance, GetAssetPath(assetAuthoring));
            assetAuthoring.AssetObject = newInstance;
            EditorUtility.SetDirty(assetAuthoring);
            if ( saveAssetsImmediately )
            {
                AssetDatabase.SaveAssets();
                assetAuthoring.AssetObject.FixAssetIDIfInvalid();
            }
        }
        public static string GetAssetPath(IAssetDirector waypointAuthoring)
        {
            return $"{waypointAuthoring.GeneratedFolder}/{waypointAuthoring.GeneratedName}.asset";
        }
        /// <summary>
        ///     this method will discard all current change on the asset at path and force serialization from disk
        /// </summary>
        /// <param name="path"></param>
        public static void ForceAssetImport(string path)
        {
            string[] text      = File.ReadAllLines(path);
            string   firstLine = text[0];
            text[0] = firstLine + " ";
            File.WriteAllLines(path, text);
            AssetDatabase.ImportAsset(path);
        }

#endif
    }
    [DisallowMultipleComponent]
    public abstract class AssetObjectDirector<T> : AssetObjectDirector
        where T : ScriptableObject, IInitializable<IAuthoring>, IAssetReferenceID {

        [SerializeField]
        // [InspectorReadOnly] 
        public T AssetObject;
#if UNITY_EDITOR
        public override bool IsValidAssetInstance => !AssetObject.IsNull() && AssetObject.IsValidAsset;

        public virtual void FixAssetInstanceIfInvalid(bool saveAssets = true)
        {
            if ( IsValidAssetInstance || EditorObjectTracker.IsInPrefabMode(gameObject) )
            {
                return;
            }

            bool assetExist = TryLoadAsset(out string path, out AssetObject);

            if ( !assetExist )
            {
                CreateAssetAndOverrideGuid(saveAssets);
            }
            else
            {
                EditorUtility.SetDirty(this);
                AssetObject.FixAssetIDIfInvalid();
            }
        }

        protected void CreateAssetAndOverrideGuid(bool saveAssets = true)
        {
            CreateAssetInstance(this, out AssetObject, saveAssets);
            AssetObject.OverWriteGuidInMetaFile(_editorObjectTracker.AssetGuid, out string path);

            if ( saveAssets )
            {
                PublishAssetChanges(path);
            }
            if ( TryLoadAsset(out path, out AssetObject) )
            {
                EditorUtility.SetDirty(this);
            }
        }

        protected bool TryLoadAsset(out string path, out T asset)
        {
            return UnityObjectEditorHelper.TryLoadAsset(_editorObjectTracker.AssetGuid, out path, out asset);
        }

        protected virtual void PublishAssetChanges(string linkedAssetPath)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            if ( !gameObject.scene.isLoaded )
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath(AssetObject);
            if ( !string.IsNullOrEmpty(path) )
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
        protected override void OnGameObjectNameChanged(string oldName)
        {
            if ( AssetObject.IsNull() )
            {
                return;
            }
            ApplyGeneratedName();
        }
        private void ApplyGeneratedName()
        {
            string currentPathName = AssetDatabase.GetAssetPath(AssetObject);
            string newName         = GeneratedName;

            string errorMessage = AssetDatabase.RenameAsset(currentPathName, newName);
            if ( !string.IsNullOrEmpty(errorMessage) )
            {
                Debug.LogWarning($"Rename Asset failed  with name: '{newName}' at path: '{currentPathName}'  with error message: {errorMessage}", this);
            }
        }
        protected override void OnDuplicate()
        {
            CreateAssetInstance();
        }
        protected override void OnCreateComponent()
        {
            CreateAssetInstance();
        }
        private void CreateAssetInstance()
        {
            CreateAssetInstance(this, out AssetObject);
            _editorObjectTracker.SetAssetGuid(AssetObject);
            ApplyGeneratedName(); //to apply the new guid to the name
        }

#endif
    }

    public interface IAssetDirector {
        string GeneratedFolder { get; }
        string GeneratedName   { get; }
    }
}