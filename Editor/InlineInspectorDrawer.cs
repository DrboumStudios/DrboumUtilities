using Drboum.Utilities.Editor.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Drboum.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(InlineInspectorAttribute))]
    public class InlineInspectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var objectField = new PropertyField(property);
            objectField.Bind(property.serializedObject);
            container.Add(objectField);

            var inspectorContainer = new VisualElement { name = "inspector-container" };
            inspectorContainer.style.marginLeft = 15;
            container.Add(inspectorContainer);

            void UpdateInspector()
            {
                property.serializedObject.Update();
                inspectorContainer.Clear();

                if ( property.objectReferenceValue != null )
                {
                    var inspector = new InspectorElement(property.objectReferenceValue);
                    inspectorContainer.Add(inspector);
                }
            }

            UpdateInspector();
            container.TrackPropertyValue(property, prop => UpdateInspector());

            return container;
        }
    }
}
