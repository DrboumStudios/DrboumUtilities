using System.Diagnostics;
using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEngine;

namespace Drboum.Utilities.Editor
{
    public class AssetReferenceIDBaseManager<TAssetInstance>
        where TAssetInstance : Object, IAssetReferenceID
    {

        private static AssetReferenceIDBaseManager<TAssetInstance> _instance;
        internal static AssetReferenceIDBaseManager<TAssetInstance> Instance => _instance;
        

        static AssetReferenceIDBaseManager()
        {
            _instance = new ();
            _instance.Initialize();
        }
        protected AssetReferenceIDBaseManager()
        {
        }

        protected virtual void Initialize()
        {
            InitializeCallBacks();
        }

        protected virtual void InitializeCallBacks()
        {
            EditorObjectsEventCallBacks<TAssetInstance>.RegisterOnAwake += Awake;
            EditorObjectsEventCallBacks<TAssetInstance>.RegisterOnEnable += OnEnable;
            EditorObjectsEventCallBacks<TAssetInstance>.RegisterOnValidate += OnValidate;
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
            if ( instance.Guid.IsValid )
                return;
            
            GenerateAndAssignNewGuid(instance);
        }

        internal void GenerateAndAssignNewGuid(TAssetInstance instance)
        {
            GuidWrapper union = default;
            instance.TryGetAssetGuid(out union.GuidValue);
            if ( instance.Guid.GuidValue == union.GuidValue )
                return;
            
            instance.Guid = union;
            instance.SetDirtySafe();
        }

        protected virtual void OnValidate(TAssetInstance instance)
        {
            FixAssetIDIfInvalid(instance);
        }
    }

    public static class AssetReferenceIDEditorExtensions
    {
        [Conditional("UNITY_EDITOR")]
        public static void FixAssetIDIfInvalid<TAssetInstance>(this TAssetInstance instance)
            where TAssetInstance : Object, IAssetReferenceID
        {
            AssetReferenceIDBaseManager<TAssetInstance>.Instance.FixAssetIDIfInvalid(instance);
        }
    }
}