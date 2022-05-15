using System;
using UnityEngine;
namespace Drboum.Utilities.Runtime.Attributes {
    public enum PropertyDeclaringType : byte {
        Component,
        ScriptableObject
    }
    /// <summary>
    ///     Attribute that require implementation of the provided interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute {
#if UNITY_EDITOR
        // Interface type.
        public Type InterfaceType {
            get;
        }
        public Type PropertyType {
            get;
        }
        public string[] Folders {
            get;
        }
        /// <summary>
        ///     Requiring implementation of the <see cref="T:RequireInterfaceAttribute" /> interface.
        /// </summary>
        /// <param name="interfaceType">Interface type.</param>
        /// <param name="propertyType">declaring propertyType type.</param>
        public RequireInterfaceAttribute(Type interfaceType, PropertyDeclaringType propertyDeclaringType)
        {
            InterfaceType = interfaceType;
            PropertyType = propertyDeclaringType switch {
                PropertyDeclaringType.Component        => typeof(Component),
                PropertyDeclaringType.ScriptableObject => typeof(ScriptableObject),
                _                                      => typeof(object)
            };
        }
        public RequireInterfaceAttribute(Type interfaceType, string lookupAssetFolder)
            : this(interfaceType, PropertyDeclaringType.ScriptableObject)
        {
            Folders = new[] {
                lookupAssetFolder
            };
        }
#else
    public RequireInterfaceAttribute(System.Type interfaceType, PropertyDeclaringType propertyDeclaringType){}
    public RequireInterfaceAttribute(System.Type interfaceType, string[] folders) {}

#endif
    }
}