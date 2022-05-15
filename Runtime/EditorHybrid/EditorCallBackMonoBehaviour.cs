using UnityEngine;
namespace Drboum.Utilities.Runtime.EditorHybrid {
    public abstract class EditorCallBackMonoBehaviour<T> : MonoBehaviour where T : EditorCallBackMonoBehaviour<T> {
        protected abstract T self { get; }
        protected virtual void Awake() => EditorObjectsEventTracker<T>.InvokeOnAwake(self);
        protected virtual void OnEnable() => EditorObjectsEventTracker<T>.InvokeOnEnable(self);
        protected virtual void Start() => EditorObjectsEventTracker<T>.InvokeOnStart(self);
        protected virtual void OnDisable() => EditorObjectsEventTracker<T>.InvokeOnDisable(self);
        protected virtual void OnValidate() => EditorObjectsEventTracker<T>.InvokeOnValidate(self);
        protected virtual void OnDestroy() => EditorObjectsEventTracker<T>.InvokeOnDestroy(self);
        protected virtual void Update() => EditorObjectsEventTracker<T>.InvokeOnUpdate(self);
    }
}