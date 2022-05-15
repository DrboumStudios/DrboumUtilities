using Drboum.Utilities.Runtime;
using Drboum.Utilities.Runtime.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Drboum.Utilities.Editor {

    [CustomEditor(typeof(AnimationStateEvent), true)]
    public class AnimationStateEventEditor : UnityEditor.Editor {
        private void OnEnable()
        {
            MapAnimatorStateToAnimationState();
        }
        [MenuItem("Tools/" + nameof(MapAnimatorStateToAnimationState))]
        public static void MapAnimatorStateToAnimationState()
        {
            AnimatorController[] assetArray           = UnityObjectEditorHelper.FindAllAssetInstances<AnimatorController>();
            var                  currentUniqueStateId = 0;
            for ( var i = 0; i < assetArray.Length; i++ ) {
                AnimatorController animatorController = assetArray[i];

                for ( var layerId = 0; layerId < animatorController.layers.Length; layerId++ ) {

                    AnimatorControllerLayer layer = animatorController.layers[layerId];
                    AssignUniqueIDForStateMachine(ref currentUniqueStateId, layer.stateMachine);
                }
            }
        }

        private static void AssignUniqueIDForStateMachine(ref int currentUniqueStateId,
            AnimatorStateMachine                                  stateMachine)
        {
            ChildAnimatorState[] states = stateMachine.states;
            AssignUniqueStateIdForBehaviour(ref currentUniqueStateId, states);
            ChildAnimatorStateMachine[] stateMachines = stateMachine.stateMachines;
            for ( var statemachineId = 0; statemachineId < stateMachines.Length; statemachineId++ ) {
                AssignUniqueIDForStateMachine(ref currentUniqueStateId, stateMachines[statemachineId].stateMachine);
            }
        }

        private static void AssignUniqueStateIdForBehaviour(ref int currentUniqueStateId, ChildAnimatorState[] states)
        {
            for ( var stateId = 0; stateId < states.Length; stateId++ ) {

                currentUniqueStateId++;
                var                     tagId      = currentUniqueStateId.ToString();
                AnimatorState           state      = states[stateId].state;
                StateMachineBehaviour[] behaviours = state.behaviours;
                for ( var behaviourID = 0; behaviourID < behaviours.Length; behaviourID++ ) {
                    if ( behaviours[behaviourID] is AnimationStateEvent ev ) {
                        state.tag       = tagId;
                        ev.OwnerStateId = Animator.StringToHash(state.tag);
                    }
                }
            }
        }
        public static void SetAnimatorStateTagName(StateMachineBehaviour target, string tag)
        {
            StateMachineBehaviourContext[] context = AnimatorController.FindStateMachineBehaviourContext(target);
            if ( context != null ) {
                // animatorObject can be an AnimatorState or AnimatorStateMachine
                var state = context[0].animatorObject as AnimatorState;
                if ( state != null ) {

                    state.tag = tag;
                }
            }
        }
    }

}