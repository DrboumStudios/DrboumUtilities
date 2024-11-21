using System;
using System.Collections.Generic;

namespace Drboum.Utilities.Runtime {
    public static class CsharpHelper {
        public delegate void   FireAndForgetEvent(FireAndForgetEvent notifierToUnsubscribeFrom);
        public static Action EmptyDelegate { get; } = delegate { };
        public static void EnsureHasEnoughCapacity<T, U>(this List<T> mappedChildIDAuthorings, U[] objs)
        {
            if ( mappedChildIDAuthorings.Capacity < objs.Length )
            {
                mappedChildIDAuthorings.Capacity = objs.Length;
            }
        }
    }
}