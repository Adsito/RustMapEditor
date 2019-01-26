using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerOptions : MonoBehaviour
{
    public static bool showBounds = false;
    MapIO mapIO;


    private void OnDrawGizmos()
    {
        if (mapIO == null)
            mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

        if (showBounds)
        {
            Vector3 offset = MapIO.getMapOffset();
            Vector3 size = MapIO.getTerrainSize();
            Gizmos.DrawWireCube(offset, size);
            Gizmos.color = new Color(51f / 255f, 158f / 255f, 204f / 255f, 0.4f);
            Gizmos.DrawCube(new Vector3(offset.x, offset.y/2, offset.z), new Vector3(size.x, size.y/2, size.z));
        }
    }

}
