using System;
using UnityEngine;

namespace Drboum.Utilities.Animation {

    public class AnimationStateReceiver : IAnimationStateReceiver {
        private bool                  _isPlaying;
        public  AnimationStateEvent[] StateEvents { get; private set; }
        public event Action           AnimationEnd   = CsharpHelper.EmptyDelegate;
        public event Action           AnimationStart = CsharpHelper.EmptyDelegate;
        public bool                   IsPlaying { get => _isPlaying; set => _isPlaying = value; }
        private void InvokeStart()
        {
            AnimationStart.Invoke();
        }
        private void InvokeEnd()
        {
            AnimationEnd.Invoke();
        }
        public void SetStateEvents<T>(Animator animator)
            where T : AnimationStateEvent
        {
            StateEvents = animator.GetBehaviours<T>();
        }
        public void CheckAnimatorState(Animator animator, int stateBitMask, int layerId)
        {
            AnimationStateEvent.CheckAnimatorState(animator, stateBitMask, layerId, ref _isPlaying);
        }
        public void SubscribeToAnimationStateEvents()
        {
            for ( var i = 0; i < StateEvents.Length; i++ ) {
                AnimationStateEvent animationStateEvent = StateEvents[i];
                animationStateEvent.AnimationStart += InvokeStart;
                animationStateEvent.AnimationEnd   += InvokeEnd;
            }
        }
        public void UnsubscribeToAnimationStateEvents()
        {
            for ( var i = 0; i < StateEvents.Length; i++ ) {
                AnimationStateEvent animationStateEvent = StateEvents[i];
                animationStateEvent.AnimationStart -= InvokeStart;
                animationStateEvent.AnimationEnd   -= InvokeEnd;
            }
        }
    }
    public interface IAnimationStateReceiver {
        bool IsPlaying { get; }

        event Action AnimationEnd;
        event Action AnimationStart;
    }
}