using System;
using System.Collections.Generic;
using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.Attributes;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace Drboum.Utilities.Editor {
    /// <summary>
    ///     Drawer for the RequireInterface attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer {
        private const string           _pickerWindowTitleDefaultPrefix = "Select ";
        private       List<GameObject> _gameObjectsBuffer;
        private       bool             _needInitialize = true;
        private       EditorWindow     _pickerWindow;
        private       List<Object>     _resultObjectLookupList;
        private       Object           _selected;
        private       bool             _selectedHasChanged;

        private void Initialize(SerializedProperty property)
        {
            _selected               = property.objectReferenceValue;
            _resultObjectLookupList = new List<Object>();
            _gameObjectsBuffer      = new List<GameObject>();
        }


        /// <summary>
        ///     Overrides GUI drawing for the attribute.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if ( _needInitialize ) {
                _needInitialize = false;
                Initialize(property);
            }
            if ( property.propertyType == SerializedPropertyType.ObjectReference ) {



                var requiredAttribute = attribute as RequireInterfaceAttribute;
                if ( requiredAttribute.InterfaceType == null ) {
                    return;
                }
                Type prop = requiredAttribute.PropertyType;
                if ( !typeof(Object).IsAssignableFrom(prop) ) {
                    return;
                }
                EditorGUI.BeginProperty(position, label, property);
                if ( _selectedHasChanged ) {
                    property.objectReferenceValue = _selected;
                    _selectedHasChanged           = false;
                }

                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue,
                    requiredAttribute.InterfaceType, true);


                // Finish drawing property field.
                EditorGUI.EndProperty();
                int id = EditorGUIUtility.GetObjectPickerControlID();

                if ( id != default ) {
                    _pickerWindow = EditorWindow.GetWindow<EditorWindow>();
                    string requiredAttrType = requiredAttribute.InterfaceType.Name;
                    if ( _pickerWindow.titleContent.text.Replace(_pickerWindowTitleDefaultPrefix, "")
                        .Contains(requiredAttrType) ) {

                        Rect pos = _pickerWindow.position;
                        _resultObjectLookupList.Clear();
                        if ( typeof(Component).IsAssignableFrom(prop) ) {
                            SceneManager.GetActiveScene().FindAllInstancesInScene(_resultObjectLookupList,requiredAttribute.InterfaceType);
                        }
                        else if ( typeof(ScriptableObject).IsAssignableFrom(prop) ) {
                            UnityObjectEditorHelper.FindAllAssetInstances<ScriptableObject>(_resultObjectLookupList,
                                requiredAttribute.InterfaceType, requiredAttribute.Folders);
                        }
                        InitializePopupWindow(_resultObjectLookupList, pos, label);
                    }

                }
            }
            else {
                // If field is not reference, show error message.
                Color previousColor = GUI.color;
                GUI.color = Color.red;

                EditorGUI.LabelField(position, label, new GUIContent("Property is not a reference type"));

                // Revert color change.
                GUI.color = previousColor;
            }
        }


        private void InitializePopupWindow(List<Object> components, Rect position, GUIContent label)
        {
            var objectpickup = EditorWindow.GetWindow<InterfaceSerializerEditorPopup>();
            objectpickup.Initialize(components, this, label, position, _selected);
        }

        public void UpdateSelected(Object component)
        {
            _selected           = component;
            _selectedHasChanged = true;
        }
    }

    public class InterfaceSerializerEditorPopup : EditorWindow {
        private const int                    IndexOffset = 1;
        private       int                    _index;
        private       string                 _label;
        private       List<Object>           _optionObjects;
        private       string[]               _options;
        private       RequireInterfaceDrawer _propertyHolder;

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _index = EditorGUI.Popup(
                new Rect(0, 0, position.width, 20),
                _label,
                _index,
                _options);
            if ( EditorGUI.EndChangeCheck() ) {

                if ( _index > 0 ) {
                    _propertyHolder.UpdateSelected(_optionObjects[_index - IndexOffset]);
                }
            }
        }

        public void Initialize(List<Object> components, RequireInterfaceDrawer propertyHolder, GUIContent label,
            Rect                            windowPos,  Object                 selectedComponent)
        {
            if ( components != null ) {

                _optionObjects = components;
                _options       = new string[components.Count + IndexOffset];
                _options[0]    = "(none)";
                if ( components.Count == 0 ) {
                    return;
                }

                for ( var i = 0; i < components.Count; i++ ) {
                    Object item = components[i];
                    _options[i + IndexOffset] = $" {item.name} : ({item.GetType().Name})";
                }
                _index = _optionObjects.IndexOf(selectedComponent) + IndexOffset;

                _propertyHolder = propertyHolder;
                _label          = label.text;
                position        = windowPos;
                Show();
            }
        }
    }
}