using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[Serializable]


public class LandData : MonoBehaviour {
    

    [HideInInspector]
    [SerializeField]
    public float[] splatMap;

    public float[,,] groundArray;
    public float[,,] biomeArray;
    public float[,,] alphaArray;

    public TerrainLayer[] groundTextures = null;
    public TerrainLayer[] biomeTextures = null;
    public TerrainLayer[] alphaTextures = null;

    Terrain terrain;
    string layerName = "";

    void Awake()
    {
        terrain = transform.parent.GetComponent<Terrain>();
        getTextures();
    }

    public void setData(float[,,] floatArray, string name)
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
                splatMap = TypeConverter.multiToSingle(floatArray);
                break;
        }
        layerName = name;
    }
    public void getTextures()
    {
        groundTextures = getGroundTextures();
        biomeTextures = getBiomeTextures();
        alphaTextures = getAlphaTextures();
    }
    public void setLayer(string layer)
    {
        MapIO.ProgressBar("Getting Textures", "Getting textures to paint.", 0.5f);
        terrain = transform.parent.GetComponent<Terrain>();
        if (groundTextures == null || biomeTextures == null || alphaTextures == null)
        {
            getTextures();
        }
        switch (layer.ToLower())
        {
            case "ground":
                layerName = "ground";
                Selection.activeGameObject = null;
                terrain.terrainData.terrainLayers = groundTextures;
                MapIO.ProgressBar("Setting Textures", "Painting textures to terrain.", 0.75f);
                terrain.terrainData.SetAlphamaps(0, 0, groundArray);
                break;
            case "biome":
                layerName = "biome";
                Selection.activeGameObject = null;
                terrain.terrainData.terrainLayers = biomeTextures;
                MapIO.ProgressBar("Setting Textures", "Painting textures to terrain.", 0.75f);
                terrain.terrainData.SetAlphamaps(0, 0, biomeArray);
                break;
            case "alpha":
                layerName = "alpha";
                Selection.activeGameObject = null;
                terrain.terrainData.terrainLayers = alphaTextures;
                MapIO.ProgressBar("Setting Textures", "Painting textures to terrain.", 0.75f);
                terrain.terrainData.SetAlphamaps(0, 0, alphaArray);
                break;
            case "topology":
                layerName = "topology";
                Selection.activeGameObject = null;
                terrain.terrainData.terrainLayers = alphaTextures;
                MapIO.ProgressBar("Setting Textures", "Painting textures to terrain.", 0.75f);
                terrain.terrainData.SetAlphamaps(0, 0, TypeConverter.singleToMulti(splatMap, 2));
                break;
            default:
                Debug.Log("Layer not set");
                break;
        }
        MapIO.ClearProgressBar();
    }
    public void save()
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
                float[,,] alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                splatMap = TypeConverter.multiToSingle(terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight));
                break;
        }
    }
    public float[] getSplat()
    {
        return splatMap;
    }
    public TerrainLayer[] getAlphaTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[2];
        textures[0] = Resources.Load<TerrainLayer>("Textures/Misc/Active");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/active");
        textures[1] = Resources.Load<TerrainLayer>("Textures/Misc/InActive");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/inactive");
        return textures;
    }


    public TerrainLayer[] getBiomeTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[4];
        textures[0] = Resources.Load<TerrainLayer>("Textures/Biome/Tundra");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/tundra");
        textures[1] = Resources.Load<TerrainLayer>("Textures/Biome/Temperate");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/temperate");
        textures[2] = Resources.Load<TerrainLayer>("Textures/Biome/Arid");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/arid");
        textures[3] = Resources.Load<TerrainLayer>("Textures/Biome/Arctic");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Biomes/arctic");
        return textures;
    }

    public TerrainLayer[] getGroundTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[8];
        textures[0] = Resources.Load<TerrainLayer>("Textures/Ground/Dirt");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/dirt");
        textures[1] = Resources.Load<TerrainLayer>("Textures/Ground/Snow");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/snow");
        textures[2] = Resources.Load<TerrainLayer>("Textures/Ground/Sand");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/sand");
        textures[3] = Resources.Load<TerrainLayer>("Textures/Ground/Rock");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/rock");
        textures[4] = Resources.Load<TerrainLayer>("Textures/Ground/Grass");
        textures[4].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/grass");
        textures[5] = Resources.Load<TerrainLayer>("Textures/Ground/Forest");
        textures[5].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/forest");
        textures[6] = Resources.Load<TerrainLayer>("Textures/Ground/Stones");
        textures[6].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/stones");
        textures[7] = Resources.Load<TerrainLayer>("Textures/Ground/Gravel");
        textures[7].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/gravel");
        return textures;
    }
}
