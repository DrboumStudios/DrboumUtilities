// using Drboum.Utilities.Runtime;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;
//
// namespace Drboum.Utilities.Editor
// {
//     [CustomPropertyDrawer(typeof(GuidWrapper))]
//     public class GuidWrapperPropertyDrawer : PropertyDrawer
//     {
//         public override VisualElement CreatePropertyGUI(SerializedProperty property)
//         {
//             // Create property container element.
//             var container = new VisualElement();
//
//             // Create property fields.
//             var guidDisplay = new PropertyField(property);
//             // Add fields to the container.
//             container.Add(guidDisplay);
//
//             return container;
//         }
//     }
// }