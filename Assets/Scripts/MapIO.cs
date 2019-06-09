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
    public string PrefabProperty
    {
        get; set;
    }
}
public class TopologyLayers : List<TopologyLayers>
{
    public float[,,] Topologies
    {
        get; set;
    }
}
public class GroundTextures : List<GroundTextures>
{
    public int Textures
    {
        get; set;
    }
}
public class BiomeTextures : List<BiomeTextures>
{
    public int Textures
    {
        get; set;
    }
}
public class Conditions : List<Conditions>
{
    public string[] LandLayers
    {
        get; set;
    }
    public TerrainTopology.Enum TopologyLayers
    {
        get; set;
    }
    public bool[] AlphaTextures
    {
        get; set;
    }
    public bool[] TopologyTextures
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
    public TerrainTopology.Enum conditionalTopology;
    public TerrainTopology.Enum topologyLayersList;
    public TerrainTopology.Enum oldTopologyLayer;
    public TerrainTopology.Enum oldTopologyLayer2;
    public TerrainBiome.Enum biomeLayer;
    public TerrainBiome.Enum conditionalBiome;
    public TerrainSplat.Enum terrainLayer;
    public TerrainSplat.Enum conditionalGround;
    public int landSelectIndex = 0;
    public string landLayer = "Ground", loadPath = "", savePath = "", prefabSavePath = "";
    LandData landData;
    private PrefabLookup prefabLookup;
    public float progressBar = 0f;
    static TopologyMesh topology;
    float progressValue = 1f;
    private Dictionary<uint, string> prefabNames = new Dictionary<uint, string>();
    private Dictionary<uint, string> prefabPaths = new Dictionary<uint, string>();
    public Dictionary<uint, GameObject> prefabsLoaded = new Dictionary<uint, GameObject>();
    public Dictionary<string, GameObject> prefabReference = new Dictionary<string, GameObject>();
    public string bundleFile = "No bundle file selected";
    public Texture terrainFilterTexture;
    public static Vector2 heightmapCentre = new Vector2(0.5f, 0.5f);
    private Terrain terrain;
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
    void Start()
    {
        loadPath = "";
        terrainFilterTexture = Resources.Load<Texture>("Textures/Brushes/White128");
        terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        landData = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>();
        RefreshAssetList(); // Refresh the auto gen asset presets.
        GetProjectPrefabs(); // Get all the prefabs saved into the project to a dictionary to reference.
        CentreSceneView(); // Centres the sceneview camera over the middle of the map on project open.
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
    public static void ProgressBar(string title, string info, float progress)
    {
        EditorUtility.DisplayProgressBar(title, info, progress);
    }
    public static void ClearProgressBar()
    {
        GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>().progressBar = 0;
        EditorUtility.ClearProgressBar();
    }
    public void setPrefabLookup(PrefabLookup prefabLookup)
    {
        this.prefabLookup = prefabLookup;
    }
    public void GetProjectPrefabs()
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
    public PrefabLookup getPrefabLookUp()
    {
        return prefabLookup;
    }
    public void ChangeLayer(string layer)
    {
        landLayer = layer;
        ChangeLandLayer();
    }
    public void ChangeLandLayer()
    {
        if (landData != null)
        {
            landData.save(TerrainTopology.TypeToIndex((int)oldTopologyLayer));
        }
        Undo.ClearAll();
        landData = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>();
        switch (landLayer.ToLower())
        {
            case "ground":
                landData.setLayer("ground");
                break;
            case "biome":
                landData.setLayer("biome");
                break;
            case "alpha":
                landData.setLayer("alpha");
                break;
            case "topology":
                landData.setLayer("topology", TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
        }
    }
    public void GetPrefabNames()
    {
        if (File.Exists("PrefabsLoaded.txt"))
        {
            var lines = File.ReadAllLines("PrefabsLoaded.txt");
            foreach (var line in lines)
            {
                var linesSplit = line.Split(':');
                prefabNames.Add(uint.Parse(linesSplit[linesSplit.Length - 1]), linesSplit[0]);
            }
        }
    }
    public void GetPrefabPaths()
    {
        if (File.Exists("PrefabsLoaded.txt"))
        {
            var lines = File.ReadAllLines("PrefabsLoaded.txt");
            foreach (var line in lines)
            {
                var linesSplit = line.Split(':');
                prefabPaths.Add(uint.Parse(linesSplit[linesSplit.Length - 1]), linesSplit[1]);
            }
        }
    }
    public GameObject SpawnPrefab(GameObject g, PrefabData prefabData, Transform parent = null)
    {
        Vector3 pos = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        Vector3 scale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        Quaternion rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(g);
        newObj.transform.parent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        newObj.transform.position = pos + getMapOffset();
        newObj.transform.rotation = rotation;
        newObj.transform.localScale = scale;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        newObj.GetComponent<PrefabDataHolder>().saveWithMap = true;
        prefabNames.TryGetValue(prefabData.id, out string prefabName); // Sets the prefab name to the string if the user has previously loaded the game bundles.
        if (prefabName != null)
        {
            newObj.name = prefabName;
        }
        return newObj;
    }
    private void CleanUpMap()
    {
        landData = null;
        GameObject mapPrefabs = GameObject.Find("Objects");
        foreach(PrefabDataHolder g in mapPrefabs.GetComponentsInChildren<PrefabDataHolder>())
        {
            DestroyImmediate(g.gameObject);
        }
        foreach (PathDataHolder g in mapPrefabs.GetComponentsInChildren<PathDataHolder>())
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
        return 0.5f * getTerrainSize();
    }
    #region RotateMap Methods
    public void RotateHeightmap(bool CW) //Rotates Terrain Map and Water Map 90°.
    {
        terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();

        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);

        if (CW)
        {
            terrain.terrainData.SetHeights(0, 0, MapTransformations.rotateCW(heightMap));
            water.terrainData.SetHeights(0, 0, MapTransformations.rotateCW(waterMap));
        }
        else
        {
            terrain.terrainData.SetHeights(0, 0, MapTransformations.rotateCCW(heightMap));
            water.terrainData.SetHeights(0, 0, MapTransformations.rotateCCW(waterMap));
        }
    }
    public void RotatePrefabs(bool CW) //Needs prefabs in scene to be all at Vector3.Zero to work. Rotates objects 90.
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
    public void RotatePaths(bool CW) //Needs prefabs in scene to be all at Vector3.Zero to work. Rotates objects 90.
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
    public void RotateGroundmap(bool CW) //Rotates Groundmap 90 degrees for CW true.
    {
        float[,,] oldGround = landData.groundArray;
        float[,,] newGround = new float[oldGround.GetLength(0), oldGround.GetLength(1), 8];
        if (CW)
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        newGround[i, j, k] = oldGround[j, oldGround.GetLength(1) - i - 1, k];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        newGround[i, j, k] = oldGround[oldGround.GetLength(0) - j - 1, i, k];
                    }
                }
            }
        }
        landData.setData(newGround, "ground");
        landData.setLayer(landLayer);
    }
    public void RotateBiomemap(bool CW) //Rotates Biomemap 90 degrees for CW true.
    {
        float[,,] oldBiome = landData.biomeArray;
        float[,,] newBiome = new float[oldBiome.GetLength(0), oldBiome.GetLength(1), 4];
        if (CW)
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        newBiome[i, j, k] = oldBiome[j, oldBiome.GetLength(1) - i - 1, k];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        newBiome[i, j, k] = oldBiome[oldBiome.GetLength(0) - j - 1, i, k];
                    }
                }
            }
        }
        landData.setData(newBiome, "biome");
        landData.setLayer(landLayer);
    }
    public void RotateAlphamap(bool CW) //Rotates Alphamap 90 degrees for CW true.
    {
        float[,,] oldAlpha = landData.alphaArray;
        float[,,] newAlpha = new float[oldAlpha.GetLength(0), oldAlpha.GetLength(1), 2];
        if (CW)
        {
            for (int i = 0; i < newAlpha.GetLength(0); i++)
            {
                for (int j = 0; j < newAlpha.GetLength(1); j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        newAlpha[i, j, k] = oldAlpha[j, oldAlpha.GetLength(1) - i - 1, k];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < newAlpha.GetLength(0); i++)
            {
                for (int j = 0; j < newAlpha.GetLength(1); j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        newAlpha[i, j, k] = oldAlpha[oldAlpha.GetLength(0) - j - 1, i, k];
                    }
                }
            }
        }
        landData.setData(newAlpha, "alpha");
        landData.setLayer(landLayer);
    }
    public void RotateTopologymap(bool CW, int topology = 0) //Rotates Topology map 90 degrees for CW true.
    {
        float[,,] oldTopology = landData.topologyArray[topology];
        float[,,] newTopology = new float[oldTopology.GetLength(0), oldTopology.GetLength(1), 2];
        if (CW)
        {
            for (int i = 0; i < newTopology.GetLength(0); i++)
            {
                for (int j = 0; j < newTopology.GetLength(1); j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        newTopology[i, j, k] = oldTopology[j, oldTopology.GetLength(1) - i - 1, k];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < newTopology.GetLength(0); i++)
            {
                for (int j = 0; j < newTopology.GetLength(1); j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        newTopology[i, j, k] = oldTopology[oldTopology.GetLength(0) - j - 1, i, k];
                    }
                }
            }
        }
        landData.setData(newTopology, "topology", topology);
        landData.setLayer(landLayer, topology);
    }
    public void RotateAllTopologymap(bool CW) //Rotates All Topology maps 90 degrees for CW true.
    {
        float[,,] newTopology = landData.topologyArray[0];
        float[,,] oldTopology = landData.topologyArray[0];
        progressValue =  1f / TerrainTopology.COUNT;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ProgressBar("Rotating Map", "Rotating " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i) + " Topology", progressBar);
            RotateTopologymap(CW, i);
        }
        ClearProgressBar();
    }
    #endregion
    #region HeightMap Methods
    public void scaleHeightmap(float scale)
    {
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        float[,] landHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        float[,] waterHeightMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        terrain.terrainData.SetHeights(0, 0, MapTransformations.scale(landHeightMap, scale));
        water.terrainData.SetHeights(0, 0, MapTransformations.scale(waterHeightMap, scale));
    }
    public void InvertHeightmap()
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Invert Terrain");
        float[,] landHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        terrain.terrainData.SetHeights(0, 0, MapTransformations.Invert(landHeightMap));
    }
    public void transposeHeightmap()
    {
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        float[,] landHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        float[,] waterHeightMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        terrain.terrainData.SetHeights(0, 0, MapTransformations.transpose(landHeightMap));
        water.terrainData.SetHeights(0, 0, MapTransformations.transpose(waterHeightMap));
    }
    public void NormaliseHeightmap(float normaliseLow, float normaliseHigh, float normaliseBlend)
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
    public void moveHeightmap()
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Move Terrain");
        Vector3 difference = terrain.transform.position;
        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        for (int i = 0; i < heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < heightMap.GetLength(1); j++)
            {
                heightMap[i, j] = heightMap[i, j] + (difference.y / terrain.terrainData.size.y);
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
        terrain.transform.position = Vector3.zero;
    }
    public void setEdgePixel(float heightToSet, bool[] sides) // Sets the very edge pixel of the heightmap to the heightToSet value. Includes toggle
    // option for sides.
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
    public void generatePerlinHeightmap(float scale) // Extremely basic first run of perlin map gen. In future this will have roughly 15 controllable elements.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Perlin Terrain");
        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);

        for (int i = 0; i < terrain.terrainData.heightmapHeight; i++)
        {
            for (int j = 0; j < terrain.terrainData.heightmapWidth; j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp01(Mathf.PerlinNoise(i2, j2));
                heightMap[i, j] = perlin;
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }
    public void offsetHeightmap(float offset, bool checkHeight, bool setWaterMap) // Increases or decreases the heightmap by the offset. Useful for moving maps up or down in the scene if the heightmap
    // isn't at the right height. If checkHeight is enabled it will make sure that the offset does not flatten a part of the map because it hits the floor or ceiling.
    // If setWaterMap is enabled it will offset the water map as well, however if this goes below 500 the watermap will be broken.
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
    public void setMaximumHeight(float maximumHeight) // Puts the heightmap level to the minimum if it's below.
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
    public float getHeight(int x, int y)
    {
        float xNorm = (float)x / (float)terrain.terrainData.alphamapHeight;
        float yNorm = (float)y / (float)terrain.terrainData.alphamapHeight;
        float height = terrain.terrainData.GetInterpolatedHeight(xNorm, yNorm);
        return height;
    }
    public float[,] getHeights()
    {
        float alphamapInterp = 1f / terrain.terrainData.alphamapWidth;
        float[,] heights = terrain.terrainData.GetInterpolatedHeights(0, 0, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapWidth, alphamapInterp, alphamapInterp);
        return heights;
    }
    public float getSlope(int x, int y)
    {
        float xNorm = (float)x / terrain.terrainData.alphamapHeight;
        float yNorm = (float)y / terrain.terrainData.alphamapHeight;
        float slope = terrain.terrainData.GetSteepness(xNorm, yNorm);
        return slope;
    }
    public float[,] getSlopes()
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
    List<int> ReturnSelectedElementsTopology()
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainTopology.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)conditionalTopology & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }
        return selectedElements;
    }
    List<int> ReturnSelectedElementsGround()
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainSplat.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)conditionalGround & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }
        return selectedElements;
    }
    List<int> ReturnSelectedElementsBiome()
    {
        List<int> selectedElements = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(TerrainBiome.Enum)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)conditionalBiome & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }
        return selectedElements;
    }
    public float[,,] GetSplatMap(string landLayer, int topology = 0)
    {
        float[,,] layer = null;
        switch (landLayer.ToLower())
        {
            case "ground":
                layer = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>().groundArray;
                break;
            case "biome":
                layer = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>().biomeArray;
                break;
            case "alpha":
                layer = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>().alphaArray;
                break;
            case "topology":
                layer = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>().topologyArray[topology];
                break;
        }
        return layer;
    }
    public int Texture(string landLayer) // Active texture selected in layer. Call method with a string type of the layer to search. 
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
    public int TextureCount(string landLayer) // Texture count in layer chosen, used for determining the size of the splatmap array.
    // Call method with the layer you are painting to.
    {
        if(landLayer.ToLower() == "ground")
        {
            return 8;
        }
        else if (landLayer == "biome")
        {
            return 4;
        }
        return 2;
    }
    public float GetTexture(string landLayer, int texture, int x, int y, int topology = 0)
    {
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        float returnedTexture = splatMap[x, y, texture];
        return returnedTexture;
    }
    public void PaintConditional(string landLayer, int texture, List<Conditions> conditions, int topology = 0) // Todo: Optimisation and cleanup.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Conditional");

        float[,,] groundSplatMap = GetSplatMap("ground");
        float[,,] biomeSplatMap = GetSplatMap("biome");
        float[,,] alphaSplatMap = GetSplatMap("alpha");
        float[,,] topologySplatMap = GetSplatMap("topology", topology);
        float[,,] splatMapPaint = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight, TextureCount(landLayer)];
        float[,,] splatMapOld = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight, TextureCount(landLayer)];
        bool paint = true;
        float slope, height;
        float[,] heights = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight];
        float[,] slopes = new float[terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapHeight];
        int  alphaTexture = 0, topologyTexture = 0;
        ProgressBar("Conditional Painter FIRST VERSION", "Preparing SplatMaps", 0.025f);
        switch (landLayer)
        {
            case "Ground":
                splatMapOld = groundSplatMap;
                break;
            case "Biome":
                splatMapOld = biomeSplatMap;
                break;
            case "Alpha":
                splatMapOld = alphaSplatMap;
                break;
            case "Topology":
                splatMapOld = topologySplatMap;
                break;
        }
        List<TopologyLayers> topologyLayers = new List<TopologyLayers>();
        List<GroundTextures> groundTexturesList = new List<GroundTextures>();
        List<BiomeTextures> biomeTexturesList = new List<BiomeTextures>();
        ProgressBar("Conditional Painter", "Gathering Conditions", 0.05f);
        foreach (Conditions item in conditions)
        {
            foreach (var topologyLayerInt in ReturnSelectedElementsTopology())
            {
                topologyLayers.Add(new TopologyLayers()
                {
                    Topologies = GetSplatMap("topology", topologyLayerInt)
                });
            }
            foreach (var groundTextureInt in ReturnSelectedElementsGround())
            {
                groundTexturesList.Add(new GroundTextures()
                {
                    Textures = groundTextureInt
                });
            }
            foreach (var biomeTextureInt in ReturnSelectedElementsBiome())
            {
                biomeTexturesList.Add(new BiomeTextures()
                {
                    Textures = biomeTextureInt
                });
            }
            if (item.CheckHeight == true)
            {
                heights = getHeights();
            }
            if (item.CheckSlope == true)
            {
                slopes = getSlopes();
            }
            progressValue = 1f / groundSplatMap.GetLength(0);
            for (int i = 0; i < groundSplatMap.GetLength(0); i++)
            {
                progressBar += progressValue;
                ProgressBar("Conditional Painter", "Checking Conditions", progressBar);
                for (int j = 0; j < groundSplatMap.GetLength(1); j++)
                {
                    paint = true;
                    if (item.CheckSlope == true)
                    {
                        slope = slopes[j, i];
                        if (slope >= item.SlopeLow && slope <= item.SlopeHigh)
                        {
                        }
                        else
                        {
                            paint = false;
                        }
                    }
                    if (item.CheckHeight == true)
                    {
                        height = heights[i, j];
                        if (height >= item.HeightLow && height <= item.HeightHigh)
                        {
                        }
                        else
                        {
                            paint = false;
                        }
                    }
                    foreach (var landLayers in item.LandLayers)
                    {
                        if (paint == true)
                        {
                            switch (landLayers)
                            {
                                case "Ground": 
                                    foreach (GroundTextures groundTextureCheck in groundTexturesList)
                                    {
                                        if (groundSplatMap[i, j, groundTextureCheck.Textures] > 0.5f)
                                        {
                                        }
                                        else
                                        {
                                            paint = false;
                                        }
                                    }
                                    break;
                                case "Biome":
                                    foreach (BiomeTextures biomeTextureCheck in biomeTexturesList)
                                    {
                                        if (biomeSplatMap[i, j, biomeTextureCheck.Textures] > 0.5f)
                                        {
                                        }
                                        else
                                        {
                                            paint = false;
                                        }
                                    }
                                    break;
                                case "Alpha":
                                    foreach (var alphaTextureBool in item.AlphaTextures)
                                    {
                                        if (alphaTextureBool == true)
                                        {
                                            if (alphaSplatMap[i, j, alphaTexture] > 0.5f)
                                            {
                                            }
                                            else
                                            {
                                                paint = false;
                                            }
                                        }
                                    }
                                    break;
                                case "Topology": 
                                    foreach (var topologyTextureBool in item.TopologyTextures)
                                    {
                                        if (topologyTextureBool == true)
                                        {
                                            foreach (TopologyLayers layer in topologyLayers)
                                            {
                                                if (layer.Topologies[i, j, topologyTexture] > 0.5f)
                                                {
                                                }
                                                else
                                                {
                                                    paint = false;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    Debug.Log("Conditional LandLayer not found" + landLayers);
                                    paint = false;
                                    break;
                            }
                        }
                        if (paint == true)
                        {
                            for (int k = 0; k < TextureCount(landLayer); k++)
                            {
                                splatMapPaint[i, j, k] = 0;
                            }
                            splatMapPaint[i, j, texture] = 1f;
                        }
                        else
                        {
                            for (int k = 0; k < TextureCount(landLayer); k++)
                            {
                                splatMapPaint[i, j, k] = splatMapOld[i, j, k];
                            }
                        }
                    }
                }
            }
            ClearProgressBar();
            landData.setData(splatMapPaint, landLayer, topology);
            landData.setLayer(landLayer, topology);
        }
    }
    public void PaintHeight(string landLayer, float heightLow, float heightHigh, float minBlendLow, float maxBlendHigh, int t, int topology = 0) // Paints height between 2 floats. Blending is attributed to the 2 blend floats.
    // The closer the height is to the heightLow and heightHigh the stronger the weight of the texture is. To paint without blending assign the blend floats to the same value as the height floats.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Height");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        int textureCount = TextureCount(landLayer);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < (float)splatMap.GetLength(1); j++)
            {
                float iNorm = (float)i / (float)splatMap.GetLength(0);
                float jNorm = (float)j / (float)splatMap.GetLength(1);
                float[] normalised = new float[textureCount];
                float height = terrain.terrainData.GetInterpolatedHeight(jNorm, iNorm); // Normalises the interpolated height to the splatmap size.
                if (height > heightLow && height < heightHigh)
                {
                    for (int k = 0; k < textureCount; k++) // Erases the textures on all the layers.
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t] = 1; // Paints the texture t.
                }
                else if (height > minBlendLow && height < heightLow)
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
                else if (height > heightHigh && height < maxBlendHigh)
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
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    }
    public void PaintLayer(string landLayer, int t, int topology = 0) // Sets whole layer to the active texture. 
    //Alpha layers are inverted because it's more logical to have clear Alpha = Terrain appears in game.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Layer");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        int textureCount = TextureCount(landLayer);
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
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    } 
    public void ClearLayer(string landLayer, int topology = 0) // Sets whole layer to the inactive texture. Alpha and Topology only. 
    //Alpha layers are inverted because it's more logical to have clear Alpha = Terrain appears in game.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Clear Layer");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        var alpha = (landLayer.ToLower() == "alpha") ? true : false;
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
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    }
    public void ClearAllLayers()
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
    public void InvertLayer(string landLayer, int topology = 0) // Inverts the active and inactive textures. Alpha and Topology only. 
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Invert Layer");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
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
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    }
    public void InvertAllLayers()
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
    public void PaintSlope(string landLayer, float slopeLow, float slopeHigh, float minBlendLow, float maxBlendHigh, int t, int topology = 0) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Slope");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        int textureCount = TextureCount(landLayer);
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
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    }
    public void PaintArea(string landLayer, int z1, int z2, int x1, int x2, int t, int topology = 0) // Paints area within these splatmap coords, Maps will always have a splatmap resolution between
    // 512 - 2048 resolution, to the nearest Power of Two (512, 1024, 2048). Face downright in the editor with Z axis facing up, and X axis facing right, and the map will draw
    // from the bottom left corner, up to the top right. So a value of z1 = 0, z2 = 500, x1 = 0, x2 = 1000, would paint 500 pixels up, and 1000 pixels left from the bottom right corner.
    // Note that the results of how much of the map is covered is dependant on the map size, a 2000 map size would paint almost the bottom half of the map, whereas a 4000 map would 
    // paint up nearly one quarter of the map, and across nearly half of the map.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint Area");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        int textureCount = TextureCount(landLayer);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                if (i >= z1 && i <= z2)
                {
                    if (j >= x1 && j <= x2)
                    {
                        for (int k = 0; k < textureCount; k++)
                        {
                            splatMap[i, j, k] = 0;
                        }
                        splatMap[i, j, t] = 1;
                    }
                }
            }
        }
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    }
    public void PaintRiver(string landLayer, bool aboveTerrain, int t, int topology = 0) // Paints the splats wherever the water is above 500 and is above the terrain. Above terrain
    // true will paint only if water is above 500 and is also above the land terrain.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Paint River");
        float[,,] splatMap = GetSplatMap(landLayer, topology);
        int textureCount = TextureCount(landLayer);
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
        landData.setData(splatMap, landLayer, topology);
        landData.setLayer(landLayer, topology);
    }
    public void AutoGenerateTopology(bool wipeLayer) // Assigns topology active to these values. If wipeLayer == true it will wipe the existing topologies on the layer before painting
    // the new topologies.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Auto Generate Topologies");
        if (wipeLayer == true) //Wipes layer then paints on active textures.
        {
            ProgressBar("Generating Topologies", "Generating Offshore", 0.1f);
            ClearLayer("Topology", TerrainTopology.TypeToIndex((int)TerrainTopology.Enum.Beach));
            PaintHeight("Topology", 0, 475, 0, 475, 0, TerrainTopology.TypeToIndex((int)TerrainTopology.Enum.Beach));

            ProgressBar("Generating Topologies", "Generating Ocean", 0.2f);
            oldTopologyLayer = TerrainTopology.Enum.Ocean;
            ClearLayer("Topology");
            PaintHeight("Topology", 0, 498, 0, 498, 0);

            ProgressBar("Generating Topologies", "Generating Beach", 0.3f);
            oldTopologyLayer = TerrainTopology.Enum.Beach;
            ClearLayer("Topology");
            PaintHeight("Topology", 500, 502, 500, 502, 0);

            ProgressBar("Generating Topologies", "Generating Oceanside", 0.4f);
            oldTopologyLayer = TerrainTopology.Enum.Oceanside;
            ClearLayer("Topology");
            PaintHeight("Topology", 500, 502, 500, 502, 0);

            ProgressBar("Generating Topologies", "Generating Mainland", 0.5f);
            oldTopologyLayer = TerrainTopology.Enum.Mainland;
            ClearLayer("Topology");
            PaintHeight("Topology", 500, 1000, 500, 1000, 0);

            ProgressBar("Generating Topologies", "Generating Cliff", 0.6f);
            oldTopologyLayer = TerrainTopology.Enum.Cliff;
            ClearLayer("Topology");
            PaintSlope("Topology", 45f, 90f, 45f, 90f, 0);

            ProgressBar("Generating Topologies", "Generating Tier 0", 0.7f);
            oldTopologyLayer = TerrainTopology.Enum.Tier0;
            ClearLayer("Topology");
            PaintArea("Topology", 0, terrain.terrainData.alphamapResolution / 3 , 0, terrain.terrainData.alphamapResolution, 0); // Gets thirds of Terrain

            ProgressBar("Generating Topologies", "Generating Tier 1", 0.8f);
            oldTopologyLayer = TerrainTopology.Enum.Tier1;
            ClearLayer("Topology");
            PaintArea("Topology", terrain.terrainData.alphamapResolution / 3, terrain.terrainData.alphamapResolution / 3 * 2, 0, terrain.terrainData.alphamapResolution, 0); // Gets thirds of Terrain

            ProgressBar("Generating Topologies", "Generating Tier 2", 0.9f);
            oldTopologyLayer = TerrainTopology.Enum.Tier2;
            ClearLayer("Topology");
            PaintArea("Topology", terrain.terrainData.alphamapResolution / 3 * 2, terrain.terrainData.alphamapResolution, 0, terrain.terrainData.alphamapResolution, 0); // Gets thirds of Terrain

            ClearProgressBar();
            ChangeLandLayer();
        }
        else if (wipeLayer == false) // Paints active texture on to layer whilst keeping the current layer's textures.
        {
            oldTopologyLayer2 = topologyLayer; //This saves the currently selected topology layer so we can swap back to it at the end, ensuring we don't accidentally erase anything.

            ProgressBar("Generating Topologies", "Generating Offshore", 0.1f);
            topologyLayer = TerrainTopology.Enum.Offshore; // This sets the new current topology layer to offshore.
            ChangeLandLayer(); // This changes the topology layer to offshore. It also saves the previous layer for us.
            oldTopologyLayer = TerrainTopology.Enum.Offshore; // This is the layer the paint the offshore height to.
            PaintHeight("Topology", 0, 475, 0, 475, 0);

            ProgressBar("Generating Topologies", "Generating Ocean", 0.2f);
            topologyLayer = TerrainTopology.Enum.Ocean;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Ocean;
            PaintHeight("Topology", 0, 498, 0, 498, 0);

            ProgressBar("Generating Topologies", "Generating Beach", 0.3f);
            topologyLayer = TerrainTopology.Enum.Beach;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Beach;
            PaintHeight("Topology", 500, 502, 500, 502, 0);

            ProgressBar("Generating Topologies", "Generating Oceanside", 0.4f);
            topologyLayer = TerrainTopology.Enum.Oceanside;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Oceanside;
            PaintHeight("Topology", 500, 502, 500, 502, 0);

            ProgressBar("Generating Topologies", "Generating Mainland", 0.5f);
            topologyLayer = TerrainTopology.Enum.Mainland;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Mainland;
            PaintHeight("Topology", 500, 1000, 500, 1000, 0);

            ProgressBar("Generating Topologies", "Generating Cliff", 0.6f);
            topologyLayer = TerrainTopology.Enum.Cliff;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Cliff;
            PaintSlope("Topology", 45f, 90f, 45, 90f, 0);

            ProgressBar("Generating Topologies", "Generating Tier 0", 0.7f);
            topologyLayer = TerrainTopology.Enum.Tier0;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier0;
            PaintArea("Topology", 0, terrain.terrainData.alphamapResolution / 3, 0, terrain.terrainData.alphamapResolution, 0); // Gets thirds of Terrain

            ProgressBar("Generating Topologies", "Generating Tier 1", 0.8f);
            topologyLayer = TerrainTopology.Enum.Tier1;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier1;
            PaintArea("Topology", terrain.terrainData.alphamapResolution / 3, terrain.terrainData.alphamapResolution / 3 * 2, 0, terrain.terrainData.alphamapResolution, 0); // Gets thirds of Terrain

            ProgressBar("Generating Topologies", "Generating Tier 2", 0.9f);
            topologyLayer = TerrainTopology.Enum.Tier2;
            ChangeLandLayer();
            oldTopologyLayer = TerrainTopology.Enum.Tier2;
            PaintArea("Topology", terrain.terrainData.alphamapResolution / 3 * 2, terrain.terrainData.alphamapResolution, 0, terrain.terrainData.alphamapResolution, 0); // Gets thirds of Terrain

            topologyLayer = oldTopologyLayer2;
            ChangeLandLayer();
            oldTopologyLayer = oldTopologyLayer2;
            ClearProgressBar();
        }
    }
    public void AutoGenerateGround() // Assigns terrain splats to these values. 
    {
        ChangeLayer("Ground");
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Auto Generate Ground");

        ProgressBar("Generating Ground Textures", "Generating: Forest", 0.15f);
        generateTwoLayersNoise("Ground", UnityEngine.Random.Range(45f, 55f), 0, 4);

        ProgressBar("Generating Ground Textures", "Generating: Grass", 0.3f);
        PaintSlope("Ground", 25f, 45, 5f, 50f, 4);

        ProgressBar("Generating Ground Textures", "Generating: Dirt", 0.4f);
        PaintSlope("Ground", 20, 20, 15, 25, 0);

        ProgressBar("Generating Ground Textures", "Generating: Snow", 0.6f);
        PaintHeight("Ground", 650, 1000, 600, 1000, 1);

        ProgressBar("Generating Ground Textures", "Generating: Rock", 0.8f);
        PaintSlope("Ground", 50f, 90f, 40f, 90f, 3);

        ProgressBar("Generating Ground Textures", "Generating: Sand", 0.9f);
        PaintHeight("Ground", 0, 502, 0, 503, 2);

        ClearProgressBar();
    } 
    public void AutoGenerateBiome() // Assigns biome splats to these values.
    {
        ChangeLayer("Biome");
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Auto Generate Biome");

        ProgressBar("Generating Biome Textures", "Generating: Temperate", 0.2f);
        PaintHeight("Biome", 0, 550, 0, 675, 1);

        ProgressBar("Generating Biome Textures", "Generating: Arctic", 0.4f);
        PaintHeight("Biome", 750, 1000, 700, 1000, 3);

        ProgressBar("Generating Biome Textures", "Generating: Tundra", 0.8f);
        PaintHeight("Biome", 650, 750, 575, 800, 2);

        ClearProgressBar();
    }
    public void alphaDebug(string landLayer) // Paints a ground texture to the corresponding coordinate if the alpha is active.
    // Used for debugging the floating ground clutter that occurs when you have a ground splat of either Grass or Forest ontop of an active alpha layer. Replaces with rock texture.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Alpha Debug");
        ProgressBar("Debug Alpha", "Debugging", 0.3f);
        float[,,] splatMap = GetSplatMap("ground");
        float[,,] alphaSplatMap = GetSplatMap("alpha"); // Always needs to be at two layers or it will break, as we can't divide landData by 0.
        ProgressBar("Debug Alpha", "Debugging", 0.5f);

        for (int i = 0; i < alphaSplatMap.GetLength(0); i++)
        {
            for (int j = 0; j < alphaSplatMap.GetLength(1); j++)
            {
                if (alphaSplatMap[i, j, 1] == 1)
                {
                    for (int k = 0; k < TextureCount(landLayer); k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, 3] = 1; // This paints the rock layer. Where 3 = the layer to paint.
                }
            }
        }
        ProgressBar("Debug Alpha", "Debugging", 0.7f);
        landData.setData(splatMap, landLayer);
        landData.setLayer(landLayer);
        ProgressBar("Debug Alpha", "Done", 1f);
        ClearProgressBar();
    }
    public void TextureCopy(string landLayerFrom, string landLayerToPaint, int textureFrom, int textureToPaint) // This copies the selected texture on a landlayer 
    // and paints the same coordinate on another landlayer with the selected texture.
    {
        ProgressBar("Copy Textures", "Copying: " + landLayerFrom, 0.2f);
        switch (landLayerFrom) // Gathers the information on which texture we are copying from in the landlayer.
        {
            default:
                Debug.Log("landLayerFrom not found!");
                break;
            case "Ground":
                ChangeLayer("Ground");
                textureFrom = TerrainSplat.TypeToIndex((int)groundLayerFrom); // Layer texture to copy from Ground Textures.
                break;
            case "Biome":
                ChangeLayer("Biome");
                textureFrom = TerrainBiome.TypeToIndex((int)biomeLayerFrom); // Layer texture to copy from Biome Textures.
                break;
            case "Topology":
                ChangeLayer("Topology");
                textureFrom = 0;
                topologyLayer = topologyLayerFrom;
                break;
        }
        float[,,] splatMapFrom = GetSplatMap(landLayerFrom); // Land layer to copy from.
        ProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.5f);
        switch (landLayerToPaint) // Gathers the information on which texture we are painting to in the landlayer.
        {
            default:
                Debug.Log("landLayerToPaint not found!");
                break;
            case "Ground":
                Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Texture Copy");
                textureToPaint = TerrainSplat.TypeToIndex((int)groundLayerToPaint); // Layer texture to copy from Ground Textures.
                break;
            case "Biome":
                Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Texture Copy");
                textureToPaint = TerrainBiome.TypeToIndex((int)biomeLayerToPaint); // Layer texture to copy from Biome Textures.
                break;
            case "Topology":
                ChangeLayer("Topology");
                Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Texture Copy");
                textureToPaint = 0;
                oldTopologyLayer2 = topologyLayer;
                topologyLayer = topologyLayerToPaint;
                ChangeLandLayer();
                oldTopologyLayer = topologyLayerToPaint;
                break;
        }
        float[,,] splatMapTo = GetSplatMap(landLayerToPaint); //  Land layer to paint to.
        ProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.75f);
        for (int i = 0; i < splatMapFrom.GetLength(0); i++)
        {
            for (int j = 0; j < splatMapFrom.GetLength(1); j++)
            {
                if (splatMapFrom [i, j, textureFrom] > 0)
                {
                    for (int k = 0; k < TextureCount(landLayerToPaint); k++)
                    {
                        splatMapTo[i, j, k] = 0;
                    }
                    splatMapTo[i, j, textureToPaint] = 1;
                }
            }
        }
        ProgressBar("Copy Textures", "Pasting: " + landLayerToPaint, 0.9f);
        landData.setData(splatMapTo, landLayerToPaint, TerrainTopology.TypeToIndex((int)topologyLayerToPaint));
        landData.setLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayerToPaint));
        ClearProgressBar();
    }
    public void generateTwoLayersNoise(string landLayer, float scale, int t1, int t2) //Generates a layer of perlin noise across two layers, the smaller the scale the smaller the blobs 
    // it generates will be. Wipes the current layer.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Two Layers Noise");
        float[,,] splatMap = GetSplatMap(landLayer);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp01(Mathf.PerlinNoise(i2, j2));
                if (perlin <= 0.2f)
                {
                    for (int k = 0; k < TextureCount(landLayer); k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t1] = 1;
                    splatMap[i, j, t2] = 0;
                }
                else
                {
                    for (int k = 0; k < TextureCount(landLayer); k++)
                    {
                        splatMap[i, j, k] = 0;
                    }
                    splatMap[i, j, t1] = 0;
                    splatMap[i, j, t2] = 1;
                }
            }
        }
        landData.setData(splatMap, landLayer, (int)topologyLayer);
        landData.setLayer(landLayer, (int)topologyLayer);
    }
    public void generateFourLayersNoise(string landLayer, float scale) //Generates a layer of perlin noise across four layers, the smaller the scale the smaller the blobs 
    // it generates will be. Wipes the current layer.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Four Layers Noise");
        float[,,] splatMap = GetSplatMap(landLayer);
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
        landData.setLayer(landLayer);
    }
    public void generateEightLayersNoise(string landLayer, float scale) //Generates a layer of perlin noise across eight layers, the smaller the scale the smaller the blobs 
    // it generates will be. Wipes the current layer.
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Eight Layers Noise");
        float[,,] splatMap = GetSplatMap(landLayer);
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
        landData.setLayer(landLayer);
    }
    public void generateLayerNoise(string landLayer, int layers, float scale)
    {
        float[,,] splatMap = GetSplatMap(landLayer);
        float layerBlend = 1 / layers;
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                float i2 = i / scale;
                float j2 = j / scale;
                float perlin = Mathf.Clamp(Mathf.PerlinNoise(i2, j2), 0, layers);
                if (perlin <= layerBlend)
                {

                }
            }
        }
    }
    #endregion
    public void removeBrokenPrefabs()
    {
        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
        Undo.RegisterCompleteObjectUndo(prefabs, "Remove Broken Prefabs");
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
    public void exportLootCrates(string prefabFilePath, bool deletePrefabs)
    {
        StreamWriter streamWriter = new StreamWriter(prefabFilePath, false);
        streamWriter.WriteLine("{");
        List<PrefabExport> prefabExports = new List<PrefabExport>();
        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
        var lootCrateCount = 0;
        foreach (PrefabDataHolder p in prefabs)
        {
            switch (p.prefabData.id)
            {
                default:
                    // Not a lootcrate. If you want you to export everything uncomment this section.
                    /*
                    if (prefabNames[p.prefabData.id] != null)
                    {
                        prefabExports.Add(new PrefabExport()
                        {
                            PrefabNumber = lootCrateCount,
                            PrefabProperty = prefabPaths[p.prefabData.id] + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
                        });
                        if (deletePrefabs == true) // If delete prefabs on export is selected this will delete the prefab from the map file.
                        {
                            DestroyImmediate(p.gameObject);
                        }
                        lootCrateCount++; // This is just used to keep track of the lootcrates exported, not important for things that arent respawning.
                    }
                    */
                    break;
                case 69: // THIS IS AN EXAMPLE FOR EXPORTING AN INDIVIDUAL PREFAB. Set this number to a prefab ID you want to export.
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        // Set the number in prefabNames to be the prefabid, this just gets the prefab name for the data file to load ingame.
                        PrefabProperty = prefabPaths[69] + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
                    });
                    if (deletePrefabs == true) // If delete prefabs on export is selected this will delete the prefab from the map file.
                    {
                        DestroyImmediate(p.gameObject); 
                    }
                    lootCrateCount++; // This is just used to keep track of the lootcrates exported, not important for things that arent respawning.
                    break;
                case 1603759333:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_basic.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_elite.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_mine.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_normal.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_normal_2.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_normal_2_food.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_normal_2_medical.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_underwater_advanced.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
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
                        PrefabProperty = "assets/bundled/prefabs/radtown/crate_underwater_basic.prefab" + ":(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + "):" + p.transform.rotation
                    });
                    if (deletePrefabs == true)
                    {
                        DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
            }
        }
        foreach (PrefabExport prefabDetail in prefabExports)
        {
            streamWriter.WriteLine("   \"" + prefabDetail.PrefabNumber + "\": \"" + prefabDetail.PrefabProperty + "\",");
        }
        streamWriter.WriteLine("   \"Prefab Count\": " + lootCrateCount);
        streamWriter.WriteLine("}");
        streamWriter.Close();
        Debug.Log("Exported " + lootCrateCount + " lootcrates.");
    }
    private void LoadMapInfo(MapInfo terrains)
    {
        if (MapIO.topology == null)
        {
            topology = GameObject.FindGameObjectWithTag("LandData").GetComponent<TopologyMesh>();
        }
        // Puts prefabs object in middle of map.
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
        landData = GameObject.FindGameObjectWithTag("LandData").GetComponent<LandData>();

        terrain.transform.position = terrainPosition;
        water.transform.position = terrainPosition;

        ProgressBar("Loading: " + loadPath, "Loading Ground Data ", 0.4f);
        topology.InitMesh(terrains.topology);

        terrain.terrainData.heightmapResolution = terrains.resolution;
        terrain.terrainData.size = terrains.size;

        water.terrainData.heightmapResolution = terrains.resolution;
        water.terrainData.size = terrains.size;

        terrain.terrainData.SetHeights(0, 0, terrains.land.heights);
        water.terrainData.SetHeights(0, 0, terrains.water.heights);

        terrain.terrainData.alphamapResolution = terrains.resolution;
        terrain.terrainData.baseMapResolution = terrains.resolution - 1;
        water.terrainData.alphamapResolution = terrains.resolution;
        water.terrainData.baseMapResolution = terrains.resolution - 1;

        terrain.GetComponent<UpdateTerrainValues>().setSize(terrains.size);
        water.GetComponent<UpdateTerrainValues>().setSize(terrains.size);
        terrain.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);
        water.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);

        ProgressBar("Loading: " + loadPath, "Loading Ground Data ", 0.5f);
        landData.setData(terrains.splatMap, "ground");

        ProgressBar("Loading: " + loadPath, "Loading Biome Data ", 0.6f);
        landData.setData(terrains.biomeMap, "biome");

        ProgressBar("Loading: " + loadPath, "Loading Alpha Data ", 0.7f);
        landData.setData(terrains.alphaMap, "alpha");

        ProgressBar("Loading: " + loadPath, "Loading Topology Data ", 0.8f);
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            landData.setData(topology.getSplatMap(TerrainTopology.IndexToType(i)), "topology", i);
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
        landData.setData(topology.getSplatMap((int)topologyLayer), "topology", TerrainTopology.TypeToIndex((int)topologyLayer));
        landData.setLayer("topology", TerrainTopology.TypeToIndex((int)topologyLayer));
        ChangeLayer("Ground");
        ClearProgressBar();
    }
    public void Load(WorldSerialization blob)
    {
        WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
        MapIO.ProgressBar("Loading: " + loadPath, "Loading Land Heightmap Data ", 0.3f);
        LoadMapInfo(terrains);
    }
    public void loadEmpty(int size)
    {
        LoadMapInfo(WorldConverter.emptyWorld(size));
    }
    public void Save(string path)
    {
        if(landData != null)
        {
            landData.save(TerrainTopology.TypeToIndex((int)topologyLayer));
        }
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
    public void newEmptyTerrain(int size)
    {
        LoadMapInfo(WorldConverter.emptyWorld(size));
        progressValue = 1f / TerrainTopology.COUNT;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            progressBar += progressValue;
            ClearLayer("Topology", i);
            ProgressBar("Creating New Map", "Wiping: " + (TerrainTopology.Enum)TerrainTopology.IndexToType(i), progressBar);
        }
        ClearLayer("Alpha");
        PaintLayer("Biome", 1);
        PaintLayer("Ground", 4);
        setMinimumHeight(503f);
        ClearProgressBar();
    }
    public void createDefaultPrefabs()
    {
        var prefabParent = GameObject.Find("PrefabsLoaded").GetComponent<Transform>();
        if (File.Exists("PrefabsLoaded.txt"))
        {
            var lines = File.ReadAllLines("PrefabsLoaded.txt");
            foreach (var line in lines)
            {
                var linesSplit = line.Split(':');
                GameObject prefabSpawned = Instantiate(Resources.Load<GameObject>("Prefabs/DefaultPrefab"), prefabParent);
                var pdh = prefabSpawned.GetComponent<PrefabDataHolder>();
                prefabSpawned.name = linesSplit[1];
                var prefabNameSplit = linesSplit[1].Split('/');
                var prefabName = prefabNameSplit[prefabNameSplit.Length - 1].Replace(".prefab", "");
                pdh.saveWithMap = false;
                pdh.prefabData = new PrefabData();
                pdh.prefabData.id = uint.Parse(linesSplit[2]);
                var shortenedId = linesSplit[2].Substring(2);
                prefabReference.Add(shortenedId, prefabSpawned);
                prefabNames.Add(uint.Parse(linesSplit[2]), prefabName);
            }
        }
    }
    public void StartPrefabLookup()
    {
        setPrefabLookup(new PrefabLookup(bundleFile, this));
    }
    public List<string> generationPresetList = new List<string>();
    public Dictionary<string, UnityEngine.Object> generationPresetLookup = new Dictionary<string, UnityEngine.Object>();
    public void RefreshAssetList()
    { 
        var list = AssetDatabase.FindAssets("t:AutoGenerationGraph");
        generationPresetList.Clear();
        generationPresetLookup.Clear();
        foreach (var item in list)
        {
            var itemName = AssetDatabase.GUIDToAssetPath(item).Split('/');
            var itemNameSplit = itemName[itemName.Length - 1].Replace(".asset", "");
            generationPresetList.Add(itemNameSplit);
            generationPresetLookup.Add(itemNameSplit, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(item), typeof(AutoGenerationGraph)));
        }
    }
    public void ParseNodeGraph(XNode.NodeGraph graph)
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
            if (mapIO.prefabReference.ContainsKey(args.draggedItemIDs[0].ToString()))
            {
                Debug.Log("Prefab Found");
                mapIO.prefabReference.TryGetValue(args.draggedItemIDs[0].ToString(), out GameObject prefabDragged);
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
