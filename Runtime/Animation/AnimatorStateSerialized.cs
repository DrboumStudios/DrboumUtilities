using UnityEngine;

namespace Drboum.Utilities.Animation {
    public struct AnimatorStateSerialized {

        public int FullPathHash;
        //
        // Summary:
        //     The hash is generated using Animator.StringToHash. The hash does not include
        //     the name of the parent layer.
        public int ShortNameHash;
        //
        // Summary:
        //     Normalized time of the State.
        public float NormalizedTime;

        public static implicit operator AnimatorStateSerialized(AnimatorStateInfo animatorStateInfo)
        {
            return new AnimatorStateSerialized {
                FullPathHash = animatorStateInfo.fullPathHash,
                NormalizedTime = animatorStateInfo.normalizedTime,
                ShortNameHash = animatorStateInfo.shortNameHash
            };
        }
    }
}