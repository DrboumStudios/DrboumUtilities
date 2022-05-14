using System;
using System.Collections.Generic;
using UnityEngine;
namespace DrboumLibrary.Serialization {
    public static class SerializationTypeManager {
        public const            string                TypeIDJsonPropertyName = "_tID";
        private static readonly Dictionary<int, Type> _typeCache;

        static SerializationTypeManager()
        {
            _typeCache = new Dictionary<int, Type>();
        }
        public static bool GetType(int hashID, out Type type)
        {
            return _typeCache.TryGetValue(hashID, out type);
        }
        public static int GetHash(Type type)
        {
            return type.GetHashCode();
        }
        public static int GetHash(in GuidWrapper guid)
        {
            return guid.GetHashCode();
        }
        public static void RegisterType<T>(int guidType)
        {
            if ( _typeCache.ContainsKey(guidType) ) {
                Debug.LogError(
                    $"the {nameof(guidType)} key is already used and registered by {_typeCache[guidType].Name}, the type {typeof(T)} couldn't get registered");
                return;
            }

            _typeCache.Add(guidType, typeof(T));
        }
        public static void RegisterType<T>(in GuidWrapper guid, ref int hashtypeIdBackField)
        {
            hashtypeIdBackField = GetHash(in guid);
            RegisterType<T>(hashtypeIdBackField);
        }
    }
}