﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static WorldConverter;
using static WorldSerialization;

[Serializable]

public class MapIO : MonoBehaviour {
    #region LayersFrom
    public TerrainTopology.Enum topologyLayerFrom;
    public TerrainTopology.Enum topologyLayerToPaint;
    public TerrainSplat.Enum groundLayerFrom;
    public TerrainSplat.Enum groundLayerToPaint;
    public TerrainBiome.Enum biomeLayerFrom;
    public TerrainBiome.Enum biomeLayerToPaint;
    #endregion
    public TerrainTopology.Enum topologyLayer;
    public TerrainTopology.Enum oldTopologyLayer;
    public TerrainTopology.Enum oldTopologyLayer2;
    public TerrainBiome.Enum biomeLayer;
    public TerrainSplat.Enum terrainLayer;
    public int landSelectIndex = 0;
    public string landLayer = "ground";
    LandData selectedLandLayer;
    private PrefabLookup prefabLookup;
    static TopologyMesh topology;

    public void setPrefabLookup(PrefabLookup prefabLookup)
    {
        this.prefabLookup = prefabLookup;
    }
    public PrefabLookup getPrefabLookUp()
    {
        return prefabLookup;
    }

    public void changeLayer(string layer)
    {
        landLayer = layer;
        changeLandLayer();
    }

    public void saveTopologyLayer()
    {
        if (topology == null)
            topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();

        LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        TerrainMap<int> topologyMap = new TerrainMap<int>(topology.top,1);
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap,2);

        if (splatMap == null)
        {
            Debug.LogError("Splatmap is null");
            return;
        }
        for (int i = 0; i < topologyMap.res; i++)
        {
            for (int j = 0; j < topologyMap.res; j++)
            {
                if(splatMap[i,j,0] > 0)
                {
                    topologyMap[i, j] = topologyMap[i, j] | (int)oldTopologyLayer;
                }
                if (splatMap[i, j, 1] > 0)
                {
                    topologyMap[i, j] = topologyMap[i, j] & ~(int)oldTopologyLayer;
                }
            }
        }
        topology.top = topologyMap.ToByteArray();
    }
    
    public void changeLandLayer()
    {
        if (topology == null)
            topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();

        if (selectedLandLayer != null)
            selectedLandLayer.save();

        switch (landLayer.ToLower())
        {
            case "ground":
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
                break;
            case "biome":
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
                break;
            case "alpha":
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
                break;
            case "topology":
                //updated topology values
                //selectedLandLayer.splatMap;
                saveTopologyLayer();
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
                selectedLandLayer.setData(topology.getSplatMap((int)topologyLayer), "topology");
                break;
        }
        selectedLandLayer.setLayer();
    }



    public GameObject spawnPrefab(GameObject g, PrefabData prefabData, Transform parent = null)
    {
        Vector3 pos = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        Vector3 scale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        Quaternion rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));

        
        GameObject newObj = Instantiate(g, pos + getMapOffset(), rotation, parent);
        newObj.transform.localScale = scale;

        return newObj;
    }

    private void cleanUpMap()
    {
        //offset = 0;
        selectedLandLayer = null;
        foreach(PrefabDataHolder g in GameObject.FindObjectsOfType<PrefabDataHolder>())
        {
            DestroyImmediate(g.gameObject);
        }

        foreach (PathDataHolder g in GameObject.FindObjectsOfType<PathDataHolder>())
        {
            DestroyImmediate(g.gameObject);
        }
    }


    public static Vector3 getTerrainSize()
    {
        return GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>().terrainData.size;
    }
    public static Vector3 getMapOffset()
    {
        //Debug.Log(0.5f * getTerrainSize());
        return 0.5f * getTerrainSize();
    }
    
    #region RotateMap Methods
    public void rotateHeightmap(bool CW) //Rotates Terrain Map and Water Map 90°.
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();

        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);

        if (CW)
        {
            land.terrainData.SetHeights(0, 0, MapTransformations.rotateCW(heightMap));
            water.terrainData.SetHeights(0, 0, MapTransformations.rotateCW(waterMap));
        }
        else
        {
            land.terrainData.SetHeights(0, 0, MapTransformations.rotateCCW(heightMap));
            water.terrainData.SetHeights(0, 0, MapTransformations.rotateCCW(waterMap));
        }
    }
    public void rotateObjects(bool CW) //Needs prefabs in scene to be all at Vector3.Zero to work. Rotates objects 90.
    {
        var prefabRotate = GameObject.FindGameObjectWithTag("Prefabs");
        var pathRotate = GameObject.FindGameObjectWithTag("Paths");
        if (CW)
        {
            prefabRotate.transform.Rotate(0, 90, 0, Space.World);
            pathRotate.transform.Rotate(0, 90, 0, Space.World);
        }
        else
        {
            prefabRotate.transform.Rotate(0, -90, 0, Space.World);
            pathRotate.transform.Rotate(0, -90, 0, Space.World);
        }
    }
    public void rotateGroundmap(bool CW) //Rotates Groundmap 90 degrees for CW true.
    {
        LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
        float[,,] oldGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);

        if (CW)
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    newGround[i, j, 0] = oldGround[j, oldGround.GetLength(1) - i - 1, 0];
                    newGround[i, j, 1] = oldGround[j, oldGround.GetLength(1) - i - 1, 1];
                    newGround[i, j, 2] = oldGround[j, oldGround.GetLength(1) - i - 1, 2];
                    newGround[i, j, 3] = oldGround[j, oldGround.GetLength(1) - i - 1, 3];
                    newGround[i, j, 4] = oldGround[j, oldGround.GetLength(1) - i - 1, 4];
                    newGround[i, j, 5] = oldGround[j, oldGround.GetLength(1) - i - 1, 5];
                    newGround[i, j, 6] = oldGround[j, oldGround.GetLength(1) - i - 1, 6];
                    newGround[i, j, 7] = oldGround[j, oldGround.GetLength(1) - i - 1, 7];
                }
            }
        }
        else
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    newGround[i, j, 0] = oldGround[oldGround.GetLength(0) - j - 1, i, 0];
                    newGround[i, j, 1] = oldGround[oldGround.GetLength(0) - j - 1, i, 1];
                    newGround[i, j, 2] = oldGround[oldGround.GetLength(0) - j - 1, i, 2];
                    newGround[i, j, 3] = oldGround[oldGround.GetLength(0) - j - 1, i, 3];
                    newGround[i, j, 4] = oldGround[oldGround.GetLength(0) - j - 1, i, 4];
                    newGround[i, j, 5] = oldGround[oldGround.GetLength(0) - j - 1, i, 5];
                    newGround[i, j, 6] = oldGround[oldGround.GetLength(0) - j - 1, i, 6];
                    newGround[i, j, 7] = oldGround[oldGround.GetLength(0) - j - 1, i, 7];
                }
            }
        }
        groundLandData.setData(newGround, "ground");
        groundLandData.setLayer();
    }
    public void rotateBiomemap(bool CW) //Rotates Biomemap 90 degrees for CW true.
    {
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
        float[,,] oldBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);

        if (CW)
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    newBiome[i, j, 0] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 0];
                    newBiome[i, j, 1] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 1];
                    newBiome[i, j, 2] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 2];
                    newBiome[i, j, 3] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 3];
                }
            }
        }
        else
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    newBiome[i, j, 0] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 0];
                    newBiome[i, j, 1] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 1];
                    newBiome[i, j, 2] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 2];
                    newBiome[i, j, 3] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 3];
                }
            }
        }
        biomeLandData.setData(newBiome, "biome");
        biomeLandData.setLayer();
    }
    public void rotateAlphamap(bool CW) //Rotates Alphamap 90 degrees for CW true.
    {
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        float[,,] newAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);
        float[,,] oldAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);

        if (CW)
        {
            for (int i = 0; i < newAlpha.GetLength(0); i++)
            {
                for (int j = 0; j < newAlpha.GetLength(1); j++)
                {
                    newAlpha[i, j, 0] = oldAlpha[j, oldAlpha.GetLength(1) - i - 1, 0];
                    newAlpha[i, j, 1] = oldAlpha[j, oldAlpha.GetLength(1) - i - 1, 1];
                }
            }
        }
        else
        {
            for (int i = 0; i < newAlpha.GetLength(0); i++)
            {
                for (int j = 0; j < newAlpha.GetLength(1); j++)
                {
                    newAlpha[i, j, 0] = oldAlpha[oldAlpha.GetLength(0) - j - 1, i, 0];
                    newAlpha[i, j, 1] = oldAlpha[oldAlpha.GetLength(0) - j - 1, i, 1];
                }
            }
        }
        alphaLandData.setData(newAlpha, "alpha");
        alphaLandData.setLayer();
    }
    public void rotateTopologymap(bool CW) //Rotates Topology map 90 degrees for CW true.
    {
        LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] newTopology = TypeConverter.singleToMulti(topologyLandData.splatMap, 2);
        float[,,] oldTopology = TypeConverter.singleToMulti(topologyLandData.splatMap, 2);

        if (CW)
        {
            for (int i = 0; i < newTopology.GetLength(0); i++)
            {
                for (int j = 0; j < newTopology.GetLength(1); j++)
                {
                    newTopology[i, j, 0] = oldTopology[j, oldTopology.GetLength(1) - i - 1, 0];
                    newTopology[i, j, 1] = oldTopology[j, oldTopology.GetLength(1) - i - 1, 1];
                }
            }
        }
        else
        {
            for (int i = 0; i < newTopology.GetLength(0); i++)
            {
                for (int j = 0; j < newTopology.GetLength(1); j++)
                {
                    newTopology[i, j, 0] = oldTopology[oldTopology.GetLength(0) - j - 1, i, 0];
                    newTopology[i, j, 1] = oldTopology[oldTopology.GetLength(0) - j - 1, i, 1];
                }
            }
        }
        topologyLandData.setData(newTopology, "topology");
        topologyLandData.setLayer();
    }
    public void rotateAllTopologymap(bool CW) //Rotates All Topology maps 90 degrees for CW true.
    {
        LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] newTopology = TypeConverter.singleToMulti(topologyLandData.splatMap, 2);
        float[,,] oldTopology = TypeConverter.singleToMulti(topologyLandData.splatMap, 2);
        oldTopologyLayer2 = topologyLayer;
        
        topologyLayer = TerrainTopology.Enum.Alt;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Alt;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Beach;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Beach;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Beachside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Beachside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Beachside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Beachside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Building;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Building;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Cliff;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Cliff;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Cliffside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Cliffside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Clutter;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Clutter;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Decor;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Decor;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Field;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Field;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Forest;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Forest;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Forestside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Forestside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Hilltop;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Hilltop;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Lake;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Lake;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Lakeside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Lakeside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Mainland;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Mainland;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Monument;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Monument;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Mountain;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Mountain;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Ocean;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Ocean;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Oceanside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Oceanside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Offshore;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Offshore;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Powerline;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Powerline;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.River;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.River;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Riverside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Riverside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Road;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Road;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Roadside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Roadside;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Runway;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Runway;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Summit;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Summit;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Swamp;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Swamp;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Tier0;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Tier0;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Tier1;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Tier1;
        rotateTopologymap(CW);

        topologyLayer = TerrainTopology.Enum.Tier2;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Tier2;
        rotateTopologymap(CW);

        topologyLayer = oldTopologyLayer2;
        changeLandLayer();
    }
    #endregion

    #region HeightMap Methods
    public float scale = 1f;
    public void scaleHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        land.terrainData.SetHeights(0, 0, MapTransformations.scale(heightMap, scale));
    }
    public void flipHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        land.terrainData.SetHeights(0, 0, MapTransformations.flip(heightMap));
    }
    public void transposeHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        land.terrainData.SetHeights(0, 0, MapTransformations.transpose(heightMap));
    }
    public void offsetHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Vector3 difference = land.transform.position;
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        for (int i = 0; i < heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < heightMap.GetLength(1); j++)
            {
                heightMap[i, j] = heightMap[i, j] + (difference.y / land.terrainData.size.y);
            }
        }
        land.terrainData.SetHeights(0, 0, heightMap);
        land.transform.position = Vector3.zero;
    }
    #endregion

    #region SplatMap Methods
    // Todo: I will rewrite the autoGenerate methods to use loops instead of a shittonne of code, however that will require rewriting the current TerrainTopology class, which will break
    // other scripts, and prevent easily merging in updates from Dez's own editor repo. I am well aware it's not the best way to iterate through the layers :)
    // It will be fixed shortly.
    public int textures(string landLayer) // Active texture selected in layer. Call method with a string type of the layer to search. 
    // Accepts "Ground", "Biome", "Alpha" and "Topology".
    {
        if (landLayer == "Ground")
        {
            return TerrainSplat.TypeToIndex((int)terrainLayer); // Layer texture to paint from Ground Textures.
        }
        if (landLayer == "Biome")
        {
            return TerrainBiome.TypeToIndex((int)biomeLayer); // Layer texture to paint from Biome Textures.
        }
        return 2;
    }
    public int textureCount(string landLayer) // Texture count in layer chosen, used for determining the size of the splatmap array.
    // Call method with the layer you are painting to.
    {
        if(landLayer == "Ground")
        {
            return 8;
        }
        if (landLayer == "Biome")
        {
            return 4;
        }
        return 2;
    }
    public void paintHeight(string landLayer, float y1, float y2, float opacity, int t ) // Paints height between 2 floats. Opacity is currently not implemented so just give it value
    // of float.MaxValue to avoid errors.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        if (landLayer == "Ground")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        else if (landLayer == "Biome")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        if (land.terrainData.heightmapWidth > 2049) //The splatmaps are clamped at 2048 so if the heightmap is double we assign each heightmap coord to every second splatmap coord.
        {
            for (int i = 0; i < 4096; i++)
            {
                for (int j = 0; j < 4096; j++)
                {
                    if (baseMap[i, j] * 1000f > y1 && baseMap[i, j] * 1000f < y2)
                    {
                        splatMap[i / 2, j / 2, 0] = 0;
                        splatMap[i / 2, j / 2, 1] = 0;
                        if (textureCount(landLayer) > 2)
                        {
                            splatMap[i / 2, j / 2, 2] = 0;
                            splatMap[i / 2, j / 2, 3] = 0;
                            if (textureCount(landLayer) > 4)
                            {
                                splatMap[i / 2, j / 2, 4] = 0;
                                splatMap[i / 2, j / 2, 5] = 0;
                                splatMap[i / 2, j / 2, 6] = 0;
                                splatMap[i / 2, j / 2, 7] = 0;
                            }
                        }
                        splatMap[i / 2, j / 2, t] = 1;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < splatMap.GetLength(0); i++)
            {
                for (int j = 0; j < splatMap.GetLength(1); j++)
                {
                    if (baseMap[i, j] * 1000f > y1 && baseMap[i, j] * 1000f < y2)
                    {
                        splatMap[i, j, 0] = 0;
                        splatMap[i, j, 1] = 0;
                        if (textureCount(landLayer) > 2)
                        {
                            splatMap[i, j, 2] = 0;
                            splatMap[i, j, 3] = 0;
                            if (textureCount(landLayer) > 4)
                            {
                                splatMap[i, j, 4] = 0;
                                splatMap[i, j, 5] = 0;
                                splatMap[i, j, 6] = 0;
                                splatMap[i, j, 7] = 0;
                            }
                        }
                        splatMap[i, j, t] = 1;
                    }
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        if (landLayer == "Topology")
        {
            saveTopologyLayer();
        }
    }
    public void paintLayer(string landLayer, int t) // Sets whole layer to the active texture. 
    //Alpha layers are inverted because it's more logical to have clear Alpha = Terrain appears in game.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        if (landLayer == "Ground")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        else if (landLayer == "Biome")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (landLayer == "Alpha")
                {
                    splatMap[i, j, 1] = float.MaxValue;
                    splatMap[i, j, 0] = float.MinValue;
                }
                else if (landLayer == "Topology")
                {
                    splatMap[i, j, 1] = float.MinValue;
                    splatMap[i, j, 0] = float.MaxValue;
                }
                else
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    if (textureCount(landLayer) > 2)
                    {
                        splatMap[i, j, 2] = 0;
                        splatMap[i, j, 3] = 0;
                        if (textureCount(landLayer) > 4)
                        {
                            splatMap[i, j, 4] = 0;
                            splatMap[i, j, 5] = 0;
                            splatMap[i, j, 6] = 0;
                            splatMap[i, j, 7] = 0;
                        }
                    }
                    splatMap[i, j, t] = 1;
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        if (landLayer == "Topology")
        {
            saveTopologyLayer();
        }
    } 
    public void clearLayer(string landLayer) // Sets whole layer to the inactive texture. Alpha and Topology only. 
    //Alpha layers are inverted because it's more logical to have clear Alpha = Terrain appears in game.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textures(landLayer));
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (landLayer == "Alpha")
                {
                    splatMap[i, j, 0] = float.MaxValue;
                    splatMap[i, j, 1] = float.MinValue;
                }
                else
                {
                    splatMap[i, j, 0] = float.MinValue;
                    splatMap[i, j, 1] = float.MaxValue;
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        if (landLayer == "Topology")
        {
            saveTopologyLayer();
        }
    } 
    public void paintSlope(string landLayer, float slopeLow, float slopeHigh, int t) // Paints slope based on the current slope input, the slope range is set to 90° - wherever the slider is set.
    // The slider to the far left = 90° - 90°, the slider in the middle = 90° - 45° and at the far right would be 90° - 0° or the whole map.
    // The slope value must be between 0.99f and 1.0f. The slider is clamped to this value.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        if (landLayer == "Ground")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        else if (landLayer == "Biome")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        if (land.terrainData.heightmapWidth > 2049) //The splatmaps are clamped at 2048 so if the heightmap is double we assign each heightmap coord to every second splatmap coord.
        {
            for (int i = 1; i < 4095; i++)
            {
                for (int j = 1; j < 4095; j++)
                {
                    if (baseMap[i, j] / baseMap[i + 1, j + 1] > slopeLow || baseMap[i, j] / baseMap[i - 1, j - 1] > slopeLow)
                    {
                        if (baseMap[i, j] / baseMap[i + 1, j + 1] < slopeHigh || baseMap[i, j] / baseMap[i - 1, j - 1] < slopeHigh)
                        {
                            splatMap[i / 2, j / 2, 0] = 0;
                            splatMap[i / 2, j / 2, 1] = 0;
                            if (textureCount(landLayer) > 2)
                            {
                                splatMap[i / 2, j / 2, 2] = 0;
                                splatMap[i / 2, j / 2, 3] = 0;
                                if (textureCount(landLayer) > 4)
                                {
                                    splatMap[i / 2, j / 2, 4] = 0;
                                    splatMap[i / 2, j / 2, 5] = 0;
                                    splatMap[i / 2, j / 2, 6] = 0;
                                    splatMap[i / 2, j / 2, 7] = 0;
                                }
                            }
                            splatMap[i / 2, j / 2, t] = 1;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < splatMap.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < splatMap.GetLength(1) - 1; j++)
                {
                    if ((baseMap[i, j] / baseMap[i + 1, j + 1] < slopeHigh) && (baseMap[i, j] / baseMap[i + 1, j + 1] > slopeLow))
                        {
                            splatMap[i, j, 0] = 0;
                            splatMap[i, j, 1] = 0;
                            if (textureCount(landLayer) > 2)
                            {
                                splatMap[i, j, 2] = 0;
                                splatMap[i, j, 3] = 0;
                                if (textureCount(landLayer) > 4)
                                {
                                    splatMap[i, j, 4] = 0;
                                    splatMap[i, j, 5] = 0;
                                    splatMap[i, j, 6] = 0;
                                    splatMap[i, j, 7] = 0;
                                }
                            }
                            splatMap[i, j, t] = 1;
                        }
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        if (landLayer == "Topology")
        {
            saveTopologyLayer();
        }
    }
    public void paintArea(string landLayer, int z1, int z2, int x1, int x2, int t) // Paints area within these splatmap coords, Maps will always have a splatmap resolution between
    // 512 - 2048 resolution, to the nearest Power of Two (512, 1024, 2048). Face downright in the editor with Z axis facing up, and X axis facing right, and the map will draw
    // from the bottom left corner, up to the top right. So a value of z1 = 0, z2 = 500, x1 = 0, x2 = 1000, would paint 500 pixels up, and 1000 pixels left from the bottom right corner.
    // Note that the results of how much of the map is covered is dependant on the map size, a 2000 map size would paint almost the bottom half of the map, whereas a 4000 map would 
    // paint up nearly one quarter of the map, and across nearly half of the map.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        if (landLayer == "Ground")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        else if (landLayer == "Biome")
        {
            t = textures(landLayer); // Active texture to paint on layer.
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (i >= z1 && i <= z2)
                {
                    if (j >= x1 && j <= x2)
                    {
                            splatMap[i, j, 0] = 0;
                            splatMap[i, j, 1] = 0;
                            if (textureCount(landLayer) > 2)
                            {
                                splatMap[i, j, 2] = 0;
                                splatMap[i, j, 3] = 0;
                                if (textureCount(landLayer) > 4)
                                {
                                    splatMap[i, j, 4] = 0;
                                    splatMap[i, j, 5] = 0;
                                    splatMap[i, j, 6] = 0;
                                    splatMap[i, j, 7] = 0;
                                }
                        }
                        splatMap[i, j, t] = 1;
                    }
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        if (landLayer == "Topology")
        {
            saveTopologyLayer();
        }
    }
    public void autoGenerateTopology(bool wipeLayer) // Assigns topology active to these values. If wipeLayer == true it will wipe the existing topologies on the layer before painting
    // the new topologies.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, 2);
        changeLayer("Topology");
        if (wipeLayer == true) //Wipes layer then paints on active textures.
        {
            oldTopologyLayer = TerrainTopology.Enum.Offshore; //If wiping layers we don't need to get the current layers splatmap detail, so we just wipe it clean then repaint.
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintHeight("Topology", 0, 475, float.MaxValue, 0);

            oldTopologyLayer = TerrainTopology.Enum.Ocean;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintHeight("Topology", 0, 498, float.MaxValue, 0);

            oldTopologyLayer = TerrainTopology.Enum.Beach;
            paintHeight("Topology", 0, 1000, float.MaxValue,  1);
            paintHeight("Topology", 500, 502, float.MaxValue, 0);

            oldTopologyLayer = TerrainTopology.Enum.Oceanside;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintHeight("Topology", 500, 502, float.MaxValue, 0);

            oldTopologyLayer = TerrainTopology.Enum.Mainland;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintHeight("Topology", 500, 1000, float.MaxValue, 0);

            oldTopologyLayer = TerrainTopology.Enum.Cliff;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintSlope("Topology", 0.995f, 1f, 0);

            oldTopologyLayer = TerrainTopology.Enum.Tier0;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintArea("Topology", 0, splatMap.GetLength(0) / 3 , 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            oldTopologyLayer = TerrainTopology.Enum.Tier1;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintArea("Topology", splatMap.GetLength(0) / 3, splatMap.GetLength(0) / 3 * 2, 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            oldTopologyLayer = TerrainTopology.Enum.Tier2;
            paintHeight("Topology", 0, 1000, float.MaxValue, 1);
            paintArea("Topology", splatMap.GetLength(0) / 3 * 2, splatMap.GetLength(0), 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            changeLandLayer();
        }
        else if (wipeLayer == false) // Paints active texture on to layer whilst keeping the current layer's textures.
        {
            oldTopologyLayer2 = topologyLayer; //This saves the currently selected topology layer so we can swap back to it at the end, ensuring we don't accidentally erase anything.

            topologyLayer = TerrainTopology.Enum.Offshore; // This sets the new current topology layer to offshore.
            changeLandLayer(); // This changes the topology layer to offshore. It also saves the previous layer for us.
            oldTopologyLayer = TerrainTopology.Enum.Offshore; // This is the layer the paint the offshore height to.
            paintHeight("Topology", 0, 475, float.MaxValue, 0);

            topologyLayer = TerrainTopology.Enum.Ocean;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Ocean;
            paintHeight("Topology", 0, 498, float.MaxValue, 0);

            topologyLayer = TerrainTopology.Enum.Beach;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Beach;
            paintHeight("Topology", 500, 502, float.MaxValue, 0);

            topologyLayer = TerrainTopology.Enum.Oceanside;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Oceanside;
            paintHeight("Topology", 500, 502, float.MaxValue, 0);

            topologyLayer = TerrainTopology.Enum.Mainland;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Mainland;
            paintHeight("Topology", 500, 1000, float.MaxValue, 0);

            topologyLayer = TerrainTopology.Enum.Cliff;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Cliff;
            paintSlope("Topology", 0.995f, 1f, 0);

            topologyLayer = TerrainTopology.Enum.Tier0;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier0;
            paintArea("Topology", 0, splatMap.GetLength(0) / 3, 0, splatMap.GetLength(0), 0);
            
            topologyLayer = TerrainTopology.Enum.Tier1;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier1;
            paintArea("Topology", splatMap.GetLength(0) / 3, splatMap.GetLength(0) / 3 * 2, 0, splatMap.GetLength(0), 0);
            
            topologyLayer = TerrainTopology.Enum.Tier2;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier2;
            paintArea("Topology", splatMap.GetLength(0) / 3 * 2, splatMap.GetLength(0), 0, splatMap.GetLength(0), 0);
            
            topologyLayer = oldTopologyLayer2;
            changeLandLayer();
        }
    }
    public void autoGenerateGround() // Assigns terrain splats to these values. 
    {
        changeLayer("Ground");

        terrainLayer = TerrainSplat.Enum.Sand;
        paintHeight("Ground", 0, 502, float.MaxValue, 0);

        terrainLayer = TerrainSplat.Enum.Snow;
        paintHeight("Ground", 750, 1000, float.MaxValue, 0);

        terrainLayer = TerrainSplat.Enum.Rock;
        paintSlope("Ground", 0.9945f, 1f, 0);
    } 
    public void autoGenerateBiome() // Assigns biome splats to these values.
    {
        changeLayer("Biome");

        biomeLayer = TerrainBiome.Enum.Arctic;
        paintHeight("Biome", 750, 1000, float.MaxValue, 0);
    }
    public void alphaDebug(string landLayer) // Paints a ground texture to the corresponding coordinate if the alpha is active.
    // Used for debugging the floating ground clutter that occurs when you have a ground splat of either Grass or Forest ontop of an active alpha layer. Replaces with rock texture.
    {

        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        float[,,] alphaSplatMap = TypeConverter.singleToMulti(alphaLandData.splatMap, 2); // Always needs to be at two layers or it will break, as we can't divide landData by 0.


        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (alphaSplatMap[i, j, 1] == 1)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 2;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 7] = 0;

                    splatMap[i, j, 3] = 1; // This paints the rock layer. Where 3 = the layer to paint.
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
    }
    public void textureCopy(string landLayerFrom, string landLayerToPaint, int textureFrom, int textureToPaint) // This copies the selected texture on a landlayer 
    // and paints the same coordinate on another landlayer with the selected texture.
    {
        switch (landLayerFrom) // Gathers the information on which texture we are copying from in the landlayer.
        {
            default:
                Debug.Log("landLayerFrom not found!");
                break;
            case "Ground":
                changeLayer("Ground");
                textureFrom = TerrainSplat.TypeToIndex((int)groundLayerFrom); // Layer texture to copy from Ground Textures.
                break;
            case "Biome":
                changeLayer("Biome");
                textureFrom = TerrainBiome.TypeToIndex((int)biomeLayerFrom); // Layer texture to copy from Biome Textures.
                break;
            case "Topology":
                changeLayer("Topology");
                textureFrom = 0;
                topologyLayer = topologyLayerFrom;
                break;
        }
        LandData landDataFrom = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayerFrom).GetComponent<LandData>();
        float[,,] splatMapFrom = TypeConverter.singleToMulti(landDataFrom.splatMap, textureCount(landLayerFrom)); // Land layer to copy from.
        switch (landLayerToPaint) // Gathers the information on which texture we are painting to in the landlayer.
        {
            default:
                Debug.Log("landLayerToPaint not found!");
                break;
            case "Ground":
                changeLayer("Ground");
                textureToPaint = TerrainSplat.TypeToIndex((int)groundLayerToPaint); // Layer texture to copy from Ground Textures.
                break;
            case "Biome":
                changeLayer("Biome");
                textureToPaint = TerrainBiome.TypeToIndex((int)biomeLayerToPaint); // Layer texture to copy from Biome Textures.
                break;
            case "Topology":
                changeLayer("Topology");
                textureToPaint = 0;
                oldTopologyLayer2 = topologyLayer;
                topologyLayer = topologyLayerToPaint;
                changeLandLayer();
                oldTopologyLayer = topologyLayerToPaint;
                break;
        }
        LandData landDataToPaint = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayerToPaint).GetComponent<LandData>();
        float[,,] splatMapTo = TypeConverter.singleToMulti(landDataToPaint.splatMap, textureCount(landLayerToPaint)); //  Land layer to paint to.

        for (int i = 0; i < splatMapFrom.GetLength(0); i++)
        {
            for (int j = 0; j < splatMapFrom.GetLength(1); j++)
            {
                if (splatMapFrom [i, j, textureFrom] > 0)
                {
                    splatMapTo[i, j, 0] = 0;
                    splatMapTo[i, j, 1] = 0;
                    if (textureCount(landLayerToPaint) > 2)
                    {
                        splatMapTo[i, j, 2] = 0;
                        splatMapTo[i, j, 3] = 0;
                        if (textureCount(landLayerToPaint) > 4)
                        {
                            splatMapTo[i, j, 4] = 0;
                            splatMapTo[i, j, 5] = 0;
                            splatMapTo[i, j, 6] = 0;
                            splatMapTo[i, j, 7] = 0;
                        }
                    }
                    splatMapTo[i, j, textureToPaint] = 1;
                }
            }
        }
        landDataToPaint.setData(splatMapTo, landLayerToPaint);
        landDataToPaint.setLayer();
        if (landLayerToPaint == "Topology")
        {
            topologyLayer = oldTopologyLayer2;
            saveTopologyLayer();
        }
    }
    public void generateTwoLayersNoise(string landLayer, float scale, int t) //Generates a layer of perlin noise across two layers, the smaller the scale the smaller the blobs 
    // it generates will be. Wipes the current layer.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, 2);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp01(Mathf.PerlinNoise(i2, j2));
                if (perlin <= 0.5f)
                {
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 0] = 1;
                }
                else
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 1;
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        if (landLayer == "Topology")
        {
            saveTopologyLayer();
        }
    }
    #endregion



    private void loadMapInfo(MapInfo terrains)
    {
        if (MapIO.topology == null)
            topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();
        
        cleanUpMap();

        var terrainPosition = 0.5f * terrains.size;

        LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();

        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();

        land.transform.position = terrainPosition;
        water.transform.position = terrainPosition;


        topology.InitMesh(terrains.topology);
       

        land.terrainData.heightmapResolution = terrains.resolution;
        land.terrainData.size = terrains.size;

        water.terrainData.heightmapResolution = terrains.resolution;
        water.terrainData.size = terrains.size;

        land.terrainData.SetHeights(0, 0, terrains.land.heights);
        water.terrainData.SetHeights(0, 0, terrains.water.heights);

        land.terrainData.alphamapResolution = terrains.resolution;
        land.terrainData.baseMapResolution = terrains.resolution - 1;
        //land.terrainData.SetDetailResolution(terrains.resolution - 1, 8);
        water.terrainData.alphamapResolution = terrains.resolution;
        water.terrainData.baseMapResolution = terrains.resolution - 1;
        //water.terrainData.SetDetailResolution(terrains.resolution - 1, 8);

        land.GetComponent<UpdateTerrainValues>().setSize(terrains.size);
        water.GetComponent<UpdateTerrainValues>().setSize(terrains.size);
        land.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);
        water.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);

        groundLandData.setData(terrains.splatMap, "ground");

        biomeLandData.setData(terrains.biomeMap, "biome");

        alphaLandData.setData(terrains.alphaMap, "alpha");

        topologyLandData.setData(topology.getSplatMap((int)topologyLayer), "topology");
        changeLandLayer();

        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");

        Dictionary<uint, GameObject> savedPrefabs = getPrefabs();

        for (int i = 0; i < terrains.prefabData.Length; i++)
        {
            GameObject spawnObj;
            if (savedPrefabs.ContainsKey(terrains.prefabData[i].id))
            {
                savedPrefabs.TryGetValue(terrains.prefabData[i].id, out spawnObj);
            }
            else
            {
                spawnObj = defaultObj;
            }

            GameObject newObj = spawnPrefab(spawnObj, terrains.prefabData[i], prefabsParent);
            newObj.GetComponent<PrefabDataHolder>().prefabData = terrains.prefabData[i];
        }


        Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        GameObject pathNodeObj = Resources.Load<GameObject>("Paths/PathNode");
        
        for (int i = 0; i < terrains.pathData.Length; i++)
        {

            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
            }
            averageLocation /= terrains.pathData[i].nodes.Length;
            GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);

            List<GameObject> pathNodes = new List<GameObject>();
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                //GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);
                GameObject newNode = Instantiate(pathNodeObj, newObject.transform);
                newNode.transform.position = terrains.pathData[i].nodes[j] + terrainPosition;
                pathNodes.Add(newNode);
            }
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
        }
    }

    public void Load(WorldSerialization blob)
    {
        Debug.Log("Map hash: " + blob.Checksum);
        WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
        loadMapInfo(terrains);
    }


    public void loadEmpty(int size)
    {
        loadMapInfo(WorldConverter.emptyWorld(size));
    }

    public void Save(string path)
    {
        if(selectedLandLayer != null)
            selectedLandLayer.save();
        saveTopologyLayer();

        if (GameObject.FindGameObjectWithTag("Water") == null)
            Debug.Log("Water not enabled");
        if (GameObject.FindGameObjectWithTag("Land") == null)
            Debug.Log("Land not enabled");
        Terrain terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        

        WorldSerialization world = WorldConverter.terrainToWorld(terrain, water);
        
        world.Save(path);
        //Debug.Log("Map hash: " + world.Checksum);
    }

    public void newEmptyTerrain(int size)
    {
        loadMapInfo(WorldConverter.emptyWorld(size));
    }


    public string bundleFile = "No bundle file selected";
    public void Start()
    {
        if (bundleFile.Equals("No bundle file selected"))
        {
            Debug.LogError("No bundle file selected");
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }
        Debug.Log("started");
        if (getPrefabLookUp() != null)
        {
            getPrefabLookUp().Dispose();
            setPrefabLookup(null);
        }
        setPrefabLookup(new PrefabLookup(bundleFile));
    }

    private void Update()
    {
        if(prefabLookup == null)
        {
            Debug.LogError("No bundle file selected");
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }

        //Debug.LogWarning("Prefabs are not saved in play mode. Export the map before stopping play mode.");

        if (prefabLookup.isLoaded)
        {
            if(GameObject.FindObjectsOfType<PrefabDataHolder>().Length > 0) { 

                

                Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
                foreach (PrefabDataHolder pdh in GameObject.FindObjectsOfType<PrefabDataHolder>())
                {
                    if (pdh.gameObject.tag == "LoadedPrefab")
                        continue;

                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        if (DragAndDrop.objectReferences[0].name.Equals(pdh.gameObject.name))
                        {
                            continue;
                        }
                    }

                    PrefabData prefabData = pdh.prefabData;
                    string name = null;
                    if (!pdh.gameObject.name.StartsWith("DefaultPrefab"))
                        name = pdh.gameObject.name;
                    GameObject go = SpawnPrefab(prefabData, prefabsParent, name);
                    go.tag = "LoadedPrefab";
                    go.AddComponent<PrefabDataHolder>().prefabData = prefabData;
                    
                    Destroy(pdh.gameObject);

                    setChildrenUnmoveable(go);
                }
            }
        }
    }

    private void setChildrenUnmoveable(GameObject root)
    {
        for(int i = 0; i < root.transform.childCount; i++)
        {
            Transform child = root.transform.GetChild(i);
            child.gameObject.AddComponent<UnmoveablePrefab>();
            if (child.childCount > 0)
                setChildrenUnmoveable(child.gameObject);
        }
    }

    private GameObject SpawnPrefab(PrefabData prefabData, Transform parent, string name = null)
    {
        var offset = getMapOffset();
        var go = GameObject.Instantiate(prefabLookup[prefabData.id], prefabData.position + offset, prefabData.rotation, parent);
        if (go)
        {
            if (name != null)
                go.name = name;
            go.transform.localScale = prefabData.scale;
            go.SetActive(true);
        }

        return go;
    }

    void OnApplicationQuit()
    {
        /*
        var offset = getMapOffset();
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        Dictionary<uint, GameObject> savedPrefabs = getPrefabs();

        Debug.Log(GameObject.FindGameObjectsWithTag("LoadedPrefab").Length);

        foreach (GameObject pdh in GameObject.FindGameObjectsWithTag("LoadedPrefab"))
        {
            PrefabData prefabData = pdh.GetComponent<PrefabDataHolder>().prefabData;
            GameObject spawnObj;

            if (savedPrefabs.ContainsKey(prefabData.id))
            {
                savedPrefabs.TryGetValue(prefabData.id, out spawnObj);
            }
            else
            {
                spawnObj = defaultObj;
            }

            GameObject go = GameObject.Instantiate(spawnObj, prefabData.position + offset, prefabData.rotation, prefabsParent);
            PrefabUtility.InstantiatePrefab(go);
            go.tag = "NotLoadedPrefab";
            go.AddComponent<PrefabDataHolder>().prefabData = prefabData;
            Destroy(pdh);
            
        }
        */
        getPrefabLookUp().Dispose();
        setPrefabLookup(null);
        /*
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        foreach (PrefabDataHolder pdh in GameObject.FindObjectsOfType<PrefabDataHolder>())
        {
            GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
            GameObject newObject = spawnPrefab(defaultObj, pdh.prefabData, prefabsParent);
            

            PrefabDataHolder prefabData = newObject.GetComponent<PrefabDataHolder>();
            if (prefabData == null)
            {
                newObject.AddComponent<PrefabDataHolder>();
            }
            prefabData.prefabData = pdh.prefabData;

            Destroy(pdh.gameObject);
        }
        */
    }

    public void SpawnPrefabs()
    {
        Terrain terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();



        WorldSerialization world = WorldConverter.terrainToWorld(terrain, water);
        Debug.Log("1");
        SpawnPrefabs(world, prefabLookup);
    }

    private void SpawnPrefabs(WorldSerialization blob, PrefabLookup prefabs)
    {
        Debug.Log("2");
        Debug.Log(blob.world.prefabs.Count);
        var offset = getMapOffset();
        foreach (var prefab in blob.world.prefabs)
        {
            var go = GameObject.Instantiate(prefabs[prefab.id], prefab.position+offset, prefab.rotation);
            if (go)
            {
                go.transform.localScale = prefab.scale;
                go.SetActive(true);
            }
        }
        Debug.Log("3");
    }

    public Dictionary<uint, GameObject> getPrefabs()
    {
        Dictionary<uint, GameObject> prefabs = new Dictionary<uint, GameObject>();
        var prefabFiles = getPrefabFiles("Assets/Resources/Prefabs");
        foreach(string s in prefabFiles)
        {
            GameObject prefabObject = Resources.Load<GameObject>(s);
            uint key = prefabObject.GetComponent<PrefabDataHolder>().prefabData.id;
            if (prefabs.ContainsKey(key))
            {
                GameObject existingObj;
                prefabs.TryGetValue(key, out existingObj);
                Debug.LogError(prefabObject.name + " Prefab ID conflicts with " + existingObj.name + ". Loading " + prefabObject.name + " as ID " + key + " instead of " + existingObj.name);
            }
            prefabs.Add(key, prefabObject);
        }
        return prefabs;
    }

    private List<string> getPrefabFiles(string dir)
    {
        List<string> prefabFiles = new List<string>();

        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(dir);
        foreach (string fileName in fileEntries)
        {
            if(fileName.EndsWith(".prefab"))
                prefabFiles.Add(fileName.Substring(17, fileName.Length - 7 - 17)); //17 to remove the "Assets/Resouces/" part, 7 to remove the ".prefab" part
        }

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(dir);
        foreach (string subdirectory in subdirectoryEntries)
            prefabFiles.AddRange(getPrefabFiles(subdirectory));
        
        return prefabFiles;
    }

}
