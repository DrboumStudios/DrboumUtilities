using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
namespace Drboum.Utilities.Editor
{
    public class AssetReferenceIdManager : AssetReferenceIDBaseManager<AssetReferenceID> {
        [InitializeOnLoadMethod]
        protected static void Create()
        {
            CreateStaticInstance<AssetReferenceIdManager>();
        }
    }
}