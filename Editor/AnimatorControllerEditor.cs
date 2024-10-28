#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Drboum.Utilities.Runtime.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Drboum.Utilities.Editor
{
    [CustomEditor(typeof(AnimatorController))]
    public class AnimatorControllerEditor : UnityEditor.Editor
    {
        public string destinationFolder;
        public const string ANIMATOR_PARAMETER_ASSET_EXTENSION = ".asset";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var animatorController = (AnimatorController)target;
            if ( string.IsNullOrWhiteSpace(destinationFolder) )
            {
                destinationFolder = animatorController.name + "/";
            }
            destinationFolder = EditorGUILayout.TextField(destinationFolder);
            if ( GUILayout.Button("Export animator parameters") )
            {
                OnExportParameter(animatorController);
            }
        }

        private void OnExportParameter(AnimatorController animatorController)
        {
            string assetPath = AssetDatabase.GetAssetPath(animatorController);
            string rootDirName = Path.GetDirectoryName(assetPath);
            string newFolder = Path.Combine(rootDirName, destinationFolder);
            if ( !AssetDatabase.IsValidFolder(newFolder) )
            {
                AssetDatabase.CreateFolder(rootDirName, destinationFolder);
            }
            UnityObjectEditorHelper.FindAllAssetInstances<AnimatorParameter>(null, out _, out var existingAnimatorParamWithPath);
            var existingAssetLookup = new Dictionary<int, List<(AnimatorParameter Asset, string Path)>>();

            foreach ( var animatorParameterWithPath in existingAnimatorParamWithPath )
            {
                var animatorParameter = animatorParameterWithPath.Asset;
                if ( !existingAssetLookup.TryGetValue(animatorParameter.HashId, out var animatorParameters) )
                {
                    animatorParameters = new();
                    existingAssetLookup.Add(animatorParameter.HashId, animatorParameters);
                }
                animatorParameters.Add(animatorParameterWithPath);
            }
            
            var changeExistingAssets = false;
            foreach ( AnimatorControllerParameter item in animatorController.parameters )
            {
                string newFilePath = Path.Combine(newFolder, item.name + ANIMATOR_PARAMETER_ASSET_EXTENSION);
                bool hashExist = existingAssetLookup.TryGetValue(item.nameHash, out var animatorParameterList);
                AnimatorParameter managedParameter = null;
                string oldPath = null;
                if ( hashExist )
                {
                    foreach ( var existingParameterWithPath in animatorParameterList )
                    {
                        var animatorParameter = existingParameterWithPath.Asset;
                        if ( animatorParameter.AnimatorController == animatorController )
                        {
                            if ( !managedParameter )
                            {
                                managedParameter = animatorParameter;
                                oldPath = existingParameterWithPath.Path;
                            }
                        }
                    }
                    hashExist = managedParameter;
                    changeExistingAssets |= managedParameter;
                    if ( changeExistingAssets && string.IsNullOrEmpty(AssetDatabase.ValidateMoveAsset(oldPath, newFilePath)) )
                    {
                        AssetDatabase.MoveAsset(oldPath, newFilePath);
                    }
                }

                if ( !managedParameter )
                {
                    managedParameter = CreateInstance<AnimatorParameter>();
                }

                managedParameter.Initialize(item, animatorController);

                if ( !hashExist )
                {
                    AssetDatabase.CreateAsset(managedParameter, newFilePath);
                }
            }

            if ( changeExistingAssets )
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}

#endif