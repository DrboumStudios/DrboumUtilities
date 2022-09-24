using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;

namespace Drboum.Utilities.Editor
{
    [CustomEditor(typeof(PrefabIdentity), true)]
    public class PrefabIdentityEditor: AssetIdentityBaseEditor{}
}