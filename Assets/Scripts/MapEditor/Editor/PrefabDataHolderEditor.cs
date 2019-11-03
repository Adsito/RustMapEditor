using UnityEditor;

[CustomEditor(typeof(PrefabDataHolder))]
public class PrefabDataHolderEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        PrefabDataHolder script = (PrefabDataHolder)target;
        if (script.prefabData == null)
        {
            return;
        }

        EditorUIFunctions.PrefabCategory(script);
        EditorUIFunctions.PrefabID(script);
        EditorUIFunctions.SnapToGround(script);
    }
}