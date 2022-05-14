#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
namespace DrboumLibrary.Serialization {
    public static class AssetReferenceHelper {

        private const           int                         OBJECT_REFENCE_BUFFER_DEFAULT_CAPACITY = 200;
        private const           int                         LOOKUP_TYPES_CAPACITY                  = 8;
        private const           int                         DEFAULT_ID_VALUE                       = 0;
        private static          List<int>                   _usedAssetIdList;
        private static readonly List<IUnityObjectReference> _unityObjectReferencesBuffer;
        private static readonly List<string>                _assetReferenceFoldersList;
        private static          string[]                    _assetReferenceFolders;
        private static readonly List<Type>                  _lookUpTypes;
        static AssetReferenceHelper()
        {

            _lookUpTypes = new List<Type>(LOOKUP_TYPES_CAPACITY) {
                typeof(ScriptableObjectAssetReference)
            };
            _unityObjectReferencesBuffer = new List<IUnityObjectReference>(OBJECT_REFENCE_BUFFER_DEFAULT_CAPACITY);
            _usedAssetIdList             = new List<int>(_unityObjectReferencesBuffer.Capacity);
            _assetReferenceFoldersList = new List<string> {
                "Assets/Game/Prefabs",
                "Assets/Game/_EditingData"
            };
            UpdatePathsArray();
        }
        public static void AddAssetReferenceLookupType<T>()
            where T : Object, IUnityObjectReference
        {
            Type type = typeof(T);
            if ( !_lookUpTypes.Contains(type) ) {
                _lookUpTypes.Add(type);
            }
        }
        public static void AddAssetReferenceLookupType<T>(string additionalLookupFolderPath)
            where T : Object, IUnityObjectReference
        {
            AddAssetReferenceLookupType<T>();
            if ( !_assetReferenceFoldersList.Contains(additionalLookupFolderPath) ) {
                _assetReferenceFoldersList.Add(additionalLookupFolderPath);
                UpdatePathsArray();
            }
        }

        private static void UpdatePathsArray()
        {
            _assetReferenceFolders = _assetReferenceFoldersList.ToArray();
        }

        public static void ProcessIDGeneration<TComponent>(bool forceExistingIdsRegeneration = false)
            where TComponent : Component, IUnityObjectReference
        {
            var idGenerationReport = Stopwatch.StartNew();
            //idGenerationReport benchmark

            _unityObjectReferencesBuffer.Clear();
            UnityObjectReferenceRegistrySO.ObjectIDMapping.Clear();
            uint highest = 0;
            highest = GetCurrentMaximumIDOnPrefabs<TComponent>(_unityObjectReferencesBuffer, _assetReferenceFolders,
                highest);
            highest = GetCurrentMaximumIDOnObjects(_unityObjectReferencesBuffer, _assetReferenceFolders, _lookUpTypes,
                highest);
            GenerateAssetReferenceIDs(_unityObjectReferencesBuffer, forceExistingIdsRegeneration ? 0 : highest,
                AssetReferenceRuntime.Registry,                     forceExistingIdsRegeneration);

            idGenerationReport.Stop();
            Debug.Log(@"["                                                                + nameof(idGenerationReport) + "]" + " :  executed in " +
                      idGenerationReport.Elapsed.TotalMilliseconds.ToString("#,0.000000") + " ms");
        }
        private static uint GetCurrentMaximumIDOnPrefabs<T>(List<IUnityObjectReference> unityObjectReferencesBuffer,
            string[]                                                                    folders, uint highestId = 0)
            where T : Component, IUnityObjectReference
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", folders);
            return ProcessHighestIdCheckAndAddToList<T>(unityObjectReferencesBuffer, highestId, guids);
        }
        private static uint GetCurrentMaximumIDOnObjects<T>(List<IUnityObjectReference> unityObjectReferencesBuffer,
            string[]                                                                    folders, uint highestId = 0)
            where T : Object, IUnityObjectReference
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(T)}", folders);
            return ProcessHighestIdCheckAndAddToList<T>(unityObjectReferencesBuffer, highestId, guids);
        }
        private static uint GetCurrentMaximumIDOnObjects(List<IUnityObjectReference> unityObjectReferencesBuffer,
            string[]                                                                 folders, List<Type> lookUpTypes, uint highestId = 0)
        {
            for ( var i = 0; i < lookUpTypes.Count; i++ ) {
                Type     type  = lookUpTypes[i];
                string[] guids = AssetDatabase.FindAssets($"t:{type.Name}", folders);
                highestId = ProcessHighestIdCheckAndAddToList(unityObjectReferencesBuffer, highestId, guids, type);
            }
            return highestId;
        }
        private static uint ProcessHighestIdCheckAndAddToList<T>(
            List<IUnityObjectReference> unityObjectReferencesBuffer, uint highestId, string[] guids)
            where T : Object, IUnityObjectReference
        {
            return ProcessHighestIdCheckAndAddToList(unityObjectReferencesBuffer, highestId, guids, typeof(T));
        }
        private static uint ProcessHighestIdCheckAndAddToList(List<IUnityObjectReference> unityObjectReferencesBuffer,
            uint                                                                          highestId, string[] guids, Type lookupType)
        {
            for ( var i = 0; i < guids.Length; ++i ) {
                string path         = AssetDatabase.GUIDToAssetPath(guids[i]);
                var    loadedObject = AssetDatabase.LoadAssetAtPath(path, lookupType) as IUnityObjectReference;
                if ( loadedObject != null ) {
                    unityObjectReferencesBuffer.Add(loadedObject);
                    CheckForUniqueness(loadedObject);
                    if ( loadedObject.UObjectID > highestId ) {
                        highestId = loadedObject.UObjectID;
                    }
                }
            }
            return highestId;
        }

        private static void CheckForUniqueness(IUnityObjectReference loadedObject)
        {
            if ( UnityObjectReferenceRegistrySO.GetUnityObjectReference(loadedObject.UObjectID,
                out IUnityObjectReference _) ) {
                loadedObject.UObjectID = 0;
            }
            else {
                UnityObjectReferenceRegistrySO.ObjectIDMapping.Add(loadedObject.UObjectID, loadedObject);
            }
        }

        private static uint GenerateAssetReferenceIDs<T>(List<T> unityObjectReferencesBuffer, uint highestId = 0,
            UnityObjectReferenceRegistrySO                       registry           = null,
            bool                                                 forceIdsGeneration = false)
            where T : class, IUnityObjectReference
        {
            registry.UnityObjectReferenceIDs.Clear();
            var countResult = 0;
            if ( registry.UnityObjectReferenceIDs.Capacity < unityObjectReferencesBuffer.Count ) {
                registry.UnityObjectReferenceIDs.Capacity = unityObjectReferencesBuffer.Count;
            }
            for ( var i = 0; i < unityObjectReferencesBuffer.Count; i++ ) {
                T loadedObject = unityObjectReferencesBuffer[i];
                registry.UnityObjectReferenceIDs.Add(loadedObject as Object);
                if ( NeedGenerateID(loadedObject) || forceIdsGeneration ) {
                    countResult++;
                    if ( loadedObject is Component component ) {
                        PrefabUtility.SavePrefabAsset(component.gameObject);
                    }
                    else if ( loadedObject is Object so ) {
                        EditorUtility.SetDirty(so);
                    }

                    GenerateUObjectID(ref highestId, loadedObject);
                }
            }
            Debug.Log($"{countResult} Ids has been generated, the highestID used is {highestId}", registry);
            return highestId;
        }
        private static bool NeedGenerateID(IUnityObjectReference loadedObject)
        {
            return loadedObject.UObjectID == DEFAULT_ID_VALUE;
        }
        private static void GenerateUObjectID(ref uint highestID, IUnityObjectReference componentID)
        {
            highestID++;
            componentID.UObjectID = Convert.ToUInt32(highestID);
        }
    }
}

#endif