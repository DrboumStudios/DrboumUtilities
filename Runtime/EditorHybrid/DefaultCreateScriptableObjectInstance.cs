using System;
using Drboum.Utilities.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Drboum.Utilities.EditorHybrid
{
    public struct DefaultCreateScriptableObjectInstance : ICreateAsset
    {
        public bool CanCreateAsset(Object parentObject, Type type)
        {
#if UNITY_EDITOR
            return typeof(ScriptableObject).IsAssignableFrom(type);
#else
            return true;
#endif
        }

        public Object CreateInstance(Object parentObject, Type type)
        {
#if UNITY_EDITOR
            switch ( type )
            {
                case var t when typeof(ScriptableObject).IsAssignableFrom(t):
                {
                    return CreatedInstanceImpl(parentObject, type);
                }

                default:
                    throw new NotSupportedException($"Default implementation of {nameof(ICreateAsset)} does not support creating instances of {type.Name}. Please provide a custom implementation.");
            }
#else
            return CreatedInstanceImpl(parentObject, type);
#endif
        }

        private static Object CreatedInstanceImpl(Object parentObject, Type type)
        {
            var createdInstance = ScriptableObject.CreateInstance(type);
            createdInstance.name = $"{parentObject.name}_{createdInstance.GetType().Name}";
            return createdInstance;
        }
    }
}