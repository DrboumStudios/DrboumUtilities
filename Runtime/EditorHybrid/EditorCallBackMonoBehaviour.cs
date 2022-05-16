using UnityEngine;
namespace Drboum.Utilities.Runtime.EditorHybrid {
    public abstract class EditorCallBackMonoBehaviour<T> : MonoBehaviour where T : EditorCallBackMonoBehaviour<T> {
        protected abstract T self { get; }
        protected virtual void Awake() => EditorObjectsEventCallBacks<T>.InvokeOnAwake(self);
        protected virtual void OnEnable() => EditorObjectsEventCallBacks<T>.InvokeOnEnable(self);
        protected virtual void Start() => EditorObjectsEventCallBacks<T>.InvokeOnStart(self);
        protected virtual void OnDisable() => EditorObjectsEventCallBacks<T>.InvokeOnDisable(self);
        protected virtual void OnValidate() => EditorObjectsEventCallBacks<T>.InvokeOnValidate(self);
        protected virtual void OnDestroy() => EditorObjectsEventCallBacks<T>.InvokeOnDestroy(self);
        protected virtual void Update() => EditorObjectsEventCallBacks<T>.InvokeOnUpdate(self);
    }
}