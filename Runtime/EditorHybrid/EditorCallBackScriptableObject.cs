using UnityEngine;
namespace Drboum.Utilities.Runtime.EditorHybrid {
    public abstract class EditorCallBackScriptableObject<T> : ScriptableObject where T : EditorCallBackMonoBehaviour<T> {
        protected abstract T self { get; }
        protected virtual void Awake() => MonoBehaviourEditorEventTracker<T>.InvokeOnAwake(self);
        protected virtual void OnEnable() => MonoBehaviourEditorEventTracker<T>.InvokeOnEnable(self);
        protected virtual void OnValidate() => MonoBehaviourEditorEventTracker<T>.InvokeOnValidate(self);
        protected virtual void OnDisable() => MonoBehaviourEditorEventTracker<T>.InvokeOnDisable(self);
        protected virtual void OnDestroy() => MonoBehaviourEditorEventTracker<T>.InvokeOnDestroy(self);
    }
}