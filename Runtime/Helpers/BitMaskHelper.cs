using UnityEngine;
namespace DrboumLibrary {
    public static class BitMaskHelper {

        public static bool ContainsBitMask(this uint x, uint y)
        {
            return (x & y) != 0;
        }

        public static bool ContainsBitMask(this int x, int y)
        {
            return (x & y) != 0;
        }
        public static uint RemoveFromBitMask(this uint mask, uint value)
        {
            return mask & ~value;
        }
        public static LayerMask RemoveFromBitMask(this LayerMask mask, LayerMask value)
        {
            return mask & ~value;
        }
        public static LayerMask AddToBitMask(this LayerMask mask, LayerMask value)
        {
            return mask | value;
        }
        public static uint AddToBitMask(this uint mask, uint value)
        {
            return mask | value;
        }
    }
}