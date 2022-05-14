using System;
using Unity.Collections;
namespace DrboumLibrary {
    public struct ReadOnlyNativeHashMap<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct {

        private NativeHashMap<TKey, TValue> _lookupTable;

        public ReadOnlyNativeHashMap(NativeHashMap<TKey, TValue> lookupTable)
        {
            _lookupTable = lookupTable;
        }

        public bool TryGetValue(TKey key, out TValue waypointBlobList)
        {
            return _lookupTable.TryGetValue(key, out waypointBlobList);
        }
    }
}