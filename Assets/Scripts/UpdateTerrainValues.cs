using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class UpdateTerrainValues : MonoBehaviour {

    Vector3 oldTerrainSize;
    Vector3 position = Vector3.zero;

    MapIO mapIO;

    Terrain terrain;
        // Update is called once per frame
    void Update ()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();
        if (mapIO == null)
            mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

        if (position.x != transform.position.x || position.z != transform.position.z)
        {
            transform.position = position;
        }

        if (transform.localScale != Vector3.one)
            transform.localScale = Vector3.one;

        if (transform.localRotation.eulerAngles != Vector3.zero)
            transform.localRotation = Quaternion.Euler(Vector3.zero);

        if(oldTerrainSize != terrain.terrainData.size)
        {

            if (oldTerrainSize.x != terrain.terrainData.size.x)
            {
                mapIO.resizeTerrain(new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y, terrain.terrainData.size.x));
            }
            else if (oldTerrainSize.z != terrain.terrainData.size.z)
            {
                mapIO.resizeTerrain(new Vector3(terrain.terrainData.size.z, terrain.terrainData.size.y, terrain.terrainData.size.z));
            }

            if (terrain.terrainData.size.y != 1000)
            {
                mapIO.resizeTerrain(new Vector3(terrain.terrainData.size.x, 1000, terrain.terrainData.size.z));
            }
            oldTerrainSize = terrain.terrainData.size;
        }
    }

    public void setSize(Vector3 oldTerrainSize)
    {
        this.oldTerrainSize = oldTerrainSize;
    }

    public void setPosition(Vector3 position)
    {
        transform.position = position;
        this.position = position;
    }

    

}
