using System;
using System.Diagnostics;
using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.EditorHybrid;
using Drboum.Utilities.Runtime.Interfaces;
using UnityEditor;
using UnityEngine;
namespace Drboum.Utilities.Editor {
    //This importer is an Override Importer for the .asset extension.
    public abstract class AssetReferenceIDBaseManager<TAssetInstance>: AssetPostprocessor
        where TAssetInstance : AssetReferenceID {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for ( var index = 0; index < importedAssets.Length; index++ )
            {
                string str = importedAssets[index];
                var assetReferenceID = AssetDatabase.LoadAssetAtPath<AssetReferenceID>(str);
                if ( assetReferenceID != null )
                {
                    assetReferenceID.FixAssetIDIfInvalid();
                }
            }
        }
        private static  AssetReferenceIDBaseManager<TAssetInstance> _instance;
        internal static AssetReferenceIDBaseManager<TAssetInstance> Instance => _instance;
        protected static void CreateStaticInstance<T>()
            where T : AssetReferenceIDBaseManager<TAssetInstance>,new()
        {
            _instance = new T();
            _instance.Initialize();
        }
        
        protected AssetReferenceIDBaseManager() { }

        protected virtual void Initialize()
        {
            InitializeEditorDuplicateProtection();
            InitializeCallBacks();
        }
        protected virtual void InitializeCallBacks()
        {
            EditorObjectsEventCallBacks<TAssetInstance>.RegisterOnAwake += Awake;
            EditorObjectsEventCallBacks<TAssetInstance>.RegisterOnEnable += OnEnable;
            EditorObjectsEventCallBacks<TAssetInstance>.RegisterOnValidate += OnValidate;
        }
        protected virtual void InitializeEditorDuplicateProtection()
        {
            EditorApplication.projectWindowItemOnGUI += delegate(string guid, Rect selectionRect)
            {
                Event current = Event.current;
                bool isCopying = Equals(current.commandName, "Copy");
                bool isDuplicate = Equals(current.commandName, "Duplicate");

                if ( current.rawType == EventType.ExecuteCommand && (isDuplicate || isCopying) )
                {
                    if ( Selection.activeObject is TAssetInstance )
                        if ( UnityObjectEditorHelper.TryLoadAsset(guid, out var _, out TAssetInstance referenceID) )
                        {
                            MarkAsDuplicateAsset(referenceID);
                            OnDuplicateOrCopyAsset(referenceID);
                        }
                }
            };
        }
        protected virtual void Awake(TAssetInstance instance)
        {
            FixAssetIDIfInvalid(instance);
        }
        protected virtual void OnEnable(TAssetInstance instance)
        {
            FixAssetIDIfInvalid(instance);
        }
        public virtual void FixAssetIDIfInvalid(TAssetInstance instance)
        {
            if ( instance.IsValidGuid )
            {
                return;
            }
            GenerateAndAssignNewGuid(instance);
        }
        protected void GenerateAndAssignNewGuid(TAssetInstance instance)
        {
            GuidWrapper union = default;
            instance.TryGetAssetGuid(out union.GuidValue);
            instance.Guid = union;
            instance.SetDirtySafe();
        }
        
        protected virtual void OnValidate(TAssetInstance instance)
        {
            if ( !instance._skipDuplication && instance.instanceId != 0 )
            {
                var instanceIDToObject = EditorUtility.InstanceIDToObject(instance.instanceId) as TAssetInstance;
                if ( !instanceIDToObject.IsNull() )
                {
                    instance.Guid = default;
                    ClearDuplicateState(instance);
                    ClearDuplicateState(instanceIDToObject);
                }
            }
            FixAssetIDIfInvalid(instance);
        }
        protected virtual void ClearDuplicateState(TAssetInstance instance)
        {
            instance.instanceId = 0;
            instance._skipDuplication = false;
        }
        protected virtual void OnDuplicateOrCopyAsset(TAssetInstance instance)
        { }
        protected static void MarkAsDuplicateAsset(TAssetInstance instance)
        {
            instance.instanceId = instance.GetInstanceID();
            instance._skipDuplication = true;
        }

    }
    public static class AssetReferenceIDEditorExtensions {
        [Conditional("UNITY_EDITOR")]
        public static void FixAssetIDIfInvalid<TAssetID>(this TAssetID instance)
            where TAssetID : AssetReferenceID
        {
            AssetReferenceIDBaseManager<TAssetID>.Instance.FixAssetIDIfInvalid(instance);
        }
    }
}