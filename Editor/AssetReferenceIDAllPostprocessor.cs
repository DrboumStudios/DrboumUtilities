using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;

namespace Drboum.Utilities.Editor
{
    class AssetReferenceIDAllPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for ( var index = 0; index < importedAssets.Length; index++ )
            {
                string str = importedAssets[index];
                var assetReferenceID = AssetDatabase.LoadAssetAtPath<AssetReferenceID>(str);
                if ( assetReferenceID != null )
                {
                    AssetReferenceIDBaseManager<AssetReferenceID>.Instance.GenerateAndAssignNewGuid(assetReferenceID);
                }
            }
        }
    }
}