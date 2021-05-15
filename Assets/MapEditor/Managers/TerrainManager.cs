using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using static WorldConverter;
using System.Threading.Tasks;

public static class TerrainManager
{
    public static class Callbacks
    {
        public delegate void TerrainManagerCallback(LandLayers layer, int? topology = null);

        /// <summary>Called after the active layer is changed. </summary>
        public static event TerrainManagerCallback LayerChanged;

        /// <summary>Called after the active layer is saved. </summary>
        public static event TerrainManagerCallback LayerSaved;

        public static void OnLayerChanged(LandLayers layer, int? topology = null) => LayerChanged?.Invoke(layer, topology);
        public static void OnLayerSaved(LandLayers layer, int? topology = null) => LayerSaved?.Invoke(layer, topology);
    }

    /// <summary>The Terrain layers used by the terrain for paint operations</summary>
    public static TerrainLayer[] GroundTextures { get; private set; } = null;

    /// <summary>The Terrain layers used by the terrain for paint operations</summary>
    public static TerrainLayer[] BiomeTextures { get; private set; } = null;

    /// <summary>The Terrain layers used by the terrain for paint operations</summary>
    public static TerrainLayer[] MiscTextures { get; private set; } = null;

    public static Data[,] Terrain { get; set; }

    public static void Set(this Data[,] data)
    {
        for (int x = 0; x < data.GetLength(0); x++)
            for (int y = 0; y < data.GetLength(0); y++)
                data[x, y] = new Data(0);
    }

    public struct Data
    {
        public Data(int i)
        {
            Ground = new float[TerrainSplat.COUNT];
            Biome = new float[TerrainBiome.COUNT];
            Alpha = true;
            Topology = new bool[TerrainTopology.COUNT];
            Slope = null;
            Height = null;
            WaterHeight = null;
        }

        #region Splats
        /// <summary>Ground textures, use <seealso cref="TerrainSplat.TypeToIndex(int)"/> for texture indexes.</summary>
        /// <value>Strength of texture at <seealso cref="TerrainSplat"/> index, normalised between 0-1.</value>
        public float[] Ground;

        /// <summary>Biome textures, use <seealso cref="TerrainBiome.TypeToIndex(int)"/> for texture indexes.</summary>
        /// <value>Strength of texture at <seealso cref="TerrainBiome"/> index, normalised between 0-1.</value>
        public float[] Biome;

        /// <summary>Alpha/Transparency value of terrain.</summary>
        /// <value>True = Visible / False = Invisible.</value>
        public bool Alpha;

        /// <summary>Topology layers and value, use <seealso cref="TerrainTopology.TypeToIndex(int)"/> for topology layer indexes.</summary>
        /// <value>True = Active / False = Inactive.</value>
        public bool[] Topology;
        #endregion

        #region HeightMap
        public void ResetHeights()
        {
            Slope = null;
            Height = null;
        }

        /// <summary>The current slope value.</summary>
        /// <value>Slope angle in degrees, between 0 - 90.</value>
        public float? Slope;

        /// <summary>The current height value.</summary>
        /// <value>Height in metres, between 0 - 1000.</value> 
        public float? Height;

        /// <summary>The current water height value.</summary>
        /// <value>Height in metres, between 0 - 1000.</value> 
        public float? WaterHeight;
        #endregion
    }

    /// <summary>The LandLayer currently being displayed on the terrain.</summary>
    public static LandLayers LandLayer { get; private set; }

    /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LandLayer is set to topology.</summary>
    public static TerrainTopology.Enum TopologyLayerEnum { get; private set; }

    /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LandLayer is set to topology.</summary>
    public static int TopologyLayer { get => TerrainTopology.TypeToIndex((int)TopologyLayerEnum); }

    /// <summary>The previously selected topology layer. Used to save the Topology layer before displaying the new one.</summary>
    public static int LastTopologyLayer { get; private set; } = 0;

    /// <summary>The land terrain in the scene.</summary>
    public static Terrain Land { get; private set; }

    /// <summary>The water terrain in the scene.</summary>
    public static Terrain Water { get; private set; }

    /// <summary>The material used by the water terrain object.</summary>
    public static Material WaterMaterial { get; private set; }

    public static Vector3 TerrainSize { get => Land.terrainData.size; }
    public static Vector3 MapOffset { get => 0.5f * TerrainSize; }
    public static int HeightMapRes { get => Land.terrainData.heightmapResolution; }
    public static int SplatMapRes { get => Land.terrainData.alphamapResolution; }

    /// <summary>The size of each splat relative to the terrain size it covers.</summary>
    public static float SplatSize { get => Land.terrainData.size.x / SplatMapRes; }

    /// <summary>The state of the layer being applied to the terrain.</summary>
    public static bool LayerSet { get; private set; }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        TerrainCallbacks.heightmapChanged += HeightmapChanged;
        EditorApplication.update += ProjectLoaded;
    }

    private static void ProjectLoaded()
    {
        EditorApplication.update -= ProjectLoaded;
        SetTerrainReferences();
    }

    #region HeightMap
    /// <summary>Checks if selected Alphamap index is a valid coord.</summary>
    /// <returns>True if Alphamap index is valid, false otherwise.</returns>
    public static bool IsValidIndex(int x, int y)
    {
        if (x < 0 || y < 0 || x >= SplatMapRes || y >= SplatMapRes)
            return false;

        return true;
    }

    /// <summary>Returns the slope of the HeightMap at the selected coords.</summary>
    /// <returns>Float within the range 0° - 90°. Null if out of bounds.</returns>
    public static float? GetSlope(int x, int y)
    {
        if (!IsValidIndex(x, y))
        {
            Debug.LogError($"Index: {x},{y} is out of bounds for GetSlope.");
            return null;
        }

        if (Terrain[x, y].Slope == null)
        {
            float slope = Land.terrainData.GetSteepness((float)x / SplatMapRes, (float)y / SplatMapRes);
            Terrain[x, y].Slope = slope;
            return slope;
        }

        return Terrain[x, y].Slope;
    }

    /// <summary>Returns a 2D array of the slope values.</summary>
    /// <returns>Floats within the range 0° - 90°.</returns>
    public static float[,] GetSlopes()
    {
        float[,] slopes = new float[SplatMapRes, SplatMapRes];
        for (int x = 0; x < SplatMapRes; x++)
        {
            for (int y = 0; y < SplatMapRes; y++)
            {
                slopes[x, y] = Land.terrainData.GetSteepness((float)x / SplatMapRes, (float)y / SplatMapRes);
                Terrain[x, y].Slope = slopes[x, y];
            }
        }

        return slopes;
    }

    /// <summary>Updates cached Slope values with current.</summary>
    public static void UpdateSlopes()
    {
        for (int x = 0; x < SplatMapRes; x++)
            for (int y = 0; y < SplatMapRes; y++)
                Terrain[x, y].Slope = Land.terrainData.GetSteepness((float)x / SplatMapRes, (float)y / SplatMapRes);
    }

    /// <summary>Returns the height of the HeightMap at the selected coords.</summary>
    /// <returns>Float within the range 0m - 1000m. Null if out of bounds.</returns>
    public static float? GetHeight(int x, int y)
    {
        if (!IsValidIndex(x, y))
        {
            Debug.LogError($"Index: {x},{y} is out of bounds for GetHeight.");
            return 0;
        }

        if (Terrain[x, y].Height == null)
        {
            float height = Land.terrainData.GetInterpolatedHeight((float)x / SplatMapRes, (float)y / SplatMapRes);
            Terrain[x, y].Height = height;
            return height;
        }

        return Terrain[x, y].Height;
    }

    /// <summary>Returns a 2D array of the height values.</summary>
    /// <returns>Floats within the range 0m - 1000m.</returns>
    public static float[,] GetHeights()
    {
        float[,] heights = Land.terrainData.GetInterpolatedHeights(0, 0, SplatMapRes, SplatMapRes, 1f / SplatMapRes, 1f / SplatMapRes);
        for (int x = 0; x < SplatMapRes; x++)
            for (int y = 0; y < SplatMapRes; y++)
                Terrain[x, y].Height = heights[x, y];

        return heights;
    }

    /// <summary>Updates cached Height values with current.</summary>
    public static void UpdateHeights() => GetHeights();

    #endregion
    public static void SetWaterTransparency(float alpha)
    {
        Color _color = WaterMaterial.color;
        _color.a = alpha;
        WaterMaterial.color = _color;
    }

    public static void SetTerrainReferences()
    {
        Water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        Land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        WaterMaterial = Water.materialTemplate;
    }

    /// <summary>Callback for whenever the heightmap is updated.</summary>
    private static void HeightmapChanged(Terrain terrain, RectInt heightRegion, bool synched)
    {
        if (0 == 1)
        {
            foreach (var item in heightRegion.allPositionsWithin)
            {
                if (item.x < 0 || item.x >= SplatMapRes)
                    continue;
                if (item.y < 0 || item.y >= SplatMapRes)
                    continue;

                Terrain[item.x, item.y].ResetHeights();
            }
        }
            
    }

    /// <summary>Changes the active Land and Topology Layers.</summary>
    /// <param name="layer">The LandLayer to change to.</param>
    /// <param name="topology">The Topology layer to change to.</param>
    public static void ChangeLandLayer(LandLayers layer, int topology = 0) => EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ChangeLayer(layer, topology));

    /// <summary>Returns the SplatMap at the selected LandLayer.</summary>
    /// <param name="landLayer">The LandLayer to return. (Ground, Biome)</param>
    /// <returns>3D float array in Alphamap format. [x, y, Texture]</returns>
    public static float[,,] GetSplatMap(LandLayers layer, int topology = 0)
    {
        if (layer.Equals(LandLayers.Alpha))
        {
            Debug.LogError($"GetSplatMap({layer}) cannot return float[,,].");
            return null;
        }

        //if (LandLayer.Equals(layer)) // Returns currently displayed Alphamap.
            //return Land.terrainData.GetAlphamaps(0, 0, SplatMapRes, SplatMapRes);

        float[,,] splatMap = new float[SplatMapRes, SplatMapRes, MapManager.TextureCount(layer)];
        int res = SplatMapRes;
        switch (layer)
        {
            case LandLayers.Ground:
                Parallel.For(0, res, x =>
                {
                    for (int y = 0; y < res; y++)
                        for (int z = 0; z < MapManager.TextureCount(layer); z++)
                            splatMap[x, y, z] = Terrain[x, y].Ground[z];
                });
                return splatMap;
            case LandLayers.Biome:
                Parallel.For(0, res, x =>
                {
                    for (int y = 0; y < res; y++)
                        for (int z = 0; z < MapManager.TextureCount(layer); z++)
                            splatMap[x, y, z] = Terrain[x, y].Biome[z];
                });
                return splatMap;

            case LandLayers.Topology:
                Parallel.For(0, res, x =>
                {
                    for (int y = 0; y < res; y++) 
                    {
                        splatMap[x, y, 0] = Terrain[x, y].Topology[topology] == true ? 0f : 1f;
                        splatMap[x, y, 1] = Terrain[x, y].Topology[topology] == true ? 1f : 0f;
                    }
                });
                return splatMap;

            default:
                return null;
        }
    }

    /// <summary>Sets the array data of LandLayer.</summary>
    /// <param name="layer">The layer to set the data to.</param>
    /// <param name="topology">The topology layer if layer is topology.</param>
    public static void SetData(float[,,] array, LandLayers layer, int topology = 0)
    {
        int res = array.GetLength(0);

        switch (layer)
        {
            case LandLayers.Ground:
                Parallel.For(0, res, x =>
                {
                    for (int y = 0; y < res; y++)
                        for (int z = 0; z < MapManager.TextureCount(layer); z++)
                            Terrain[x, y].Ground[z] = array[x, y, z];
                });
                break;

            case LandLayers.Biome:
                Parallel.For(0, res, x =>
                {
                    for (int y = 0; y < res; y++)
                        for (int z = 0; z < MapManager.TextureCount(layer); z++)
                            Terrain[x, y].Biome[z] = array[x, y, z];
                });
                break;

            case LandLayers.Topology:
                Parallel.For(0, res, x =>
                {
                    for (int y = 0; y < res; y++)
                        Terrain[x, y].Topology[topology] = (array[x, y, 0] == 0f);
                });
                break;

            default:
                Debug.LogWarning($"SetData(float[,,], {layer}) is not a valid layer to set. Use SetData(bool[,], {layer}) to set {layer}.");
                break;
        }

        //if (LandLayer.Equals(layer))
            //Land.terrainData.SetAlphamaps(0, 0, array);
    }

    /// <summary>Sets the array data of LandLayer.</summary>
    /// <param name="layer">The layer to set the data to.</param>
    public static void SetData(bool[,] array, LandLayers layer)
    {
        if (layer.Equals(LandLayers.Alpha))
            Land.terrainData.SetHoles(0, 0, array);
        else
            Debug.LogWarning($"SetData(bool[,], {layer}) is not a valid layer to set. Use SetData(float[,,], {layer}) to set {layer}.");
    }

    /// <summary>Sets the terrain alphamaps to the LandLayer.</summary>
    /// <param name="layer">The LandLayer to set.</param>
    /// <param name="topology">The Topology layer to set.</param>
    public static void SetLayer(LandLayers layer, int topology = 0)
    {
        LayerSet = false;
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SetLayer(layer, topology));
    }

    /// <summary>Saves any changes made to the Alphamaps, including paint operations.</summary>
    public static void SaveLayer() => EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SaveLayer());
    
    /// <summary>Loads and sets the Land and Water terrain objects.</summary>
    public static void SetTerrain(MapInfo mapInfo, int progressID)
    {
        Land.terrainData.heightmapResolution = mapInfo.terrainRes;
        Land.terrainData.size = mapInfo.size;
        Land.terrainData.alphamapResolution = mapInfo.splatRes;
        Land.terrainData.baseMapResolution = mapInfo.splatRes;
        Land.terrainData.SetHeights(0, 0, mapInfo.land.heights);
        Progress.Report(progressID, .5f, "Loaded: Land");

        Water.terrainData.heightmapResolution = mapInfo.terrainRes;
        Water.terrainData.size = mapInfo.size;
        Water.terrainData.alphamapResolution = mapInfo.splatRes;
        Water.terrainData.baseMapResolution = mapInfo.splatRes;
        Water.terrainData.SetHeights(0, 0, mapInfo.water.heights);
        Progress.Report(progressID, .9f, "Loaded: Water");


        SetData(mapInfo.alphaMap, LandLayers.Alpha);
        AreaManager.Reset();

        Progress.Report(progressID, 0.99f, "Loaded " + TerrainSize.x + " size map.");
    }

    public static void SetSplatMaps(MapInfo mapInfo)
    {
        Terrain = new Data[mapInfo.splatRes, mapInfo.splatRes];
        Terrain.Set();

        TopologyData.InitMesh(mapInfo.topology);
        SetData(mapInfo.splatMap, LandLayers.Ground);
        SetData(mapInfo.biomeMap, LandLayers.Biome);
        Parallel.For(0, TerrainTopology.COUNT, i =>
        {
            SetData(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(i)), LandLayers.Topology, i);
        });
    }

    private static void GetTerrainTextures()
    {
        GroundTextures = GetGroundTextures();
        BiomeTextures = GetBiomeTextures();
        MiscTextures = GetMiscTextures();
        AssetDatabase.SaveAssets();
    }

    private static TerrainLayer[] GetMiscTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[2];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Misc/Active.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/active");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Misc/InActive.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/inactive");
        return textures;
    }

    private static TerrainLayer[] GetBiomeTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[4];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arid.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arid");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Temperate.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/temperate");
        textures[2] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Tundra.terrainlayer");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/tundra");
        textures[3] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arctic.terrainlayer");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arctic");
        return textures;
    }

    private static TerrainLayer[] GetGroundTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[8];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Dirt.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/dirt");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Snow.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/snow");
        textures[2] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Sand.terrainlayer");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/sand");
        textures[3] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Rock.terrainlayer");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/rock");
        textures[4] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Grass.terrainlayer");
        textures[4].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/grass");
        textures[5] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Forest.terrainlayer");
        textures[5].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/forest");
        textures[6] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Stones.terrainlayer");
        textures[6].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/stones");
        textures[7] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Gravel.terrainlayer");
        textures[7].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/gravel");
        return textures;
    }

    private static class Coroutines
    {
        public static IEnumerator ChangeLayer(LandLayers layer, int topology = 0)
        {
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(SaveLayer());
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(SetLayer(layer, topology));
            Callbacks.OnLayerChanged(layer);
        }

        public static IEnumerator SetLayer(LandLayers layer, int topology = 0)
        {
            if (GroundTextures == null || BiomeTextures == null || MiscTextures == null)
                GetTerrainTextures();

            switch (layer)
            {
                case LandLayers.Ground:
                    Land.terrainData.terrainLayers = GroundTextures;
                    Land.terrainData.SetAlphamaps(0, 0, GetSplatMap(layer));
                    break;
                case LandLayers.Biome:
                    Land.terrainData.terrainLayers = BiomeTextures;
                    Land.terrainData.SetAlphamaps(0, 0, GetSplatMap(layer));
                    break;
                case LandLayers.Topology:
                    LastTopologyLayer = topology;
                    Land.terrainData.terrainLayers = MiscTextures;
                    Land.terrainData.SetAlphamaps(0, 0, GetSplatMap(layer, topology));
                    break;
            }
            LandLayer = layer;
            TopologyLayerEnum = (TerrainTopology.Enum)TerrainTopology.IndexToType(topology);
            LayerSet = true;
            yield return null;
        }

        public static IEnumerator SaveLayer()
        {
            while (!LayerSet)
                yield return null;

            switch (LandLayer)
            {
                case LandLayers.Ground:
                    //GroundArray = Land.terrainData.GetAlphamaps(0, 0, Land.terrainData.alphamapWidth, Land.terrainData.alphamapHeight);
                    break;
                case LandLayers.Biome:
                    //BiomeArray = Land.terrainData.GetAlphamaps(0, 0, Land.terrainData.alphamapWidth, Land.terrainData.alphamapHeight);
                    break;
                case LandLayers.Topology:
                    //TopologyArray[LastTopologyLayer] = Land.terrainData.GetAlphamaps(0, 0, Land.terrainData.alphamapWidth, Land.terrainData.alphamapHeight);
                    break;
            }
            foreach (var item in Land.terrainData.alphamapTextures)
                Undo.ClearUndo(item);

            Callbacks.OnLayerSaved(LandLayer);
            yield return null;
        }
    }
}