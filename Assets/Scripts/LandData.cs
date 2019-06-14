using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[Serializable]


public class LandData : MonoBehaviour
{
    public float[,,] groundArray;
    public float[,,] biomeArray;
    public float[,,] alphaArray;
    public float[][,,] topologyArray = new float[TerrainTopology.COUNT][,,];

    [HideInInspector]
    public TerrainLayer[] groundTextures = null;
    [HideInInspector]
    public TerrainLayer[] biomeTextures = null;
    [HideInInspector]
    public TerrainLayer[] miscTextures = null;

    Terrain terrain;
    string layerName = "";
    [HideInInspector]
    public int topologyLayer = 0;

    void Awake()
    {
        terrain = transform.parent.GetComponent<Terrain>();
        getTextures();
    }

    public void setData(float[,,] floatArray, string name, int topology = 0)
    {
        switch (name.ToLower())
        {
            case "ground":
                groundArray = floatArray;
                break;
            case "biome":
                biomeArray = floatArray;
                break;
            case "alpha":
                alphaArray = floatArray;
                break;
            case "topology":
                topologyArray[topology] = floatArray; 
                break;
        }
        layerName = name;
    }
    public void getTextures()
    {
        groundTextures = getGroundTextures();
        biomeTextures = getBiomeTextures();
        miscTextures = getAlphaTextures();
    }
    public void setLayer(string layer, int topology = 0)
    {
        terrain = transform.parent.GetComponent<Terrain>();
        if (groundTextures == null || biomeTextures == null || miscTextures == null)
        {
            getTextures();
        }
        Selection.activeGameObject = null;
        switch (layer.ToLower())
        {
            case "ground":
                layerName = "ground";
                terrain.terrainData.terrainLayers = groundTextures;
                terrain.terrainData.SetAlphamaps(0, 0, groundArray);
                break;
            case "biome":
                layerName = "biome";
                terrain.terrainData.terrainLayers = biomeTextures;
                terrain.terrainData.SetAlphamaps(0, 0, biomeArray);
                break;
            case "alpha":
                layerName = "alpha";
                terrain.terrainData.terrainLayers = miscTextures;
                terrain.terrainData.SetAlphamaps(0, 0, alphaArray);
                break;
            case "topology":
                layerName = "topology";
                terrain.terrainData.terrainLayers = miscTextures;
                terrain.terrainData.SetAlphamaps(0, 0, topologyArray[topology]);
                break;
            default:
                Debug.Log("Layer not set");
                break;
        }
    }
    public void save(int topologyLayer = 0)
    {
        switch (layerName)
        {
            case "ground":
                groundArray = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                break;
            case "biome":
                biomeArray = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                break;
            case "alpha":
                alphaArray = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                break;
            case "topology":
                topologyArray[topologyLayer] = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                break;
        }
    }
    public TerrainLayer[] getAlphaTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[2];
        textures[0] = Resources.Load<TerrainLayer>("Textures/Misc/Active");
        //textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/active");
        textures[1] = Resources.Load<TerrainLayer>("Textures/Misc/InActive");
        //textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/inactive");
        return textures;
    }


    public TerrainLayer[] getBiomeTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[4];
        textures[0] = Resources.Load<TerrainLayer>("Textures/Biome/Tundra");
        //textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/tundra");
        textures[1] = Resources.Load<TerrainLayer>("Textures/Biome/Temperate");
        //textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/temperate");
        textures[2] = Resources.Load<TerrainLayer>("Textures/Biome/Arid");
        //textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/arid");
        textures[3] = Resources.Load<TerrainLayer>("Textures/Biome/Arctic");
        //textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/arctic");
        return textures;
    }

    public TerrainLayer[] getGroundTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[8];
        textures[0] = Resources.Load<TerrainLayer>("Textures/Ground/Dirt");
        //textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/dirt");
        textures[1] = Resources.Load<TerrainLayer>("Textures/Ground/Snow");
        //textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/snow");
        textures[2] = Resources.Load<TerrainLayer>("Textures/Ground/Sand");
        //textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/sand");
        textures[3] = Resources.Load<TerrainLayer>("Textures/Ground/Rock");
        //textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/rock");
        textures[4] = Resources.Load<TerrainLayer>("Textures/Ground/Grass");
        //textures[4].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/grass");
        textures[5] = Resources.Load<TerrainLayer>("Textures/Ground/Forest");
        //textures[5].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/forest");
        textures[6] = Resources.Load<TerrainLayer>("Textures/Ground/Stones");
        //textures[6].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/stones");
        textures[7] = Resources.Load<TerrainLayer>("Textures/Ground/Gravel");
        //textures[7].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/gravel");
        return textures;
    }
}
