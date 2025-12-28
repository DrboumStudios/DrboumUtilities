using Drboum.Utilities.Attributes;
using Drboum.Utilities.Interfaces;
using UnityEngine;

namespace Drboum.Utilities.EditorHybrid {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(EditorObjectTracker))]
    [ExecuteInEditMode]
    public abstract class AssetObjectDirector<T> : EditorCallBackMonoBehaviour<AssetObjectDirector<T>>, IAuthoring
        where T : AssetReferenceID, IInitializable<IAuthoring>  {

        [SerializeField] [InspectorReadOnly] public T AssetObject;
#if UNITY_EDITOR
        internal EditorObjectTracker _editorObjectTracker;
        internal bool                _subscribed;
#endif
    }
}