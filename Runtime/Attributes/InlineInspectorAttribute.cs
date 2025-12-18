using System;
using System.Diagnostics;
using UnityEngine;

namespace Drboum.Utilities.Attributes
{
    /// <summary>
    /// Shows inline inspector for ScriptableObject fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional( "UNITY_EDITOR")]
    public class InlineInspectorAttribute : PropertyAttribute
    { }
}
