using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathDataHolder : MonoBehaviour {

    public Vector3 offset = Vector3.zero;
    public WorldSerialization.PathData pathData;

    // Use this for initialization
    void Start () {
		  
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmosSelected()
    {
        Vector3 lastNode = Vector3.zero;
        foreach (WorldSerialization.VectorData v in pathData.nodes)
        {
            Vector3 pos = new Vector3(v.x,v.y,v.z) + offset;
            if (lastNode == Vector3.zero)
            {
                lastNode = pos;
                continue;
            }
            else
            {
                Gizmos.DrawSphere(pos,2f);
                Gizmos.DrawLine(lastNode, pos);
            }
            lastNode = pos;
        }
        
    }
}
