using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using static WorldConverter;
using static WorldSerialization;

public class PrefabExport
{
    public int PrefabNumber
    {
        get; set;
    }
    public uint PrefabID
    {
        get; set;
    }
    public string PrefabPath
    {
        get; set;
    }
    public string PrefabPosition
    {
        get; set;
    }
    public string PrefabScale
    {
        get; set;
    }
    public string PrefabRotation
    {
        get; set;
    }
}

public struct TopologyLayers
{
    public float[,,] Topologies
    {
        get; set;
    }
}
public struct GroundTextures
{
    public int Texture
    {
        get; set;
    }
}
public struct BiomeTextures
{
    public int Texture
    {
        get; set;
    }
}
public struct Conditions
{
    public TerrainSplat.Enum GroundConditions
    {
        get; set;
    }
    public TerrainBiome.Enum BiomeConditions
    {
        get; set;
    }
    public TerrainTopology.Enum TopologyLayers
    {
        get; set;
    }
    public bool CheckAlpha
    {
        get; set;
    }
    public int AlphaTexture
    {
        get; set;
    }
    public int TopologyTexture
    {
        get; set;
    }
    public bool CheckHeight
    {
        get; set;
    }
    public float HeightLow
    {
        get; set;
    }
    public float HeightHigh
    {
        get; set;
    }
    public bool CheckSlope
    {
        get; set;
    }
    public float SlopeLow
    {
        get; set;
    }
    public float SlopeHigh
    {
        get; set;
    }
    public int[,,,] AreaRange
    {
        get; set;
    }
}
public static class MapIO
{
    #region Layers
    public static TerrainTopology.Enum topologyLayerFrom, topologyLayerToPaint, conditionalTopology, topologyLayersList;
    public static TerrainSplat.Enum groundLayerFrom, groundLayerToPaint, groundLayer, conditionalGround;
    public static TerrainBiome.Enum biomeLayerFrom, biomeLayerToPaint, biomeLayer, conditionalBiome;
    #endregion
    public static string loadPath = "", savePath = "", prefabSavePath = "";
    public static float progressBar = 0f, progressValue = 1f;
    public static Texture terrainFilterTexture;
    public static Vector2 heightmapCentre = new Vector2(0.5f, 0.5f);
    public static Terrain terrain, water;
    public static GameObject defaultPrefab;
    #region Editor Input Manager
    [InitializeOnLoadMethod]
    static void EditorInit()
    {
        FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
        EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

        value += EditorGlobalKeyPress;
        info.SetValue(null, value);
    }
    static void EditorGlobalKeyPress()
    {
        
    }
    #endregion

    [InitializeOnLoadMethod]
    public static void Start()
    {
        terrainFilterTexture = Resources.Load<Texture>("Textures/Brushes/White128");
        defaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        RefreshAssetList(); // Refreshes the node gen presets.
        SetLayers(); // Resets all the layers to default values.
        EditorApplication.update += OnProjectLoad;
    }
    /// <summary>
    /// Executes once when the project finished loading.
    /// </summary>
    static void OnProjectLoad()
    {
        EditorApplication.update -= OnProjectLoad;
        if (EditorApplication.timeSinceStartup < 30.0) // Prevents methods from being called everytime the assembly is recompiled.
        {
            CreateNewMap(1000);
        }
    }
    public static void CentreSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.orthographic = false;
            sceneView.pivot = new Vector3(500f, 600f, 500f);
            sceneView.rotation = Quaternion.Euler(25f, 0f, 0f);
        }
    }
    /// <summary>
    /// Loads and sets the Land and Water terrains.
    /// </summary>
    /// <param name="terrains"></param>
    public static void LoadTerrains(MapInfo terrains)
    {
        terrain.terrainData.heightmapResolution = terrains.resolution;
        terrain.terrainData.size = terrains.size;

        water.terrainData.heightmapResolution = terrains.resolution;
        water.terrainData.size = terrains.size;

        terrain.terrainData.SetHeights(0, 0, terrains.land.heights);
        water.terrainData.SetHeights(0, 0, terrains.water.heights);

        terrain.terrainData.alphamapResolution = terrains.resolution - 1;
        terrain.terrainData.baseMapResolution = terrains.resolution - 1;
        water.terrainData.alphamapResolution = terrains.resolution - 1;
        water.terrainData.baseMapResolution = terrains.resolution - 1;
    }
    public static void SetLayers()
    {
        topologyLayerFrom = TerrainTopology.Enum.Beach;
        topologyLayerToPaint = TerrainTopology.Enum.Beach;
        groundLayerFrom = TerrainSplat.Enum.Grass;
        groundLayerToPaint = TerrainSplat.Enum.Grass;
        biomeLayerFrom = TerrainBiome.Enum.Temperate;
        biomeLayerToPaint = TerrainBiome.Enum.Temperate;
        LandData.topologyLayer = TerrainTopology.Enum.Beach;
        conditionalTopology = TerrainTopology.NOTHING;
        topologyLayersList = TerrainTopology.Enum.Beach;
        biomeLayer = TerrainBiome.Enum.Temperate;
        conditionalBiome = TerrainBiome.NOTHING;
        groundLayer = TerrainSplat.Enum.Grass;
        conditionalGround = TerrainSplat.NOTHING;
    }
    /// <summary>
    /// Displays a popup progress bar, the progress is also visible in the taskbar.
    /// </summary>
    /// <param name="title">The Progress Bar title.</param>
    /// <param name="info">The info to be displayed next to the loading bar.</param>
    /// <param name="progress">The progress amount. Between 0f - 1f.</param>
    public static void ProgressBar(string title, string info, float progress)
    {
        EditorUtility.DisplayProgressBar(title, info, progress);
    }
    /// <summary>
    /// Clears the popup progress bar. Needs to be called otherwise it will persist in the editor.
    /// </summary>
    public static void ClearProgressBar()
    {
        progressBar = 0;
        EditorUtility.ClearProgressBar();
    }
    
    public static GameObject SpawnPrefab(GameObject g, PrefabData prefabData, Transform parent = null)
    {
        GameObject newObj = GameObject.Instantiate(g);
        newObj.transform.parent = parent;
        newObj.transform.position = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z) + GetMapOffset();
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        return newObj;
    }
    /// <summary>
    /// Removes all the map objects from the scene.
    /// </summary>
    /// <param name="prefabs">Delete Prefab objects.</param>
    /// <param name="paths">Delete Path objects.</param>
    public static void RemoveMapObjects(bool prefabs, bool paths)
    {
        if (prefabs)
        {
            foreach (PrefabDataHolder g in GameObject.FindGameObjectWithTag("Prefabs").GetComponentsInChildren<PrefabDataHolder>())
            {
                if (g != null)
                {
                    GameObject.DestroyImmediate(g.gameObject);
                }
            }
            foreach (CustomPrefabData p in GameObject.FindGameObjectWithTag("Prefabs").GetComponentsInChildren<CustomPrefabData>())
            {
                GameObject.DestroyImmediate(p.gameObject);
            }
        }
        if (paths)
        {
            foreach (PathDataHolder g in GameObject.FindGameObjectWithTag("Paths").GetComponentsInChildren<PathDataHolder>())
            {
                GameObject.DestroyImmediate(g.gameObject);
            }
        }
    }
    public static Vector3 GetTerrainSize()
    {
        return GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>().terrainData.size;
    }
    public static Vector3 GetMapOffset()
    {
        return 0.5f * GetTerrainSize();
    }
    public static List<int> ParseObjectSelection(EditorEnums.Selections.ObjectSelection objectSelection)
    {
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(EditorEnums.Selections.ObjectSelection)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)objectSelection & layer) != 0)
            {
                selectedEnums.Add(i);
                
            }
        }
        return selectedEnums;
    }
    public static List<int> ParseTerrainSelection(EditorEnums.Selections.Terrains terrainSelection)
    {
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(EditorEnums.Selections.Terrains)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)terrainSelection & layer) != 0)
            {
                selectedEnums.Add(i);
            }
        }
        return selectedEnums;
    }
    #region RotateMap Methods
    public static void RotateMap(EditorEnums.Selections.ObjectSelection objectSelection, bool CW)
    {
        foreach (var item in ParseObjectSelection(objectSelection))
        {
            switch (item)
            {
                case 0:
                    RotateLayer("ground", CW);
                    break;
                case 1:
                    RotateLayer("biome", CW);
                    break;
                case 2:
                    RotateLayer("alpha", CW);
                    break;
                case 3:
                    RotateAllTopologyLayers(CW);
                    break;
                case 4:
                    RotateHeightMap(CW);
                    break;
                case 5:
                    RotatePrefabs(CW);
                    break;
                case 6:
                    RotatePaths(CW);
                    break;
            }
        }
    }
    /// <summary>
    /// Rotates prefabs 90°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePrefabs(bool CW)
    {
        var prefabRotate = GameObject.FindGameObjectWithTag("Prefabs");
        if (CW)
        {
            prefabRotate.transform.Rotate(0, 90, 0, Space.World);
        }
        else
        {
            prefabRotate.transform.Rotate(0, -90, 0, Space.World);
        }
    }
    /// <summary>
    /// Rotates paths 90°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePaths(bool CW)
    {
        var pathRotate = GameObject.FindGameObjectWithTag("Paths");
        if (CW)
        {
            pathRotate.transform.Rotate(0, 90, 0, Space.World);
        }
        else
        {
            pathRotate.transform.Rotate(0, -90, 0, Space.World);
        }
    }
    #endregion
    #region HeightMap Methods
    /// <summary>
    /// Rotates Terrain Map and Water Map 90°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateHeightMap(bool CW)
    {
        float[,] oldHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        float[,] newHeightMap = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        float[,] oldWaterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        float[,] newWaterMap = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        if (CW)
        {
            for (int i = 0; i < oldHeightMap.GetLength(0); i++)
            {
                for (int j = 0; j < oldHeightMap.GetLength(1); j++)
                {
                    newHeightMap[i, j] = oldHeightMap[j, oldHeightMap.GetLength(1) - i - 1];
                    newWaterMap[i, j] = oldWaterMap[j, oldWaterMap.GetLength(1) - i - 1];
                }
            }
        }
        else
        {
            for (int i = 0; i < oldHeightMap.GetLength(0); i++)
            {
                for (int j = 0; j < oldHeightMap.GetLength(1); j++)
                {
                    newHeightMap[i, j] = oldHeightMap[oldHeightMap.GetLength(0) - j - 1, i];
                    newWaterMap[i, j] = oldWaterMap[oldWaterMap.GetLength(0) - j - 1, i];
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, newHeightMap);
        water.terrainData.SetHeights(0, 0, newWaterMap);
    }
    /// <summary>
    /// Sets the selected terrains to the height set.
    /// </summary>
    /// <param name="height">The height to set.</param>
    /// <param name="terrains">The selected terrains.</param>
    public static void SetHeightmap(float height, EditorEnums.Selections.Terrains terrains)
    {
        height /= 1000f;
        foreach (var item in ParseTerrainSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Set Terrain Height");
                    terrain.terrainData.SetHeights(0, 0, ArrayOperations.SetValues(terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight), height));
                    break;
                case 1:
                    Undo.RegisterCompleteObjectUndo(water.terrainData, "Set Water Height");
                    water.terrainData.SetHeights(0, 0, ArrayOperations.SetValues(water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight), height));
                    break;
            }
        }
    }
    /// <summary>
    /// Inverts the HeightMap.
    /// </summary>
    public static void InvertHeightmap()
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Invert Terrain");
        terrain.terrainData.SetHeights(0, 0, ArrayOperations.Invert(terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight)));
    }
    /// <summary>
    /// Normalises the HeightMap between two heights.
    /// </summary>
    /// <param name="normaliseLow">The lowest height the HeightMap should be.</param>
    /// <param name="normaliseHigh">The highest height the HeightMap should be.</param>
    public static void NormaliseHeightmap(float normaliseLow, float normaliseHigh)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Normalise Terrain");
        terrain.terrainData.SetHeights(0, 0, ArrayOperations.Normalise(terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight), normaliseLow, normaliseHigh));
    }
    /// <summary>
    /// Terraces the HeightMap.
    /// </summary>
    /// <param name="featureSize">The height of each terrace.</param>
    /// <param name="interiorCornerWeight">The weight of the terrace effect.</param>
    public static void TerraceErodeHeightmap(float featureSize, float interiorCornerWeight)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Terrace Terrain");
        Material mat = new Material((Shader)AssetDatabase.LoadAssetAtPath("Packages/com.unity.terrain-tools/Shaders/TerraceErosion.shader", typeof(Shader)));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, heightmapCentre, terrain.terrainData.size.x, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(1.0f, featureSize, interiorCornerWeight, 0.0f);
        mat.SetTexture("_BrushTex", terrainFilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - TerraceErosion");
    }
    /// <summary>
    /// Smooths the HeightMap.
    /// </summary>
    /// <param name="filterStrength">The strength of the smoothing.</param>
    /// <param name="blurDirection">The direction the smoothing should preference. Between -1f - 1f.</param>
    public static void SmoothHeightmap(float filterStrength, float blurDirection)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Smooth Terrain");
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, heightmapCentre, terrain.terrainData.size.x, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(filterStrength, 0.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", terrainFilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        Vector4 smoothWeights = new Vector4(Mathf.Clamp01(1.0f - Mathf.Abs(blurDirection)), Mathf.Clamp01(-blurDirection), Mathf.Clamp01(blurDirection), 0.0f);
        mat.SetVector("_SmoothWeights", smoothWeights);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SmoothHeights);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - Smooth Heights");
    }
    /// <summary>
    /// Sets the edge row of pixels on the HeightMap.
    /// </summary>
    /// <param name="heightToSet">The height to set.</param>
    /// <param name="sides">The sides to set.</param>
    public static void SetEdgePixel(float heightToSet, bool[] sides)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Set Edge Pixel");
        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        for (int i = 0; i < terrain.terrainData.heightmapHeight; i++)
        {
            for (int j = 0; j < terrain.terrainData.heightmapWidth; j++)
            {
                if (i == 0 && sides[2] == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
                if (i == terrain.terrainData.heightmapHeight - 1 && sides[0] == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
                if (j == 0 && sides[3] == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
                if (j == terrain.terrainData.heightmapWidth - 1 && sides[1] == true)
                {
                    heightMap[i, j] = heightToSet / 1000f;
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }
    /// <summary>
    /// Increases or decreases the HeightMap by the offset.
    /// </summary>
    /// <param name="offset">The amount to offset by. Negative values offset down.</param>
    /// <param name="checkHeight">Check if offsetting the heightmap would exceed the min-max values.</param>
    /// <param name="setWaterMap">Offset the water heightmap.</param>
    public static void OffsetHeightmap(float offset, bool checkHeight, bool setWaterMap)
    {
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        offset = offset / 1000f;
        bool heightOutOfRange = false;
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int j = 0; j < terrain.terrainData.heightmapHeight; j++)
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
            terrain.terrainData.SetHeights(0, 0, heightMap);
            water.terrainData.SetHeights(0, 0, waterMap);
        }
        else if (heightOutOfRange == true)
        {
            Debug.Log("Heightmap offset exceeds heightmap limits, try a smaller value.");
        }
    }
    /// <summary>
    /// Sets the water level up to 500 if it's below 500 in height.
    /// </summary>
    public static void DebugWaterLevel()
    {
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
    /// <summary>
    /// Sets the HeightMap level to the minimum if it's below.
    /// </summary>
    /// <param name="minimumHeight">The minimum height to set.</param>
    public static void SetMinimumHeight(float minimumHeight)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Minimum Height Terrain");
        float[,] landMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
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
        terrain.terrainData.SetHeights(0, 0, landMap);
    }
    /// <summary>
    /// Puts the heightmap level to the maximum if it's above.
    /// </summary>
    /// <param name="maximumHeight">The maximum height to set.</param>
    public static void SetMaximumHeight(float maximumHeight)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Maximum Height Terrain");
        float[,] landMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
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
        terrain.terrainData.SetHeights(0, 0, landMap);
    }
    /// <summary>
    /// Returns the height of the HeightMap at the selected coords.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <returns></returns>
    public static float GetHeight(int x, int z)
    {
        float xNorm = (float)x / (float)terrain.terrainData.alphamapHeight;
        float yNorm = (float)z / (float)terrain.terrainData.alphamapHeight;
        float height = terrain.terrainData.GetInterpolatedHeight(xNorm, yNorm);
        return height;
    }
    /// <summary>
    /// Returns a 2D array of the height values.
    /// </summary>
    /// <returns></returns>
    public static float[,] GetHeights()
    {
        float alphamapInterp = 1f / terrain.terrainData.alphamapWidth;
        float[,] heights = terrain.terrainData.GetInterpolatedHeights(0, 0, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapWidth, alphamapInterp, alphamapInterp);
        return heights;
    }
    /// <summary>
    /// Returns the slope of the HeightMap at the selected coords.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <returns></returns>
    public static float GetSlope(int x, int z)
    {
        float xNorm = (float)x / terrain.terrainData.alphamapHeight;
        float yNorm = (float)z / terrain.terrainData.alphamapHeight;
        float slope = terrain.terrainData.GetSteepness(xNorm, yNorm);
        return slope;
    }
    /// <summary>
    /// Returns a 2D array of the slope values.
    /// </summary>
    /// <returns></returns>
    public static float[,] GetSlopes()
    {
        float[,] slopes = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight];
        for (int i = 0; i < terrain.terrainData.alphamapHeight; i++)
        {
            for (int j = 0; j < terrain.terrainData.alphamapHeight; j++)
            {
                float iNorm = (float)i / (float)terrain.terrainData.alphamapHeight;
                float jNorm = (float)j / (float)terrain.terrainData.alphamapHeight;
                slopes[i, j] = terrain.terrainData.GetSteepness(iNorm, jNorm);
            }
        }
        return slopes;
    }
    #endregion
    #region SplatMap Methods
    /// <summary>
    /// Returns the selected TerrainSplat enums.
    /// </summary>
    /// <param name="ground">The TerrainSplat Enum to parse.</param>
    /// <returns></returns>
    public static List<int> ReturnSelectedSplats(TerrainSplat.Enum ground)
    {
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainSplat.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)ground & layer) != 0)
            {
                selectedEnums.Add(i);
            }
        }
        return selectedEnums;
    }
    /// <summary>
    /// Returns the selected TerrainBiome enums.
    /// </summary>
    /// <param name="biome">The TerrainBiome Enum to parse.</param>
    /// <returns></returns>
    public static List<int> ReturnSelectedBiomes(TerrainBiome.Enum biome)
    {
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainBiome.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)biome & layer) != 0)
            {
                selectedEnums.Add(i);
            }
        }
        return selectedEnums;
    }
    /// <summary>
    /// Returns the selected TerrainTopology enums.
    /// </summary>
    /// <param name="topology">The TerrainTopology Enum to parse.</param>
    /// <returns></returns>
    public static List<int> ReturnSelectedTopologies(TerrainTopology.Enum topology)
    {
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainTopology.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)topology & layer) != 0)
            {
                selectedEnums.Add(i);
            }
        }
        return selectedEnums;
    }
    /// <summary>
    /// Returns the SplatMap at the selected LandLayer.
    /// </summary>
    /// <param name="landLayer">The LandLayer to return. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    /// <returns></returns>
    public static float[,,] GetSplatMap(string landLayer, int topology = 0)
    {
        switch (landLayer.ToLower())
        {
            default:
                return null;
            case "ground":
                return LandData.groundArray;
            case "biome":
                return LandData.biomeArray;
            case "alpha":
                return LandData.alphaArray;
            case "topology":
                return LandData.topologyArray[topology];
        }
    }
    /// <summary>
    /// Texture count in layer chosen, used for determining the size of the splatmap array.
    /// </summary>
    /// <param name="landLayer">The LandLayer to return the texture count from. (Ground, Biome, Alpha, Topology)</param>
    /// <returns></returns>
    public static int TextureCount(string landLayer)
    {
        if (landLayer.ToLower() == "ground")
        {
            return 8;
        }
        else if (landLayer.ToLower() == "biome")
        {
            return 4;
        }
        return 2;
    }
    /// <summary>
    /// Returns the value of a texture at the selected coords.
    /// </summary>
    /// <param name="landLayer">The LandLayer of the texture. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to get.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    /// <returns></returns>
    public static float GetTexture(string landLayer, int texture, int x, int z, int topology = 0)
    {
        return GetSplatMap(landLayer, topology)[x, z, texture];
    }
    /// <summary>
    /// Rotates the selected layer.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to rotate. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void RotateLayer(string landLayerToPaint, bool CW, int topology = 0)
    {
        int textureCount = TextureCount(landLayerToPaint);
        float[,,] oldLayer = GetSplatMap(landLayerToPaint, topology);
        float[,,] newLayer = new float[oldLayer.GetLength(0), oldLayer.GetLength(1), textureCount];
        if (CW)
        {
            for (int i = 0; i < newLayer.GetLength(0); i++)
            {
                for (int j = 0; j < newLayer.GetLength(1); j++)
                {
                    for (int k = 0; k < textureCount; k++)
                    {
                        newLayer[i, j, k] = oldLayer[j, oldLayer.GetLength(1) - i - 1, k];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < newLayer.GetLength(0); i++)
            {
                for (int j = 0; j < newLayer.GetLength(1); j++)
                {
                    for (int k = 0; k < textureCount; k++)
                    {
                        newLayer[i, j, k] = oldLayer[oldLayer.GetLength(0) - j - 1, i, k];
                    }
                }
            }
        }
        LandData.SetData(newLayer, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Rotates the selected topologies.
    /// </summary>
    /// <param name="topologyLayers">The Topology layers to rotate.</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateTopologyLayers(TerrainTopology.Enum topologyLayers, bool CW)
    {
        List<int> topologyElements = ReturnSelectedTopologies(topologyLayers);
        progressValue = 1f / topologyElements.Count;
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressBar += progressValue;
            ProgressBar("Rotating Topologies", "Rotating: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressBar);
            RotateLayer("topology", CW, i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Rotates all Topology layers°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateAllTopologyLayers(bool CW)
    {
        progressValue = 1f / TerrainTopology.COUNT;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Rotating Topologies", "Rotating: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressBar);
            RotateLayer("topology", CW, TerrainTopology.TypeToIndex(i));
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Paints if all the conditions passed in are true.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to paint.</param>
    /// <param name="conditions">The conditions to check.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintConditional(string landLayerToPaint, int texture, Conditions conditions, int topology = 0)
    {
        float[,,] groundSplatMap = GetSplatMap("ground");
        float[,,] biomeSplatMap = GetSplatMap("biome");
        float[,,] alphaSplatMap = GetSplatMap("alpha");
        float[,,] topologySplatMap = GetSplatMap("topology", topology);
        float[,,] splatMapPaint = new float[groundSplatMap.GetLength(0), groundSplatMap.GetLength(1), TextureCount(landLayerToPaint)];
        int textureCount = TextureCount(landLayerToPaint);
        float slope, height;
        float[,] heights = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight];
        float[,] slopes = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight];
        ProgressBar("Conditional Painter", "Preparing SplatMaps", 0.025f);
        switch (landLayerToPaint.ToLower())
        {
            case "ground":
                splatMapPaint = groundSplatMap;
                break;
            case "biome":
                splatMapPaint = biomeSplatMap;
                break;
            case "alpha":
                splatMapPaint = alphaSplatMap;
                break;
            case "topology":
                splatMapPaint = topologySplatMap;
                break;
        }
        List<TopologyLayers> topologyLayersList = new List<TopologyLayers>();
        List<GroundTextures> groundTexturesList = new List<GroundTextures>();
        List<BiomeTextures> biomeTexturesList = new List<BiomeTextures>();
        ProgressBar("Conditional Painter", "Gathering Conditions", 0.05f);
        foreach (var topologyLayerInt in ReturnSelectedTopologies(conditions.TopologyLayers))
        {
            topologyLayersList.Add(new TopologyLayers()
            {
                Topologies = GetSplatMap("topology", topologyLayerInt)
            });
        }
        foreach (var groundTextureInt in ReturnSelectedSplats(conditions.GroundConditions))
        {
            groundTexturesList.Add(new GroundTextures()
            {
                Texture = groundTextureInt
            });
        }
        foreach (var biomeTextureInt in ReturnSelectedBiomes(conditions.BiomeConditions))
        {
            biomeTexturesList.Add(new BiomeTextures()
            {
                Texture = biomeTextureInt
            });
        }
        if (conditions.CheckHeight)
        {
            heights = GetHeights();
        }
        if (conditions.CheckSlope)
        {
            slopes = GetSlopes();
        }
        progressValue = 1f / groundSplatMap.GetLength(0);
        for (int i = 0; i < groundSplatMap.GetLength(0); i++)
        {
            progressBar += progressValue;
            ProgressBar("Conditional Painter", "Painting", progressBar);
            for (int j = 0; j < groundSplatMap.GetLength(1); j++)
            {
                if (conditions.CheckSlope)
                {
                    slope = slopes[j, i];
                    if (!(slope >= conditions.SlopeLow && slope <= conditions.SlopeHigh))
                    {
                        continue;
                    }
                }
                if (conditions.CheckHeight)
                {
                    height = heights[i, j];
                    if (!(height >= conditions.HeightLow & height <= conditions.HeightHigh))
                    {
                        continue;
                    }
                }
                foreach (GroundTextures groundTextureCheck in groundTexturesList)
                {
                    if (groundSplatMap[i, j, groundTextureCheck.Texture] < 0.5f)
                    {
                        continue;
                    }
                }
                foreach (BiomeTextures biomeTextureCheck in biomeTexturesList)
                {
                    if (biomeSplatMap[i, j, biomeTextureCheck.Texture] < 0.5f)
                    {
                        continue;
                    }
                }
                if (conditions.CheckAlpha)
                {
                    if (alphaSplatMap[i, j, conditions.AlphaTexture] < 1f)
                    {
                        continue;
                    }
                }
                foreach (TopologyLayers layer in topologyLayersList)
                {
                    if (layer.Topologies[i, j, conditions.TopologyTexture] < 0.5f)
                    {
                        continue;
                    }
                }
                for (int k = 0; k < textureCount; k++)
                {
                    splatMapPaint[i, j, k] = 0;
                }
                splatMapPaint[i, j, texture] = 1f;
            }
        }
        ClearProgressBar();
        groundTexturesList.Clear();
        biomeTexturesList.Clear();
        topologyLayersList.Clear();
        LandData.SetData(splatMapPaint, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Paints the layer wherever the height conditions are met.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintHeight(string landLayerToPaint, float heightLow, float heightHigh, int t, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / splatMap.GetLength(0);
                float jNorm = (float)j / splatMap.GetLength(1);
                float height = terrain.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                if (height >= heightLow && height <= heightHigh)
                {
                    for (int k = 0; k < textureCount; k++) // Erases the textures on all the layers.
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1; // Paints the texture t.
                }
            }
        }
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Paints the layer wherever the height conditions are met with a weighting determined by the range the height falls in. 
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="heightBlendLow">The minimum height to start to paint. The texture weight will increase as it gets closer to the heightlow.</param>
    /// <param name="heightBlendHigh">The maximum height to start to paint. The texture weight will increase as it gets closer to the heighthigh.</param>
    /// <param name="t">The texture to paint.</param>
    public static void PaintHeightBlend(string landLayerToPaint, float heightLow, float heightHigh, float heightBlendLow, float heightBlendHigh, int t, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float[] normalised = new float[textureCount];
                float height = terrain.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                if (height >= heightLow && height <= heightHigh)
                {
                    for (int k = 0; k < textureCount; k++) // Erases the textures on all the layers.
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1; // Paints the texture t.
                }
                else if (height > heightBlendLow && height < heightLow)
                {
                    float normalisedHeight = height - heightBlendLow;
                    float heightRange = heightLow - heightBlendLow;
                    float heightBlend = normalisedHeight / heightRange; // Holds data about the texture weight between the blend ranges.
                    for (int k = 0; k < textureCount; k++)
                    {
                        if (k == t)
                        {
                            splatMap[i, j, t] = heightBlend;
                        }
                        else
                        {
                            splatMap[i, j, k] = splatMap[i, j, k] * Mathf.Clamp01(1f - heightBlend);
                        }
                        normalised[k] = splatMap[i, j, k];
                    }
                    float normalisedWeights = normalised.Sum();
                    for (int k = 0; k < normalised.GetLength(0); k++)
                    {
                        normalised[k] /= normalisedWeights;
                        splatMap[i, j, k] = normalised[k];
                    }
                }
                else if (height > heightHigh && height < heightBlendHigh)
                {
                    float normalisedHeight = height - heightHigh;
                    float heightRange = heightBlendHigh - heightHigh;
                    float heightBlendInverted = normalisedHeight / heightRange; // Holds data about the texture weight between the blend ranges.
                    float heightBlend = 1 - heightBlendInverted; // We flip this because we want to find out how close the slope is to the max blend.
                    for (int k = 0; k < textureCount; k++)
                    {
                        if (k == t)
                        {
                            splatMap[i, j, t] = heightBlend;
                        }
                        else
                        {
                            splatMap[i, j, k] = splatMap[i, j, k] * Mathf.Clamp01(1f - heightBlend);
                        }
                        normalised[k] = splatMap[i, j, k];
                    }
                    float normalisedWeights = normalised.Sum();
                    for (int k = 0; k < normalised.GetLength(0); k++)
                    {
                        normalised[k] /= normalisedWeights;
                        splatMap[i, j, k] = normalised[k];
                    }
                }
            }
        }
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Sets whole layer to the active texture. 
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintLayer(string landLayerToPaint, int t, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                for (int k = 0; k < textureCount; k++)
                {
                    splatMap[i, j, k] = 0;
                }
                splatMap[i, j, t] = 1;
            }
        }
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Paints the selected Topology layers.
    /// </summary>
    /// <param name="topologyLayers">The Topology layers to clear.</param>
    public static void PaintTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = ReturnSelectedTopologies(topologyLayers);
        progressValue = 1f / topologyElements.Count;
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressBar += progressValue;
            ProgressBar("Painting Topologies", "Painting: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressBar);
            PaintLayer("topology", i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Paints all the topology layers.
    /// </summary>
    public static void PaintAllTopologyLayers()
    {
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Painting Layers", "Painting: " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i), progressBar);
            PaintLayer("topology", i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Sets whole layer to the inactive texture. Alpha and Topology only. 
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to clear. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void ClearLayer(string landLayerToPaint, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        var alpha = (landLayerToPaint.ToLower() == "alpha") ? true : false;
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (alpha)
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
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Clears the selected Topology layers.
    /// </summary>
    /// <param name="topologyLayers">The Topology layers to clear.</param>
    public static void ClearTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = ReturnSelectedTopologies(topologyLayers);
        progressValue = 1f / topologyElements.Count;
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressBar += progressValue;
            ProgressBar("Clearing Topologies", "Clearing: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressBar);
            ClearLayer("topology", i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Clears all the topology layers.
    /// </summary>
    public static void ClearAllTopologyLayers()
    {
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Clearing Layers", "Clearing: " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i), progressBar);
            ClearLayer("topology", i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Inverts the active and inactive textures. Alpha and Topology only. 
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to invert. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void InvertLayer(string landLayerToPaint, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
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
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Inverts the selected Topology layers.
    /// </summary>
    /// <param name="topologyLayers">The Topology layers to invert.</param>
    public static void InvertTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = ReturnSelectedTopologies(topologyLayers);
        progressValue = 1f / topologyElements.Count;
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressBar += progressValue;
            ProgressBar("Inverting Topologies", "Inverting: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressBar);
            InvertLayer("topology", i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Inverts all the Topology layers.
    /// </summary>
    public static void InvertAllTopologyLayers()
    {
        progressValue = 1f / TerrainTopology.COUNT;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Inverting Layers", "Inverting: " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i), progressBar);
            InvertLayer("topology", i);
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Paints the layer wherever the slope conditions are met. Includes option to blend.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="slopeLow">The minimum slope to paint at 100% weight.</param>
    /// <param name="slopeHigh">The maximum slope to paint at 100% weight.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintSlope(string landLayerToPaint, float slopeLow, float slopeHigh, int t, int topology = 0) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float slope = terrain.terrainData.GetSteepness(jNorm, iNorm); // Normalises the steepness coords to match the splatmap array size.
                if (slope >= slopeLow && slope <= slopeHigh)
                {
                    for (int k = 0; k < textureCount; k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1;
                }
            }
        }
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Paints the layer wherever the slope conditions are met. Includes option to blend.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="slopeLow">The minimum slope to paint at 100% weight.</param>
    /// <param name="slopeHigh">The maximum slope to paint at 100% weight.</param>
    /// <param name="minBlendLow">The minimum slope to start to paint. The texture weight will increase as it gets closer to the slopeLow.</param>
    /// <param name="maxBlendHigh">The maximum slope to start to paint. The texture weight will increase as it gets closer to the slopeHigh.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintSlopeBlend(string landLayerToPaint, float slopeLow, float slopeHigh, float minBlendLow, float maxBlendHigh, int t) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float[] normalised = new float[textureCount];
                float slope = terrain.terrainData.GetSteepness(jNorm, iNorm); // Normalises the steepness coords to match the splatmap array size.
                if (slope >= slopeLow && slope <= slopeHigh)
                {
                    for (int k = 0; k < textureCount; k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1;
                }
                else if (slope > minBlendLow && slope < slopeLow)
                {
                    float normalisedSlope = slope - minBlendLow;
                    float slopeRange = slopeLow - minBlendLow;
                    float slopeBlend = normalisedSlope / slopeRange; // Holds data about the texture weight between the blend ranges.
                    for (int k = 0; k < textureCount; k++) // Gets the weights of the textures in the pos. 
                    {
                        if (k == t)
                        {
                            splatMap[i, j, t] = slopeBlend;
                        }
                        else
                        {
                            splatMap[i, j, k] = splatMap[i, j, k] * Mathf.Clamp01(1f - slopeBlend);
                        }
                        normalised[k] = splatMap[i, j, k];
                    }
                    float normalisedWeights = normalised.Sum();
                    for (int k = 0; k < normalised.GetLength(0); k++)
                    {
                        normalised[k] /= normalisedWeights;
                        splatMap[i, j, k] = normalised[k];
                    }
                }
                else if (slope > slopeHigh && slope < maxBlendHigh)
                {
                    float normalisedSlope = slope - slopeHigh;
                    float slopeRange = maxBlendHigh - slopeHigh;
                    float slopeBlendInverted = normalisedSlope / slopeRange; // Holds data about the texture weight between the blend ranges.
                    float slopeBlend = 1 - slopeBlendInverted; // We flip this because we want to find out how close the slope is to the max blend.
                    for (int k = 0; k < textureCount; k++)
                    {
                        if (k == t)
                        {
                            splatMap[i, j, t] = slopeBlend;
                        }
                        else
                        {
                            splatMap[i, j, k] = splatMap[i, j, k] * Mathf.Clamp01(1f - slopeBlend);
                        }
                        normalised[k] = splatMap[i, j, k];
                    }
                    float normalisedWeights = normalised.Sum();
                    for (int k = 0; k < normalised.GetLength(0); k++)
                    {
                        normalised[k] /= normalisedWeights;
                        splatMap[i, j, k] = normalised[k];
                    }
                }
            }
        }
        LandData.SetData(splatMap, landLayerToPaint);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Paints area within these splatmap coords, Maps will always have a splatmap resolution between 512 - 2048 resolution, to the nearest Power of Two (512, 1024, 2048).
    /// Paints from bottom left to top right corner of map if world rotation is 0° in the editor.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintArea(string landLayerToPaint, int z1, int z2, int x1, int x2, int t, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        z1 = Mathf.Clamp(z1, 0, splatMap.GetLength(0));
        z2 = Mathf.Clamp(z2, 0, splatMap.GetLength(1));
        x1 = Mathf.Clamp(x1, 0, splatMap.GetLength(0));
        x2 = Mathf.Clamp(x2, 0, splatMap.GetLength(1));
        for (int i = z1; i < z2; i++)
        {
            for (int j = x1; j < x2; j++)
            {
                for (int k = 0; k < textureCount; k++)
                {
                    splatMap[i, j, k] = 0;
                }
                splatMap[i, j, t] = 1;
            }
        }
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    /// <summary>
    /// Paints the splats wherever the water is above 500 and is above the terrain.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="aboveTerrain">Check if the watermap is above the terrain before painting.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintRiver(string landLayerToPaint, bool aboveTerrain, int t, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float waterHeight = water.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                float landHeight = terrain.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                switch (aboveTerrain)
                {
                    case true:
                        if (waterHeight > 500 && waterHeight > landHeight)
                        {
                            for (int k = 0; k < textureCount; k++)
                            {
                                splatMap[i, j, k] = 0;
                            }
                            splatMap[i, j, t] = 1;
                        }
                        break;
                    case false:
                        if (waterHeight > 500)
                        {
                            for (int k = 0; k < textureCount; k++)
                            {
                                splatMap[i, j, k] = 0;
                            }
                            splatMap[i, j, t] = 1;
                        }
                        break;
                }
            }
        }
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
    
    /// <summary>
    /// Copies the selected texture on a landlayer and paints the same coordinate on another landlayer with the other selected texture.
    /// </summary>
    /// <param name="landLayerFrom">The LandLayer to copy. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="textureFrom">The texture to copy.</param>
    /// <param name="textureToPaint">The texture to paint.</param>
    /// <param name="topologyFrom">The Topology layer to copy from, if selected.</param>
    /// <param name="topologyToPaint">The Topology layer to paint to, if selected.</param>
    public static void CopyTexture(string landLayerFrom, string landLayerToPaint, int textureFrom, int textureToPaint, int topologyFrom = 0, int topologyToPaint = 0)
    {
        ProgressBar("Copy Textures", "Copying: " + landLayerFrom, 0.3f);
        float[,,] splatMapFrom = GetSplatMap(landLayerFrom, topologyFrom);
        float[,,] splatMapTo = GetSplatMap(landLayerToPaint, topologyToPaint);
        ProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.5f);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMapFrom.GetLength(0); i++)
        {
            for (int j = 0; j < splatMapFrom.GetLength(1); j++)
            {
                if (splatMapFrom[i, j, textureFrom] > 0)
                {
                    for (int k = 0; k < textureCount; k++)
                    {
                        splatMapTo[i, j, k] = 0;
                    }
                    splatMapTo[i, j, textureToPaint] = 1;
                }
            }
        }
        ProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.9f);
        LandData.SetData(splatMapTo, landLayerToPaint, topologyToPaint);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
        ClearProgressBar();
    }
    #endregion
    /// <summary>
    /// Changes all the prefab categories to a the RustEdit custom prefab format. Hide's prefabs from appearing in RustEdit.
    /// </summary>
    public static void HidePrefabsInRustEdit()
    {
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        ProgressBar("Hide Prefabs in RustEdit", "Hiding prefabs: ", 0f);
        int prefabsHidden = 0;
        progressValue = 1f / prefabDataHolders.Length;
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressBar += progressValue;
            ProgressBar("Hide Prefabs in RustEdit", "Hiding prefabs: " + i + " / " + prefabDataHolders.Length, progressBar);
            prefabDataHolders[i].prefabData.category = @":\RustEditHiddenPrefab:" + prefabsHidden + ":";
            prefabsHidden++;
        }
        Debug.Log("Hid " + prefabsHidden + " prefabs.");
        ClearProgressBar();
    }
    /// <summary>
    /// Breaks down RustEdit custom prefabs back into the individual prefabs.
    /// </summary>
    public static void BreakRustEditCustomPrefabs()
    {
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        ProgressBar("Break RustEdit Custom Prefabs", "Scanning prefabs", 0f);
        int prefabsBroken = 0;
        progressValue = 1f / prefabDataHolders.Length;
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressBar += progressValue;
            ProgressBar("Break RustEdit Custom Prefabs", "Scanning prefabs: " + i + " / " + prefabDataHolders.Length, progressBar);
            if (prefabDataHolders[i].prefabData.category.Contains(':'))
            {
                prefabDataHolders[i].prefabData.category = "Decor";
                prefabsBroken++;
            }
        }
        Debug.Log("Broke down " + prefabsBroken + " prefabs.");
        ClearProgressBar();
    }
    /// <summary>
    /// Parents all the RustEdit custom prefabs in the map to parent gameobjects.
    /// </summary>
    public static void GroupRustEditCustomPrefabs()
    {
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        Transform prefabHierachy = GameObject.FindGameObjectWithTag("Prefabs").transform;
        Dictionary<string, GameObject> prefabParents = new Dictionary<string, GameObject>();
        ProgressBar("Group RustEdit Custom Prefabs", "Scanning prefabs", 0f);
        progressValue = 1f / prefabDataHolders.Length;
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressBar += progressValue;
            ProgressBar("Break RustEdit Custom Prefabs", "Scanning prefabs: " + i + " / " + prefabDataHolders.Length, progressBar);
            if (prefabDataHolders[i].prefabData.category.Contains(':'))
            {
                var categoryFields = prefabDataHolders[i].prefabData.category.Split(':');
                if (!prefabParents.ContainsKey(categoryFields[1]))
                {
                    GameObject customPrefabParent = new GameObject(categoryFields[1]);
                    customPrefabParent.transform.SetParent(prefabHierachy);
                    customPrefabParent.transform.localPosition = prefabDataHolders[i].transform.localPosition;
                    customPrefabParent.AddComponent<CustomPrefabData>();
                    prefabParents.Add(categoryFields[1], customPrefabParent);
                }
                if (prefabParents.TryGetValue(categoryFields[1], out GameObject prefabParent))
                {
                    prefabDataHolders[i].gameObject.transform.SetParent(prefabParent.transform);
                }
            }
        }
        ClearProgressBar();
    }
    /// <summary>
    /// Exports information about all the map prefabs to a JSON file.
    /// </summary>
    /// <param name="mapPrefabFilePath">The JSON file path and name.</param>
    /// <param name="deletePrefabs">Deletes the prefab after the data is exported.</param>
    public static void ExportMapPrefabs(string mapPrefabFilePath, bool deletePrefabs)
    {
        List<PrefabExport> mapPrefabExports = new List<PrefabExport>();
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        ProgressBar("Export Map Prefabs", "Exporting...", 0f);
        progressValue = 1f / prefabDataHolders.Length;
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressBar += progressValue;
            ProgressBar("Export Map Prefabs", "Exporting prefab: " + i + " / " + prefabDataHolders.Length, progressBar);
            mapPrefabExports.Add(new PrefabExport()
            {
                PrefabNumber = i,
                PrefabID = prefabDataHolders[i].prefabData.id,
                PrefabPosition = prefabDataHolders[i].transform.localPosition.ToString(),
                PrefabScale = prefabDataHolders[i].transform.localScale.ToString(),
                PrefabRotation = prefabDataHolders[i].transform.rotation.ToString()
            });
            if (deletePrefabs)
            {
                GameObject.DestroyImmediate(prefabDataHolders[i].gameObject);
            }
        }
        using (StreamWriter streamWriter = new StreamWriter(mapPrefabFilePath, false))
        {
            streamWriter.WriteLine("{");
            foreach (PrefabExport prefabDetail in mapPrefabExports)
            {
                streamWriter.WriteLine("   \"" + prefabDetail.PrefabNumber + "\": \"" + prefabDetail.PrefabID + ":" + prefabDetail.PrefabPosition + ":" + prefabDetail.PrefabScale + ":" + prefabDetail.PrefabRotation + "\",");
            }
            streamWriter.WriteLine("   \"Prefab Count\": " + prefabDataHolders.Length);
            streamWriter.WriteLine("}");
        }
        mapPrefabExports.Clear();
        ClearProgressBar();
        Debug.Log("Exported " + prefabDataHolders.Length + " prefabs.");
    }
    /// <summary>
    /// Exports lootcrates to a JSON for use with Oxide.
    /// </summary>
    /// <param name="prefabFilePath">The path to save the JSON.</param>
    /// <param name="deletePrefabs">Delete the lootcrates after exporting.</param>
    public static void ExportLootCrates(string prefabFilePath, bool deletePrefabs)
    {
        List<PrefabExport> prefabExports = new List<PrefabExport>();
        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
        int lootCrateCount = 0;
        foreach (PrefabDataHolder p in prefabs)
        {
            switch (p.prefabData.id)
            {
                case 1603759333:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_basic.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 3286607235:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_elite.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1071933290:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_mine.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 2857304752:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1546200557:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal_2.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 2066926276:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal_2_food.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1791916628:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal_2_medical.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1892026534:
                    p.transform.Rotate(Vector3.zero, 180f);
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_underwater_advanced.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 3852690109:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_underwater_basic.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
            }
        }
        using (StreamWriter streamWriter = new StreamWriter(prefabFilePath, false))
        {
            streamWriter.WriteLine("{");
            foreach (PrefabExport prefabDetail in prefabExports)
            {
                streamWriter.WriteLine("   \"" + prefabDetail.PrefabNumber + "\": \"" + prefabDetail.PrefabPath + ":" + prefabDetail.PrefabPosition + ":" + prefabDetail.PrefabRotation + "\",");
            }
            streamWriter.WriteLine("   \"Prefab Count\": " + lootCrateCount);
            streamWriter.WriteLine("}");
        }
        prefabExports.Clear();
        Debug.Log("Exported " + lootCrateCount + " lootcrates.");
    }
    /// <summary>
    /// Centres the Prefab and Path parent objects.
    /// </summary>
    /// <param name="terrains"></param>
    static void CentreSceneObjects(MapInfo terrains)
    {
        var worldCentrePrefab = GameObject.FindGameObjectWithTag("Prefabs");
        worldCentrePrefab.transform.position = new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2);
        var worldCentrePath = GameObject.FindGameObjectWithTag("Paths");
        worldCentrePath.transform.position = new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2);
    }
    /// <summary>
    /// Loads and sets up the map Prefabs.
    /// </summary>
    /// <param name="terrains"></param>
    static void LoadPrefabs(MapInfo terrains)
    {
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        ProgressBar("Loading: " + loadPath, "Spawning Prefabs ", 0.8f);
        float progressValue = 0f;
        if (PrefabManager.prefabsLoaded)
        {
            for (int i = 0; i < terrains.prefabData.Length; i++)
            {
                progressValue += 0.2f / terrains.prefabData.Length;
                ProgressBar("Loading: " + loadPath, "Spawning Prefabs: " + i + " / " + terrains.prefabData.Length, progressValue + 0.8f);
                SpawnPrefab(PrefabManager.LoadPrefab(terrains.prefabData[i].id), terrains.prefabData[i], prefabsParent);
            }
        }
        else
        {
            for (int i = 0; i < terrains.prefabData.Length; i++)
            {
                progressValue += 0.2f / terrains.prefabData.Length;
                ProgressBar("Loading: " + loadPath, "Spawning Prefabs: " + i + " / " + terrains.prefabData.Length, progressValue + 0.8f);
                SpawnPrefab(defaultPrefab, terrains.prefabData[i], prefabsParent);
            }
        }
    }
    /// <summary>
    /// Loads and sets up the map Paths.
    /// </summary>
    /// <param name="terrains"></param>
    static void LoadPaths(MapInfo terrains)
    {
        var terrainPosition = 0.5f * terrains.size;
        Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        GameObject pathNodeObj = Resources.Load<GameObject>("Paths/PathNode");
        ProgressBar("Loading:" + loadPath, "Spawning Paths ", 0.99f);
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
            }
            averageLocation /= terrains.pathData[i].nodes.Length;
            GameObject newObject = GameObject.Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);

            List<GameObject> pathNodes = new List<GameObject>();
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                GameObject newNode = GameObject.Instantiate(pathNodeObj, newObject.transform);
                newNode.transform.position = terrains.pathData[i].nodes[j] + terrainPosition;
                pathNodes.Add(newNode);
            }
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
        }
    }
    static void LoadSplatMaps(MapInfo terrains)
    {
        ProgressBar("Loading: " + loadPath, "Loading Ground Data ", 0.4f);
        TopologyData.InitMesh(terrains.topology);

        ProgressBar("Loading: " + loadPath, "Loading Ground Data ", 0.5f);
        LandData.SetData(terrains.splatMap, "ground");

        ProgressBar("Loading: " + loadPath, "Loading Biome Data ", 0.6f);
        LandData.SetData(terrains.biomeMap, "biome");

        ProgressBar("Loading: " + loadPath, "Loading Alpha Data ", 0.7f);
        LandData.SetData(terrains.alphaMap, "alpha");

        ProgressBar("Loading: " + loadPath, "Loading Topology Data ", 0.8f);
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            LandData.SetData(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(i)), "topology", i);
        }
    }
    /// <summary>
    /// Loads and sets up the map.
    /// </summary>
    static void LoadMapInfo(MapInfo terrains)
    {
        water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        RemoveMapObjects(true, true);
        CentreSceneView();
        CentreSceneObjects(terrains);
        LoadTerrains(terrains);
        LoadSplatMaps(terrains);
        LoadPrefabs(terrains);
        LoadPaths(terrains);
        LandData.SetLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer)); // Sets the Alphamaps to the layer currently selected.
        ClearProgressBar();
    }
    /// <summary>
    /// Loads a WorldSerialization and calls LoadMapInfo.
    /// </summary>
    public static void Load(WorldSerialization world)
    {
        ProgressBar("Loading: " + loadPath, "Loading Land Heightmap Data ", 0.3f);
        LoadMapInfo(WorldConverter.WorldToTerrain(world));
    }
    /// <summary>
    /// Saves the map.
    /// </summary>
    /// <param name="path">The path to save to.</param>
    public static void Save(string path)
    {
        LandData.SaveLayer(TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
        ProgressBar("Saving Map: " + savePath, "Saving Prefabs ", 0.4f);
        WorldSerialization world = WorldConverter.TerrainToWorld(terrain, water);
        ProgressBar("Saving Map: " + savePath, "Saving to disk ", 0.8f);
        world.Save(path);
        ClearProgressBar();
    }
    /// <summary>
    /// Creates a new flat terrain.
    /// </summary>
    /// <param name="size">The size of the terrain.</param>
    public static void CreateNewMap(int size)
    {
        LoadMapInfo(WorldConverter.EmptyMap(size));
        PaintLayer("Alpha", 0);
        PaintLayer("Biome", 1);
        PaintLayer("Ground", 4);
        SetMinimumHeight(503f);
    }
    public static List<string> generationPresetList = new List<string>();
    public static Dictionary<string, UnityEngine.Object> nodePresetLookup = new Dictionary<string, UnityEngine.Object>();
    /// <summary>
    /// Refreshes and adds the new NodePresets in the generationPresetList.
    /// </summary>
    public static void RefreshAssetList()
    {
        var list = AssetDatabase.FindAssets("t:" + NodeAsset.nodeAssetName);
        generationPresetList.Clear();
        nodePresetLookup.Clear();
        foreach (var item in list)
        {
            var itemName = AssetDatabase.GUIDToAssetPath(item).Split('/');
            var itemNameSplit = itemName[itemName.Length - 1].Replace(".asset", "");
            generationPresetList.Add(itemNameSplit);
            nodePresetLookup.Add(itemNameSplit, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(item), typeof(NodePreset)));
        }
    }
}