using System.IO;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;

namespace Drboum.Utilities.Editor
{
#if !DISABLE_ASSETREF_POSTPROCESSOR
    class AssetReferenceIDAllPostprocessor : AssetPostprocessor
    {

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for ( var index = 0; index < importedAssets.Length; index++ )
            {
                string str = importedAssets[index];
                bool isSOAsset = Equals(Path.GetExtension(str), AssetReferenceID.ASSET_REFERENCE_EXTENSION);
                if ( isSOAsset )
                {
                    var assetReferenceID = AssetDatabase.LoadAssetAtPath<AssetReferenceID>(str);
                    if ( assetReferenceID != null )
                    {
                        AssetReferenceIDBaseManager<AssetReferenceID>.Instance.GenerateAndAssignNewGuid(assetReferenceID);
                    }
                    continue;
                }

                bool isPrefab = Equals(Path.GetExtension(str), ".prefab");
                if ( isPrefab )
                {
                    var prefabId = AssetDatabase.LoadAssetAtPath<PrefabIdentity>(str);
                    if ( prefabId != null )
                    {
                        AssetReferenceIDBaseManager<PrefabIdentity>.Instance.GenerateAndAssignNewGuid(prefabId);
                    }
                }
            }
        }
    }
#endif
}