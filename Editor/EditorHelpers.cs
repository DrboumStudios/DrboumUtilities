using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.SceneManagement;
using BoxCollider = UnityEngine.BoxCollider;
using CapsuleCollider = UnityEngine.CapsuleCollider;
using MeshCollider = UnityEngine.MeshCollider;
using SphereCollider = UnityEngine.SphereCollider;

namespace Drboum.Utilities.Editor {
    public static class EditorHelpers
    {
        class ManagedFields
        {
            internal static readonly System.Type _ProjectWindowUtilType = typeof(ProjectWindowUtil);
            internal static readonly MethodInfo _ProjectWindowGetActiveFolderPathMethod = _ProjectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        }
        public static T ParseClipboardText<T>()
        {
            return JsonUtility.FromJson<T>(EditorGUIUtility.systemCopyBuffer);
        }
        public static string GetProjectWindowActiveFolderPath()
        {
            return ManagedFields._ProjectWindowGetActiveFolderPathMethod.Invoke(null, Array.Empty<object>())?.ToString();
        }
    }
}