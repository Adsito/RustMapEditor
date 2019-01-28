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

    public SplatPrototype[] textures;
    Terrain terrain;
    [SerializeField]
    string layerName = "";

    void Awake()
    {
        terrain = transform.parent.GetComponent<Terrain>();    
    }

    public void setData(float[,,] floatArray, string name)
    {
        splatMap = TypeConverter.multiToSingle(floatArray);
        layerName = name;
    }

    public void getTextures()
    {
        switch (layerName)
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
        if(terrain == null)
            terrain = transform.parent.GetComponent<Terrain>();

        if (textures == null)
            getTextures();

        terrain.terrainData.splatPrototypes = textures;
        float[,,] splats = TypeConverter.singleToMulti(splatMap, textures.Length);
        /*Debug.Log(splats.GetLength(0) + " " + splats.GetLength(1) + " " + splats.GetLength(2) + " ");
        Debug.Log(terrain.terrainData.alphamapResolution);
        Debug.Log(terrain.terrainData.alphamapHeight + " " + terrain.terrainData.alphamapWidth);*/
        terrain.terrainData.SetAlphamaps(0, 0, splats);
    }


    public void save()
    {
        float[,,] alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        splatMap = TypeConverter.multiToSingle(terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight));
    }

    public float[] getSplat()
    {
        return splatMap;
    }


    public SplatPrototype[] getAlphaTextures()
    {
        SplatPrototype[] textures = new SplatPrototype[2];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new SplatPrototype();
        }

        textures[0].texture = Resources.Load<Texture2D>("Textures/misc/active");
        textures[1].texture = Resources.Load<Texture2D>("Textures/misc/inactive");

        return textures;
    }


    public SplatPrototype[] getBiomeTextures()
    {
        SplatPrototype[] textures = new SplatPrototype[4];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new SplatPrototype();
        }

        textures[3].texture = Resources.Load<Texture2D>("Textures/biomes/arctic");
        textures[2].texture = Resources.Load<Texture2D>("Textures/biomes/arid");
        textures[1].texture = Resources.Load<Texture2D>("Textures/biomes/temperate");
        textures[0].texture = Resources.Load<Texture2D>("Textures/biomes/tundra");

        return textures;
    }

    public SplatPrototype[] getGroundTextures()
    {
        SplatPrototype[] textures = new SplatPrototype[8];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = new SplatPrototype();
        }

        textures[0].texture = Resources.Load<Texture2D>("Textures/ground/dirt");
        textures[1].texture = Resources.Load<Texture2D>("Textures/ground/snow");
        textures[2].texture = Resources.Load<Texture2D>("Textures/ground/sand");
        textures[3].texture = Resources.Load<Texture2D>("Textures/ground/rock");
        textures[4].texture = Resources.Load<Texture2D>("Textures/ground/grass");
        textures[5].texture = Resources.Load<Texture2D>("Textures/ground/forest");
        textures[6].texture = Resources.Load<Texture2D>("Textures/ground/stones");
        textures[7].texture = Resources.Load<Texture2D>("Textures/ground/gravel");

        return textures;
    }
}
