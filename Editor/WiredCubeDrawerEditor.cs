#if UNITY_EDITOR && SHAPES_URP
using Drboum.Utilities.Rendering;
using UnityEditor;

namespace Drboum.Utilities.Editor
{
    [CustomEditor(typeof(WiredCubeDrawer))]
    [CanEditMultipleObjects]
    public class WiredCubeDrawerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            foreach ( var obj in targets )
            {
                var wiredCubeDrawer = obj as WiredCubeDrawer;
                wiredCubeDrawer._debugDisplay = true;
            }
        }

        private void OnEnable()
        { }

        private void OnDisable()
        {
            foreach ( var obj in targets )
            {
                var wiredCubeDrawer = obj as WiredCubeDrawer;
                wiredCubeDrawer._debugDisplay = false;
            }
        }
    }
}
#endif