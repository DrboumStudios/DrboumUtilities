using Drboum.Utilities.Attributes;
using UnityEditor;
using UnityEngine;

namespace Drboum.Utilities.Editor.Attributes {

    /// <summary>
    /// This class allows the display of a message box (warning, info, error...) next to a property (before or after)
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorMessageAttribute))]
    public class InspectorMessageAttributeDrawer : PropertyDrawer {

        // determines the space after the help box, the space before the text box, and the width of the help box icon
        private const int SPACE_BEFORE_THE_TEXT_BOX = 5;
        private const int SPACE_AFTER_THE_TEXT_BOX  = 10;
        private const int ICON_WIDTH                = 55;

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

                textFieldPosition.y      += helpPosition.height + SPACE_BEFORE_THE_TEXT_BOX;
                textFieldPosition.height =  GetPropertyHeight(prop, label);
            }
            else {
                // we position the property first, then the message
                textFieldPosition.height = GetPropertyHeight(prop, label);

                helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);
                // we add the complete property height (property + helpbox, as overridden in this very script), and substract both to get just the property
                helpPosition.y += GetPropertyHeight(prop, label)                       -
                                  DetermineTextboxHeight(informationAttribute.Message) - SPACE_AFTER_THE_TEXT_BOX;
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
                   SPACE_AFTER_THE_TEXT_BOX                  + SPACE_BEFORE_THE_TEXT_BOX;
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

            float newHeight = style.CalcHeight(new GUIContent(message), EditorGUIUtility.currentViewWidth - ICON_WIDTH);
            return newHeight;
        }
    }
}