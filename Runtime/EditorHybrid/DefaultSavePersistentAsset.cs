using Drboum.Utilities.Interfaces;
using UnityEngine;

namespace Drboum.Utilities.EditorHybrid
{
    public struct DefaultSavePersistentAsset : ISavePersistentAsset
    {
        public void SaveAsset(Object parentObject, Object createdInstance)
        {
#if UNITY_EDITOR
            string assetCreationPath = this.GetPreferredDirectoryPath(parentObject, createdInstance);
            this.SaveCreatedInstanceToDatabase(parentObject, createdInstance, assetCreationPath);
#endif
        }
    }
}