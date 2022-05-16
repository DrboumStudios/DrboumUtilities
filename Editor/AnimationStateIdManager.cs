using Drboum.Utilities.Runtime.Animation;
using UnityEditor;
namespace Drboum.Utilities.Editor {
    public class AnimationStateIdManager : AssetReferenceIDEditorManager<AnimationStateId> {
        [InitializeOnLoadMethod]
        private static void CreateStaticInstance()
        {
            CreateStaticInstance<AnimationStateIdManager>();
        }
    }
}