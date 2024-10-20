using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Drboum.Utilities.Runtime.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Drboum.Utilities.Runtime
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    [Serializable]
    public unsafe struct GuidWrapper : IEquatable<GuidWrapper>
    {
        [FieldOffset(0)] public uint4 HashValue;
        [FieldOffset(0), NonSerialized] public FixedBytes16 Bytes16Value;
        [FieldOffset(0), NonSerialized] public Guid GuidValue;
        [FieldOffset(0), NonSerialized] public Hash128 Hash128Value;
        [FieldOffset(0)] private fixed byte _buffer[16];

        #region InternalGuidRepresentation
        [FieldOffset(0)] private int _a;
        [FieldOffset(4)] private short _b;
        [FieldOffset(6)] private short _c;
        [FieldOffset(8)] private byte _d;
        [FieldOffset(9)] private byte _e;
        [FieldOffset(10)] private byte _f;
        [FieldOffset(11)] private byte _g;
        [FieldOffset(12)] private byte _h;
        [FieldOffset(13)] private byte _i;
        [FieldOffset(14)] private byte _j;
        [FieldOffset(15)] private byte _k;
        #endregion

        public byte this[byte index] {
            get {
                CollectionCustomHelper.CheckElementAccess(index, 16);
                return _buffer[index];
            }
        }

        public bool IsValid => Hash128Value != default;

        public void SetData(byte[] src)
        {
            AssertIsAValidSizeArray(src);
            fixed ( byte* bDestRoot = _buffer )
            {
                fixed ( byte* bSrc = src )
                {

                    CopyGuidTo(bDestRoot, bSrc);
                }
            }
        }

        public void CopyTo(byte[] dest)
        {
            AssertIsAValidSizeArray(dest);

            fixed ( byte* bDestRoot = dest )
            {
                fixed ( byte* bSrc = _buffer )
                {

                    CopyGuidTo(bDestRoot, bSrc);
                }
            }
        }

        public bool Equals(GuidWrapper other)
        {
            return HashValue.Equals(other.HashValue);
        }

        public override bool Equals(object obj)
        {
            return
                obj is GuidWrapper guidWrapper && Equals(guidWrapper) ||
                obj is Guid guid && Equals(guid) ||
                obj is uint4 hashValue && Equals(hashValue) ||
                obj is FixedBytes16 bytes16 && Equals(bytes16) ||
                obj is Hash128 hash128 && Equals(hash128)
                ;
        }

        public override int GetHashCode()
        {
            return GuidValue.GetHashCode();
        }

        public override string ToString()
        {
            return ToFixedString().ToString();
        }

        public FixedString128Bytes ToFixedString(bool dash = true)
        {
            FixedString128Bytes guidChars = default;

            HexsToChars(ref guidChars, _a >> 24, _a >> 16);
            HexsToChars(ref guidChars, _a >> 8, _a);
            if ( dash ) guidChars.Append('-');
            HexsToChars(ref guidChars, _b >> 8, _b);
            if ( dash ) guidChars.Append('-');
            HexsToChars(ref guidChars, _c >> 8, _c);
            if ( dash ) guidChars.Append('-');
            HexsToChars(ref guidChars, _d, _e);
            if ( dash ) guidChars.Append('-');
            HexsToChars(ref guidChars, _f, _g);
            HexsToChars(ref guidChars, _h, _i);
            HexsToChars(ref guidChars, _j, _k);
            return guidChars;
        }

        [ContextMenu("Print the Guid as string like 'ca761232-ed42-11ce-bacd-00aa0057b223'")]
        internal void PrintAsGuidString()
        {
            Debug.Log($"{nameof(GuidWrapper)}: {ToString()}");
        }

        public static bool IsEquals(in FixedBytes16 lh, in FixedBytes16 rh)
        {
            return
                lh.byte0000 == rh.byte0000 &&
                lh.byte0001 == rh.byte0001 &&
                lh.byte0002 == rh.byte0002 &&
                lh.byte0003 == rh.byte0003 &&
                lh.byte0004 == rh.byte0004 &&
                lh.byte0005 == rh.byte0005 &&
                lh.byte0006 == rh.byte0006 &&
                lh.byte0007 == rh.byte0007 &&
                lh.byte0008 == rh.byte0008 &&
                lh.byte0009 == rh.byte0009 &&
                lh.byte0010 == rh.byte0010 &&
                lh.byte0011 == rh.byte0011 &&
                lh.byte0012 == rh.byte0012 &&
                lh.byte0013 == rh.byte0013 &&
                lh.byte0014 == rh.byte0014 &&
                lh.byte0015 == rh.byte0015;
        }

        public static bool operator ==(FixedBytes16 bytes16, GuidWrapper guidWrapper)
        {
            return IsEquals(bytes16, guidWrapper);
        }

        public static bool operator !=(FixedBytes16 bytes16, GuidWrapper guidWrapper)
        {
            return !IsEquals(bytes16, guidWrapper);
        }

        public static implicit operator GuidWrapper(FixedBytes16 bytes16)
        {
            GuidWrapper @default = default;
            @default.Bytes16Value = bytes16;
            return @default;
        }

        public static implicit operator GuidWrapper(Guid guid)
        {
            GuidWrapper @default = default;
            @default.GuidValue = guid;
            return @default;
        }

        public static implicit operator GuidWrapper(string guid)
        {
            GuidWrapper @default = default;
            @default.GuidValue = string.IsNullOrWhiteSpace(guid) ? default : new Guid(guid);
            return @default;
        }

        public static implicit operator GuidWrapper(Hash128 guid)
        {
            GuidWrapper @default = default;
            @default.Hash128Value = guid;
            return @default;
        }

        public static implicit operator GuidWrapper(uint4 guid)
        {
            GuidWrapper @default = default;
            @default.HashValue = guid;
            return @default;
        }

        public static implicit operator Guid(GuidWrapper bytes16)
        {
            return bytes16.GuidValue;
        }

        public static implicit operator Hash128(GuidWrapper bytes16)
        {
            return bytes16.Hash128Value;
        }

        public static implicit operator uint4(GuidWrapper bytes16)
        {
            return bytes16.HashValue;
        }

        public static implicit operator FixedBytes16(GuidWrapper bytes16)
        {
            return bytes16.Bytes16Value;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void AssertIsAValidSizeArray(byte[] src)
        {
            Assert.IsTrue(src.Length == 16, $"the array passed as a paramater need to be of exactly {sizeof(Guid)} and was {src.Length}");
        }

        private static void CopyGuidTo(byte* dest, byte* src)
        {
            UnsafeUtility.MemCpy(dest, src, 16);
        }

        private static unsafe void HexsToChars(ref FixedString128Bytes guidChars, int a, int b)
        {
            HexsToChars(ref guidChars, a, b, false);
        }

        private static unsafe void HexsToChars(ref FixedString128Bytes guidChars, int a, int b, bool hex)
        {
            // if ( hex )
            // {
            //     guidChars[offset++] = '0';
            //     guidChars[offset++] = 'x';
            // }
            guidChars.Append(HexToChar(a >> 4));
            guidChars.Append(HexToChar(a));
            // if ( hex )
            // {
            //     guidChars[offset++] = ',';
            //     guidChars[offset++] = '0';
            //     guidChars[offset++] = 'x';
            // }
            guidChars.Append(HexToChar(b >> 4));
            guidChars.Append(HexToChar(b));
        }

        private static char HexToChar(int a)
        {
            a &= 0xf;
            return (char)((a > 9) ? a - 10 + 0x61 : a + 0x30);
        }

        public static GuidWrapper NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}