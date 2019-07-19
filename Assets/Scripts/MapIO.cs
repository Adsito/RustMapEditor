using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using static WorldConverter;
using static WorldSerialization;

[Serializable]
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
[ExecuteAlways]
public class MapIO : MonoBehaviour
{
    #region Layers
    public static TerrainTopology.Enum topologyLayerFrom, topologyLayerToPaint, topologyLayer, conditionalTopology, topologyLayersList, oldTopologyLayer;
    public static TerrainSplat.Enum groundLayerFrom, groundLayerToPaint, terrainLayer, conditionalGround;
    public static TerrainBiome.Enum biomeLayerFrom, biomeLayerToPaint, biomeLayer, conditionalBiome;
    #endregion
    public static int landSelectIndex = 0;
    public static string landLayer = "Ground", loadPath = "", savePath = "", prefabSavePath = "", bundleFile = "No bundle file selected";
    private static PrefabLookup prefabLookup;
    public static float progressBar = 0f, progressValue = 1f;
    public static Dictionary<uint, GameObject> prefabsLoaded = new Dictionary<uint, GameObject>();
    public static Dictionary<string, GameObject> prefabReference = new Dictionary<string, GameObject>();
    public static Texture terrainFilterTexture;
    public static Vector2 heightmapCentre = new Vector2(0.5f, 0.5f);
    private static Terrain terrain;
    #region Editor Input Manager
    [InitializeOnLoadMethod]
    static void EditorInit()
    {
        System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

        value += EditorGlobalKeyPress;

        info.SetValue(null, value);
    }
    static void EditorGlobalKeyPress()
    {
        //Debug.Log("KEY CHANGE " + Event.current.keyCode);
    }
    #endregion
    public void Start()
    {
        loadPath = "";
        terrainFilterTexture = Resources.Load<Texture>("Textures/Brushes/White128");
        terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        RefreshAssetList(); // Refresh the auto gen asset presets.
        GetProjectPrefabs(); // Get all the prefabs saved into the project to a dictionary to reference.
        CentreSceneView(); // Centres the sceneview camera over the middle of the map on project open.
        SetLayers(); // Resets all the layers to default values.
    }
    public static void CentreSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.pivot = new Vector3(500f, 1000f, 500f);
            sceneView.LookAt(new Vector3(500f, 750f, 500f));
        }
    }
    public static void SetLayers()
    {
        topologyLayerFrom = TerrainTopology.Enum.Beach;
        topologyLayerToPaint = TerrainTopology.Enum.Beach;
        groundLayerFrom = TerrainSplat.Enum.Grass;
        groundLayerToPaint = TerrainSplat.Enum.Grass;
        biomeLayerFrom = TerrainBiome.Enum.Temperate;
        biomeLayerToPaint = TerrainBiome.Enum.Temperate;
        topologyLayer = TerrainTopology.Enum.Beach;
        conditionalTopology = (TerrainTopology.Enum)TerrainTopology.NOTHING;
        topologyLayersList = TerrainTopology.Enum.Beach;
        oldTopologyLayer = TerrainTopology.Enum.Beach;
        biomeLayer = TerrainBiome.Enum.Temperate;
        conditionalBiome = (TerrainBiome.Enum)TerrainBiome.NOTHING;
        terrainLayer = TerrainSplat.Enum.Grass;
        conditionalGround = (TerrainSplat.Enum)TerrainSplat.NOTHING;
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
        MapIO.progressBar = 0;
        EditorUtility.ClearProgressBar();
    }
    public static void SetPrefabLookup(PrefabLookup prefabLookup)
    {
        MapIO.prefabLookup = prefabLookup;
    }
    public static void GetProjectPrefabs()
    {
        prefabsLoaded.Clear();
        foreach (var asset in AssetDatabase.GetAllAssetPaths())
        {
            if (asset.EndsWith(".prefab"))
            {
                GameObject loadedAsset = AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject)) as GameObject;
                if (loadedAsset != null)
                {
                    if (loadedAsset.GetComponent<PrefabDataHolder>() != null)
                    {
                        prefabsLoaded.Add(loadedAsset.GetComponent<PrefabDataHolder>().prefabData.id, loadedAsset);
                    }
                }
            }
        }
    }
    public static PrefabLookup GetPrefabLookUp()
    {
        return prefabLookup;
    }
    /// <summary>
    /// Change the active land layer.
    /// </summary>
    /// <param name="layer">The LandLayer to change to. (Ground, Biome, Alpha & Topology)</param>
    public static void ChangeLayer(string layer)
    {
        landLayer = layer;
        ChangeLandLayer();
    }
    public static void ChangeLandLayer()
    {
        LandData.Save(TerrainTopology.TypeToIndex((int)oldTopologyLayer));
        Undo.ClearAll();
        switch (landLayer.ToLower())
        {
            case "ground":
                LandData.SetLayer("ground");
                break;
            case "biome":
                LandData.SetLayer("biome");
                break;
            case "alpha":
                LandData.SetLayer("alpha");
                break;
            case "topology":
                LandData.SetLayer("topology", TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
        }
    }
    public static GameObject SpawnPrefab(GameObject g, PrefabData prefabData, Transform parent = null)
    {
        Vector3 pos = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        Vector3 scale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        Quaternion rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(g);
        newObj.transform.parent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        newObj.transform.position = pos + GetMapOffset();
        newObj.transform.rotation = rotation;
        newObj.transform.localScale = scale;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        return newObj;
    }
    /// <summary>
    /// Removes all prefabs and path objects in the scene.
    /// </summary>
    private static void CleanUpMap()
    {
        GameObject mapPrefabs = GameObject.Find("Objects");
        foreach (PrefabDataHolder g in mapPrefabs.GetComponentsInChildren<PrefabDataHolder>())
        {
            DestroyImmediate(g.gameObject);
        }
        foreach (PathDataHolder g in mapPrefabs.GetComponentsInChildren<PathDataHolder>())
        {
            DestroyImmediate(g.gameObject);
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
    #region RotateMap Methods
    /// <summary>
    /// Rotates Terrain Map and Water Map 90°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public void RotateHeightmap(bool CW)
    {
        terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
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
    /// Rotates prefabs 90°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public void RotatePrefabs(bool CW)
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
    public void RotatePaths(bool CW)
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
    /// <summary>
    /// Rotates the selected layer 90°.
    /// </summary>
    /// <param name="landLayer">The LandLayer. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void RotateLayer(string landLayer, bool CW, int topology = 0)
    {
        int textureCount = TextureCount(landLayer);
        float[,,] oldLayer = GetSplatMap(landLayer, topology);
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
        LandData.SetData(newLayer, landLayer, topology);
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Rotates all Topology layers 90°.
    /// </summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public void RotateAllTopologymap(bool CW)
    {
        progressValue = 1f / TerrainTopology.COUNT;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Rotating Map", "Rotating " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i) + " Topology", progressBar);
            RotateLayer("topology", CW, i);
        }
        ClearProgressBar();
    }
    #endregion
    #region HeightMap Methods
    /// <summary>
    /// Inverts the HeightMap.
    /// </summary>
    public void InvertHeightmap()
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Invert Terrain");
        float[,] landHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        for (int i = 0; i < landHeightMap.GetLength(0); i++)
        {
            for (int j = 0; j < landHeightMap.GetLength(1); j++)
            {
                landHeightMap[i, j] = 1 - landHeightMap[i, j];
            }
        }
        terrain.terrainData.SetHeights(0, 0, landHeightMap);
    }
    /// <summary>
    /// Normalises the HeightMap between two heights.
    /// </summary>
    /// <param name="normaliseLow">The lowest height the HeightMap should be.</param>
    /// <param name="normaliseHigh">The highest height the HeightMap should be.</param>
    public void NormaliseHeightmap(float normaliseLow, float normaliseHigh)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Normalise Terrain");
        float[,] landHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        float highestPoint = 0f, lowestPoint = 1f, currentHeight = 0f, heightRange = 0f, normalisedHeightRange = 0f, normalisedHeight = 0f;
        for (int i = 0; i < landHeightMap.GetLength(0); i++)
        {
            for (int j = 0; j < landHeightMap.GetLength(1); j++)
            {
                currentHeight = landHeightMap[i, j];
                if (currentHeight < lowestPoint)
                {
                    lowestPoint = currentHeight;
                }
                else if (currentHeight > highestPoint)
                {
                    highestPoint = currentHeight;
                }
            }
        }
        heightRange = highestPoint - lowestPoint;
        normalisedHeightRange = normaliseHigh - normaliseLow;
        for (int i = 0; i < landHeightMap.GetLength(0); i++)
        {
            for (int j = 0; j < landHeightMap.GetLength(1); j++)
            {
                normalisedHeight = ((landHeightMap[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                landHeightMap[i, j] = normaliseLow + normalisedHeight;
            }
        }
        terrain.terrainData.SetHeights(0, 0, landHeightMap);
    }
    /// <summary>
    /// Terraces the HeightMap.
    /// </summary>
    /// <param name="featureSize">The height of each terrace.</param>
    /// <param name="interiorCornerWeight">The weight of the terrace effect.</param>
    public void TerraceErodeHeightmap(float featureSize, float interiorCornerWeight)
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
    public void SmoothHeightmap(float filterStrength, float blurDirection)
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
    public void SetEdgePixel(float heightToSet, bool[] sides)
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
    public void OffsetHeightmap(float offset, bool checkHeight, bool setWaterMap)
    {
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        offset = offset / 1000f;
        bool heightOutOfRange = false;
        for (int i = 0; i < terrain.terrainData.heightmapHeight; i++)
        {
            for (int j = 0; j < terrain.terrainData.heightmapWidth; j++)
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
    public void DebugWaterLevel()
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
    /// <summary>
    /// Sets the HeightMap level to the minimum if it's below.
    /// </summary>
    /// <param name="minimumHeight">The minimum height to set.</param>
    public void SetMinimumHeight(float minimumHeight)
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
    public void SetMaximumHeight(float maximumHeight)
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
    public float GetHeight(int x, int z)
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
    public float[,] GetHeights()
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
    public float GetSlope(int x, int z)
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
    public float[,] GetSlopes()
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
    /// Returns the enums selected in the corresponding TerrainLayer enum group.
    /// </summary>
    /// <param name="ground">The TerrainSplat Enum to parse.</param>
    /// <returns></returns>
    public static List<int> ReturnSelectedElements(TerrainSplat.Enum ground)
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainSplat.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)ground & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }
        return selectedElements;
    }
    /// <summary>
    /// Returns the enums selected in the corresponding TerrainLayer enum group.
    /// </summary>
    /// <param name="biome">The TerrainBiome Enum to parse.</param>
    /// <returns></returns>
    public static List<int> ReturnSelectedElements(TerrainBiome.Enum biome)
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainBiome.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)biome & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }
        return selectedElements;
    }
    /// <summary>
    /// Returns the enums selected in the corresponding TerrainLayer enum group.
    /// </summary>
    /// <param name="topology">The TerrainTopology Enum to parse.</param>
    /// <returns></returns>
    public static List<int> ReturnSelectedElements(TerrainTopology.Enum topology)
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainTopology.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)topology & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }
        return selectedElements;
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
    /// Paints if all the conditions passed in are true.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to paint.</param>
    /// <param name="conditions">The conditions to check.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void PaintConditional(string landLayerToPaint, int texture, Conditions conditions, int topology = 0)
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
        foreach (var topologyLayerInt in ReturnSelectedElements(conditions.TopologyLayers))
        {
            topologyLayersList.Add(new TopologyLayers()
            {
                Topologies = GetSplatMap("topology", topologyLayerInt)
            });
        }
        foreach (var groundTextureInt in ReturnSelectedElements(conditions.GroundConditions))
        {
            groundTexturesList.Add(new GroundTextures()
            {
                Texture = groundTextureInt
            });
        }
        foreach (var biomeTextureInt in ReturnSelectedElements(conditions.BiomeConditions))
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
        LandData.SetLayer(landLayerToPaint, topology);
    }
    /// <summary>
    /// Paints the layer wherever the height conditions are met. Includes option to blend.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="minBlendLow">The minimum height to start to paint. The texture weight will increase as it gets closer to the heightlow.</param>
    /// <param name="maxBlendHigh">The maximum height to start to paint. The texture weight will increase as it gets closer to the heighthigh.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void PaintHeight(string landLayerToPaint, float heightLow, float heightHigh, float minBlendLow, float maxBlendHigh, int t, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < (float)splatMap.GetLength(1); j++)
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
                else if (height >= minBlendLow && height <= heightLow)
                {
                    float normalisedHeight = height - minBlendLow;
                    float heightRange = heightLow - minBlendLow;
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
                else if (height >= heightHigh && height <= maxBlendHigh)
                {
                    float normalisedHeight = height - heightHigh;
                    float heightRange = maxBlendHigh - heightHigh;
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
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Sets whole layer to the active texture. 
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void PaintLayer(string landLayerToPaint, int t, int topology = 0)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Layer");
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
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Sets whole layer to the inactive texture. Alpha and Topology only. 
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void ClearLayer(string landLayerToPaint, int topology = 0)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Clear Layer");
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
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Clears all the topology layers.
    /// </summary>
    public void ClearAllTopologyLayers()
    {
        progressValue = 1f / TerrainTopology.COUNT;
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
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void InvertLayer(string landLayerToPaint, int topology = 0)
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
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Inverts all the Topology layers.
    /// </summary>
    public void InvertAllTopologyLayers()
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
    /// <param name="minBlendLow">The minimum slope to start to paint. The texture weight will increase as it gets closer to the slopeLow.</param>
    /// <param name="maxBlendHigh">The maximum slope to start to paint. The texture weight will increase as it gets closer to the slopeHigh.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void PaintSlope(string landLayerToPaint, float slopeLow, float slopeHigh, float minBlendLow, float maxBlendHigh, int t, int topology = 0) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
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
                else if (slope >= minBlendLow && slope <= slopeLow)
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
                else if (slope >= slopeHigh && slope <= maxBlendHigh)
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
        LandData.SetData(splatMap, landLayerToPaint, topology);
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Paints area within these splatmap coords, Maps will always have a splatmap resolution between 512 - 2048 resolution, to the nearest Power of Two (512, 1024, 2048).
    /// Face downright in the editor with Z axis facing up, and X axis facing right, and the map will draw from the bottom left corner, up to the top right. 
    /// Note that the results of how much of the map is covered is dependant on the map size, a 2000 map size would paint almost the bottom half of the map,
    /// whereas a 4000 map would paint up nearly one quarter of the map, and across nearly half of the map.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void PaintArea(string landLayerToPaint, int z1, int z2, int x1, int x2, int t, int topology = 0)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Area");
        float[,,] splatMap = GetSplatMap(landLayerToPaint, topology);
        int textureCount = TextureCount(landLayerToPaint);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if ((i >= z1 && i <= z2) && (j >= x1 && j <= x2))
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
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Paints the splats wherever the water is above 500 and is above the terrain.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="aboveTerrain">Check if the watermap is above the terrain before painting.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public void PaintRiver(string landLayerToPaint, bool aboveTerrain, int t, int topology = 0)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint River");
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
        LandData.SetLayer(landLayer, topology);
    }
    /// <summary>
    /// Paints a ground texture to the corresponding coordinate if the alpha is active. 
    /// Used for debugging the floating ground clutter that occurs when you have a ground splat of either Grass or Forest ontop of an active alpha layer.
    /// </summary>
    public void AlphaDebug()
    {
        float[,,] splatMap = GetSplatMap("ground");
        float[,,] alphaSplatMap = GetSplatMap("alpha");
        int textureCount = TextureCount("ground");
        for (int i = 0; i < alphaSplatMap.GetLength(0); i++)
        {
            for (int j = 0; j < alphaSplatMap.GetLength(1); j++)
            {
                if (alphaSplatMap[i, j, 1] == 1)
                {
                    for (int k = 0; k < textureCount; k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, 3] = 1; // This paints the rock layer. Where 3 = the layer to paint.
                }
            }
        }
        LandData.SetData(splatMap, landLayer);
        LandData.SetLayer(landLayer);
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
    public void CopyTexture(string landLayerFrom, string landLayerToPaint, int textureFrom, int textureToPaint, int topologyFrom = 0, int topologyToPaint = 0)
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
        LandData.SetLayer(landLayer, topologyToPaint);
        ClearProgressBar();
    }
    #endregion
    /// <summary>
    /// ToDo: Read from a text file instead of having a switch.
    /// </summary>
    public static void RemoveBrokenPrefabs()
    {
        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
        Undo.RegisterCompleteObjectUndo(prefabs, "Remove Broken Prefabs");
        var prefabsRemovedCount = 0;
        foreach (PrefabDataHolder p in prefabs)
        {
            switch (p.prefabData.id)
            {
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
                        DestroyImmediate(p.gameObject);
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
    private static void LoadMapInfo(MapInfo terrains)
    {
        var worldCentrePrefab = GameObject.FindGameObjectWithTag("Prefabs");
        worldCentrePrefab.transform.position = new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2);
        var worldCentrePath = GameObject.FindGameObjectWithTag("Paths");
        worldCentrePath.transform.position = new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2);
        var worldCentreMapIO = GameObject.FindGameObjectWithTag("MapIO");
        worldCentreMapIO.transform.position = new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2);
        CleanUpMap();
        CentreSceneView();

        var terrainPosition = 0.5f * terrains.size;
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();

        terrain.transform.position = terrainPosition;
        water.transform.position = terrainPosition;

        ProgressBar("Loading: " + loadPath, "Loading Ground Data ", 0.4f);
        TopologyMesh.InitMesh(terrains.topology);

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

        terrain.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);
        water.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);

        ProgressBar("Loading: " + loadPath, "Loading Ground Data ", 0.5f);
        LandData.SetData(terrains.splatMap, "ground");

        ProgressBar("Loading: " + loadPath, "Loading Biome Data ", 0.6f);
        LandData.SetData(terrains.biomeMap, "biome");

        ProgressBar("Loading: " + loadPath, "Loading Alpha Data ", 0.7f);
        LandData.SetData(terrains.alphaMap, "alpha");

        ProgressBar("Loading: " + loadPath, "Loading Topology Data ", 0.8f);
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            LandData.SetData(TopologyMesh.getSplatMap(TerrainTopology.IndexToType(i)), "topology", i);
        }
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        ProgressBar("Loading: " + loadPath, "Spawning Prefabs ", 0.9f);
        float progressValue = 0f;
        for (int i = 0; i < terrains.prefabData.Length; i++)
        {
            progressValue += 0.1f / terrains.prefabData.Length;
            ProgressBar("Loading: " + loadPath, "Spawning Prefabs: " + i + " / " + terrains.prefabData.Length, progressValue + 0.9f);
            if (prefabsLoaded.TryGetValue(terrains.prefabData[i].id, out GameObject newObj))
            {
                newObj = SpawnPrefab(prefabsLoaded[terrains.prefabData[i].id], terrains.prefabData[i], prefabsParent);
            }
            else
            {
                newObj = SpawnPrefab(defaultObj, terrains.prefabData[i], prefabsParent);
            }
        }
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
            GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);

            List<GameObject> pathNodes = new List<GameObject>();
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                GameObject newNode = Instantiate(pathNodeObj, newObject.transform);
                newNode.transform.position = terrains.pathData[i].nodes[j] + terrainPosition;
                pathNodes.Add(newNode);
            }
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
        }
        // Will clean this up later, without the below the topology layer last selected is not saved to the array properly, and if ground selected it shows the topology until
        // the layer is swapped.
        ChangeLayer("Ground");
        LandData.SetData(TopologyMesh.getSplatMap((int)topologyLayer), "topology", TerrainTopology.TypeToIndex((int)topologyLayer));
        LandData.SetLayer("topology", TerrainTopology.TypeToIndex((int)topologyLayer));
        ChangeLayer("Ground");
        ClearProgressBar();
    }
    public static void Load(WorldSerialization blob)
    {
        WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
        MapIO.ProgressBar("Loading: " + loadPath, "Loading Land Heightmap Data ", 0.3f);
        LoadMapInfo(terrains);
    }
    /// <summary>
    /// Saves the map.
    /// </summary>
    /// <param name="path">The path to save to.</param>
    public void Save(string path)
    {
        LandData.Save(TerrainTopology.TypeToIndex((int)topologyLayer));
        foreach (var item in GameObject.FindGameObjectWithTag("World").GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.SetActive(true);
        }
        Terrain terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        ProgressBar("Saving Map: " + savePath, "Saving Watermap ", 0.25f);
        ProgressBar("Saving Map: " + savePath, "Saving Prefabs ", 0.4f);
        WorldSerialization world = WorldConverter.terrainToWorld(terrain, water);
        ProgressBar("Saving Map: " + savePath, "Saving Layers ", 0.6f);
        world.Save(path);
        ProgressBar("Saving Map: " + savePath, "Saving to disk ", 0.8f);
        ClearProgressBar();
    }
    /// <summary>
    /// Creates a new flat terrain.
    /// </summary>
    /// <param name="size">The size of the terrain.</param>
    public void NewEmptyTerrain(int size)
    {
        LoadMapInfo(WorldConverter.emptyWorld(size));
        progressValue = 1f / TerrainTopology.COUNT;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Creating New Map", "Wiping: " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i), progressBar);
            ClearLayer("Topology", i);
        }
        ClearLayer("Alpha");
        PaintLayer("Biome", 1);
        PaintLayer("Ground", 4);
        SetMinimumHeight(503f);
        ClearProgressBar();
    }
    public void StartPrefabLookup()
    {
        SetPrefabLookup(new PrefabLookup(bundleFile, this));
    }
    public static List<string> generationPresetList = new List<string>();
    public static Dictionary<string, UnityEngine.Object> nodePresetLookup = new Dictionary<string, UnityEngine.Object>();
    public static void RefreshAssetList()
    {
        var list = AssetDatabase.FindAssets("t:AutoGenerationGraph");
        generationPresetList.Clear();
        nodePresetLookup.Clear();
        foreach (var item in list)
        {
            var itemName = AssetDatabase.GUIDToAssetPath(item).Split('/');
            var itemNameSplit = itemName[itemName.Length - 1].Replace(".asset", "");
            generationPresetList.Add(itemNameSplit);
            nodePresetLookup.Add(itemNameSplit, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(item), typeof(AutoGenerationGraph)));
        }
    }
    /// <summary>
    /// Runs the selected NodeGraph.
    /// </summary>
    /// <param name="graph">The NodeGraph to run.</param>
    public static void ParseNodeGraph(XNode.NodeGraph graph)
    {
        foreach (var node in graph.nodes)
        {
            if (node.name == "Start")
            {
                if (node.GetOutputPort("NextTask").GetConnections().Count == 0) // Check for start node being in graph but not linked.
                {
                    return;
                }
                XNode.Node nodeIteration = node.GetOutputPort("NextTask").Connection.node;
                if (nodeIteration != null)
                {
                    do
                    {
                        MethodInfo runNode = nodeIteration.GetType().GetMethod("RunNode");
                        runNode.Invoke(nodeIteration, null);
                        if (nodeIteration.GetOutputPort("NextTask").IsConnected)
                        {
                            nodeIteration = nodeIteration.GetOutputPort("NextTask").Connection.node;
                        }
                        else
                        {
                            nodeIteration = null;
                        }
                    }
                    while (nodeIteration != null);
                    MapIO.ChangeLandLayer(); // Puts the layer back to the one selected in MapIO LandLayer.
                }
            }
        }
    }
}
public class PrefabHierachy : TreeView
{
    MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

    public PrefabHierachy(TreeViewState treeViewState)
        : base(treeViewState)
    {
        Reload();
    }
    protected override bool CanStartDrag(CanStartDragArgs args)
    {
        return true;
    }
    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        DragAndDrop.PrepareStartDrag();
        if (args.draggedItemIDs[0] > 100) // Only drag prefabs not the treeview parents.
        {
            if (MapIO.prefabReference.ContainsKey(args.draggedItemIDs[0].ToString()))
            {
                Debug.Log("Prefab Found");
                MapIO.prefabReference.TryGetValue(args.draggedItemIDs[0].ToString(), out GameObject prefabDragged);
                GameObject newPrefab = prefabDragged;
                UnityEngine.Object[] objectArray = new UnityEngine.Object[1];
                objectArray[0] = newPrefab;
                DragAndDrop.objectReferences = objectArray;
                DragAndDrop.StartDrag("Prefab");
            }
            else
            {
                Debug.Log("Prefab not found.");
            }
        }
    }
    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        if (args.performDrop)
        {
            Debug.Log("Dropped");
            return DragAndDropVisualMode.Copy;
        }
        return DragAndDropVisualMode.None;
    }
    Dictionary<string, TreeViewItem> treeviewParents = new Dictionary<string, TreeViewItem>();
    List<TreeViewItem> allItems = new List<TreeViewItem>();
    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        allItems.Add(new TreeViewItem { id = 1, depth = 0, displayName = "Editor Tools" });
        // Add other editor shit like custom prefabs here.
        if (File.Exists("PrefabsLoaded.txt"))
        {
            var lines = File.ReadAllLines("PrefabsLoaded.txt");
            var parentId = -1000000; // Set this really low so it doesn't ever go into the positives or otherwise run into the risk of being the same id as a prefab.
            foreach (var line in lines)
            {
                var linesSplit = line.Split(':');
                var assetNameSplit = linesSplit[1].Split('/');
                for (int i = 0; i < assetNameSplit.Length; i++)
                {
                    var treePath = "";
                    for (int j = 0; j <= i; j++)
                    {
                        treePath += assetNameSplit[j];
                    }
                    if (!treeviewParents.ContainsKey(treePath))
                    {
                        var prefabName = assetNameSplit[assetNameSplit.Length - 1].Replace(".prefab", "");
                        var shortenedId = linesSplit[2].Substring(2);
                        if (i != assetNameSplit.Length - 1)
                        {
                            var treeviewItem = new TreeViewItem { id = parentId, depth = i, displayName = assetNameSplit[i] };
                            allItems.Add(treeviewItem);
                            treeviewParents.Add(treePath, treeviewItem);
                            parentId++;
                        }
                        else
                        {
                            var treeviewItem = new TreeViewItem { id = int.Parse(shortenedId), depth = i, displayName = prefabName };
                            allItems.Add(treeviewItem);
                            treeviewParents.Add(treePath, treeviewItem);
                        }
                    }
                }
            }
        }
        SetupParentsAndChildrenFromDepths(root, allItems);
        return root;
    }
}