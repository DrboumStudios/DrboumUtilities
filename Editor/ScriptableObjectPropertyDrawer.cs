#if UNITY_EDITOR
using Drboum.Utilities.EditorHybrid;
using UnityEditor;
using UnityEngine;
namespace Drboum.Utilities.Editor {
    [CustomPropertyDrawer(typeof(EditableScriptableObject), true)]
    public class ScriptableObjectPropertyDrawer : PropertyDrawer {
        private UnityEditor.Editor _editor;

        protected bool OpenNewWindow {
            get;
            set;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);

            if ( property.objectReferenceValue == null ) {
                return;
            }

            if ( !_editor ) {
                Initialize(property, position);
            }

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);

            if ( !property.isExpanded ) {
                return;
            }

            if ( OpenNewWindow ) {
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("Expanded Drawer Property", new GUIStyle {
                    fontSize = 14
                });
                EditorGUILayout.LabelField("");
            }
            EditorGUI.indentLevel++;
            _editor.OnInspectorGUI();
            EditorGUI.indentLevel--;

        }

        protected virtual void Initialize(SerializedProperty property, Rect position)
        {
            property.isExpanded = EditorGUI.Foldout(position, false, GUIContent.none);
            _editor             = UnityEditor.Editor.CreateEditor(property.objectReferenceValue, null);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviorUpdatedEditor : UnityEditor.Editor { }
}
#endif