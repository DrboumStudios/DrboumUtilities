using System;
using System.Reflection;
using Drboum.Utilities.Attributes;
using Drboum.Utilities.Editor.Attributes;
using Drboum.Utilities.Interfaces;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Drboum.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(CreateAssetFromPropertyAttribute))]
    public class CreateAssetFromPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row
                }
            };

            var objectField = new PropertyField(property) {
                style = {
                    flexGrow = 1
                }
            };
            objectField.Bind(property.serializedObject);
            container.Add(objectField);

            Object parentObject = property.serializedObject.targetObject;
            FieldInfo field = parentObject.GetType().GetField(property.propertyPath,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            var createButtonAttribute = field.GetCustomAttribute<CreateAssetFromPropertyAttribute>();
            Type fieldType = field.FieldType;
            ICreateAsset createAssetImplem = createButtonAttribute.GetInstanceCreator(parentObject);
            ISavePersistentAsset iSavePersistentAsset = createButtonAttribute.GetConfigurePersistentAsset(parentObject);

            var createButton = new Button(() =>
            {
                CreateNewInstance(property, field, parentObject, createAssetImplem, iSavePersistentAsset);
            }) {
                text = "+",
                style = { width = 30, marginLeft = 2 }
            };

            void UpdateButtonVisibility()
            {
                property.serializedObject.Update();
                createButton.style.display = (property.objectReferenceValue == null) && createAssetImplem.CanCreateAsset(parentObject, fieldType)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            UpdateButtonVisibility();
            container.Add(createButton);
            container.TrackPropertyValue(property, prop => UpdateButtonVisibility());

            return container;
        }

        private static void CreateNewInstance(SerializedProperty property, FieldInfo field, Object parentObject, ICreateAsset instanceCreator, [CanBeNull] ISavePersistentAsset savePersistentAsset)
        {
            var newInstance = instanceCreator.CreateInstance(parentObject, field.FieldType);
            savePersistentAsset?.SaveAsset(parentObject, newInstance);

            property.objectReferenceValue = newInstance;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}