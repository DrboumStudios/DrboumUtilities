using Drboum.Utilities.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Drboum.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(GuidWrapper))]
    public class GuidWrapperPropertyDrawer : PropertyDrawer
    {
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     // Create property container element.
        //     var container = new VisualElement();
        //
        //     // Create property fields.
        //     var guidDisplay = new PropertyField(property);
        //     // Add fields to the container.
        //     container.Add(guidDisplay);
        //
        //     return container;
        // }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var amountRect = new Rect(position.x, position.y, 30, position.height);

            var t = property.stringValue;

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(amountRect, t);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}