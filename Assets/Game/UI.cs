using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    MapIO script;
    string loadFile = "";

    string saveFile = "";
    string mapName = "";

    int mapSize = 1000;
    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;
    float heightToSet = 450f, scale = 0f, offset = 0f, minimumHeight = 450f, maximumHeight = 1000f;
    bool top = false, left = false, right = false, bottom = false, checkHeight = true, setWaterMap = false;

    void Start()
    {
        script = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
    }
    
    public void autoGenerateTopology(bool trueFalse)
    {
        /*if (GUILayout.Button("Paint Default Topologies"))
        {
            script.autoGenerateTopology(false);
        }*/
        // This ^^ function from the MapIOEditor is now.
        script.autoGenerateTopology(false);
    }
}
