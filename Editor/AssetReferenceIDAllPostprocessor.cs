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
                if ( !Equals(Path.GetExtension(str), AssetReferenceID.AssetReferenceExtension) )
                    continue;

                var assetReferenceID = AssetDatabase.LoadAssetAtPath<AssetReferenceID>(str);
                if ( assetReferenceID != null )
                {
                    AssetReferenceIDBaseManager<AssetReferenceID>.Instance.GenerateAndAssignNewGuid(assetReferenceID);
                }
            }
        }
    }
#endif
}