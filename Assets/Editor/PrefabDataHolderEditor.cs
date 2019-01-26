using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabDataHolder))]
public class PrefabDataHolderEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        PrefabDataHolder script = (PrefabDataHolder)target;
        if (script.prefabData == null)
            return;

        EditorGUILayout.LabelField("Category", script.prefabData.category);
        script.prefabData.id = uint.Parse(EditorGUILayout.TextField("Id", script.prefabData.id + ""));
        script.spawnOnPlay = EditorGUILayout.Toggle("Spawn Prefab on Play", script.spawnOnPlay);
        
        if (GUILayout.Button("Snap to ground"))
        {
            script.snapToGround();
        }
    }
}

 