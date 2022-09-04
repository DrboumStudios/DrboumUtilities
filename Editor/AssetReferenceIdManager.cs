using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
namespace Drboum.Utilities.Editor
{
    public class AssetReferenceIdManager : AssetReferenceIDEditorManager<AssetReferenceID> {
        [InitializeOnLoadMethod]
        protected static void Create()
        {
            CreateStaticInstance<AssetReferenceIdManager>();
        }
    }
}