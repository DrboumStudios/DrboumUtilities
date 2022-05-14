using System.Runtime.CompilerServices;
using DrboumLibrary.Inputs;
using Unity.Collections;
using Unity.Mathematics;
namespace DrboumLibrary {
    public static class SerializationUtils {
        public const byte COMPRESSION_MAX_VALUE = 200;
        public const byte COMPRESSION_MIN_VALUE = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(in int writer, int position)
        {
            return (writer & 1 << position) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBoolAndIncrementPosition(in int writer, ref int position)
        {
            return ReadBool(in writer, position++);
        }
        public static void PackBooleans(ref int packedBooleans, NativeArray<bool> array, byte startPosition = 0)
        {
            for ( byte i = startPosition; i < array.Length; i++ )
            {
                WriteBool(ref packedBooleans, i, array[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBoolAndIncrementPosition(ref int writer, ref int position, bool b)
        {
            WriteBool(ref writer, position, b);
            position++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteBool(ref int writer, int position, bool b)
        {
            writer |= *((byte*)(&b)) << position;
        }


        /// <summary>
        /// scale a float within min/max range to an ushort between min/max range
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="minTarget"></param>
        /// <param name="maxTarget"></param>
        /// <returns></returns>
        public static byte ScaleFloatToByte(float value, float minValue, float maxValue, byte minTarget = COMPRESSION_MIN_VALUE, byte maxTarget = COMPRESSION_MAX_VALUE)
        {
            int   targetRange   = maxTarget - minTarget;
            float valueRange    = maxValue  - minValue;
            float valueRelative = value     - minValue;
            return (byte)(minTarget + (byte)(valueRelative / valueRange * targetRange));
        }

        /// <summary>
        ///   scale a byte within min/max range to a float between min/max range
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minTarget"></param>
        /// <param name="maxTarget"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static float ScaleByteToFloat(byte value, float minTarget, float maxTarget, byte minValue = COMPRESSION_MIN_VALUE, byte maxValue = COMPRESSION_MAX_VALUE)
        {
            // note: C# ushort - ushort => int, hence so many casts
            float targetRange   = maxTarget - minTarget;
            var   valueRange    = (byte)(maxValue - minValue);
            var   valueRelative = (byte)(value    - minValue);
            return minTarget + valueRelative / (float)valueRange * targetRange;
        }

    }
}