using System;
using Drboum.Utilities.Runtime.Attributes;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
namespace Drboum.Utilities.Runtime.Animation {
    /// <summary>
    ///     Detect if a gameobject is being copied or duplicated while identifying it uniquely.
    ///     is used to bind a scene object to an asset for example
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class EditorObjectTracker : EditorCallBackMonoBehaviour<EditorObjectTracker> {
        protected override EditorObjectTracker self => this;

#if UNITY_EDITOR
        [SerializeField] [InspectorReadOnly] internal int    instanceId;
        [SerializeField] [InspectorReadOnly] internal string assetInstanceGuid;
        [SerializeField] [InspectorReadOnly] internal string assetInstanceReadableName;
        
        internal                                      bool   skipDuplication;
        internal                                      bool   _duplicate;
        internal                                      bool   _created;

        public                                        string AssetGuid => assetInstanceGuid;
        internal Action _onDuplicate = CsharpHelper.EmptyDelegate;
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
        internal Action _onCreateComponent = CsharpHelper.EmptyDelegate;
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
        internal Action<string> _onGameObjectNameChanged = delegate { };

        public event Action<string> OnGameObjectNameChanged {
            add { _onGameObjectNameChanged += value; }
            remove { _onGameObjectNameChanged -= value; }
        }

 
      
        public static bool IsInPrefabMode(GameObject gameObject)
        {
            PrefabStage currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return !currentPrefabStage.IsNull() && currentPrefabStage.IsPartOfPrefabContents(gameObject);
        }
#endif
    }
}