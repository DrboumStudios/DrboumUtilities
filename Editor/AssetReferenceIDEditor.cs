using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
                
                _fieldNamesPerType.Add(targetType, fieldsNames);
            }
            return fieldsNames;
        }

        private static VisualElement CreateGuidWrapperUIElements(in GuidWrapper guidWrapper)
        {
            var guidWrapperDisplay = new VisualElement();
            guidWrapperDisplay.style.flexDirection = FlexDirection.Row;

            var guidLabel = new Label("AssetId");
            guidLabel.style.flexGrow = new StyleFloat(0.1f);
            guidLabel.style.flexBasis = Length.Percent(41f);
            guidLabel.AddToClassList("unity-base-field");
            var guidField = new Label() {
                text = (guidWrapper.ToString()),
            };
            guidField.isSelectable = true;
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
    }
}