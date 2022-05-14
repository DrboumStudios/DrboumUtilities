#if ADRESSABLES_EXIST
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
namespace DrboumLibrary.Editors {

    public static class AdressableFixTools {
        private const string AdressableAssetsDataFolderPath = "Assets/AddressableAssetsData/AssetGroups";
        private const string AdressableSchemaDataFolder     = "/Schemas";
        private const string AdressableSchemaDataFolderPath =
            AdressableAssetsDataFolderPath + AdressableSchemaDataFolder;
        private static readonly string[]                          _adressablesFolders;
        private static readonly string[]                          _adressablesSchemaFolders;
        private static readonly List<AddressableAssetGroup>       _addressableAssetGroupBuffer;
        private static readonly List<AddressableAssetGroupSchema> _addressableAssetGroupSchemaBuffer;

        static AdressableFixTools()
        {
            _addressableAssetGroupBuffer       = new List<AddressableAssetGroup>();
            _addressableAssetGroupSchemaBuffer = new List<AddressableAssetGroupSchema>();
            _adressablesFolders = new[] {
                AdressableAssetsDataFolderPath
            };
            _adressablesSchemaFolders = new[] {
                AdressableAssetsDataFolderPath
            };
        }

        [MenuItem("Tools/Fix Corrupted AdressableGroups")]
        public static void FixCorruptedAdressableGroups()
        {
            var reportNumber = 0;
            UnityObjectEditorHelper.FindAllAssetInstances(_addressableAssetGroupBuffer, _adressablesFolders);
            UnityObjectEditorHelper.FindAllAssetInstances(_addressableAssetGroupSchemaBuffer,
                _adressablesSchemaFolders);
            List<AddressableAssetGroup> addressableAssetGroup = _addressableAssetGroupBuffer;
            for ( var i = 0; i < addressableAssetGroup.Count; i++ ) {
                AddressableAssetGroup adrAssetGrp = addressableAssetGroup[i];

                if ( adrAssetGrp.Schemas.Count == 0 ) {
                    AddAdressableGroupSchemaTo(adrAssetGrp, ref reportNumber);
                }
            }
            Debug.Log($"{reportNumber} schemas have been fixed");
        }
        [MenuItem("Tools/Refresh AdressableGroupsSchemas")]
        public static void UpdateAdressableGroupsSchemasLink()
        {
            var reportNumber = 0;
            UnityObjectEditorHelper.FindAllAssetInstances(_addressableAssetGroupBuffer, _adressablesFolders);
            UnityObjectEditorHelper.FindAllAssetInstances(_addressableAssetGroupSchemaBuffer,
                _adressablesSchemaFolders);
            for ( var i = 0; i < _addressableAssetGroupBuffer.Count; i++ ) {
                AddressableAssetGroup adrAssetGrp = _addressableAssetGroupBuffer[i];
                adrAssetGrp.ClearSchemas(false);
                AddAdressableGroupSchemaTo(adrAssetGrp, ref reportNumber);
            }
            Debug.Log($"{reportNumber} schemas have their group link verified");
        }

        private static void AddAdressableGroupSchemaTo(AddressableAssetGroup adrAssetGrp, ref int reportNumber)
        {
            List<AddressableAssetGroupSchema> addressableAssetGroupSchemaBuffer = _addressableAssetGroupSchemaBuffer;
            for ( var ii = 0; ii < addressableAssetGroupSchemaBuffer.Count; ii++ ) {
                AddressableAssetGroupSchema schema  = addressableAssetGroupSchemaBuffer[ii];
                int                         indexof = schema.name.IndexOf(adrAssetGrp.name, StringComparison.InvariantCulture);
                if ( indexof == 0 ) {
                    reportNumber++;
                    adrAssetGrp.AddSchema(schema);
                }
            }
        }
    }
}
#endif