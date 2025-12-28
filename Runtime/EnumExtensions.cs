using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Drboum.Utilities
{
    public static partial class EnumExtensions
    {
        /// <summary>
        ///     allocate only once ever for each enum type
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool EqualsNoAlloc<T>(this T first, T second)
            where T : Enum
        {
            return EqualityComparer<T>.Default.Equals(first, second);
        }

        public static TEnum Parse<TEnum, TValue>(this TValue enumVal)
            where TEnum : Enum
            where TValue : unmanaged
        {
            return (TEnum)Enum.Parse(typeof(TEnum), enumVal.ToString());
        }

        /// <summary>
        ///     parse an enum to bytes allocate coz boxing, only use for conversions
        /// </summary>
        /// <param name="enumVal"></param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static byte Parse<TEnum>(this TEnum enumVal)
            where TEnum : IComparable, IFormattable, IConvertible
        {
            return enumVal.ToByte(null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool HasFlagNoAlloc<T>(this T value, T flag)
            where T : unmanaged, Enum
        {
            return sizeof(T) switch {
                1 => (*(byte*)&value & *(byte*)&flag) == *(byte*)&flag,
                2 => (*(ushort*)&value & *(ushort*)&flag) == *(ushort*)&flag,
                4 => (*(uint*)&value & *(uint*)&flag) == *(uint*)&flag,
                8 => (*(ulong*)&value & *(ulong*)&flag) == *(ulong*)&flag,
                _ => false
            };
        }
    }
}