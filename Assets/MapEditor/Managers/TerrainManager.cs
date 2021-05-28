using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;
using static WorldConverter;
using System.Threading.Tasks;

public static class TerrainManager
{
    public static class Callbacks
    {
        public delegate void Layer(LandLayers layer, int? topology = null);

        /// <summary>Called after the active layer is changed. </summary>
        public static event Layer LayerChanged;
        /// <summary>Called after the active layer is saved. </summary>
        public static event Layer LayerSaved;
        /// <summary>Called when the active layer is dirtied/updated.</summary>
        public static event Layer LayerUpdated;

        public static void OnLayerChanged(LandLayers layer, int topology) => LayerChanged?.Invoke(layer, topology);
        public static void OnLayerSaved(LandLayers layer, int topology) => LayerSaved?.Invoke(layer, topology);
        public static void OnLayerUpdated(LandLayers layer, int topology) => LayerUpdated?.Invoke(layer, topology);
    }

    #region Splats
    #region Fields
    /// <summary>Ground textures [x, y, texture] use <seealso cref="TerrainSplat.TypeToIndex(int)"/> for texture indexes.</summary>
    /// <value>Strength of texture at <seealso cref="TerrainSplat"/> index, normalised between 0 - 1.</value>
    public static float[,,] Ground { get; private set; }
    /// <summary>Biome textures [x, y, texture] use <seealso cref="TerrainBiome.TypeToIndex(int)"/> for texture indexes.</summary>
    /// <value>Strength of texture at <seealso cref="TerrainBiome"/> index, normalised between 0-1.</value>
    public static float[,,] Biome { get; private set; }
    /// <summary>Alpha/Transparency value of terrain.</summary>
    /// <value>True = Visible / False = Invisible.</value>
    public static bool[,] Alpha { get; private set; }
    /// <summary>Topology layers [topology][x, y, texture] and value, use <seealso cref="TerrainTopology.TypeToIndex(int)"/> for topology layer indexes.</summary>
    /// <value>True = Active / False = Inactive.</value>
    public static float[][,,] Topology { get; private set; } = new float[TerrainTopology.COUNT][,,];
    /// <summary>Resolution of the splatmap/alphamap.</summary>
    /// <value>Power of ^2, between 512 - 2048.</value>
    public static int SplatMapRes { get; private set; }
    /// <summary>The size of each splat relative to the terrain size it covers.</summary>
    public static float SplatSize { get => Land.terrainData.size.x / SplatMapRes; }

    public static bool AlphaDirty = false;
    #endregion

    #region Methods
    /// <summary>Returns the SplatMap at the selected LandLayer.</summary>
    /// <param name="landLayer">The LandLayer to return. (Ground, Biome)</param>
    /// <returns>3D float array in Alphamap format. [x, y, Texture]</returns>
    public static float[,,] GetSplatMap(LandLayers layer, int topology = 0)
    {
        switch (layer)
        {
            case LandLayers.Ground:
                if (LandLayer.Equals(layer) && LayerDirty)
                {
                    Ground = Land.terrainData.GetAlphamaps(0, 0, SplatMapRes, SplatMapRes);
                    LayerDirty = false;
                }
                return Ground;
            case LandLayers.Biome:
                if (LandLayer.Equals(layer) && LayerDirty)
                {
                    Biome = Land.terrainData.GetAlphamaps(0, 0, SplatMapRes, SplatMapRes);
                    LayerDirty = false;
                }
                return Biome;
            case LandLayers.Topology:
                if (LandLayer.Equals(layer) && TopologyLayer == topology && LayerDirty)
                {
                    Topology[topology] = Land.terrainData.GetAlphamaps(0, 0, SplatMapRes, SplatMapRes);
                    LayerDirty = false;
                }
                return Topology[topology];
            default:
                Debug.LogError($"GetSplatMap({layer}) cannot return type float[,,].");
                return null;
        }
    }

    public static bool[,] GetAlphaMap()
    {
        if (AlphaDirty)
        {
            bool[,] tempAlpha = Land.terrainData.GetHoles(0, 0, AlphaMapRes, AlphaMapRes);
            Parallel.For(0, SplatMapRes, i =>
            {
                for (int j = 0; j < SplatMapRes; j++)
                    Alpha[i, j] = tempAlpha[i * 2, j * 2];
            });
            AlphaDirty = false;
        }
        return Alpha;
    }

    /// <summary>Sets SplatMap of the selected LandLayer.</summary>
    /// <param name="layer">The layer to set the data to.</param>
    /// <param name="topology">The topology layer if layer is topology.</param>
    public static void SetSplatMap(float[,,] array, LandLayers layer, int topology = 0)
    {
        if (array == null)
        {
            Debug.LogError($"SetSplatMap(array) is null.");
            return;
        }

        if (layer.Equals(LandLayers.Alpha))
        {
            Debug.LogWarning($"SetSplatMap(float[,,], {layer}) is not a valid layer to set. Use SetAlphaMap(bool[,]) to set {layer}.");
            return;
        }

        // Check for array dimensions not matching alphamap.
        if (array.GetLength(0) != SplatMapRes || array.GetLength(1) != SplatMapRes || array.GetLength(2) != LayerCount(layer))
        {
            Debug.LogError($"SetSplatMap(array[{array.GetLength(0)}, {array.GetLength(1)}, {LayerCount(layer)}]) dimensions invalid, should be " +
                $"array[{ SplatMapRes}, { SplatMapRes}, {LayerCount(layer)}].");
            return;
        }

        switch (layer)
        {
            case LandLayers.Ground:
                Ground = array;
                break;
            case LandLayers.Biome:
                Biome = array;
                break;
            case LandLayers.Topology:
                Topology[topology] = array;
                break;
        }

        if (LandLayer.Equals(layer))
        {
            if (!GetTerrainLayers().Equals(Land.terrainData.terrainLayers))
                Land.terrainData.terrainLayers = GetTerrainLayers();

            Land.terrainData.SetAlphamaps(0, 0, array);
            LayerDirty = false;
        }
    }

    /// <summary>Sets the AlphaMap (Holes) of the terrain.</summary>
    public static void SetAlphaMap(bool[,] array)
    {
        if (array == null)
        {
            Debug.LogError($"SetAlphaMap(array) is null.");
            return;
        }

        // Check for array dimensions not matching alphamap.
        if (array.GetLength(0) != AlphaMapRes || array.GetLength(1) != AlphaMapRes)
        {
            // Special case for converting Alphamaps from the Rust resolution to the Unity Editor resolution. 
            if (array.GetLength(0) == SplatMapRes && array.GetLength(1) == SplatMapRes)
            {
                if (Alpha == null || Alpha.GetLength(0) != AlphaMapRes)
                    Alpha = new bool[AlphaMapRes, AlphaMapRes];

                Parallel.For(0, AlphaMapRes, i =>
                {
                    for (int j = 0; j < AlphaMapRes; j++)
                        Alpha[i, j] = array[i / 2, j / 2];
                });

                Land.terrainData.SetHoles(0, 0, Alpha);
                AlphaDirty = false;
                return;
            }

            else
            {
                Debug.LogError($"SetAlphaMap(array[{array.GetLength(0)}, {array.GetLength(1)}]) dimensions invalid, should be array[{AlphaMapRes}, {AlphaMapRes}].");
                return;
            }
        }

        Alpha = array;
        Land.terrainData.SetHoles(0, 0, Alpha);
        AlphaDirty = false;
    }

    /// <summary>Sets and initialises the Splat/AlphaMaps of all layers from MapInfo. Called when first loading/creating a map.</summary>
    private static void SetSplatMaps(MapInfo mapInfo)
    {
        SplatMapRes = mapInfo.splatRes;

        SetSplatMap(mapInfo.splatMap, LandLayers.Ground);
        SetSplatMap(mapInfo.biomeMap, LandLayers.Biome);
        SetAlphaMap(mapInfo.alphaMap);

        TopologyData.Set(mapInfo.topology);
        for (int i = 0; i < TerrainTopology.COUNT; i++)
            SetSplatMap(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(i)), LandLayers.Topology, i);
    }

    private static void SplatMapChanged(Terrain terrain, string textureName, RectInt texelRegion, bool synched)
    {
        if (!IsLoading && Land.Equals(terrain) && Mouse.current.leftButton.isPressed)
        {
            Callbacks.OnLayerUpdated(LandLayer, TopologyLayer);
            LayerDirty = true;
        }
    }
    #endregion
    #endregion

    #region HeightMap
    #region Fields
    /// <summary>The current slope values stored as [x, y].</summary>
    /// <value>Slope angle in degrees, between 0 - 90.</value>
    private static float[,] Slope;
    /// <summary>The current height values stored as [x, y].</summary>
    /// <value>Height in metres, between 0 - 1000.</value> 
    private static float[,] Height;
    /// <summary>Resolution of the HeightMap.</summary>
    /// <value>Power of ^2 + 1, between 1025 - 4097.</value>
    public static int HeightMapRes { get; private set; }
    /// <summary>Resolution of the AlphaMap.</summary>
    /// <value>Power of ^2, between 1024 - 4096.</value>
    public static int AlphaMapRes { get => HeightMapRes - 1; }
    #endregion

    #region Methods
    public static void ResetHeightCache()
    {
        Slope = null;
        Height = null;
    }

    public static void UpdateHeightCache()
    {
        UpdateSlopes();
        UpdateHeights();
    }

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

        if (Slope == null)
            return Land.terrainData.GetSteepness((float)x / SplatMapRes, (float)y / SplatMapRes);

        return Slope[x, y];
    }

    /// <summary>Returns a 2D array of the slope values.</summary>
    /// <returns>Floats within the range 0° - 90°.</returns>
    public static float[,] GetSlopes()
    {
        if (Slope != null)
            return Slope;

        for (int x = 0; x < SplatMapRes; x++)
            for (int y = 0; y < SplatMapRes; y++)
                Slope[x, y] = Land.terrainData.GetSteepness((float)x / SplatMapRes, (float)y / SplatMapRes);

        return Slope;
    }

    /// <summary>Updates cached Slope values with current.</summary>
    public static void UpdateSlopes() => GetSlopes();

    /// <summary>Returns the height of the HeightMap at the selected coords.</summary>
    /// <returns>Float within the range 0m - 1000m. Null if out of bounds.</returns>
    public static float? GetHeight(int x, int y)
    {
        if (!IsValidIndex(x, y))
        {
            Debug.LogError($"Index: {x},{y} is out of bounds for GetHeight.");
            return 0;
        }

        if (Height == null)
            return Land.terrainData.GetInterpolatedHeight((float)x / SplatMapRes, (float)y / SplatMapRes); ;

        return Height[x, y];
    }

    /// <summary>Returns a 2D array of the height values.</summary>
    /// <returns>Floats within the range 0m - 1000m.</returns>
    public static float[,] GetHeights(Enum terrain = Enum.Land)
    {
        if (Height != null && terrain == Enum.Land)
            return Height;
        if (terrain == Enum.Land)
        {
            Height = Land.terrainData.GetInterpolatedHeights(0, 0, SplatMapRes, SplatMapRes, 1f / SplatMapRes, 1f / SplatMapRes);
            return Height;
        }
        return Water.terrainData.GetInterpolatedHeights(0, 0, SplatMapRes, SplatMapRes, 1f / SplatMapRes, 1f / SplatMapRes);
    }

    /// <summary>Updates cached Height values with current.</summary>
    public static void UpdateHeights() => GetHeights();

    /// <summary>Callback for whenever the heightmap is updated.</summary>
    private static void HeightMapChanged(Terrain terrain, RectInt heightRegion, bool synched)
    {
        if (terrain.Equals(Land))
            ResetHeightCache();
    }
    #endregion
    #endregion

    #region Terrains
    #region Fields
    /// <summary>The land terrain in the scene.</summary>
    public static Terrain Land { get; private set; }
    /// <summary>The water terrain in the scene.</summary>
    public static Terrain Water { get; private set; }
    /// <summary>The material used by the water terrain object.</summary>
    public static Material WaterMaterial { get; private set; }
    /// <summary>The size of the Land and Water terrains in the scene.</summary>
    public static Vector3 TerrainSize { get => Land.terrainData.size; }
    /// <summary>The offset of the terrain from World Space.</summary>
    public static Vector3 MapOffset { get => 0.5f * TerrainSize; }
    /// <summary>The condition of the current terrain.</summary>
    /// <value>True = Terrain is loading / False = Terrain is loaded.</value>
    public static bool IsLoading { get; private set; } = true;

    public enum Enum
    {
        Land,
        Water
    }
    #endregion

    #region Methods
    public static void SetTerrainReferences()
    {
        Water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        Land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        WaterMaterial = Water.materialTemplate;
    }

    public static void SetWaterTransparency(float alpha)
    {
        Color _color = WaterMaterial.color;
        _color.a = alpha;
        WaterMaterial.color = _color;
    }

    /// <summary>Sets up the inputted terrain's terraindata.</summary>
    private static void SetupTerrain(MapInfo mapInfo, Terrain terrain)
    {
        terrain.terrainData.heightmapResolution = mapInfo.terrainRes;
        terrain.terrainData.size = mapInfo.size;
        terrain.terrainData.alphamapResolution = mapInfo.splatRes;
        terrain.terrainData.baseMapResolution = mapInfo.splatRes;
        terrain.terrainData.SetHeights(0, 0, terrain.Equals(Land) ? mapInfo.land.heights : mapInfo.water.heights);
    }

    /// <summary>Loads and sets the Land and Water terrain objects.</summary>
    private static void SetTerrain(MapInfo mapInfo, int progressID)
    {
        HeightMapRes = mapInfo.terrainRes;

        SetupTerrain(mapInfo, Land);
        Progress.Report(progressID, .5f, "Loaded: Land");
        SetupTerrain(mapInfo, Water);
        Progress.Report(progressID, .9f, "Loaded: Water");
        
        AreaManager.Reset();
        Progress.Report(progressID, 0.99f, "Loaded " + TerrainSize.x + " size map.");
    }

    /// <summary>Loads and sets up the terrain and associated splatmaps.</summary>
    /// <param name="mapInfo">Struct containing all info about the map to initialise.</param>
    public static void Load(MapInfo mapInfo, int progressID)
    {
        IsLoading = true;
        SetTerrain(mapInfo, progressID);
        SetSplatMaps(mapInfo);
        IsLoading = false;
    }
    #endregion
    #endregion

    #region Terrain Layers
    /// <summary>The Terrain layers used by the terrain for paint operations</summary>
    private static TerrainLayer[] GroundLayers = null, BiomeLayers = null, TopologyLayers = null;

    #region Methods
    /// <summary>Clears the alphamap textures currently on the undo heap.</summary>
    private static void ClearTerrainUndo()
    {
        foreach (var item in Land.terrainData.alphamapTextures)
            Undo.ClearUndo(item);
    }

    /// <summary>Sets the unity terrain references if not already set, and returns the current terrain layers.</summary>
    /// <returns>Array of TerrainLayers currently displayed on the Land Terrain.</returns>
    public static TerrainLayer[] GetTerrainLayers()
    {
        if (GroundLayers == null || BiomeLayers == null || TopologyLayers == null)
            SetTerrainLayers();

        return LandLayer switch
        {
            LandLayers.Ground => GroundLayers,
            LandLayers.Biome => BiomeLayers,
            _ => TopologyLayers
        };
    }

    /// <summary>Sets the TerrainLayer references in TerrainManager to the asset on disk.</summary>
    public static void SetTerrainLayers()
    {
        GroundLayers = GetGroundLayers();
        BiomeLayers = GetBiomeLayers();
        TopologyLayers = GetTopologyLayers();
        AssetDatabase.SaveAssets();
    }

    private static TerrainLayer[] GetGroundLayers()
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

    private static TerrainLayer[] GetBiomeLayers()
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

    private static TerrainLayer[] GetTopologyLayers()
    {
        TerrainLayer[] textures = new TerrainLayer[2];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Topology/Active.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Topology/active");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Topology/InActive.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Topology/inactive");
        return textures;
    }
    #endregion
    #endregion

    #region Layers
    #region Fields
    /// <summary>The LandLayer currently being displayed on the terrain.</summary>
    public static LandLayers LandLayer { get; private set; }
    /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LandLayer is set to topology.</summary>
    public static TerrainTopology.Enum TopologyLayerEnum { get; private set; }
    /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LandLayer is set to topology.</summary>
    public static int TopologyLayer { get => TerrainTopology.TypeToIndex((int)TopologyLayerEnum); }
    /// <summary>The previously selected topology layer. Used to save the Topology layer before displaying the new one.</summary>
    public static int LastTopologyLayer { get; private set; } = 0;
    /// <summary>The state of the current layer data.</summary>
    /// <value>True = Layer has been modified and not saved / False = Layer has not been modified since last saved.</value>
    public static bool LayerDirty { get; private set; } = false;
    /// <summary>The amount of TerrainLayers used on the current LandLayer.</summary>
    public static int Layers => LayerCount(LandLayer);
    #endregion

    #region Methods
    /// <summary>Saves any changes made to the Alphamaps, including paint operations.</summary>
    public static void SaveLayer()
    {
        SetSplatMap(GetSplatMap(LandLayer, TopologyLayer), LandLayer, TopologyLayer);
        Callbacks.OnLayerSaved(LandLayer, TopologyLayer);
    }

    /// <summary>Changes the active Land and Topology Layers.</summary>
    /// <param name="layer">The LandLayer to change to.</param>
    /// <param name="topology">The Topology layer to change to.</param>
    public static void ChangeLayer(LandLayers layer, int topology = 0)
    {
        if (layer.Equals(LandLayers.Alpha))
            return;

        SaveLayer();
        LandLayer = layer;
        SetSplatMap(GetSplatMap(layer, topology), layer, topology);
        
        Callbacks.OnLayerChanged(layer, topology);
    }

    /// <summary>Layer count in layer chosen, used for determining the size of the splatmap array.</summary>
    /// <param name="layer">The LandLayer to return the texture count from. (Ground, Biome or Topology)</param>
    public static int LayerCount(LandLayers layer)
    {
        return layer switch
        {
            LandLayers.Ground => 8,
            LandLayers.Biome => 4,
            _ => 2
        };
    }
    #endregion
    #endregion

    #region Other
    [InitializeOnLoadMethod]
    private static void Init()
    {
        TerrainCallbacks.heightmapChanged += HeightMapChanged;
        TerrainCallbacks.textureChanged += SplatMapChanged;
        EditorApplication.update += ProjectLoaded;
    }

    private static void ProjectLoaded()
    {
        EditorApplication.update -= ProjectLoaded;
        SetTerrainReferences();
    }
    #endregion
}