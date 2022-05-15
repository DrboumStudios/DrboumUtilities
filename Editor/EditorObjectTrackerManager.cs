﻿using System;
using Drboum.Utilities.Runtime.Animation;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
using UnityEngine;
namespace Drboum.Utilities.Editor {
    public static class EditorObjectTrackerManager {

        static EditorObjectTrackerManager()
        {
            EditorApplication.hierarchyWindowItemOnGUI += delegate(int instanceID, Rect selectionRect)
            {
                Event current = Event.current;
                bool isCopying = Equals(current.commandName, "Copy");
                bool isDuplicate = Equals(current.commandName, "Duplicate");

                if ( current.rawType == EventType.ExecuteCommand && (isDuplicate || isCopying) )
                {

                    if ( !Selection.Contains(instanceID) )
                    {
                        return;
                    }

                    if ( EditorUtility.InstanceIDToObject(instanceID) is GameObject gameObject )
                    {

                        foreach ( EditorObjectTracker objectTracker in gameObject.GetComponentsInChildren<EditorObjectTracker>() )
                        {
                            MarkAsDuplicate(objectTracker);
                        }
                    }
                }
            };
            EditorObjectsEventTracker<EditorObjectTracker>.RegisterOnStart += Start;
            EditorObjectsEventTracker<EditorObjectTracker>.RegisterOnValidate += OnValidate;
            EditorObjectsEventTracker<EditorObjectTracker>.RegisterOnUpdate += Update;
            EditorObjectsEventTracker<EditorObjectTracker>.RegisterOnDestroy += OnDestroy;
        }
        private static void Start(EditorObjectTracker instance)
        {
            instance._duplicate = false;
            instance._created = false;
        }

        private static void OnValidate(EditorObjectTracker instance)
        {
            GameObject gameObject = instance.gameObject;
            if ( Application.isPlaying || !gameObject.scene.isLoaded || instance.IsNull() )
            {
                return;
            }
            if ( EditorObjectTracker.IsInPrefabMode(gameObject) )
            {
                instance.assetInstanceGuid = null;
                instance.assetInstanceReadableName = null;
                return;
            }


            if ( !instance.skipDuplication && instance.instanceId != 0 )
            {
                var instanceIDToObject = EditorUtility.InstanceIDToObject(instance.instanceId) as EditorObjectTracker;
                if ( !instanceIDToObject.IsNull() )
                {
                    instance.assetInstanceGuid = null;
                    instance._duplicate = true;
                    ClearDuplicateState(instance);
                    ClearDuplicateState(instanceIDToObject);
                    instance._onDuplicate();
                }
            }
            else if ( string.IsNullOrEmpty(instance.assetInstanceGuid) )
            {
                GenerateAndAssignNewGuid(instance);
                instance._created = true;
                instance._onCreateComponent();
            }

        }
        private static void OnDestroy(EditorObjectTracker instance)
        {
            if ( instance.gameObject.scene.isLoaded )
            {
                instance.instanceId = 0;
            }
        }

        private static void Update(EditorObjectTracker instance)
        {

            string name = instance.name;
            if ( instance.IsNull() || Equals(name, instance.assetInstanceReadableName) )
            {
                return;
            }
            if ( EditorObjectTracker.IsInPrefabMode(instance.gameObject) )
            {
                return;
            }
            string old = instance.assetInstanceReadableName;
            instance.assetInstanceReadableName = name;
            instance._onGameObjectNameChanged(old);
        }
        private static void GenerateAndAssignNewGuid(EditorObjectTracker instance)
        {
            GenerateNewGuid(out Guid guid);
            instance.assetInstanceGuid = $"{guid.ToString("n")}";
        }
        private static void MarkAsDuplicate(EditorObjectTracker instance)
        {
            instance.instanceId = instance.GetInstanceID();
            instance.skipDuplication = true;
        }
        private static void ClearDuplicateState(EditorObjectTracker instance)
        {
            instance.instanceId = 0;
            instance.skipDuplication = false;
        }
        private static void GenerateNewGuid(out Guid guid)
        {
            guid = Guid.NewGuid();
            if ( !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid.ToString("n"))) )
            {
                GenerateNewGuid(out guid);
            }
        }
        public static void SetAssetGuid<T>(this EditorObjectTracker instance, T obj)
            where T : UnityEngine.Object
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            if ( !string.IsNullOrEmpty(guid) )
            {
               instance.assetInstanceGuid = guid;
            }
        }
    }
}