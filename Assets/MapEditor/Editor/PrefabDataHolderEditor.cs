using UnityEditor;
using RustMapEditor.UI;

[CustomEditor(typeof(PrefabDataHolder))]
public class PrefabDataHolderEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        PrefabDataHolder script = (PrefabDataHolder)target;
        if (script.prefabData == null)
            return;

        Functions.PrefabCategory(script);
        Functions.PrefabID(script);
        Functions.SnapToGround(script);
        Functions.ToggleLights(script);
        //Functions.BreakPrefab(script);
    }
}