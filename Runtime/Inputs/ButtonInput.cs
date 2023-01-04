using System;
using Drboum.Utilities.Runtime.Collections;
using Unity.Collections;
using Unity.Properties;
namespace Drboum.Utilities.Runtime.Inputs {
    [Serializable]
    public struct ButtonInput {
        public const              string             TO_STRING_FORMAT         = nameof(PressedThisTick) + "= {0},  " + nameof(IsPressed) + "= {1}";
        public static readonly    FixedString64Bytes ToFixedStringFormat      = TO_STRING_FORMAT;
        public static readonly    int                MaximumFixedStringLength = TO_STRING_FORMAT.Length + (5 * 2) - (3 *2);
        [CreateProperty] internal bool               Value;
        [CreateProperty] public bool PressedThisTick {
            get;
            internal set;
        }
        [CreateProperty] private uint _performedTick;

        public ButtonInput(bool value, bool pressedThisTick) : this()
        {
            Value             = value;
            PressedThisTick = pressedThisTick;
        }
        public void SetValue(bool performed, uint tick)
        {
            if ( performed )
            {
                _performedTick    = tick;
                PressedThisTick = true;
            }
            Value = performed;
        }
        public void UpdateIsPressedThisTick(uint currentTick)
        {
            PressedThisTick = _performedTick == currentTick;
        }
        public bool IsPressed()
        {
            return Value || PressedThisTick;
        }
        public override string ToString()
        {
            return string.Format(TO_STRING_FORMAT, PressedThisTick.ToString(), IsPressed().ToString());
        }

        public FixedString64Bytes ToFixedString()
        {
            FixedString64Bytes fixedString = default;
            AppendFixedString(ref fixedString);
            return fixedString;
        }
        public void AppendFixedString<T>(ref T fixedString)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            PressedThisTick.ToFixedString(out FixedString32Bytes pressedThisFrameStr);
            IsPressed().ToFixedString(out FixedString32Bytes pressedStr);
            fixedString.AppendFormat(in ToFixedStringFormat, pressedThisFrameStr, pressedStr);
        }
        public void PackButtonInput(ref int bitBuffer, ref int position)
        {
            SerializationUtils.WriteBoolAndIncrementPosition(ref bitBuffer, ref position, Value);
            SerializationUtils.WriteBoolAndIncrementPosition(ref bitBuffer, ref position, PressedThisTick);
        }
        public void UnpackButtonInput(in int bitBuffer, ref int position)
        {
            Value             = SerializationUtils.ReadBoolAndIncrementPosition(in bitBuffer, ref position);
            PressedThisTick = SerializationUtils.ReadBoolAndIncrementPosition(in bitBuffer, ref position);
        }
    }
}