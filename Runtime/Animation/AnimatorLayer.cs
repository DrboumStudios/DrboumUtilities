using System.Collections.Generic;
using DrboumLibrary.Attributes;
using DrboumLibrary.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace DrboumLibrary.Animation {
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "AnimatorLayer", menuName = nameof(DrboumLibrary) + "/Animation/AnimatorLayer")]
#endif
    public class AnimatorLayer : ScriptableObjectAssetReference {

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
                animatorLayer        = this,
                Weigth               = GetLayerWeight(animator),
                currentAnimatorState = animator.GetCurrentAnimatorStateInfo(_layerId)
            };
        }

#if UNITY_EDITOR
        [SerializeField] private AnimatorController animatorController;
        public                   AnimatorController AnimatorController => animatorController;
#endif
#if UNITY_EDITOR
        public static void SyncLayerIndexes(List<AnimatorLayer> _animatorLayerBuffer, string[] _folders)
        {
            UnityObjectEditorHelper.FindAllAssetInstances(_animatorLayerBuffer, _folders);
            for ( var i = 0; i < _animatorLayerBuffer.Count; i++ ) {
                AnimatorLayer animatorLayer = _animatorLayerBuffer[i];
                animatorLayer.SyncLayerIndex();
            }

        }

        [ContextMenu(nameof(SyncLayerIndex))]
        public void SyncLayerIndex()
        {
            if ( animatorController == null ) {
                return;
            }

            for ( var i = 0; i < animatorController.layers.Length; i++ ) {
                AnimatorControllerLayer layer = animatorController.layers[i];
                SyncedLayerIndex = layer.syncedLayerIndex;
                if ( Equals(layer.name, name) ) {
                    _defaultWeight = layer.defaultWeight;
                    _layerId       = i;
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
    public struct AnimatorLayerPersistence {
        public AnimatorLayer           animatorLayer;
        public float                   Weigth;
        public AnimatorStateSerialized currentAnimatorState;
    }
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
                FullPathHash   = animatorStateInfo.fullPathHash,
                NormalizedTime = animatorStateInfo.normalizedTime,
                ShortNameHash  = animatorStateInfo.shortNameHash
            };
        }
    }
}