using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using DefaultFloatFixedStringType = Unity.Collections.FixedString64Bytes;
namespace Drboum.Utilities.Collections
{
    public static unsafe class FixedStringExt {
        private const           char                DEFAULT_LABEL_SEPARATOR    = '=';
        private const           char                DEFAULT_VALUE_SEPARATOR    = ',';
        private static readonly FixedString32Bytes  TRUE                       = "true";
        private static readonly FixedString32Bytes  FALSE                      = "false";
        private static readonly FixedString32Bytes  ZERO                       = "0";
        private static readonly FixedString512Bytes PositionDebugFstringFormat = "Vector3({0},{1},{2})";
        public static readonly  int                 SizeOfFloatFixedString     = sizeof(DefaultFloatFixedStringType);

        /// <summary>
        ///     <paramref name="bytes" /> is not modified here, only passed as a ref to avoid copying on large unmanaged
        /// </summary>
        /// <param name="fstring"></param>
        /// <param name="bytes"></param>
        /// <param name="separator"></param>
        /// <typeparam name="TFstring"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static unsafe void AppendBytesAsFixedString<TFstring, TValue>(this ref TFstring fstring, ref TValue bytes, char separator = ',')
            where TFstring : unmanaged, INativeList<byte>, IUTF8Bytes
            where TValue : unmanaged
        {
            int size = sizeof(TValue);
            var ptr = (byte*)UnsafeUtility.AddressOf(ref bytes);
            for ( var i = 0; i < size - 1; i++ )
            {
                AppendByteAsFixedString<TFstring, TValue>(ref fstring, ptr[i]);
                fstring.Append(separator);
            }
            AppendByteAsFixedString<TFstring, TValue>(ref fstring, ptr[size - 1]);
        }
        public static TFixedString ToFixedStringAsByte<TFixedString, TValue>(this ref TValue bytes, char separator = ',')
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
            where TValue : unmanaged
        {
            TFixedString fstring = default;
            fstring.AppendBytesAsFixedString(ref bytes, separator);
            return fstring;
        }
        /// <summary>
        ///     this include the 0 as a character in the final string
        /// </summary>
        /// <param name="fstring"></param>
        /// <param name="inputIn"></param>
        /// <typeparam name="TFstring"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void AppendByteAsFixedString<TFstring, TValue>(ref TFstring fstring, byte inputIn)
            where TFstring : unmanaged, INativeList<byte>, IUTF8Bytes
            where TValue : unmanaged
        {
            if ( inputIn == 0 )
            {
                fstring.Append('0');
            }
            else
            {
                fstring.Append(inputIn);
            }
        }
        public static void ToFixedString<T>(this NativeText textStream, out T fixedString)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fixedString = default;
            CollectionCustomHelper.CheckCapacity(fixedString, textStream);
            UnsafeUtility.MemCpy(fixedString.GetUnsafePtr(), textStream.GetUnsafePtr(), textStream.Length);
            fixedString.Length = textStream.Length;
        }

        #region floats
        public static FixedString128Bytes ToFixedStringAsVector3InspectorString(this float3 position)
        {
            FixedString128Bytes fstring = default;
            fstring.AppendFormat(in PositionDebugFstringFormat, position.x.ToFixedString(), position.y.ToFixedString(), position.z.ToFixedString());
            return fstring;
        }
        public static DefaultFloatFixedStringType ToFixedString(this float value)
        {
            value.ToFixedString(out DefaultFloatFixedStringType fixedString);
            return fixedString;
        }
        public static void ToFixedString(this float value, out DefaultFloatFixedStringType fixedString)
        {
            fixedString = default;
            fixedString.Append(value);
        }
        public static void AppendFixedString<T>(this float value, ref T fixedString, char valueChar,
            char labelSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fixedString.Append(valueChar);
            fixedString.Append(labelSeparator);
            fixedString.Append(value);
        }
        public static FixedString32Bytes ToFixedString(this float2 value, char labelSeparator = DEFAULT_LABEL_SEPARATOR)
        {
            FixedString32Bytes fixedString = default;

            value.AppendFixedString(ref fixedString, labelSeparator);

            return fixedString;
        }
        public static void AppendFixedString<T>(this float2 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {

            value.x.AppendFixedString(ref fixedString, 'x', labelSeparator);
            fixedString.Append(valueSeparator);
            value.y.AppendFixedString(ref fixedString, 'y', labelSeparator);

        }
        public static FixedString32Bytes ToFixedString(this float3 value, char labelSeparator = DEFAULT_LABEL_SEPARATOR,
            char valueSeparator = DEFAULT_VALUE_SEPARATOR)
        {
            FixedString32Bytes fixedString = default;

            value.AppendFixedString(ref fixedString, labelSeparator, valueSeparator);

            return fixedString;
        }
        public static void AppendFixedString<T>(this float3 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {

            value.xy.AppendFixedString(ref fixedString, labelSeparator, valueSeparator);
            fixedString.Append(valueSeparator);
            value.z.AppendFixedString(ref fixedString, 'z', labelSeparator);
        }
        public static FixedString64Bytes ToFixedString(this float4 value, char labelSeparator = DEFAULT_LABEL_SEPARATOR,
            char valueSeparator = DEFAULT_VALUE_SEPARATOR)
        {
            FixedString64Bytes fixedString = default;
            value.AppendFixedString(ref fixedString, labelSeparator, valueSeparator);
            return fixedString;
        }
        public static void AppendFixedString<T>(this float4 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            AppendFixedString(value.xyz, ref fixedString, labelSeparator, valueSeparator);
            fixedString.Append(valueSeparator);
            AppendFixedString(value.w, ref fixedString, 'w', labelSeparator);
        }
        #endregion
        #region ints
        public static FixedString32Bytes ToFixedString(this int value)
        {
            FixedString32Bytes fixedString = default;
            fixedString.Append(value);
            return fixedString;
        }
        public static FixedString32Bytes ToFixedString(this int2 value)
        {
            FixedString32Bytes fixedString = default;
            value.AppendFixedString(ref fixedString);
            return fixedString;
        }
        public static void AppendFixedString<T>(this int value, ref T fixedString, char valueChar,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fixedString.Append(valueChar);
            fixedString.Append(labelSeparator);
            fixedString.Append(value);
        }

        public static void AppendFixedString<T>(this int2 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {

            value.x.AppendFixedString(ref fixedString, 'x', labelSeparator);
            fixedString.Append(valueSeparator);
            value.y.AppendFixedString(ref fixedString, 'y', labelSeparator);
        }
        public static void AppendFixedString<T>(this int3 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            AppendFixedString(value.xy, ref fixedString, labelSeparator, valueSeparator);
            fixedString.Append(valueSeparator);
            AppendFixedString(value.z, ref fixedString, 'z', labelSeparator);
        }
        public static void AppendFixedString<T>(this int4 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            AppendFixedString(value.xyz, ref fixedString, labelSeparator, valueSeparator);
            fixedString.Append(valueSeparator);
            AppendFixedString(value.w, ref fixedString, 'w', labelSeparator);
        }
        #endregion
        #region bools
        public static void ToFixedString(this bool value, out FixedString32Bytes fixedString)
        {
            fixedString = default;
            value.AppendFixedString(ref fixedString);
        }
        public static void AppendFixedString<T>(this bool value, ref T fixedString)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fixedString.Append(value ? TRUE : FALSE);
        }
        public static void AppendFixedString<T>(this bool value, ref T fixedString, char valueChar,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            fixedString.Append(valueChar);
            fixedString.Append(labelSeparator);
            value.AppendFixedString(ref fixedString);
        }

        public static void AppendFixedString<T>(this bool2 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            AppendFixedString(value.x, ref fixedString, 'x', labelSeparator);
            fixedString.Append(valueSeparator);
            AppendFixedString(value.y, ref fixedString, 'y', labelSeparator);
        }
        public static void AppendFixedString<T>(this bool3 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            AppendFixedString(value.xy, ref fixedString, labelSeparator, valueSeparator);
            fixedString.Append(valueSeparator);
            AppendFixedString(value.z, ref fixedString, 'z', labelSeparator);
        }
        public static void AppendFixedString<T>(this bool4 value, ref T fixedString,
            char labelSeparator = DEFAULT_LABEL_SEPARATOR, char valueSeparator = DEFAULT_VALUE_SEPARATOR)
            where T : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            AppendFixedString(value.xyz, ref fixedString, labelSeparator, valueSeparator);
            fixedString.Append(valueSeparator);
            AppendFixedString(value.w, ref fixedString, 'w', labelSeparator);
        }
        #endregion
    }
}