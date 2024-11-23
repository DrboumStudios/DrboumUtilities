using System;
using Unity.Collections;
using Unity.Properties;

namespace Drboum.Utilities.Runtime.Inputs
{
    [Serializable]
    public struct ButtonInput
    {
        public const string TO_STRING_FORMAT = nameof(PressedThisTick) + "= {0},  " + nameof(IsPressed) + "= {1}";
        public static readonly FixedString64Bytes ToFixedStringFormat = TO_STRING_FORMAT;
        public static readonly int MaximumFixedStringLength = TO_STRING_FORMAT.Length + (5 * 2) - (3 * 2);

        [CreateProperty] internal bool Value;
        [CreateProperty] private uint _performedTick;

        public readonly bool PressedThisTick(uint tick)
        {
            return tick == _performedTick;
        }

        public void SetValue(bool performed, uint tick)
        {
            if ( performed )
            {
                _performedTick = tick;
            }
            Value = performed;
        }

        public readonly bool IsPressed(uint tick)
        {
            return Value || PressedThisTick(tick);
        }

        public override string ToString()
        {
            return string.Format(TO_STRING_FORMAT, _performedTick, Value);
        }

        public FixedString64Bytes ToFixedString()
        {
            FixedString64Bytes fixedString = $"{(FixedString64Bytes)nameof(PressedThisTick)}= {_performedTick}, {(FixedString64Bytes)nameof(IsPressed)}= {Value}";
            return fixedString;
        }


        public void PackButtonInput(ref int bitBuffer, ref int position)
        {
            SerializationUtils.WriteBoolAndIncrementPosition(ref bitBuffer, ref position, Value);
            // SerializationUtils.WriteBoolAndIncrementPosition(ref bitBuffer, ref position, PressedThisTick(tick));
        }

        public void UnpackButtonInput(in int bitBuffer, ref int position)
        {
            Value = SerializationUtils.ReadBoolAndIncrementPosition(in bitBuffer, ref position);
            // PressedThisTick = SerializationUtils.ReadBoolAndIncrementPosition(in bitBuffer, ref position);
        }
    }
}