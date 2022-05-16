using System.Diagnostics;
namespace Drboum.Utilities.Runtime.EditorHybrid {
    public static class EditorObjectsEventCallBacks<T> {
        public delegate void MonoBehaviourCallBack(T instance);

        public static event MonoBehaviourCallBack RegisterOnDisable  = delegate { };
        public static event MonoBehaviourCallBack RegisterOnEnable   = delegate { };
        public static event MonoBehaviourCallBack RegisterOnAwake    = delegate { };
        public static event MonoBehaviourCallBack RegisterOnValidate = delegate { };
        public static event MonoBehaviourCallBack RegisterOnStart    = delegate { };
        public static event MonoBehaviourCallBack RegisterOnDestroy  = delegate { };
        public static event MonoBehaviourCallBack RegisterOnUpdate   = delegate { };

        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnAwake(T instance) => RegisterOnAwake(instance);
        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnEnable(T instance) => RegisterOnEnable(instance);
        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnStart(T instance) => RegisterOnStart(instance);
        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnValidate(T instance) => RegisterOnValidate(instance);
        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnDisable(T instance) => RegisterOnDisable(instance);
        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnDestroy(T instance) => RegisterOnDestroy(instance);
        [Conditional("UNITY_EDITOR")]
        public static void InvokeOnUpdate(T instance) => RegisterOnUpdate(instance);

    }
}