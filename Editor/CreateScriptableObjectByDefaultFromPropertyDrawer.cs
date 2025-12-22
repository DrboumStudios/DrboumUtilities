using Drboum.Utilities.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
}