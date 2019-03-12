using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Threading;
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
    //public Thread th1;

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

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alt Topology ", 0.4f);
        topologyLayer = TerrainTopology.Enum.Alt;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Alt;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Beach Topology ", 0.425f);
        topologyLayer = TerrainTopology.Enum.Beach;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Beach;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Beachside Topology ", 0.45f);
        topologyLayer = TerrainTopology.Enum.Beachside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Beachside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Building Topology ", 0.475f);
        topologyLayer = TerrainTopology.Enum.Building;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Building;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Cliff Topology ", 0.5f);
        topologyLayer = TerrainTopology.Enum.Cliff;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Cliff;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Cliffside Topology ", 0.525f);
        topologyLayer = TerrainTopology.Enum.Cliffside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Cliffside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Clutter Topology ", 0.55f);
        topologyLayer = TerrainTopology.Enum.Clutter;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Clutter;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Decor Topology ", 0.575f);
        topologyLayer = TerrainTopology.Enum.Decor;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Decor;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Field Topology ", 0.6f);
        topologyLayer = TerrainTopology.Enum.Field;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Field;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Forest Topology ", 0.625f);
        topologyLayer = TerrainTopology.Enum.Forest;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Forest;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Forestside Topology ", 0.65f);
        topologyLayer = TerrainTopology.Enum.Forestside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Forestside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Hilltop Topology ", 0.675f);
        topologyLayer = TerrainTopology.Enum.Hilltop;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Hilltop;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Lake Topology ", 0.7f);
        topologyLayer = TerrainTopology.Enum.Lake;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Lake;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Lakeside Topology ", 0.725f);
        topologyLayer = TerrainTopology.Enum.Lakeside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Lakeside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Mainland Topology ", 0.75f);
        topologyLayer = TerrainTopology.Enum.Mainland;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Mainland;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Monument Topology ", 0.775f);
        topologyLayer = TerrainTopology.Enum.Monument;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Monument;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Mountain Topology ", 0.8f);
        topologyLayer = TerrainTopology.Enum.Mountain;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Mountain;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ocean Topology ", 0.825f);
        topologyLayer = TerrainTopology.Enum.Ocean;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Ocean;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Oceanside Topology ", 0.85f);
        topologyLayer = TerrainTopology.Enum.Oceanside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Oceanside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Offshore Topology ", 0.875f);
        topologyLayer = TerrainTopology.Enum.Offshore;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Offshore;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Powerline Topology ", 0.9f);
        topologyLayer = TerrainTopology.Enum.Powerline;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Powerline;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating River Topology ", 0.91f);
        topologyLayer = TerrainTopology.Enum.River;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.River;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating River Topology ", 0.922f);
        topologyLayer = TerrainTopology.Enum.Riverside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Riverside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Road Topology ", 0.93f);
        topologyLayer = TerrainTopology.Enum.Road;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Road;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Roadside Topology ", 0.94f);
        topologyLayer = TerrainTopology.Enum.Roadside;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Roadside;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Runway Topology ", 0.95f);
        topologyLayer = TerrainTopology.Enum.Runway;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Runway;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Summit Topology ", 0.96f);
        topologyLayer = TerrainTopology.Enum.Summit;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Summit;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Swamp Topology ", 0.97f);
        topologyLayer = TerrainTopology.Enum.Swamp;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Swamp;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Tier0 Topology ", 0.98f);
        topologyLayer = TerrainTopology.Enum.Tier0;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Tier0;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Tier1 Topology ", 0.99f);
        topologyLayer = TerrainTopology.Enum.Tier1;
        changeLandLayer();
        oldTopologyLayer = TerrainTopology.Enum.Tier1;
        rotateTopologymap(CW);

        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Tier2 Topology ", 0.995f);
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
    public void moveHeightmap()
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
    public void setEdgePixel(float heightToSet, bool top, bool left, bool right, bool bottom) // Sets the very edge pixel of the heightmap to the heightToSet value. Includes toggle
    // option for sides.
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        for (int i = 0; i < land.terrainData.heightmapHeight; i++)
        {
            for (int j = 0; j < land.terrainData.heightmapWidth; j++)
            {
                if (i == 0 && bottom == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
                if (i == land.terrainData.heightmapHeight - 1 && top == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
                if (j == 0 && left == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
                if (j == land.terrainData.heightmapWidth - 1 && right == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
            }
        }
        land.terrainData.SetHeights(0, 0, heightMap);
    }
    public void generatePerlinHeightmap(float scale) // Extremely basic first run of perlin map gen. In future this will have roughly 15 controllable elements.
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);

        for (int i = 0; i < land.terrainData.heightmapHeight; i++)
        {
            for (int j = 0; j < land.terrainData.heightmapWidth; j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp01(Mathf.PerlinNoise(i2, j2));
                heightMap[i, j] = perlin;
            }
        }
        land.terrainData.SetHeights(0, 0, heightMap);
    }
    public void offsetHeightmap(float offset, bool checkHeight, bool setWaterMap) // Increases or decreases the heightmap by the offset. Useful for moving maps up or down in the scene if the heightmap
    // isn't at the right height. If checkHeight is enabled it will make sure that the offset does not flatten a part of the map because it hits the floor or ceiling.
    // If setWaterMap is enabled it will offset the water map as well, however if this goes below 500 the watermap will be broken.
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        offset = offset / 1000f;
        bool heightOutOfRange = false;
        for (int i = 0; i < land.terrainData.heightmapHeight; i++)
        {
            for (int j = 0; j < land.terrainData.heightmapWidth; j++)
            {
                if (checkHeight == true)
                {
                    if ((heightMap[i, j] + offset > 1f || heightMap[i, j] + offset < 0f) || (waterMap[i, j] + offset > 1f || waterMap[i, j] + offset < 0f))
                    {
                        heightOutOfRange = true;
                        break;
                    }
                    else
                    {
                        heightMap[i, j] += offset;
                        if (setWaterMap == true)
                        {
                            waterMap[i, j] += offset;
                        }
                    }
                }
                else
                {
                    heightMap[i, j] += offset;
                    if (setWaterMap == true)
                    {
                        waterMap[i, j] += offset;
                    }
                }
            }
        }
        if (heightOutOfRange == false)
        {
            land.terrainData.SetHeights(0, 0, heightMap);
            water.terrainData.SetHeights(0, 0, waterMap);
        }
        else if (heightOutOfRange == true)
        {
            Debug.Log("Heightmap offset exceeds heightmap limits, try a smaller value." );
        }
    }
    public void debugWaterLevel() // Puts the water level up to 500 if it's below 500 in height.
    {
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        for (int i = 0; i < waterMap.GetLength(0); i++)
        {
            for (int j = 0; j < waterMap.GetLength(1); j++)
            {
                if (waterMap[i, j] < 0.5f)
                {
                    waterMap[i, j] = 0.5f;
                }
            }
        }
        water.terrainData.SetHeights(0, 0, waterMap);
    }
    public void setMinimumHeight(float minimumHeight) // Puts the heightmap level to the minimum if it's below.
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] landMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        minimumHeight /= 1000f; // Normalise the input to a value between 0 and 1.
        for (int i = 0; i < landMap.GetLength(0); i++)
        {
            for (int j = 0; j < landMap.GetLength(1); j++)
            {
                if (landMap[i, j] < minimumHeight)
                {
                    landMap[i, j] = minimumHeight;
                }
            }
        }
        land.terrainData.SetHeights(0, 0, landMap);
    }
    public void setMaximumHeight(float maximumHeight) // Puts the heightmap level to the minimum if it's below.
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] landMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        maximumHeight /= 1000f; // Normalise the input to a value between 0 and 1.
        for (int i = 0; i < landMap.GetLength(0); i++)
        {
            for (int j = 0; j < landMap.GetLength(1); j++)
            {
                if (landMap[i, j] > maximumHeight)
                {
                    landMap[i, j] = maximumHeight;
                }
            }
        }
        land.terrainData.SetHeights(0, 0, landMap);
    }
    #endregion

    #region SplatMap Methods
    // Todo: I will rewrite the autoGenerate methods to use loops instead of a shittonne of code, however that will require rewriting the current TerrainTopology class, which will break
    // other scripts, and prevent easily merging in updates from Dez's own editor repo. I am well aware it's not the best way to iterate through the layers :)
    // It will be fixed shortly.
    public int texture(string landLayer) // Active texture selected in layer. Call method with a string type of the layer to search. 
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
    public void paintHeight(string landLayer, float heightLow, float heightHigh, float minBlendLow, float maxBlendHigh, int t ) // Paints height between 2 floats. Blending is attributed to the 2 blend floats.
    // The closer the height is to the heightLow and heightHigh the stronger the weight of the texture is. To paint without blending assign the blend floats to the same value as the height floats.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[] splatMapLayers = new float[land.terrainData.alphamapLayers];
        switch (landLayer)
        {
            case "Ground":
                t = texture(landLayer);
                break;
            case "Biome":
                t = texture(landLayer);
                break;
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < (float)splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float height = land.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                if (height > heightLow && height < heightHigh)
                {
                    for (int k = 0; k < textureCount(landLayer); k++) // Erases the textures on all the layers.
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1; // Paints the texture t.
                }
                else if (height > minBlendLow && height < heightLow)
                {
                    for (int k = 0; k < textureCount(landLayer); k++) // Gets the weights of the textures in the pos. 
                    {
                        splatMapLayers[k] = splatMap[i, j, k];
                    }
                    float newHeight = height - minBlendLow;
                    float newHeightLow = heightLow - minBlendLow;
                    float heightBlend = newHeight / newHeightLow; // Holds data about the texture weight between the blend ranges.
                    splatMapLayers[t] = heightBlend;
                    float textureWeight = splatMapLayers.Sum(); // Calculates the sum of all the textures.
                    for (int l = 0; l < land.terrainData.alphamapLayers; l++)
                    {
                        splatMapLayers[l] /= textureWeight;
                        splatMap[i, j, l] = splatMapLayers[l];
                    }
                }
                else if (height > heightHigh && height < maxBlendHigh)
                {
                    for (int k = 0; k < textureCount(landLayer); k++) // Gets the weights of the textures in the pos. 
                    {
                        splatMapLayers[k] = splatMap[i, j, k];
                    }
                    float newHeight = height - heightHigh;
                    float newMaxBlendHigh = maxBlendHigh - heightHigh;
                    float heightBlendInverted = newHeight / newMaxBlendHigh; // Holds data about the texture weight between the blend ranges.
                    float heightBlend = 1 - heightBlendInverted; // We flip this because we want to find out how close the slope is to the max blend.
                    splatMapLayers[t] = heightBlend;
                    float textureWeight = splatMapLayers.Sum(); // Calculates the sum of all the textures.
                    for (int l = 0; l < land.terrainData.alphamapLayers; l++)
                    {
                        splatMapLayers[l] /= textureWeight;
                        splatMap[i, j, l] = splatMapLayers[l];
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
            t = texture(landLayer); // Active texture to paint on layer.
        }
        else if (landLayer == "Biome")
        {
            t = texture(landLayer); // Active texture to paint on layer.
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (landLayer == "Alpha")
                {
                    splatMap[i, j, 1] = 1;
                    splatMap[i, j, 0] = 0;
                }
                else if (landLayer == "Topology")
                {
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 0] = 1;
                }
                else
                {
                    for (int k = 0; k < textureCount(landLayer); k++)
                    {
                        splatMap[i, j, k] = 0;
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
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, texture(landLayer));
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (landLayer == "Alpha")
                {
                    splatMap[i, j, 0] = 1;
                    splatMap[i, j, 1] = 0;
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
    public void invertLayer(string landLayer) // Inverts the active and inactive textures. Alpha and Topology only. 
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (splatMap[i, j, 0] < 0.5f)
                {
                    splatMap[i, j, 0] = 1;
                    splatMap[i, j, 1] = 0;
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
    public void paintSlope(string landLayer, float slopeLow, float slopeHigh, float minBlendLow, float maxBlendHigh, int t) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[] splatMapLayers = new float[land.terrainData.alphamapLayers];
        switch (landLayer)
        {
            case "Ground":
                t = texture(landLayer);
                break;
            case "Biome":
                t = texture(landLayer);
                break;
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float slope = land.terrainData.GetSteepness(jNorm, iNorm); // Normalises the steepness coords to match the splatmap array size.
                if (slope > slopeLow && slope < slopeHigh)
                {
                    for (int k = 0; k < textureCount(landLayer); k++) 
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1;
                }
                else if (slope > minBlendLow && slope < slopeLow)
                {
                    for (int k = 0; k < textureCount(landLayer); k++) // Gets the weights of the textures in the pos. 
                    {
                        splatMapLayers[k] = splatMap[i, j, k];
                    }
                    float newSlope = slope - minBlendLow;
                    float newSlopeLow = slopeLow - minBlendLow;
                    float slopeBlend = newSlope / newSlopeLow; // Holds data about the texture weight between the blend ranges.
                    splatMapLayers[t] = slopeBlend; // Assigns the texture we are painting to equal a value between 0 - 1, depending on how far away it is from the solid texture.
                    float textureWeight = splatMapLayers.Sum(); // Calculates the sum of all the textures.
                    for (int l = 0; l < land.terrainData.alphamapLayers; l++)
                    {
                        splatMapLayers[l] /= textureWeight; // Averages out all the texture weights. If you want a stronger blend adjust this value.
                        splatMap[i, j, l] = splatMapLayers[l];
                    }         
                }
                else if (slope > slopeHigh && slope < maxBlendHigh)
                {
                    for (int k = 0; k < textureCount(landLayer); k++) // Gets the weights of the textures in the pos. 
                    {
                        splatMapLayers[k] = splatMap[i, j, k];
                    }
                    float newSlope = slope - slopeHigh;
                    float newMaxBlendHigh = maxBlendHigh - slopeHigh; 
                    float slopeBlendInverted = newSlope / newMaxBlendHigh; // Holds data about the texture weight between the blend ranges.
                    float slopeBlend = 1 - slopeBlendInverted; // We flip this because we want to find out how close the slope is to the max blend.
                    splatMapLayers[t] = slopeBlend;
                    float textureWeight = splatMapLayers.Sum(); // Calculates the sum of all the textures.
                    for (int l = 0; l < land.terrainData.alphamapLayers; l++)
                    {
                        splatMapLayers[l] /= textureWeight;
                        splatMap[i, j, l] = splatMapLayers[l];
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
        switch (landLayer)
        {
            case "Ground":
                t = texture(landLayer);
                break;
            case "Biome":
                t = texture(landLayer);
                break;
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (i >= z1 && i <= z2)
                {
                    if (j >= x1 && j <= x2)
                    {
                        for (int k = 0; k < textureCount(landLayer); k++)
                        {
                            splatMap[i, j, k] = 0;
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
    public void paintRiver(string landLayer, bool aboveTerrain, int t) // Paints the splats wherever the water is above 500 and is above the terrain. Above terrain
    // true will paint only if water is above 500 and is also above the land terrain.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        switch (landLayer)
        {
            case "Ground":
                t = texture(landLayer);
                break;
            case "Biome":
                t = texture(landLayer);
                break;
        }
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float waterHeight = water.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                float landHeight = land.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                switch (aboveTerrain)
                {
                    case true:
                        if (waterHeight > 500 && waterHeight > landHeight)
                        {
                            for (int k = 0; k < textureCount(landLayer); k++)
                            {
                                splatMap[i, j, k] = 0;
                            }
                            splatMap[i, j, t] = 1;
                        }
                        break;
                    case false:
                        if (waterHeight > 500)
                        {
                            for (int k = 0; k < textureCount(landLayer); k++)
                            {
                                splatMap[i, j, k] = 0;
                            }
                            splatMap[i, j, t] = 1;
                        }
                        break;
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
            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Offshore", 0.1f);
            oldTopologyLayer = TerrainTopology.Enum.Offshore; //If wiping layers we don't need to get the current layers splatmap detail, so we just wipe it clean then repaint.
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintHeight("Topology", 0, 475, 0, 475, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Ocean", 0.2f);
            oldTopologyLayer = TerrainTopology.Enum.Ocean;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintHeight("Topology", 0, 498, 0, 498, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Beach", 0.3f);
            oldTopologyLayer = TerrainTopology.Enum.Beach;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintHeight("Topology", 500, 502, 500, 502, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Oceanside", 0.4f);
            oldTopologyLayer = TerrainTopology.Enum.Oceanside;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintHeight("Topology", 500, 502, 500, 502, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Mainland", 0.5f);
            oldTopologyLayer = TerrainTopology.Enum.Mainland;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintHeight("Topology", 500, 1000, 500, 1000, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Cliff", 0.6f);
            oldTopologyLayer = TerrainTopology.Enum.Cliff;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintSlope("Topology", 45f, 90f, 45f, 90f, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Tier 0", 0.7f);
            oldTopologyLayer = TerrainTopology.Enum.Tier0;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintArea("Topology", 0, splatMap.GetLength(0) / 3 , 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Tier 1", 0.8f);
            oldTopologyLayer = TerrainTopology.Enum.Tier1;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintArea("Topology", splatMap.GetLength(0) / 3, splatMap.GetLength(0) / 3 * 2, 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Tier 2", 0.9f);
            oldTopologyLayer = TerrainTopology.Enum.Tier2;
            paintHeight("Topology", 0, 1000, 0, 1000, 1);
            paintArea("Topology", splatMap.GetLength(0) / 3 * 2, splatMap.GetLength(0), 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            EditorUtility.ClearProgressBar();
            changeLandLayer();
        }
        else if (wipeLayer == false) // Paints active texture on to layer whilst keeping the current layer's textures.
        {
            oldTopologyLayer2 = topologyLayer; //This saves the currently selected topology layer so we can swap back to it at the end, ensuring we don't accidentally erase anything.

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Offshore", 0.1f);
            topologyLayer = TerrainTopology.Enum.Offshore; // This sets the new current topology layer to offshore.
            changeLandLayer(); // This changes the topology layer to offshore. It also saves the previous layer for us.
            oldTopologyLayer = TerrainTopology.Enum.Offshore; // This is the layer the paint the offshore height to.
            paintHeight("Topology", 0, 475, 0, 475, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Ocean", 0.2f);
            topologyLayer = TerrainTopology.Enum.Ocean;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Ocean;
            paintHeight("Topology", 0, 498, 0, 498, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Beach", 0.3f);
            topologyLayer = TerrainTopology.Enum.Beach;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Beach;
            paintHeight("Topology", 500, 502, 500, 502, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Oceanside", 0.4f);
            topologyLayer = TerrainTopology.Enum.Oceanside;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Oceanside;
            paintHeight("Topology", 500, 502, 500, 502, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Mainland", 0.5f);
            topologyLayer = TerrainTopology.Enum.Mainland;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Mainland;
            paintHeight("Topology", 500, 1000, 500, 1000, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Cliff", 0.6f);
            topologyLayer = TerrainTopology.Enum.Cliff;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Cliff;
            paintSlope("Topology", 45f, 90f, 45, 90f, 0);

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Tier 0", 0.7f);
            topologyLayer = TerrainTopology.Enum.Tier0;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier0;
            paintArea("Topology", 0, splatMap.GetLength(0) / 3, 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Tier 1", 0.8f);
            topologyLayer = TerrainTopology.Enum.Tier1;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier1;
            paintArea("Topology", splatMap.GetLength(0) / 3, splatMap.GetLength(0) / 3 * 2, 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            EditorUtility.DisplayProgressBar("Generating Topologies", "Generating Tier 2", 0.9f);
            topologyLayer = TerrainTopology.Enum.Tier2;
            changeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier2;
            paintArea("Topology", splatMap.GetLength(0) / 3 * 2, splatMap.GetLength(0), 0, splatMap.GetLength(0), 0); // Gets thirds of Terrain

            EditorUtility.ClearProgressBar();
            topologyLayer = oldTopologyLayer2;
            changeLandLayer();
        }
    }
    public void autoGenerateGround() // Assigns terrain splats to these values. 
    {
        changeLayer("Ground");

        terrainLayer = TerrainSplat.Enum.Sand;
        paintHeight("Ground", 0, 502, 0, 505, 0);

        terrainLayer = TerrainSplat.Enum.Grass;
        paintHeight("Ground", 503, 675, 502, 700, 0);

        terrainLayer = TerrainSplat.Enum.Dirt;
        paintSlope("Ground", 20, 20, 10, 30, 0);

        terrainLayer = TerrainSplat.Enum.Snow;
        paintHeight("Ground", 700, 1000, 650, 1000, 0);

        terrainLayer = TerrainSplat.Enum.Rock;
        paintSlope("Ground", 50f, 90f, 40f, 90f, 0);
    } 
    public void autoGenerateBiome() // Assigns biome splats to these values.
    {
        changeLayer("Biome");

        biomeLayer = TerrainBiome.Enum.Arctic;
        paintHeight("Biome", 750, 1000, 750, 1000, 0);

        biomeLayer = TerrainBiome.Enum.Tundra;
        paintHeight("Biome", 675, 750, 675, 750, 0);
    }
    public void alphaDebug(string landLayer) // Paints a ground texture to the corresponding coordinate if the alpha is active.
    // Used for debugging the floating ground clutter that occurs when you have a ground splat of either Grass or Forest ontop of an active alpha layer. Replaces with rock texture.
    {
        EditorUtility.DisplayProgressBar("Debug Alpha", "Debugging", 0.3f);
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, textureCount(landLayer));
        float[,,] alphaSplatMap = TypeConverter.singleToMulti(alphaLandData.splatMap, 2); // Always needs to be at two layers or it will break, as we can't divide landData by 0.
        EditorUtility.DisplayProgressBar("Debug Alpha", "Debugging", 0.5f);

        for (int i = 0; i < alphaSplatMap.GetLength(0); i++)
        {
            for (int j = 0; j < alphaSplatMap.GetLength(1); j++)
            {
                if (alphaSplatMap[i, j, 1] == 1)
                {
                    for (int k = 0; k < textureCount(landLayer); k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, 3] = 1; // This paints the rock layer. Where 3 = the layer to paint.
                }
            }
        }
        EditorUtility.DisplayProgressBar("Debug Alpha", "Debugging", 0.7f);
        landData.setData(splatMap, landLayer);
        landData.setLayer();
        EditorUtility.DisplayProgressBar("Debug Alpha", "Done", 1f);
        EditorUtility.ClearProgressBar();
    }
    public void textureCopy(string landLayerFrom, string landLayerToPaint, int textureFrom, int textureToPaint) // This copies the selected texture on a landlayer 
    // and paints the same coordinate on another landlayer with the selected texture.
    {
        EditorUtility.DisplayProgressBar("Copy Textures", "Copying: " + landLayerFrom, 0.2f);
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
        EditorUtility.DisplayProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.5f);
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
        EditorUtility.DisplayProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.75f);
        for (int i = 0; i < splatMapFrom.GetLength(0); i++)
        {
            for (int j = 0; j < splatMapFrom.GetLength(1); j++)
            {
                if (splatMapFrom [i, j, textureFrom] > 0)
                {
                    for (int k = 0; k < textureCount(landLayerToPaint); k++)
                    {
                        splatMapTo[i, j, k] = 0;
                    }
                    splatMapTo[i, j, textureToPaint] = 1;
                }
            }
        }
        EditorUtility.DisplayProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.9f);
        landDataToPaint.setData(splatMapTo, landLayerToPaint);
        landDataToPaint.setLayer();
        if (landLayerToPaint == "Topology")
        {
            topologyLayer = oldTopologyLayer2;
            saveTopologyLayer();
        }
        EditorUtility.ClearProgressBar();
    }
    public void generateTwoLayersNoise(string landLayer, float scale) //Generates a layer of perlin noise across two layers, the smaller the scale the smaller the blobs 
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
    public void generateFourLayersNoise(string landLayer, float scale) //Generates a layer of perlin noise across four layers, the smaller the scale the smaller the blobs 
    // it generates will be. Wipes the current layer.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, 4);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp01(Mathf.PerlinNoise(i2, j2));
                if (perlin < 0.25f)
                {
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 0] = 1;
                }
                else if (perlin < 0.5f)
                {
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 1;
                }
                else if (perlin < 0.75f)
                {
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 2] = 1;
                }
                else
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 1;
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
    }
    public void generateEightLayersNoise(string landLayer, float scale) //Generates a layer of perlin noise across eight layers, the smaller the scale the smaller the blobs 
    // it generates will be. Wipes the current layer.
    {
        LandData landData = GameObject.FindGameObjectWithTag("Land").transform.Find(landLayer).GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(landData.splatMap, 8);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp01(Mathf.PerlinNoise(i2, j2));
                if (perlin < 0.125f)
                {
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 0] = 1;
                }
                else if (perlin < 0.25f)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 1] = 1;
                }
                else if (perlin < 0.375f)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 2] = 1;
                }
                else if (perlin < 0.5f)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 3] = 1;
                }
                else if (perlin < 0.675f)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 4] = 1;
                }
                else if (perlin < 0.75f)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 5] = 1;
                }
                else if (perlin < 0.875f)
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 7] = 0;
                    splatMap[i, j, 6] = 1;
                }
                else
                {
                    splatMap[i, j, 0] = 0;
                    splatMap[i, j, 1] = 0;
                    splatMap[i, j, 2] = 0;
                    splatMap[i, j, 3] = 0;
                    splatMap[i, j, 4] = 0;
                    splatMap[i, j, 5] = 0;
                    splatMap[i, j, 6] = 0;
                    splatMap[i, j, 7] = 1;
                }
            }
        }
        landData.setData(splatMap, landLayer);
        landData.setLayer();
    }
    #endregion

    public void removeBrokenPrefabs()
    {
        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
        var prefabsRemovedCount = 0;
        foreach (PrefabDataHolder p in prefabs)
        {
            switch (p.prefabData.id)
            {
                default:
                    // Do nothing
                    break;
                case 3493139359:
                    DestroyImmediate(p.gameObject);
                    prefabsRemovedCount++;
                    break;
                case 1655878423:
                    DestroyImmediate(p.gameObject);
                    prefabsRemovedCount++;
                    break;
                case 350141265:
                    DestroyImmediate(p.gameObject);
                    prefabsRemovedCount++;
                    break;
            }
        }
        Debug.Log("Removed " + prefabsRemovedCount + " broken prefabs.");
    }

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

        EditorUtility.DisplayProgressBar("Loading Map", "Loading Ground Data ", 0.4f);
        groundLandData.setData(terrains.splatMap, "ground");

        EditorUtility.DisplayProgressBar("Loading Map", "Loading Biome Data ", 0.5f);
        biomeLandData.setData(terrains.biomeMap, "biome");

        EditorUtility.DisplayProgressBar("Loading Map", "Loading Alpha Data ", 0.6f);
        alphaLandData.setData(terrains.alphaMap, "alpha");

        EditorUtility.DisplayProgressBar("Loading Map", "Loading Topology Data ", 0.7f);
        topologyLandData.setData(topology.getSplatMap((int)topologyLayer), "topology");
        changeLandLayer();

        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");

        EditorUtility.DisplayProgressBar("Loading Map", "Spawning Prefabs ", 0.8f);

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
        EditorUtility.DisplayProgressBar("Loading Map", "Spawning Paths ", 0.9f);
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
        EditorUtility.DisplayProgressBar("Loading Map", "Finishing ", 1f);
        EditorUtility.ClearProgressBar();
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
        EditorUtility.DisplayProgressBar("Saving Map", "Saving Heightmap ", 0.1f);
        if (GameObject.FindGameObjectWithTag("Water") == null)
            Debug.Log("Water not enabled");
        if (GameObject.FindGameObjectWithTag("Land") == null)
            Debug.Log("Land not enabled");
        Terrain terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        EditorUtility.DisplayProgressBar("Saving Map", "Saving Watermap ", 0.25f);
        EditorUtility.DisplayProgressBar("Saving Map", "Saving Prefabs ", 0.4f);
        WorldSerialization world = WorldConverter.terrainToWorld(terrain, water);
        EditorUtility.DisplayProgressBar("Saving Map", "Saving Layers ", 0.6f);
        world.Save(path);
        EditorUtility.DisplayProgressBar("Saving Map", "Saving to disk ", 0.8f);
        EditorUtility.ClearProgressBar();
        //Debug.Log("Map hash: " + world.Checksum);
    }

    public void newEmptyTerrain(int size)
    {
        loadMapInfo(WorldConverter.emptyWorld(size));
        changeLayer("Alpha");
        clearLayer("Alpha");
        changeLayer("Biome");
        paintLayer("Biome", 0);
        changeLayer("Ground");
        setMinimumHeight(505f);
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
