using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace DrboumLibrary.Animation {
    [StructLayout(LayoutKind.Explicit)]
    public struct AnimatorParameterValue {

        [FieldOffset(0)] public int   ValueAsInt;
        [FieldOffset(0)] public float ValueAsFloat;
        [FieldOffset(0)] public bool  ValueAsBool;
    }
    public struct AnimatorParameterSerializedData : IAnimatorParameter {
        private AnimatorParameterValue          _animParamVal;
        public                 AnimatorParameter               AnimatorParameter;
        public                 int                             HashId        => AnimatorParameter.HashId;
        public                 AnimatorControllerParameterType ParameterType => AnimatorParameter.ParameterType;

        public void GetValue(out bool value)
        {
            value = _animParamVal.ValueAsBool;
        }
        public void GetValue(out int value)
        {
            value = _animParamVal.ValueAsInt;
        }
        public void GetValue(out float value)
        {
            value = _animParamVal.ValueAsFloat;
        }
        public void SetValue(bool value, string parameterName = "")
        {
            _animParamVal.ValueAsBool = value;
#if UNITY_EDITOR || DEBUG
            if ( ParameterType != AnimatorControllerParameterType.Bool ) {
                LogTypeMismatch(value.GetType(), parameterName);
            }
#endif
        }
        public void SetValue(float Value, string parameterName = "")
        {
            _animParamVal.ValueAsFloat = Value;
#if UNITY_EDITOR || DEBUG
            if ( ParameterType != AnimatorControllerParameterType.Float ) {
                LogTypeMismatch(Value.GetType(), parameterName);
            }
#endif
        }
        public void SetValue(int Value, string parameterName = "")
        {
            _animParamVal.ValueAsInt = Value;
#if UNITY_EDITOR || DEBUG
            if ( ParameterType != AnimatorControllerParameterType.Int ) {
                LogTypeMismatch(Value.GetType(), parameterName);
            }
#endif
        }

        private void LogTypeMismatch(Type Value, string parameterName)
        {
            LogHelper.LogStackTraceErrorMessage(
                $"the attempted value assignment to {nameof(AnimatorParameterSerializedData)} with name, {parameterName} has a type mismatch. The passed animator parameter value is of type {Value}, expected animator parameter type : {ParameterType}");
        }
    }
}