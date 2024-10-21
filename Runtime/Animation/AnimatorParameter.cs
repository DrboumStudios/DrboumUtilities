using System;
using Drboum.Utilities.Runtime.Attributes;
using UnityEditor.Animations;
using UnityEngine;

namespace Drboum.Utilities.Runtime.Animation
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = nameof(AnimatorParameter), menuName = nameof(Drboum) + "/" + nameof(Utilities) + "/" + nameof(Animation) + "/" + nameof(AnimatorParameter))]
#endif
    public class AnimatorParameter : ScriptableObject, IAnimatorParameter, IEquatable<AnimatorParameter>
    {
        [SerializeField] protected string parameterName;
        [SerializeField] protected AnimatorControllerParameterType parameterType;
        [SerializeField] [InspectorReadOnly] protected int hashId;
#if UNITY_EDITOR
        [SerializeField] [InspectorReadOnly] private UnityEditor.Animations.AnimatorController animatorController;
#endif

        public string ParameterName => parameterName;
        public int HashId => hashId;
        public AnimatorControllerParameterType ParameterType => parameterType;

        /// <summary>
        ///     In order for 2 <see cref="AnimatorParameter" /> to have be equals, the presence of the <see cref="Animator" /> is
        ///     needed
        ///     as the hashid based on the name could be used for another animator
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AnimatorParameter other)
        {
            if ( ReferenceEquals(other, null) )
            {
                return false;
            }
            if ( ReferenceEquals(other, null) )
            {
                return false;
            }
            return IsEquals(this, other);
        }

#if UNITY_EDITOR
        public UnityEditor.Animations.AnimatorController AnimatorController => animatorController;

        [ContextMenu(nameof(GenerateHashId))]
#endif
        public void GenerateHashId()
        {
            hashId = GenerateHashId(parameterName);
        }

        public void Initialize() { }

        public void ResetToDefaultValue(Animator animator)
        {
            switch ( ParameterType )
            {
                case AnimatorControllerParameterType.Float:
                    UpdateAnimator(animator, default(float));
                    break;
                case AnimatorControllerParameterType.Int:
                    UpdateAnimator(animator, default(int));
                    break;
                case AnimatorControllerParameterType.Bool:
                    UpdateAnimator(animator, default(bool));
                    break;
            }

        }

        public void UpdateAnimator(Animator animator, bool value)
        {
            if ( animator != null )
            {
                animator.SetBool(hashId, value);
            }
        }

        public void UpdateAnimator(Animator animator, float value)
        {
            if ( animator != null )
            {
                animator.SetFloat(hashId, value);
            }
        }

        public void UpdateAnimator(Animator animator, float value, float dampTime, float deltaTime)
        {
            if ( animator != null )
            {
                animator.SetFloat(hashId, value, dampTime, deltaTime);
            }
        }

        public void UpdateAnimator(Animator animator, int value)
        {
            if ( animator != null )
            {
                animator.SetInteger(hashId, value);
            }
        }

        public void UpdateAnimatorTrigger(Animator animator)
        {
            if ( animator != null )
            {
                animator.SetTrigger(hashId);
            }
        }

        public bool GetBool(Animator animator)
        {
            return animator != null && animator.GetBool(hashId);
        }

        public int GetInt(Animator animator)
        {
            if ( animator != null )
            {
                return animator.GetInteger(hashId);
            }

            return default;
        }

        public float GetFloat(Animator animator)
        {
            if ( animator != null )
            {
                return animator.GetFloat(hashId);
            }

            return default;
        }

        public override bool Equals(object obj)
        {
            if ( obj == null )
            {
                return false;
            }
            var objAsPart = obj as AnimatorParameter;
            if ( objAsPart == null )
            {
                return false;
            }
            return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return HashId;
        }

        public AnimatorParameterSerializedData AsStruct(Animator animator)
        {
            var paramData = new AnimatorParameterSerializedData();
            paramData.AnimatorParameter = this;
            switch ( parameterType )
            {
                case AnimatorControllerParameterType.Float:
                    paramData.SetValue(animator.GetFloat(hashId));
                    break;
                case AnimatorControllerParameterType.Int:
                    paramData.SetValue(animator.GetInteger(hashId));
                    break;
                case AnimatorControllerParameterType.Bool:
                    paramData.SetValue(animator.GetBool(hashId));
                    break;
            }
            return paramData;
        }

        public static int GenerateHashId(string parameterName)
        {
            return Animator.StringToHash(parameterName);
        }

        public static bool IsEquals(IAnimatorParameter animatorParameterA,
            IAnimatorParameter animatorParameterB)
        {
            return animatorParameterA.HashId == animatorParameterB.HashId &&
                   animatorParameterA.ParameterType == animatorParameterB.ParameterType;
        }



        public static bool operator ==(AnimatorParameter x, AnimatorParameter y)
        {
            return !ReferenceEquals(x, null) && x.Equals(y);
        }

        public static bool operator !=(AnimatorParameter x, AnimatorParameter y)
        {
            return !ReferenceEquals(x, null) && !x.Equals(y);
        }


#if UNITY_EDITOR
        internal void Initialize(AnimatorControllerParameter animatorControllerParameter, AnimatorController animatorController)
        {
            if ( animatorControllerParameter != null )
            {
                parameterName = animatorControllerParameter.name;
                hashId = animatorControllerParameter.nameHash;
                parameterType = animatorControllerParameter.type;
                this.animatorController = animatorController;
            }
        }

        private void OnValidate()
        {
            if ( !string.IsNullOrEmpty(parameterName) )
            {
                GenerateHashId();
            }
        }
#endif
    }

    public interface IAnimatorParameter
    {
        int HashId { get; }
        AnimatorControllerParameterType ParameterType { get; }
    }
}