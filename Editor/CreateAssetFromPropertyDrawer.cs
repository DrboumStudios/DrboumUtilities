using System;
using System.Reflection;
using Drboum.Utilities.Attributes;
using Drboum.Utilities.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Drboum.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class CreateScriptableObjectByDefaultFromPropertyDrawer : CreateAssetFromPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Object parentObject = GetPropertyData(property, out var propertyFieldInfo, out var createButtonAttribute);
            // we have an attribute that overrides the default behavior -> let the other drawer do its job
            if ( createButtonAttribute != null )
                return base.CreatePropertyGUI(property);

            return BuildVisualElements(property, parentObject, propertyFieldInfo?.FieldType, default(DefaultCreateScriptableObjectInstance), default(DefaultSavePersistentAsset));
        }
    }

    [CustomPropertyDrawer(typeof(CreateAssetFromPropertyAttribute))]
    public class CreateAssetFromPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Object parentObject = GetPropertyData(property, out var field, out var createButtonAttribute);
            ICreateAsset createAssetImplem = createButtonAttribute.GetInstanceCreator(parentObject);
            ISavePersistentAsset iSavePersistentAsset = createButtonAttribute.GetConfigurePersistentAsset(parentObject);

            return BuildVisualElements(property, parentObject, field?.FieldType, createAssetImplem, iSavePersistentAsset, !(typeof(ScriptableObject).IsAssignableFrom(field.FieldType) && GetType() == typeof(CreateAssetFromPropertyDrawer)));
        }

        protected static Object GetPropertyData(SerializedProperty property, out FieldInfo propertyFieldInfo, out CreateAssetFromPropertyAttribute createButtonAttribute)
        {
            Object parentObject = property.serializedObject.targetObject;

            propertyFieldInfo = parentObject.GetType().GetField(property.propertyPath,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            createButtonAttribute = propertyFieldInfo?.GetCustomAttribute<CreateAssetFromPropertyAttribute>();
            return parentObject;
        }

        protected static VisualElement BuildVisualElements<TCreateAsset, TSaveAsset>(SerializedProperty property, Object parentObject, Type fieldType, TCreateAsset createAssetImplem, TSaveAsset iSavePersistentAsset, bool displayButton = true)
            where TCreateAsset : ICreateAsset
            where TSaveAsset : ISavePersistentAsset
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
            if ( displayButton && fieldType != null )
            {
                var createButton = new Button(() =>
                {
                    CreateNewInstance(property, parentObject, fieldType, createAssetImplem, iSavePersistentAsset);
                }) {
                    text = "+",
                    style = { width = 30, marginLeft = 2 },
                    name = "unity-input-unlockableAsset",
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
            }
            return container;
        }

        private static void CreateNewInstance<TCreateAsset, TSaveAsset>(SerializedProperty property, Object parentObject, Type instanceType, TCreateAsset createAsset, TSaveAsset saveAsset)
            where TCreateAsset : ICreateAsset
            where TSaveAsset : ISavePersistentAsset
        {
            var newInstance = createAsset.CreateInstance(parentObject, instanceType);
            saveAsset.SaveAsset(parentObject, newInstance);

            property.objectReferenceValue = newInstance;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}