using System;
using System.Collections;
using System.Collections.Generic;
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

    public TerrainLayer[] textures;
    Terrain terrain;
    [SerializeField]
    string layerName = "";

    void Awake()
    {
        terrain = transform.parent.GetComponent<Terrain>();    
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
        switch (layerName.ToLower())
        {
            case "ground":
                textures = getGroundTextures();
                break;

            case "biome":
                textures = getBiomeTextures();
                break;

            case "alpha":
                textures = getAlphaTextures();
                break;

            case "topology":
                textures = getAlphaTextures();
                break;
        }
    }

    public void setLayer()
    {
        MapIO.ProgressBar("Getting Textures", "Getting textures to paint.", 0.5f);
        terrain = transform.parent.GetComponent<Terrain>();
        if (textures == null)
        {
            getTextures();
        }
        terrain.terrainData.terrainLayers = textures;
        MapIO.ProgressBar("Setting Textures", "Painting textures to terrain.", 0.75f);
        switch (layerName.ToLower())
        {
            case "ground":
                terrain.terrainData.SetAlphamaps(0, 0, groundArray);
                break;
            case "biome":
                terrain.terrainData.SetAlphamaps(0, 0, biomeArray);
                break;
            case "alpha":
                terrain.terrainData.SetAlphamaps(0, 0, alphaArray);
                break;
            case "topology":
                terrain.terrainData.SetAlphamaps(0, 0, TypeConverter.singleToMulti(splatMap, textures.Length));
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
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new TerrainLayer();
        }

        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/misc/active");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/misc/inactive");

        return textures;
    }


    public TerrainLayer[] getBiomeTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[4];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new TerrainLayer();
        }

        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/biomes/arctic");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/biomes/arid");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/biomes/temperate");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/biomes/tundra");

        return textures;
    }

    public TerrainLayer[] getGroundTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[8];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new TerrainLayer();
        }

        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/dirt");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/snow");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/sand");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/rock");
        textures[4].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/grass");
        textures[5].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/forest");
        textures[6].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/stones");
        textures[7].diffuseTexture = Resources.Load<Texture2D>("Textures/ground/gravel");

        return textures;
    }
}
