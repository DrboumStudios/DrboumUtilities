using System;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using static Drboum.Utilities.Editor.EditorHelpers;

namespace Drboum.Utilities.Entities
{
    public static class ContextMenuConverter
    {
        [InitializeOnLoadMethod]
        static void OnEnable()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        }

        static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            switch ( property.propertyType )
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Color:
                    break;
                case SerializedPropertyType.ObjectReference:
                    break;
                case SerializedPropertyType.Vector2:
                    TryParseAndAddToMenu<float2>(menu, property, (propertyCopy, parsedClipboardData) =>
                    {
                        propertyCopy.vector2Value = parsedClipboardData;
                    });
                    break;
                case SerializedPropertyType.Vector3:
                    TryParseAndAddToMenu<float3>(menu, property, (propertyCopy, parsedClipboardData) =>
                    {
                        propertyCopy.vector3Value = parsedClipboardData;
                    });
                    break;
                case SerializedPropertyType.Vector4:
                    TryParseAndAddToMenu<float4>(menu, property, (propertyCopy, parsedClipboardData) =>
                    {
                        propertyCopy.vector4Value = parsedClipboardData;
                    });
                    break;
                case SerializedPropertyType.Quaternion:
                    TryParseAndAddToMenu<quaternion>(menu, property, (propertyCopy, parsedClipboardData) =>
                    {
                        propertyCopy.quaternionValue = parsedClipboardData;
                    }, "Rotation");
                    break;
                case SerializedPropertyType.Bounds:
                    //TODO parse bounds types?
                    break;
                case SerializedPropertyType.Vector2Int:
                    TryParseAndAddToMenu<int2>(menu, property, (propertyCopy, parsedClipboardData) =>
                    {
                        propertyCopy.vector2IntValue = new Vector2Int(parsedClipboardData.x, parsedClipboardData.y);
                    });
                    break;
                case SerializedPropertyType.Vector3Int:
                    TryParseAndAddToMenu<int3>(menu, property, (propertyCopy, parsedClipboardData) =>
                    {
                        propertyCopy.vector3IntValue = new Vector3Int(parsedClipboardData.x, parsedClipboardData.y);
                    });
                    break;
                case SerializedPropertyType.Hash128:
                    break;
            }
        }

        private static bool TryParseAndAddToMenu<T>(GenericMenu menu, SerializedProperty property, Action<SerializedProperty, T> applyPropertyDataMethod)
        {
            return TryParseAndAddToMenu(menu, property, applyPropertyDataMethod, typeof(T).Name);
        }

        private static bool TryParseAndAddToMenu<T>(GenericMenu menu, SerializedProperty property, Action<SerializedProperty, T> applyPropertyDataMethod, string displayString)
        {
            if ( !TryParseClipBoard(out T parsedClipboardData) )
                return false;

            var propertyCopy = property.Copy();
            menu.AddItem(new GUIContent($"Paste {displayString}"), false, () =>
            {
                applyPropertyDataMethod(propertyCopy, parsedClipboardData);
                propertyCopy.serializedObject.ApplyModifiedProperties();
            });
            return true;
        }

        public static bool TryParseClipBoard<T>(out T parsedClipboardData)
        {
            parsedClipboardData = default;
            try
            {
                parsedClipboardData = ParseClipboardText<T>();
            }
            catch
            {
                return false;
            }
            return true;
        }

        [MenuItem("CONTEXT/Transform/ApplyDOTSTransform", true)]
        public static bool ValidateCanDOTSMatrixOnTransform(MenuCommand command)
        {
            var selection = command.context as Transform;
            return (selection != null) && (TryParseClipBoard(out LocalToWorld localToWorld) || TryParseClipBoard(out float4x4 matrix));
        }

        [MenuItem("CONTEXT/Transform/ApplyDOTSTransform")]
        public static void ApplyDOTSMatrixOnTransform(MenuCommand command)
        {
            var selection = (Transform)command.context;
            Undo.RecordObject(selection, $"{nameof(ApplyDOTSMatrixOnTransform)} on {selection.name}");
            if ( TryParseClipBoard(out LocalToWorld localToWorld) )
            {
                selection.SetPositionAndRotation(localToWorld.Position, localToWorld.Rotation);
                selection.localScale = localToWorld.Value.Scale();
            }
            else if ( TryParseClipBoard(out float4x4 matrix) )
            {
                selection.SetPositionAndRotation(matrix.c3.xyz, matrix.Rotation());
                selection.localScale = matrix.Scale();
            }
        }
    }
}
