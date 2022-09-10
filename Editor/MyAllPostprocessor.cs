using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
namespace Drboum.Utilities.Editor
{
    class MyAllPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            for ( var index = 0; index < importedAssets.Length; index++ )
            {
                string str = importedAssets[index];
                var assetReferenceID = AssetDatabase.LoadAssetAtPath<AssetReferenceID>(str);
                if ( assetReferenceID != null )
                {
                    assetReferenceID.FixAssetIDIfInvalid();
                }
                LogHelper.LogDebugMessage($"Reimported Asset of type {assetReferenceID} : {str}");
            }
            
            foreach (string str in deletedAssets)
            {
                LogHelper.LogDebugMessage("Deleted Asset: " + str);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                LogHelper.LogDebugMessage("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            }

            if (didDomainReload)
            {
                LogHelper.LogDebugMessage("Domain has been reloaded");
            }
        }
    }
}