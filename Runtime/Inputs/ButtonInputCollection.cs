using System;
using Drboum.Utilities.Runtime.Interfaces;
using Unity.Collections;
using Unity.Properties;
using static Drboum.Utilities.Runtime.Inputs.InputSystemHelper;
namespace Drboum.Utilities.Runtime.Inputs {
    public struct ButtonInputCollection<TCollection, TButtonNamesContainer> : IProvideFixedString
        where TCollection : unmanaged, INativeList<ButtonInput> {

        private static readonly FixedString128Bytes[] _buttonNames                   = GetButtonListNamesAsFixedString<TButtonNamesContainer>();
        private static readonly FixedString512Bytes[] _fStringButtonInputValueFormat = BuildInputButtonValueFormatArray();
        public static FixedString512Bytes[] BuildInputButtonValueFormatArray()
        {
            if ( _buttonNames.Length == 0 )
            {
                return Array.Empty<FixedString512Bytes>();
            }
            var fStringButtonInputValueFormat = new FixedString512Bytes[_buttonNames.Length];
            var index                         = 0;
            for ( ; index < _buttonNames.Length - 1; index++ )
            {
                FixedString128Bytes fstring             = _buttonNames[index];
                FixedString512Bytes buildKeyValueFormat = BuildKeyValueFormat(fstring.ConvertToString(), 0);
                buildKeyValueFormat.Append(',');
                fStringButtonInputValueFormat[index] = buildKeyValueFormat;
            }
            FixedString128Bytes lastfstring = _buttonNames[index];
            fStringButtonInputValueFormat[index] = BuildKeyValueFormat(lastfstring.ConvertToString(), 0);

            return fStringButtonInputValueFormat;
        }
        private static string BuildKeyValueFormat(string keyName, int valueFormatCounter)
        {
            return $" {keyName}= [" + "{" + valueFormatCounter + "}" + "]";
        }

        [CreateProperty] private TCollection _buttonInputs;

        public int Length {
            get => _buttonInputs.Length;
            set => _buttonInputs.Length = value;
        }
        public void Initialize()
        {
            _buttonInputs.Length = _buttonNames.Length;
        }
        public ref ButtonInput ElementAt(int   index)       => ref _buttonInputs.ElementAt(index);
        public     void        Invalidate(uint currentTick) => _buttonInputs.InvalidateButtonInputs(currentTick);

        public bool IsAnyButtonPressedThisTick()
        {
            for ( int i = 0; i < _buttonInputs.Length; i++ )
            {
                if ( _buttonInputs[i].PressedThisTick )
                {
                    return true;
                }
            }
            return false;
        }
        public void ToFixedString(ref NativeText textStream)
        {
            for ( var index = 0; index < _buttonInputs.Length; index++ )
            {
                ButtonInput serializedButtonInput = _buttonInputs[index];
                textStream.AppendFormat(_fStringButtonInputValueFormat[index], serializedButtonInput.ToFixedString());
            }
        }
        public int MaximumFixedStringLength => (_buttonInputs.Capacity * ButtonInput.MaximumFixedStringLength);
        public void PackButtonInputs(ref int bitBuffer, ref int position)
        {
            for ( int i = 0; i < Length; i++ )
            {
                ref ButtonInput btn = ref ElementAt(i);
                btn.PackButtonInput(ref bitBuffer,ref position);
            }
        }
        public void UnpackButtonInputs(in int bitBuffer, ref int position)
        {
            for ( int i = 0; i < Length; i++ )
            {
                ref ButtonInput btn = ref ElementAt(i);
                btn.UnpackButtonInput(in bitBuffer, ref position);
            }
        }
    }
}