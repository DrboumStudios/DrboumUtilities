using Drboum.Utilities.EditorHybrid;
using UnityEditor;

namespace Drboum.Utilities.Editor
{
    [CustomEditor(typeof(PrefabIdentity), true)]
    public class PrefabIdentityEditor : AssetIdentityBaseEditor
    {
        protected override string GuidPropertyName => nameof(PrefabIdentity._guid);
    }
}