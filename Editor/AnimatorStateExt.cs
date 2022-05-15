using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.Animation;
using JetBrains.Annotations;
using UnityEditor.Animations;
using UnityEngine;
namespace Drboum.Utilities.Editor {
    public static class AnimatorStateExt {
#if UNITY_EDITOR
        [CanBeNull]
        public static AnimatorState FindMatchingAnimatorState(this AnimationStateId animationStateId, AnimatorControllerLayer baseLayer)
        {
            AnimatorStateMachine rootStateMachine = baseLayer.stateMachine;
            for ( var index = 0; index < rootStateMachine.states.Length; index++ )
            {
                var childAnimatorState = rootStateMachine.states[index];
                AnimatorState animatorState = childAnimatorState.state;
                for ( var i = 0; i < animatorState.behaviours.Length; i++ )
                {
                    StateMachineBehaviour animatorStateBehaviour = animatorState.behaviours[i];
                    if ( animatorStateBehaviour is AnimationStateIdBehaviour idBehaviour && idBehaviour.Id == animationStateId )
                    {
                        return animatorState;
                    }
                }
            }
            return null;
        }
        public static void ValidateIsNotNull(this AnimatorState matchingAnimatorState, AnimationStateId animationStateId, MonoBehaviour authoring, AnimatorControllerLayer animatorControllerLayer, string category)
        {
            if ( matchingAnimatorState.IsNull() )
            {
                LogHelper.LogErrorMessage($"the animation state {animationStateId.name} was not found in the animator, make sure that the " +
                                          $"{nameof(AnimationStateId)} added to the component '{authoring.GetType().Name}' on the gameobject '{authoring.name}' match to the corresponding" +
                                          $"{nameof(AnimationStateIdBehaviour)} attached to the animator state in the layer '{animatorControllerLayer.name}'",
                    $"{category}: {authoring.GetType().Name}", animatorControllerLayer);
                return;
            }
        }
        public static AnimatorController GetAnimatorController(this Animator animator)
        {
            return animator.runtimeAnimatorController as AnimatorController;
        }
#endif
    }
}