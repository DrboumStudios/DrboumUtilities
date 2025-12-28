#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Drboum.Utilities.EditorHybrid
{
    [CreateAssetMenu(fileName = nameof(AssetFactorySettings), menuName = "GameProject/" + nameof(AssetFactorySettings), order = 0)]
    public class AssetFactorySettings : ScriptableObject
    {
        public Object TargetFolder;

        private void OnValidate()
        {
            if ( TargetFolder != null && !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(TargetFolder)) )
            {
                Debug.LogError($"expected folder type but was instead {TargetFolder.GetType()}");
                TargetFolder = null;
            }
        }
    }
}
#endif