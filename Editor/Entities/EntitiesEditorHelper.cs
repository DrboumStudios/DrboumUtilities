using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using Unity.Physics;
#if UNITY_PHYSICS_CUSTOM
     using Unity.Physics.Authoring;
#endif
using Unity.Scenes;
using Unity.Transforms;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Drboum.Utilities.Editor.EditorHelpers;
using BoxCollider = UnityEngine.BoxCollider;
using CapsuleCollider = UnityEngine.CapsuleCollider;
using MeshCollider = UnityEngine.MeshCollider;
using SphereCollider = UnityEngine.SphereCollider;

public static class EntitiesEditorHelper
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
#if UNITY_PHYSICS_CUSTOM
    [MenuItem("Tools/" + nameof(ConvertOnSelectedColliderToShapeAuthoring))]
    private static void ConvertOnSelectedColliderToShapeAuthoring()
    {
        var allColliderGameObjects = new List<GameObject>(128);
        var colliderTempBuffer = new List<UnityEngine.Collider>();
        for ( var index = 0; index < Selection.gameObjects.Length; index++ )
        {
            var selectedGameObject = Selection.gameObjects[index];
            if ( selectedGameObject.TryGetComponent(out SubScene subScene) )
            {
                subScene.EditingScene.FindAllInstancesInScene(colliderTempBuffer);
            }
            else
            {
                selectedGameObject.GetComponentsInChildren(true, colliderTempBuffer);
            }
            allColliderGameObjects.AddRange(colliderTempBuffer.Select(o => o.gameObject));
            colliderTempBuffer.Clear();
        }
        if ( allColliderGameObjects.Count == 0 )
        {
            SceneManager.GetActiveScene().FindAllInstancesInScene(colliderTempBuffer);
        }
        var logStringBuilder = new StringBuilder(allColliderGameObjects.Count * 50);
        logStringBuilder.AppendLine($"Starting conversion process. click to see more detail : ");
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName($"{nameof(ConvertOnSelectedColliderToShapeAuthoring)}");
        for ( var index = 0; index < allColliderGameObjects.Count; index++ )
        {
            var colliderGameObject = allColliderGameObjects[index];
            ConvertUnityEngineColliderToShapeAuthoring(colliderGameObject, logStringBuilder);
        }
        LogHelper.LogInfoMessage(logStringBuilder.ToString(), $"{nameof(ConvertOnSelectedColliderToShapeAuthoring)}");
    }

    public static void ApplyDefaultPresetIfExist(this PhysicsShapeAuthoring shapeAuthoring)
    {
        var presets = Preset.GetDefaultPresetsForObject(shapeAuthoring);
        if ( presets.Length != 0 )
        {
            presets[^1].ApplyTo(shapeAuthoring);
        }
    }

    public static void ConvertUnityEngineColliderToShapeAuthoring(GameObject colliderGameObject, StringBuilder logStringBuilder = null)
    {
        if ( !colliderGameObject.TryGetComponent(out UnityEngine.Collider collider) )
            return;

        ConvertUnityEngineColliderToShapeAuthoring(colliderGameObject, collider, logStringBuilder);
    }

    private static void ConvertUnityEngineColliderToShapeAuthoring(GameObject colliderGameObject, UnityEngine.Collider collider, StringBuilder logStringBuilder)
    {
        bool successfulConversion = true;
        if ( !colliderGameObject.TryGetComponent(out PhysicsShapeAuthoring addedPhysicsShape) )
        {
            addedPhysicsShape = Undo.AddComponent<PhysicsShapeAuthoring>(colliderGameObject);
            var presetsForObject = Preset.GetDefaultPresetsForObject(addedPhysicsShape);
            if ( presetsForObject.Length > 0 )
            {
                presetsForObject[^1].ApplyTo(addedPhysicsShape);
            }
            var defaultOrientation = quaternion.identity;
            switch ( collider )
            {
                case BoxCollider boxCollider:
                    addedPhysicsShape.SetBox(new BoxGeometry {
                        Size = boxCollider.size,
                        Center = boxCollider.center,
                        BevelRadius = .05f,
                        Orientation = defaultOrientation, //no equivalent in monobehaviour land
                    });
                    break;
                case CapsuleCollider capsuleCollider:
                    addedPhysicsShape.SetCapsule(new CapsuleGeometryAuthoring {
                        Orientation = defaultOrientation, //no equivalent in monobehaviour land
                        Center = capsuleCollider.center,
                        Radius = capsuleCollider.radius
                    });
                    break;
                case MeshCollider meshCollider:
                    Mesh assetObject = meshCollider.GetComponentInChildren<MeshFilter>().sharedMesh;
                    if ( !assetObject.isReadable )
                    {
                        successfulConversion = false;
                        LogHelper.LogErrorMessage($"Attempt to convert a mesh collider with a non readable mesh ('{assetObject.name}' isReadable: false) aborting on collider '{collider.name}'", nameof(ConvertOnSelectedColliderToShapeAuthoring), assetObject);
                    }
                    else
                    {
                        if ( meshCollider.convex )
                        {
                            addedPhysicsShape.SetConvexHull(ConvexHullGenerationParameters.Default);
                        }
                        else
                        {
                            addedPhysicsShape.SetMesh();
                        }
                    }
                    break;
                case SphereCollider sphereCollider:
                    addedPhysicsShape.SetSphere(new SphereGeometry {
                        Center = sphereCollider.center,
                        Radius = sphereCollider.radius
                    }, defaultOrientation);
                    break;
                default:
                    LogHelper.LogDebugWarningMessage($"Attempt to convert unsupported collider of type {collider.GetType().Name}", nameof(ConvertUnityEngineColliderToShapeAuthoring), collider);
                    successfulConversion = false;
                    addedPhysicsShape.RemoveComponent();
                    break;
            }
        }
        if ( successfulConversion )
        {
            logStringBuilder?.AppendLine($"Successfully converted collider on '{collider.name}'");
            Undo.DestroyObjectImmediate(collider);
        }
    }
#endif
}