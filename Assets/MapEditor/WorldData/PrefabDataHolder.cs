using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static RustMapEditor.Data.TerrainManager;

[DisallowMultipleComponent]
[SelectionBase]
public class PrefabDataHolder : MonoBehaviour
{
    public WorldSerialization.PrefabData prefabData;

    public void Setup()
    {
        for (int i = 0; i < GetComponents<Component>().Length; i++)
            ComponentUtility.MoveComponentUp(this);
    }

    public void UpdatePrefabData()
    {
        prefabData.position = gameObject.transform.position - (0.5f * land.terrainData.size);
        prefabData.rotation = transform.rotation;
        prefabData.scale = transform.localScale;
    }

    public void SnapToGround()
    {
        Vector3 newPos = transform.position;
        Undo.RecordObject(transform, "Snap to Ground");
        newPos.y = land.SampleHeight(transform.position);
        transform.position = newPos;
    }
}