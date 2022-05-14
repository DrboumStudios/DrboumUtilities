using UnityEngine;
using static CsharpHelper;

namespace DrboumLibrary {
    public class SceneInitializer : MonoBehaviour {

        private void Awake()
        {
            OnAwake?.Invoke(OnAwake);
        }
        private void Start()
        {
            OnStart?.Invoke(OnStart);
        }
        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke(OnDestroyEvent);
        }
        public event FireAndForgetEvent OnAwake;
        public event FireAndForgetEvent OnStart;
        public event FireAndForgetEvent OnDestroyEvent;
    }
}