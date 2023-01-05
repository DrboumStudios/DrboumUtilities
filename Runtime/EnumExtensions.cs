using System;
using System.Collections.Generic;
public static partial class EnumExtensions {
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

    public static bool HasFlagNoAlloc<T>(this T lh,T rh)
        where T : Enum,IConvertible
    {
        return (lh.ToInt32(null) & rh.ToInt32(null)) != 0;
    }
}