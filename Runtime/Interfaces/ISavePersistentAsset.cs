using UnityEngine;

namespace Drboum.Utilities.Interfaces
{
    public interface ISavePersistentAsset
    {
        void SaveAsset(Object parent, Object createdInstance);
    }
}