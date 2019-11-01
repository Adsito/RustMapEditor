using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[SelectionBase]
public class PrefabDataHolder : MonoBehaviour
{
    public WorldSerialization.PrefabData prefabData;
    
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