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

        MapIOEditor.PrefabCategory(script);
        MapIOEditor.PrefabID(script);
        MapIOEditor.SnapToGround(script);
    }
}