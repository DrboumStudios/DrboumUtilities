using System;
using System.Collections.Concurrent;
using UnityEngine;
namespace DrboumLibrary {
    [ExecuteAlways]
    public class GizmoHandler : MonoBehaviour {
#if UNITY_EDITOR
        private static readonly MyGizmo _holder;

        static GizmoHandler()
        {
            if ( _holder == null ) {
                _holder = new MyGizmo();
            }
        }

        public static void GizmoOneFrame(Action oneTimeAction)
        {
            _holder?.GizmoOneFrame(oneTimeAction);
        }

        private void OnDrawGizmos()
        {
            while ( _holder.OnGizmo.Count > 0 ) {
                Action a;
                _holder.OnGizmo.TryDequeue(out a);
                a.Invoke();
            }
        }

#endif
    }

    public class MyGizmo {
        internal ConcurrentQueue<Action> OnGizmo;

        public MyGizmo()
        {
            OnGizmo = new ConcurrentQueue<Action>();
        }

        public void GizmoOneFrame(Action oneTimeAction)
        {
            OnGizmo.Enqueue(oneTimeAction);
        }
    }
}