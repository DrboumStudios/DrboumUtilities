using System;
using DrboumLibrary.Attributes;
using DrboumLibrary.Serialization;
using UnityEngine;
#if UNITY_EDITOR


#endif

namespace DrboumLibrary.Animation {
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "AnimatorParameter", menuName = nameof(DrboumLibrary) + "/Animation/AnimatorParameter")]
#endif
    public class AnimatorParameter : ScriptableObject, IUnityObjectReference, IAnimatorParameter,
        IEquatable<AnimatorParameter> {

        [SerializeField]                     protected string                          parameterName;
        [SerializeField]                     protected AnimatorControllerParameterType parametertype;
        [SerializeField] [InspectorReadOnly] protected int                             _hashId;
        [SerializeField] [InspectorReadOnly] private   uint                            uObjectID;

        public string                          ParameterName => parameterName;
        public int                             HashId        => _hashId;
        public AnimatorControllerParameterType ParameterType => parametertype;

        /// <summary>
        ///     In order for 2 <see cref="AnimatorParameter" /> to have be equals, the presence of the <see cref="Animator" /> is
        ///     needed
        ///     as the hashid based on the name could be used for another animator
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AnimatorParameter other)
        {
            if ( ReferenceEquals(other, null) ) {
                return false;
            }
            if ( ReferenceEquals(other, null) ) {
                return false;
            }
            return IsEquals(this, other);
        }
        public uint UObjectID    { get => uObjectID; set => uObjectID = value; }
        public void Initialize() { }
#if UNITY_EDITOR
        [ContextMenu(nameof(GenerateHashId))]
#endif
        public void GenerateHashId()
        {
            _hashId = GenerateHashId(parameterName);
        }
        public void ResetToDefaultValue(Animator animator)
        {
            switch ( ParameterType ) {
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
            if ( animator != null ) {
                animator.SetBool(_hashId, value);
            }
        }

        public void UpdateAnimator(Animator animator, float value)
        {
            if ( animator != null ) {
                animator.SetFloat(_hashId, value);
            }
        }
        public void UpdateAnimator(Animator animator, float value, float dampTime, float deltaTime)
        {
            if ( animator != null ) {
                animator.SetFloat(_hashId, value, dampTime, deltaTime);
            }
        }
        public void UpdateAnimator(Animator animator, int value)
        {
            if ( animator != null ) {
                animator.SetInteger(_hashId, value);
            }
        }

        public void UpdateAnimatorTrigger(Animator animator)
        {
            if ( animator != null ) {
                animator.SetTrigger(_hashId);
            }
        }

        public bool GetBool(Animator animator)
        {
            return animator != null && animator.GetBool(_hashId);
        }
        public int GetInt(Animator animator)
        {
            if ( animator != null ) {
                return animator.GetInteger(_hashId);
            }

            return default;
        }
        public float GetFloat(Animator animator)
        {
            if ( animator != null ) {
                return animator.GetFloat(_hashId);
            }

            return default;
        }

        public override bool Equals(object obj)
        {
            if ( obj == null ) {
                return false;
            }
            var objAsPart = obj as AnimatorParameter;
            if ( objAsPart == null ) {
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
            switch ( parametertype ) {
                case AnimatorControllerParameterType.Float:
                    paramData.SetValue(animator.GetFloat(_hashId));
                    break;
                case AnimatorControllerParameterType.Int:
                    paramData.SetValue(animator.GetInteger(_hashId));
                    break;
                case AnimatorControllerParameterType.Bool:
                    paramData.SetValue(animator.GetBool(_hashId));
                    break;
            }
            return paramData;
        }
        public static int GenerateHashId(string parameterName)
        {
            return Animator.StringToHash(parameterName);
        }

        public static bool IsEquals(IAnimatorParameter animatorParameterA,
            IAnimatorParameter                         animatorParameterB)
        {
            return animatorParameterA.HashId        == animatorParameterB.HashId &&
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
        protected bool _initialized;
        public void Initialize(AnimatorControllerParameter animatorControllerParameter)
        {
            if ( !_initialized && animatorControllerParameter != null ) {
                _initialized  = true;
                parameterName = animatorControllerParameter.name;
                _hashId       = animatorControllerParameter.nameHash;
                parametertype = animatorControllerParameter.type;
            }
        }

        private void OnValidate()
        {
            if ( !string.IsNullOrEmpty(parameterName) ) {
                GenerateHashId();
            }
        }
#endif
    }
    public interface IAnimatorParameter {
        int                             HashId        { get; }
        AnimatorControllerParameterType ParameterType { get; }
    }
}