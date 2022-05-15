#if UNITY_EDITOR
using Drboum.Utilities.Runtime.Attributes;
using UnityEditor;
using UnityEngine;
namespace Drboum.Utilities.Editor.Attributes {

    [CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute))]
    public class ReadOnlyInspectorAttributeDrawer : PropertyDrawer {

        // Necessary since some properties tend to collapse smaller than their content
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // Draw a disabled property field
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false; // Disable fields
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true; // Enable fields
        }
    }
}

#endif