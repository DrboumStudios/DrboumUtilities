using System.Diagnostics;

namespace Drboum.Utilities.EditorHybrid {
    public static class EditorObjectsEventCallBacks<T> {
        public delegate void ObjectInstanceCallBack(T instance);

        public static event ObjectInstanceCallBack RegisterOnDisable  = delegate { };
        public static event ObjectInstanceCallBack RegisterOnEnable   = delegate { };
        public static event ObjectInstanceCallBack RegisterOnAwake    = delegate { };
        public static event ObjectInstanceCallBack RegisterOnValidate = delegate { };
        public static event ObjectInstanceCallBack RegisterOnStart    = delegate { };
        public static event ObjectInstanceCallBack RegisterOnDestroy  = delegate { };
        public static event ObjectInstanceCallBack RegisterOnUpdate   = delegate { };

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