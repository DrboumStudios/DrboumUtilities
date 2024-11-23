using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Drboum.Utilities.Runtime.Attributes;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Drboum.Utilities.Entities
{
    public abstract class AssetRegisterAuthoring<TAsset> : MonoBehaviour
        where TAsset : Object
    {
        public Object[] ItemAssetRootLookupFolders;
        [SerializeField, InspectorReadOnly] protected List<TAsset> AllCachedItems;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            for ( var index = 0; index < ItemAssetRootLookupFolders.Length; index++ )
            {
                Object unityObject = ItemAssetRootLookupFolders[index];
                if ( !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(unityObject)) && unityObject )
                {
                    Debug.LogError($"{unityObject} must be a valid folder", this);
                }
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        protected void CheckAndCollectAssets()
        {
#if UNITY_EDITOR
            var lookupFolders = GetValidLookupFolderPaths(ItemAssetRootLookupFolders);

            var hashSet = new HashSet<TAsset>(AllCachedItems.Count);
            for ( var index = AllCachedItems.Count - 1; index >= 0; index-- )
            {
                TAsset entryAttempt = AllCachedItems[index];
                if ( !entryAttempt )
                {
                    AllCachedItems.RemoveAtSwapBack(index);
                }
                else if ( !hashSet.Add(entryAttempt) )
                {
                    hashSet.TryGetValue(entryAttempt, out var cachedAsset);
                    LogHelper.LogErrorMessage($"duplicate Ids between : {entryAttempt.name} and {cachedAsset.name}", $"Asset Validation", this);
                }
            }
            UnityObjectEditorHelper.FindAllAssetInstances(AllCachedItems, lookupFolders);
            var hasItemListChanged = AllCachedItems.Count != hashSet.Count;

            for ( var index = 0; index < AllCachedItems.Count && !hasItemListChanged; index++ )
            {
                TAsset cachedItem = AllCachedItems[index];
                hasItemListChanged = !hashSet.Contains(cachedItem);
            }


            if ( hasItemListChanged )
            {
                this.SetDirtySafe();
            }
#endif
        }

#if UNITY_EDITOR
        protected static string[] GetValidLookupFolderPaths(Object[] authoringItemAssetRootLookupFolders)
        {
            return authoringItemAssetRootLookupFolders
                .Where(o => o != null)
                .Select(AssetDatabase.GetAssetPath)
                .Where(AssetDatabase.IsValidFolder)
                .ToArray();
        }
#endif
    }
}