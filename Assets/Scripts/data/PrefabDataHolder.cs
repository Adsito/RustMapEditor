using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[Serializable]
public class PrefabDataHolder : MonoBehaviour {
    
    public WorldSerialization.PrefabData prefabData;
    public bool showBtn = false;
    public bool saveWithMap = false;

    void Update ()
    {
        if (prefabData == null)
            Debug.LogError("Prefabdata may not be tagged with serialize in SDK");

        prefabData.position = gameObject.transform.position - MapIO.getMapOffset();
        prefabData.rotation = transform.rotation;
        prefabData.scale = transform.localScale;
    }

    public void snapToGround()
    {
        Vector3 newPos = transform.position;
        //Debug.Log((Vector3)transform.position);
        float y = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>().SampleHeight(transform.position);
        newPos.y = y;
        transform.position = newPos;
    }

}

