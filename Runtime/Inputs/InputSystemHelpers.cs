using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DrboumLibrary.Inputs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace DrboumLibrary {
    [Flags]
    public enum SubscribeToInputActionPhase : uint {
        All       = uint.MaxValue,
        Performed = 1 << 0,
        Started   = 1 << 1,
        Canceled  = 1 << 2
    }
    public struct InputActionUpdater {
        public InputAction         InputAction;
        public Action<InputAction> Callback;
    }
    public struct InputActionCallback {
        public InputAction                 InputAction;
        public Action<CallbackContext>     Callback;
        public SubscribeToInputActionPhase EventSubscribeInputActionPhase;
    }
    public static class InputSystemHelper {
        public const byte   RELEASEDVALUE         = 0;
        public const byte   CONSUMEDVALUE         = 1;
        public const byte   PERFORMEDVALUE        = 2;
        public const string INPUTSYSTEM_LOG_DEBUG = nameof(INPUTSYSTEM_LOG_DEBUG);

        public static void SetButtonInput(CallbackContext context, ref ButtonInput inputFieldData, uint performedTick,
            string                                        additionalMessage)
        {
            SetButtonInput(context, ref inputFieldData, Time.realtimeSinceStartup, performedTick, additionalMessage);
        }
        public static void SetButtonInput(InputAction action, ref ButtonInput inputFieldData, double elapsedTime,
            uint                                      tick,   string          additionalMessage)
        {
            bool btn = action.ReadValue<float>() > 0;
            LogInputActivity(action.phase, action.name, elapsedTime, tick, additionalMessage);
            inputFieldData.SetValue(btn, tick);
        }

        public static void SetButtonInput(CallbackContext context, ref ButtonInput inputFieldData, double elapsedTime,
            uint                                          tick,    string          additionalMessage)
        {
            bool btn = context.ReadValueAsButton();
            LogInputActivity(context.phase, context.action.name, elapsedTime, tick, additionalMessage);
            inputFieldData.SetValue(btn, tick);
        }
        public static void InvalidateButtonInputs<T>(this ref T serializedButtonInputs, uint currentInputTick)
            where T : unmanaged, INativeList<ButtonInput>
        {
            for ( int i = 0; i < serializedButtonInputs.Length; i++ )
            {
                ref ButtonInput buttonInput = ref serializedButtonInputs.ElementAt(i);
                buttonInput.UpdateIsPressedThisTick(currentInputTick);
            }
        }
        public static int GetButtonInputCountFromType<T>()
        {
            return GetButtonsList<T>().Count();
        }
        public static string[] GetButtonListNames<T>()
        {
            return GetButtonsList<T>().Select(x => x.Name).ToArray();
        }
        public static FixedString128Bytes[] GetButtonListNamesAsFixedString<T>()
        {
            return GetButtonsList<T>().Select(x => new FixedString128Bytes(x.Name)).ToArray();
        }
        public static string BuildKeyValueFormat(string keyName, int valueFormatCounter)
        {
            return $" {keyName}= [" + "{" + valueFormatCounter + "}" + "]";
        }
        private static IEnumerable<PropertyInfo> GetButtonsList<T>()
        {
            return typeof(T).GetProperties().Where(x => x.PropertyType.Name.StartsWith(nameof(ButtonInput)));
        }
        [Conditional(INPUTSYSTEM_LOG_DEBUG)]
        private static void LogInputActivity(InputActionPhase phase, string name, double elapsedTime, ulong tick,
            string                                            additionalMessage = "")
        {
            if ( phase == InputActionPhase.Waiting ) {
                return;
            }
            string phaseString = GetInputActionPhaseName(phase);
            LogHelper.LogTime(elapsedTime,
                $"Tick= {LogHelper.ColorMessage(tick.ToString(), "blue")} {LogHelper.ColorMessage(name, "yellow")} is {LogHelper.ColorMessage(phaseString, "green")} {additionalMessage}");
        }

        public static string GetInputActionPhaseName(InputActionPhase inputActionPhase)
        {
            return inputActionPhase switch {
                InputActionPhase.Performed => nameof(InputActionPhase.Performed),
                InputActionPhase.Waiting   => nameof(InputActionPhase.Waiting),
                InputActionPhase.Canceled  => nameof(InputActionPhase.Canceled),
                InputActionPhase.Started   => nameof(InputActionPhase.Started),
                InputActionPhase.Disabled  => nameof(InputActionPhase.Disabled),
                _                          => throw new ArgumentException("invalid enum value", nameof(inputActionPhase))
            };
        }

        public static void SubscribeToInputActionEvent(InputActionCallback inputActionEvent)
        {
            SubscribeToInputActionEvent(inputActionEvent.InputAction, inputActionEvent.Callback,
                inputActionEvent.EventSubscribeInputActionPhase);
        }

        public static void SubscribeToInputActionEvent(InputAction inputAction, Action<CallbackContext> Callback,
            SubscribeToInputActionPhase                            EventSubscribeInputActionPhase = SubscribeToInputActionPhase.All)
        {
            if ( (EventSubscribeInputActionPhase & SubscribeToInputActionPhase.Started) != 0 ) {
                inputAction.started += Callback;
            }
            if ( (EventSubscribeInputActionPhase & SubscribeToInputActionPhase.Performed) != 0 ) {
                inputAction.performed += Callback;
            }
            if ( (EventSubscribeInputActionPhase & SubscribeToInputActionPhase.Canceled) != 0 ) {
                inputAction.canceled += Callback;
            }
        }

        public static void UnsubscribeToInputActionEvent(InputActionCallback inputActionCallback)
        {
            if ( inputActionCallback.InputAction == null ) {

                LogHelper.LogNullParameterErrorMessage(nameof(inputActionCallback.InputAction),
                    nameof(UnsubscribeToInputActionEvent));

                return;
            }

            inputActionCallback.InputAction.started   -= inputActionCallback.Callback;
            inputActionCallback.InputAction.performed -= inputActionCallback.Callback;
            inputActionCallback.InputAction.canceled  -= inputActionCallback.Callback;
        }

        public static void UnsubscribeToInputActionEvent(InputAction inputAction,
            Action<CallbackContext>                                  inputActionEvent)
        {
            if ( inputAction == null ) {

                LogHelper.LogNullParameterErrorMessage(nameof(inputAction), nameof(UnsubscribeToInputActionEvent));

                return;
            }

            inputAction.started   -= inputActionEvent;
            inputAction.performed -= inputActionEvent;
            inputAction.canceled  -= inputActionEvent;
        }
        public static void EnableInputActions(InputAction[] inputActions)
        {
            for ( var i = 0; i < inputActions.Length; i++ ) {
                inputActions[i].Enable();
            }
        }

        public static void DisableInputActions(InputAction[] inputActions)
        {
            for ( var i = 0; i < inputActions.Length; i++ ) {
                inputActions[i].Disable();
            }
        }
        public static bool IsPressed(byte inputValue)
        {
            return inputValue > RELEASEDVALUE;
        }

        public static void ReleaseInput(out byte inputValue)
        {
            inputValue = RELEASEDVALUE;
        }
        public static bool IsPressedThisFrame(ulong currentTick, ulong inputButtonTick)
        {
            return inputButtonTick != 0 && currentTick == inputButtonTick;
        }

    }
}