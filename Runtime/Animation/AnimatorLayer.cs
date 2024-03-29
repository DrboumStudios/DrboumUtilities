﻿using Drboum.Utilities.Runtime.Attributes;
using UnityEngine;

namespace Drboum.Utilities.Runtime.Animation {
    [CreateAssetMenu(fileName = "AnimatorLayer", menuName = nameof(Drboum) + "/" + nameof(Utilities) + "/" + nameof(Animation) + "/" + nameof(AnimatorLayer))]
    public class AnimatorLayer : ScriptableObject {

        [SerializeField] [InspectorReadOnly] private int   _layerId = -1;
        [SerializeField] [InspectorReadOnly] private float _defaultWeight;

        public string LayerName => name;

        public int LayerId          => _layerId;
        public int SyncedLayerIndex { get; private set; }

        public float GetLayerWeight(Animator animator)
        {
            return animator.GetLayerWeight(_layerId);
        }
        public void SetLayerWeight(Animator animator, float value)
        {
            animator.SetLayerWeight(_layerId, value);
        }
        public void DisableLayer(Animator animator)
        {
            SetLayerWeight(animator, 0f);
        }
        public void EnableLayer(Animator animator)
        {
            SetLayerWeight(animator, 1);
        }
        public void SetDefaultWeight(Animator animator)
        {
            animator.SetLayerWeight(_layerId, _defaultWeight);
        }
        public AnimatorLayerPersistence AsStruct(Animator animator)
        {
            return new AnimatorLayerPersistence {
                animatorLayer = this,
                Weigth = GetLayerWeight(animator),
                currentAnimatorState = animator.GetCurrentAnimatorStateInfo(_layerId)
            };
        }

#if UNITY_EDITOR
        [SerializeField] private UnityEditor.Animations.AnimatorController animatorController;
        public                   UnityEditor.Animations.AnimatorController AnimatorController => animatorController;

      

        [ContextMenu(nameof(SyncLayerIndex))]
        public void SyncLayerIndex()
        {
            if ( animatorController == null )
            {
                return;
            }

            for ( var i = 0; i < animatorController.layers.Length; i++ )
            {
                UnityEditor.Animations.AnimatorControllerLayer layer = animatorController.layers[i];
                SyncedLayerIndex = layer.syncedLayerIndex;
                if ( Equals(layer.name, name) )
                {
                    _defaultWeight = layer.defaultWeight;
                    _layerId = i;
                    break;
                }
            }
        }
        public void OnEnable()
        {
            SyncLayerIndex();
        }
#endif
    }
}