using UnityEditor;
using RustMapEditor.UI;

[CustomEditor(typeof(CustomPrefabHolder))]
public class CustomPrefabHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CustomPrefabHolder script = (CustomPrefabHolder)target;
        if (script == null)
            return;

        Functions.CustomPrefabName(script);
        Functions.CustomPrefabAuthor(script);
        Functions.SaveCustomPrefab(script);
    }
}