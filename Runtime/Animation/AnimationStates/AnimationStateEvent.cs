using System;
using System.Collections.Generic;
using DrboumLibrary.Attributes;
using UnityEngine;
using UnityEngine.Animations;
namespace DrboumLibrary.Animation.AnimationStates {
    public abstract class AnimationStateEvent : StateMachineBehaviour {
        private static readonly Dictionary<int, int> _animationStateEventMap = new Dictionary<int, int>();
        private static readonly List<string>         _animatorCheckedList    = new List<string>();
        protected static        int                  Counter;

        [SerializeField] [InspectorReadOnly] private int _ownerStateId;
        public abstract                              int StateBitMask { get; }
        public                                       int OwnerStateId { get => _ownerStateId; set => _ownerStateId = value; }
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationStart?.Invoke();
        }
        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationEnd?.Invoke();
        }
        public static void RegisterAnimatorToAnimationEvents(Animator animator)
        {
            if ( !_animatorCheckedList.Contains(animator.runtimeAnimatorController.name) ) {
                _animatorCheckedList.Add(animator.runtimeAnimatorController.name);
                AnimationStateEvent[] behaviours = animator.GetBehaviours<AnimationStateEvent>();
                for ( var i = 0; i < behaviours.Length; i++ ) {
                    AnimationStateEvent animationStateEvent = behaviours[i];
                    AddStateMap(animationStateEvent.OwnerStateId, animationStateEvent.StateBitMask);
                }
            }
        }
        public static void AddStateMap(int hashId, int stateMask)
        {
            if ( _animationStateEventMap.TryGetValue(hashId, out int currentValue) ) {
                _animationStateEventMap[hashId] = currentValue | stateMask;
            }
            else {
                _animationStateEventMap.Add(hashId, stateMask);
            }
        }
        public static void CheckAnimatorState(Animator animator, int stateBitMask, int layerId, ref bool isPlaying)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(layerId);
            isPlaying = _animationStateEventMap.TryGetValue(state.tagHash, out int bitmask) &&
                        (bitmask & stateBitMask) != 0;
        }
        protected static int GetNewBitMask()
        {
            Counter++;
            return 1 << Counter;
        }
        public event Action AnimationStart;
        public event Action AnimationEnd;
    }
}