using Drboum.Utilities.Runtime.EditorHybrid;
using Drboum.Utilities.Runtime.EditorHybrid;
using UnityEditor;
using UnityEngine;
namespace GameProject.Editors;

[CustomEditor(typeof(AssetReferenceID), true)]
public class AssetReferenceIDEditor : Editor {
    public override void OnInspectorGUI()
    {
        var assetId = target as AssetReferenceID;
        if ( !assetId.IsNull() )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(nameof(AssetReferenceID.Guid),GUILayout.Width(50));
            EditorGUILayout.SelectableLabel(assetId.Guid.ToString(),GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }
        base.OnInspectorGUI();
    }
}