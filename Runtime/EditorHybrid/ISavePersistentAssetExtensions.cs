using System.Diagnostics;
using System.IO;
using Drboum.Utilities.EditorHybrid;
using Drboum.Utilities.Interfaces;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ISavePersistentAssetExtensions
{
    public static string GetPreferredDirectoryPath<T>(this T savePersistentAsset, Object parentObject, Object createdInstance)
        where T : ISavePersistentAsset
    {
        if ( createdInstance is IAssetFactorySettings assetFactorySettings && assetFactorySettings.AssetFactorySettings != null )
        {
            string fromAssetFactorySettings = AssetDatabase.GetAssetPath(assetFactorySettings.AssetFactorySettings.TargetFolder);
            return fromAssetFactorySettings;
        }

        string parentAssetDirectoryAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(parentObject));
        return parentAssetDirectoryAssetPath;
    }

    public static string GetPreferredDirectoryPath<T>(this T createdInstance, string defaultDirectoryPath = "Assets/")
        where T : Object, IAssetFactorySettings
    {
        if ( createdInstance && createdInstance.AssetFactorySettings != null )
        {
            string fromAssetFactorySettings = AssetDatabase.GetAssetPath(createdInstance.AssetFactorySettings.TargetFolder);
            return fromAssetFactorySettings;
        }
        return defaultDirectoryPath;
    }

    [Conditional("UNITY_EDITOR")]
    public static void SaveCreatedInstanceToDatabase<T>(this T savePersistentAsset, Object parentObject, Object newInstance, string assetPath, string fileExtension = "asset")
        where T : ISavePersistentAsset
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(parentObject);
        AssetDatabase.CreateAsset(newInstance, Path.Combine(assetPath, $"{newInstance.name}.{fileExtension}"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(newInstance);
#endif
    }
}