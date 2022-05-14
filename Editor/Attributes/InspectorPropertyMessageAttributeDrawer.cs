﻿#if UNITY_EDITOR
using DrboumLibrary.Attributes;
using UnityEditor;
using UnityEngine;
namespace DrboumLibrary.Editors.Attributes {

    [CustomPropertyDrawer(typeof(InspectorMessageAttribute))]
    /// <summary>
    /// This class allows the display of a message box (warning, info, error...) next to a property (before or after)
    /// </summary>
    public class InspectorMessageAttributeDrawer : PropertyDrawer {

        // determines the space after the help box, the space before the text box, and the width of the help box icon
        private const int spaceBeforeTheTextBox = 5;

        private const int spaceAfterTheTextBox = 10;
        private const int iconWidth            = 55;

        private InspectorMessageAttribute informationAttribute => (InspectorMessageAttribute)attribute;

        /// <summary>
        ///     OnGUI, displays the property and the textbox in the specified order
        /// </summary>
        /// <param name="rect">Rect.</param>
        /// <param name="prop">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            EditorStyles.helpBox.richText = true;
            Rect helpPosition      = rect;
            Rect textFieldPosition = rect;

            if ( !informationAttribute.MessageAfterProperty ) {
                // we position the message before the property
                helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);

                textFieldPosition.y      += helpPosition.height + spaceBeforeTheTextBox;
                textFieldPosition.height =  GetPropertyHeight(prop, label);
            }
            else {
                // we position the property first, then the message
                textFieldPosition.height = GetPropertyHeight(prop, label);

                helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);
                // we add the complete property height (property + helpbox, as overridden in this very script), and substract both to get just the property
                helpPosition.y += GetPropertyHeight(prop, label)                       -
                                  DetermineTextboxHeight(informationAttribute.Message) - spaceAfterTheTextBox;
            }

            EditorGUI.HelpBox(helpPosition, informationAttribute.Message, informationAttribute.Type);
            EditorGUI.PropertyField(textFieldPosition, prop, label, true);
        }

        /// <summary>
        ///     Returns the complete height of the whole block (property + help text)
        /// </summary>
        /// <returns>The block height.</returns>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) + DetermineTextboxHeight(informationAttribute.Message) +
                   spaceAfterTheTextBox                  + spaceBeforeTheTextBox;
        }

        /// <summary>
        ///     Determines the height of the textbox.
        /// </summary>
        /// <returns>The textbox height.</returns>
        /// <param name="message">Message.</param>
        protected virtual float DetermineTextboxHeight(string message)
        {
            var style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;

            float newHeight = style.CalcHeight(new GUIContent(message), EditorGUIUtility.currentViewWidth - iconWidth);
            return newHeight;
        }
    }
}

#endif