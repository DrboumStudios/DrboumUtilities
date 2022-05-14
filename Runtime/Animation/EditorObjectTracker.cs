using System;
using DrboumLibrary.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif
namespace DrboumLibrary.Animation {
    /// <summary>
    ///     Detect if a gameobject is being copied or duplicated while identifying it uniquely.
    ///     is used to bind a scene object to an asset for example
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class EditorObjectTracker : MonoBehaviour {

#if UNITY_EDITOR
        static EditorObjectTracker()
        {

            EditorApplication.hierarchyWindowItemOnGUI += delegate(int instanceID, Rect selectionRect)
            {
                Event current     = Event.current;
                bool  isCopying   = Equals(current.commandName, "Copy");
                bool  isDuplicate = Equals(current.commandName, "Duplicate");

                if ( current.rawType == EventType.ExecuteCommand && (isDuplicate || isCopying) )
                {

                    if ( !Selection.Contains(instanceID) )
                    {
                        return;
                    }

                    if ( EditorUtility.InstanceIDToObject(instanceID) is GameObject gameObject )
                    {

                        foreach ( EditorObjectTracker objectTracker in gameObject.GetComponentsInChildren<EditorObjectTracker>() )
                        {

                            objectTracker.MarkAsDuplicate();
                        }
                    }
                }
            };
        }

        protected                                      bool   skipDuplication;
        [SerializeField] [InspectorReadOnly] protected int    instanceId;
        [SerializeField] [InspectorReadOnly] protected string assetInstanceGuid;
        [SerializeField] [InspectorReadOnly] private   string assetInstanceReadableName;

        private Action _onDuplicate = CsharpHelper.EmptyDelegate;
        public event Action OnDuplicate {
            add {
                if ( _duplicate )
                {
                    value();
                }
                _onDuplicate += value;

            }
            remove => _onDuplicate -= value;
        }
        private Action _onCreateComponent = CsharpHelper.EmptyDelegate;
        public event Action OnCreateComponent {
            add {
                if ( _created )
                {
                    value();
                }
                _onCreateComponent += value;

            }
            remove => _onCreateComponent -= value;
        }
        public event Action<string> OnGameObjectNameChanged = delegate { };
        public  string              AssetGuid => assetInstanceGuid;
        private bool                _duplicate;
        private bool                _created;

        protected virtual void Start()
        {
            _duplicate = false;
            _created   = false;
        }
        protected virtual void OnValidate()
        {
            if ( Application.isPlaying || !gameObject.scene.isLoaded || this.IsNull() )
            {
                return;
            }
            if ( IsInPrefabMode(gameObject) )
            {
                assetInstanceGuid         = null;
                assetInstanceReadableName = null;
                return;
            }


            if ( !skipDuplication && instanceId != 0 )
            {
                var instanceIDToObject = EditorUtility.InstanceIDToObject(instanceId) as EditorObjectTracker;
                if ( !instanceIDToObject.IsNull() )
                {
                    assetInstanceGuid = null;
                    _duplicate        = true;
                    ClearDuplicateState();
                    instanceIDToObject.ClearDuplicateState();
                    _onDuplicate();
                }
            }
            else if ( string.IsNullOrEmpty(assetInstanceGuid) )
            {
                GenerateAndAssignNewGuid();
                _created = true;
                _onCreateComponent();
            }

        }
        private void GenerateAndAssignNewGuid()
        {
            GenerateNewGuid(out Guid guid);
            assetInstanceGuid = $"{guid.ToString("n")}";
        }
        private void MarkAsDuplicate()
        {
            instanceId      = GetInstanceID();
            skipDuplication = true;
        }
        private void ClearDuplicateState()
        {
            instanceId      = 0;
            skipDuplication = false;
        }
        protected static void GenerateNewGuid(out Guid guid)
        {
            guid = Guid.NewGuid();
            if ( !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid.ToString("n"))) )
            {
                GenerateNewGuid(out guid);
            }
        }

        protected virtual void OnDestroy()
        {
            if ( gameObject.scene.isLoaded )
            {
                instanceId = 0;
            }
        }

        private void Update()
        {

            if ( this.IsNull() || Equals(name, assetInstanceReadableName) )
            {
                return;
            }
            if ( IsInPrefabMode(gameObject) )
            {
                return;
            }
            string old = assetInstanceReadableName;
            assetInstanceReadableName = name;
            OnGameObjectNameChanged(old);
        }

        public void SetAssetGuid<T>(T obj)
            where T : Object
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            if ( !string.IsNullOrEmpty(guid) )
            {
                assetInstanceGuid = guid;
            }
        }
        public static bool IsInPrefabMode(GameObject gameObject)
        {
            PrefabStage currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return !currentPrefabStage.IsNull() && currentPrefabStage.IsPartOfPrefabContents(gameObject);
        }
#endif
    }
}