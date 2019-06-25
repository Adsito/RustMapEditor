using System;
using UnityEngine;

[ExecuteInEditMode]
[Serializable]
public class PrefabDataHolder : MonoBehaviour {
    
    public WorldSerialization.PrefabData prefabData;
    public bool showBtn = false;

    void Start()
    {
        //this.gameObject.transform.parent = GameObject.Find("Prefabs").transform;
    }
    public void MapSave()
    {
        prefabData.position = gameObject.transform.position - MapIO.GetMapOffset();
        prefabData.rotation = transform.rotation;
        prefabData.scale = transform.localScale;
    }
    public void snapToGround()
    {
        Vector3 newPos = transform.position;
        float y = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>().SampleHeight(transform.position);
        newPos.y = y;
        transform.position = newPos;
    }
}

