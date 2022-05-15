#if UNITY_EDITOR
using System.IO;
using Drboum.Utilities.Runtime.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Drboum.Utilities.Editor {
    [CustomEditor(typeof(AnimatorController))]
    public class AnimatorControllerEditor : UnityEditor.Editor {
        public string destinationFolder;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var animatorController = (AnimatorController)target;
            destinationFolder = EditorGUILayout.TextField(destinationFolder);
            if ( GUILayout.Button("Export animator parameters") ) {
                string assetPath   = AssetDatabase.GetAssetPath(animatorController.GetInstanceID());
                string rootDirName = Path.GetDirectoryName(assetPath);
                if ( string.IsNullOrWhiteSpace(destinationFolder) ) {
                    destinationFolder = animatorController.name;
                }
                string newFolder = Path.Combine(rootDirName, destinationFolder);
                if ( !AssetDatabase.IsValidFolder(newFolder) ) {
                    AssetDatabase.CreateFolder(rootDirName, destinationFolder);
                }

                foreach ( AnimatorControllerParameter item in animatorController.parameters ) {
                    string newfilePath  = Path.Combine(newFolder, item.name + ".asset");
                    var    newParameter = CreateInstance<AnimatorParameter>();
                    Debug.Log(newfilePath);
                    newParameter.Initialize(item);
                    AssetDatabase.CreateAsset(newParameter, newfilePath);
                }
            }
        }
    }
}

#endif