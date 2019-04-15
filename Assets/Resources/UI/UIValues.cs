using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIValues : MonoBehaviour
{
    MapIO script;

    string loadFile = "";

    string saveFile = "";
    string mapName = "";
    string prefabSaveFile = "";
    //Todo: Clean this up. It's coarse and rough and irritating and it gets everywhere.
    int mapSize = 1000, mainMenuOptions = 0, toolsOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0;
    float heightToSet = 450f, scale = 50f, offset = 0f, mapScale = 1f;
    bool top = false, left = false, right = false, bottom = false, checkHeight = true, setWaterMap = false;
    bool allLayers = false, ground = false, biome = false, alpha = false, topology = false, heightmap = false, prefabs = false, paths = false;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float minBlendLow = 25f, maxBlendLow = 40f, minBlendHigh = 60f, maxBlendHigh = 75f, blendStrength = 5f;
    float minBlendLowHeight = 0f, maxBlendHighHeight = 1000f;
    float normaliseLow = 450f, normaliseHigh = 1000f, normaliseBlend = 1f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;
    int layerConditionalInt, texture = 0;
    bool AlphaVisible = false, AlphaInvisible = false;
    bool TopoActive = false, TopoInactive = false;
    bool deletePrefabs = false;

    bool checkHeightCndtl = false, checkSlopeCndtl = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f;
    float heightLowCndtl = 500f, heightHighCndtl = 600f;

    private TerrainBiome.Enum biomeLayerToPaint;
    private TerrainBiome.Enum biomeLayerConditional;
    private TerrainSplat.Enum groundLayerToPaint;
    private TerrainSplat.Enum groundLayerConditional;

    bool layerSet = false;
    bool[] groundTxtCndtl = new bool[8] { true, true, true, true, true, true, true, true };
    bool[] biomeTxtCndtl = new bool[4] { true, true, true, true };
    bool[] alphaTxtCndtl = new bool[2] { true, true };
    bool[] topoTxtCndtl = new bool[2] { true, true };
    string[] landLayersCndtl = new string[4] { "Ground", "Biome", "Alpha", "Topology" };
    int[] topoLayersCndtl = new int[] { };

    void Start()
    {
         script = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
    }

    public void LoadMap()
    {
        loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");

        var blob = new WorldSerialization();
        if (loadFile == "")
        {
            return;
        }
        blob.Load(loadFile);
        script.loadPath = loadFile;
        script.Load(blob);
    }
}
