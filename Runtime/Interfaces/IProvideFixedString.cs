﻿using Unity.Collections;
namespace Drboum.Utilities.Runtime.Interfaces {
    public interface IProvideFixedString : IProvideFixedString<NativeText> { }
    public interface IProvideFixedString<TFixedString> where TFixedString : INativeList<byte>, IUTF8Bytes {
        void ToFixedString(ref TFixedString textStream);
        int  MaximumFixedStringLength { get; }
    }
}