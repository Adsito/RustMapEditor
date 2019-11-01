using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[DisallowMultipleComponent]
[SelectionBase]
[ExecuteAlways]
public class PrefabDataHolder : MonoBehaviour
{
    public WorldSerialization.PrefabData prefabData;

    private void Start()
    {
        for (int i = 0; i < GetComponents<Component>().Length; i++)
        {
            ComponentUtility.MoveComponentUp(this);
        }
    }
    public void UpdatePrefabData()
    {
        prefabData.position = gameObject.transform.position - (0.5f * MapIO.terrain.terrainData.size);
        prefabData.rotation = transform.rotation;
        prefabData.scale = transform.localScale;
    }
    public void SnapToGround()
    {
        Vector3 newPos = transform.position;
        Undo.RecordObject(transform, "Snap to Ground");
        newPos.y = MapIO.terrain.SampleHeight(transform.position);
        transform.position = newPos;
    }
}