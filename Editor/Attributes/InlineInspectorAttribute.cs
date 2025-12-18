using System;
using UnityEngine;

namespace Drboum.Utilities.Editor.Attributes
{
    /// <summary>
    /// Shows inline inspector for ScriptableObject fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InlineInspectorAttribute : PropertyAttribute
    { }
}
