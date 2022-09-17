using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.EditorHybrid;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Drboum.Utilities.Editor
{
    [CustomEditor(typeof(AssetReferenceID), true)]
    public class AssetReferenceIDEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<Type, List<string>> _fieldNamesPerType = new();

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            Type targetType = target.GetType();
            
            List<string> fieldsNames = GetFieldsNameList(targetType);

            //add script field
            SerializedProperty scriptSerializedProperty = serializedObject.FindProperty("m_Script");
            var scriptPropertyField = new PropertyField(scriptSerializedProperty);
            scriptPropertyField.SetEnabled(false);
            container.Add(scriptPropertyField);

            GetGuidWrapperValue(out GuidWrapper guidWrapper);
            container.Add(CreateGuidWrapperUIElements(in guidWrapper));

            for ( int i = 0; i < fieldsNames.Count; i++ )
            {
                var fieldName = fieldsNames[i];
                SerializedProperty fieldProp = serializedObject.FindProperty(fieldName);
                container.Add(new PropertyField(fieldProp));
            }

            return container;
        }

        private static List<string> GetFieldsNameList(Type targetType)
        {
            if ( !_fieldNamesPerType.TryGetValue(targetType, out var fieldsNames) )
            {
                var sw = Stopwatch.StartNew();
                fieldsNames = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField)
                    .Where(fi => (fi.IsPublic || fi.IsDefined(typeof(SerializeField), true)) && fi.Name != nameof(AssetReferenceID._guid))
                    .Select(fi => fi.Name)
                    .ToList();

                Debug.Log($"reflection {nameof(AssetReferenceIDEditor)} took {sw.ElapsedMilliseconds.ToString()} ms");

                _fieldNamesPerType.Add(targetType, fieldsNames);
            }
            return fieldsNames;
        }

        private static VisualElement CreateGuidWrapperUIElements(in GuidWrapper guidWrapper)
        {
            var guidWrapperDisplay = new VisualElement();
            guidWrapperDisplay.style.flexDirection = FlexDirection.Row;

            var guidLabel = new Label("AssetId :");
            guidLabel.style.flexGrow = new StyleFloat(0.7f);

            var guidField = new Label {
                text = (guidWrapper.ToString()),
            };
            guidWrapperDisplay.Add(guidLabel);
            guidWrapperDisplay.Add(guidField);
            return guidWrapperDisplay;
        }

        private void GetGuidWrapperValue(out GuidWrapper guidWrapper)
        {
            var guidProp = serializedObject.FindProperty(nameof(AssetReferenceID._guid));
            SerializedProperty hashValue = guidProp.FindPropertyRelative($"{nameof(GuidWrapper.HashValue)}");
            guidWrapper = default;
            int count = 0;
            foreach ( SerializedProperty o in hashValue )
            {
                guidWrapper.HashValue[count++] = (uint)o.intValue;
            }
        }

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
        // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        // {
        //     // Using BeginProperty / EndProperty on the parent property means that
        //     // prefab override logic works on the entire property.
        //     EditorGUI.BeginProperty(position, label, property);
        //
        //     // Draw label
        //     position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        //
        //     // Don't make child fields be indented
        //     var indent = EditorGUI.indentLevel;
        //     EditorGUI.indentLevel = 0;
        //
        //     // Calculate rects
        //     var amountRect = new Rect(position.x, position.y, 30, position.height);
        //
        //     var t = property.FindPropertyRelative(nameof(GuidWrapper.HashValue)).rectIntValue;
        //
        //     var guid = new GuidWrapper() {
        //         HashValue = new uint4 {
        //             x = (uint)t.x,
        //             y = (uint)t.y,
        //             z = (uint)t.width,
        //             w = (uint)t.height
        //         }
        //     };
        //
        //     // Draw fields - pass GUIContent.none to each so they are drawn without labels
        //     EditorGUI.LabelField(amountRect, guid.ToString());
        //
        //     // Set indent back to what it was
        //     EditorGUI.indentLevel = indent;
        //
        //     EditorGUI.EndProperty();
        // }
    }
}