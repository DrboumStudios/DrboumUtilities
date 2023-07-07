using System;
using Unity.Collections;
namespace Drboum.Utilities.Runtime.Collections {
    public struct ReadOnlyNativeHashMap<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged {

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