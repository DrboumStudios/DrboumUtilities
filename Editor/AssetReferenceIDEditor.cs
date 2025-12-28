using Drboum.Utilities.EditorHybrid;
using UnityEditor;

namespace Drboum.Utilities.Editor
{
    [CustomEditor(typeof(AssetReferenceID), true)]
    public class AssetReferenceIDEditor: AssetIdentityBaseEditor
    {

        protected override string GuidPropertyName => nameof(AssetReferenceID._guid);
    }
}