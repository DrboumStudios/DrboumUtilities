using DrboumLibrary;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace GameProject.Utils {
    public interface IAssetReferenceID {
        GuidWrapper Guid {
            get;
        }
        bool IsValidAsset {
            get;
        }
#if UNITY_EDITOR
        void FixAssetIDIfInvalid();
#endif
    }

    public abstract class AssetReferenceID : ScriptableObject, IAssetReferenceID {
#if UNITY_EDITOR
      static AssetReferenceID()
        {

            EditorApplication.projectWindowItemOnGUI += delegate(string guid, Rect selectionRect)
            {
                Event current     = Event.current;
                bool  isCopying   = Equals(current.commandName, "Copy");
                bool  isDuplicate = Equals(current.commandName, "Duplicate");

                if ( current.rawType == EventType.ExecuteCommand && (isDuplicate || isCopying) )
                {
                    if ( Selection.activeObject is AssetReferenceID )
                        if ( UnityObjectEditorHelper.TryLoadAsset(guid, out var _, out AssetReferenceID referenceID) )
                        {
                            referenceID.MarkAsDuplicate();
                        }
                }
            };
        }

#endif
        [SerializeField] [HideInInspector] protected uint4 _guid;
        [SerializeField] [HideInInspector] private   int     instanceId;
        private                                      bool    _skipDuplication;

        protected      bool        IsValidGuid  => !_guid.Equals(default);
        public         GuidWrapper Guid         => _guid;
        public virtual bool        IsValidAsset => IsValidGuid;
#if UNITY_EDITOR


        protected virtual void Awake()
        {
            FixAssetIDIfInvalid();
        }
        protected virtual void OnEnable()
        {
            FixAssetIDIfInvalid();
        }
        public virtual void FixAssetIDIfInvalid()
        {

            if ( IsValidGuid )
            {
                return;
            }
            GenerateAndAssignNewGuid();
        }
        private void GenerateAndAssignNewGuid()
        {
            GuidWrapper union = default;
            this.TryGetAssetGuid(out union.GuidValue);
            _guid = union;
            EditorUtility.SetDirty(this);
        }
        [ContextMenu(nameof(PrintGUIDAsGuidWrapper))]
        protected void PrintGUIDAsGuidWrapper()
        {
            LogHelper.LogInfoTypedMessage(Guid, $"{name}");
        }
        protected virtual void OnValidate()
        {
            if ( !_skipDuplication && instanceId != 0 )
            {
                var instanceIDToObject = EditorUtility.InstanceIDToObject(instanceId) as AssetReferenceID;
                if ( !instanceIDToObject.IsNull() )
                {
                    _guid = default;
                    ClearDuplicateState();
                    instanceIDToObject.ClearDuplicateState();
                }
            }
            FixAssetIDIfInvalid();
        }
        private void ClearDuplicateState()
        {
            instanceId       = 0;
            _skipDuplication = false;
        }
        private void MarkAsDuplicate()
        {
            instanceId       = GetInstanceID();
            _skipDuplication = true;
        }
#endif
    }
}