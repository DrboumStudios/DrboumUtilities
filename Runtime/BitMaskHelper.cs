using System.Runtime.CompilerServices;
using UnityEngine;

namespace Drboum.Utilities
{
    public static class BitMaskHelper
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBitMask(this uint x, uint y)
        {
            return (x & y) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBitMask(this int x, int y)
        {
            return (x & y) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsBitMask(this GameObject x, int y)
        {
            return (y & (1 << x.layer)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RemoveFromBitMask(this uint mask, uint value)
        {
            return mask & ~value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask RemoveFromBitMask(this LayerMask mask, LayerMask value)
        {
            return mask & ~value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask AddToBitMask(this LayerMask mask, LayerMask value)
        {
            return mask | value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AddToBitMask(this uint mask, uint value)
        {
            return mask | value;
        }
    }
}