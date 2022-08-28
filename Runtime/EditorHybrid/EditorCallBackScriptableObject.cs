using UnityEngine;
namespace Drboum.Utilities.Runtime.EditorHybrid {
    public abstract class EditorCallBackScriptableObject<T> : ScriptableObject where T : EditorCallBackScriptableObject<T>
    {
        protected T self => this as T; 
        protected virtual void Awake() => EditorObjectsEventCallBacks<T>.InvokeOnAwake(self);
        protected virtual void OnEnable() => EditorObjectsEventCallBacks<T>.InvokeOnEnable(self);
        protected virtual void OnValidate() => EditorObjectsEventCallBacks<T>.InvokeOnValidate(self);
        protected virtual void OnDisable() => EditorObjectsEventCallBacks<T>.InvokeOnDisable(self);
        protected virtual void OnDestroy() => EditorObjectsEventCallBacks<T>.InvokeOnDestroy(self);
    }
}