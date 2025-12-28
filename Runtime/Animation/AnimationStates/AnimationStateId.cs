using Drboum.Utilities.EditorHybrid;
using UnityEngine;

namespace Drboum.Utilities.Animation {
    [CreateAssetMenu(fileName = "new" + nameof(AnimationStateId), menuName = nameof(Drboum) + "/" + nameof(Utilities) + "/" + nameof(Animation) + "/" + nameof(AnimationStateId))]
    public class AnimationStateId : AssetReferenceID
    {
        public override void OnCreateAsset()
        { }
    }
}