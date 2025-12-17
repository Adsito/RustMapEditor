using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Threading.Tasks;
using RustMapEditor.Maths;
using static WorldConverter;
using static AreaManager;

public static class TerrainManager
{
    #region Init
    [InitializeOnLoadMethod]
    private static void Init()
    {
        TerrainCallbacks.heightmapChanged += HeightMapChanged;
        TerrainCallbacks.textureChanged += SplatMapChanged;
        EditorApplication.update += OnProjectLoad;
    }

    private static void OnProjectLoad()
    {
        EditorApplication.update -= OnProjectLoad;
        FilterTexture = Resources.Load<Texture>("Textures/Brushes/White128");
        SetTerrainReferences();
    }
    #endregion

    public static class Callbacks
    {
        public delegate void Layer(LayerType layer, int? topology = null);
        public delegate void HeightMap(TerrainType terrain);

        /// <summary>Called after the active layer is changed. </summary>
        public static event Layer LayerChanged;
        /// <summary>Called after the active layer is saved. </summary>
        public static event Layer LayerSaved;
        /// <summary>Called when the active layer is dirtied/updated.</summary>
        public static event Layer LayerUpdated;
        /// <summary>Called when the Land/Water heightmap is dirtied/updated.</summary>
        public static event HeightMap HeightMapUpdated;

        public static void InvokeLayerChanged(LayerType layer, int topology) => LayerChanged?.Invoke(layer, topology);
        public static void InvokeLayerSaved(LayerType layer, int topology) => LayerSaved?.Invoke(layer, topology);
        public static void InvokeLayerUpdated(LayerType layer, int topology) => LayerUpdated?.Invoke(layer, topology);
        public static void InvokeHeightMapUpdated(TerrainType terrain) => HeightMapUpdated?.Invoke(terrain);
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
    /// <summary>Topology layers [topology][x, y, texture] use <seealso cref="TerrainTopology.TypeToIndex(int)"/> for topology layer indexes.</summary>
    /// <value>Texture 0 = Active / Texture 1 = Inactive.</value>
    public static float[][,,] Topology { get; private set; } = new float[TerrainTopology.COUNT][,,];
    /// <summary>Resolution of the splatmap/alphamap.</summary>
    /// <value>Power of ^2, between 512 - 2048.</value>
    public static int SplatMapRes { get; private set; }
    /// <summary>The world size of each splat relative to the terrain size it covers.</summary>
    public static float SplatSize { get => Land.terrainData.size.x / SplatMapRes; }

    public static bool AlphaDirty { get; set; } = true;
    #endregion

    #region Methods
    /// <summary>Returns the SplatMap at the selected LayerType.</summary>
    /// <param name="layer">The LayerType to return. (Ground, Biome)</param>
    /// <returns>3D float array in Alphamap format. [x, y, Texture]</returns>
    public static float[,,] GetSplatMap(LayerType layer, int topology = -1)
    {
        switch (layer)
        {
            case LayerType.Ground:
                if (CurrentLayerType == layer && LayerDirty)
                {
                    Ground = Land.terrainData.GetAlphamaps(0, 0, SplatMapRes, SplatMapRes);
                    LayerDirty = false;
                }
                return Ground;
            case LayerType.Biome:
                if (CurrentLayerType == layer && LayerDirty)
                {
                    Biome = Land.terrainData.GetAlphamaps(0, 0, SplatMapRes, SplatMapRes);
                    LayerDirty = false;
                }
                return Biome;
            case LayerType.Topology:
                if (topology < 0 || topology >= TerrainTopology.COUNT)
                {
                    Debug.LogError($"GetSplatMap({layer}, {topology}) topology parameter out of bounds. Should be between 0 - {TerrainTopology.COUNT - 1}");
                    return null;
                }
                if (CurrentLayerType == layer && TopologyLayer == topology && LayerDirty)
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
            Alpha = Land.terrainData.GetHoles(0, 0, AlphaMapRes, AlphaMapRes);
            AlphaDirty = false;
        }
        return Alpha;
    }

    /// <summary>Sets SplatMap of the selected LayerType.</summary>
    /// <param name="layer">The layer to set the data to.</param>
    /// <param name="topology">The topology layer if layer is topology.</param>
    public static void SetSplatMap(float[,,] array, LayerType layer, int topology = -1)
    {
        if (array == null)
        {
            Debug.LogError($"SetSplatMap(array) is null.");
            return;
        }

        if (layer == LayerType.Alpha)
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
            case LayerType.Ground:
                Ground = array;
                break;
            case LayerType.Biome:
                Biome = array;
                break;
            case LayerType.Topology:
                if (topology < 0 || topology >= TerrainTopology.COUNT)
                {
                    Debug.LogError($"SetSplatMap({layer}, {topology}) topology parameter out of bounds. Should be between 0 - {TerrainTopology.COUNT - 1}");
                    return;
                }
                Topology[topology] = array;
                break;
        }

        if (CurrentLayerType == layer)
        {
            if (CurrentLayerType == LayerType.Topology && TopologyLayer != topology)
                return;
            if (!GetTerrainLayers().Equals(Land.terrainData.terrainLayers))
                Land.terrainData.terrainLayers = GetTerrainLayers();

            RegisterSplatMapUndo($"{layer}");
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

    private static void SplatMapChanged(Terrain terrain, string textureName, RectInt texelRegion, bool synched)
    {
        if (!IsLoading && Land.Equals(terrain) && Mouse.current.leftButton.isPressed)
        {
            switch (textureName)
            {
                case "holes":
                    AlphaDirty = true;
                    Callbacks.InvokeLayerUpdated(LayerType.Alpha, TopologyLayer);
                    break;
                case "alphamap":
                    LayerDirty = true;
                    Callbacks.InvokeLayerUpdated(CurrentLayerType, TopologyLayer);
                    break;
            }
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

    private static Texture FilterTexture;
    private static Vector2 HeightMapCentre { get => new Vector2(0.5f, 0.5f); }
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
            return Land.terrainData.GetSteepness((float)y / SplatMapRes, (float)x / SplatMapRes);

        return Slope[x, y];
    }

    /// <summary>Returns a 2D array of the slope values.</summary>
    /// <returns>Floats within the range 0° - 90°.</returns>
    public static float[,] GetSlopes()
    {
        if (Slope != null)
            return Slope;
        if (Slope == null)
            Slope = new float[SplatMapRes, SplatMapRes];

        for (int x = 0; x < SplatMapRes; x++)
            for (int y = 0; y < SplatMapRes; y++)
                Slope[x, y] = Land.terrainData.GetSteepness((float)y / SplatMapRes, (float)x / SplatMapRes);

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
    public static float[,] GetHeights(TerrainType terrain = TerrainType.Land)
    {
        if (Height != null && terrain == TerrainType.Land)
            return Height;
        if (terrain == TerrainType.Land)
        {
            Height = Land.terrainData.GetInterpolatedHeights(0, 0, SplatMapRes, SplatMapRes, 1f / SplatMapRes, 1f / SplatMapRes);
            return Height;
        }
        return Water.terrainData.GetInterpolatedHeights(0, 0, SplatMapRes, SplatMapRes, 1f / SplatMapRes, 1f / SplatMapRes);
    }

    /// <summary>Updates cached Height values with current.</summary>
    public static void UpdateHeights() => GetHeights();

    /// <summary>Returns the HeightMap array of the selected terrain.</summary>
    /// <param name="terrain">The HeightMap to return.</param>
    public static float[,] GetHeightMap(TerrainType terrain = TerrainType.Land)
    {
        if (terrain == TerrainType.Land)
            return Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes);
        return Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes);
    }

    /// <summary>Rotates the HeightMap 90° Clockwise or Counter Clockwise.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateHeightMap(bool CW, TerrainType terrain = TerrainType.Land, Area dmns = null)
    {
        RegisterHeightMapUndo(terrain, "Rotate HeightMap");
        if (terrain == TerrainType.Land)
            Land.terrainData.SetHeights(0, 0, Array.Rotate(GetHeightMap(), CW, dmns));
        else
            Water.terrainData.SetHeights(0, 0, Array.Rotate(GetHeightMap(TerrainType.Water), CW, dmns));
    }

    /// <summary>Sets the HeightMap to the height input.</summary>
    /// <param name="height">The height to set.</param>
    public static void SetHeightMapHeight(float height, TerrainType terrain = TerrainType.Land, Area dmns = null)
    {
        height /= 1000f; // Normalises user input to a value between 0 - 1f.
        RegisterHeightMapUndo(terrain, "Set HeightMap Height");

        if (terrain == TerrainType.Land)
            Land.terrainData.SetHeights(0, 0, Array.SetValues(GetHeightMap(), height, dmns));
        else
            Water.terrainData.SetHeights(0, 0, Array.SetValues(GetHeightMap(TerrainType.Water), height, dmns));
    }

    /// <summary>Inverts the HeightMap heights.</summary>
    public static void InvertHeightMap(TerrainType terrain = TerrainType.Land, Area dmns = null)
    {
        RegisterHeightMapUndo(terrain, "Invert HeightMap");
        if (terrain == TerrainType.Land)
            Land.terrainData.SetHeights(0, 0, Array.Invert(GetHeightMap(), dmns));
        else
            Water.terrainData.SetHeights(0, 0, GetHeightMap(TerrainType.Water));
    }

    /// <summary> Normalises the HeightMap between two heights.</summary>
    /// <param name="normaliseLow">The lowest height the HeightMap should be.</param>
    /// <param name="normaliseHigh">The highest height the HeightMap should be.</param>
    public static void NormaliseHeightMap(float normaliseLow, float normaliseHigh, TerrainType terrain = TerrainType.Land, Area dmns = null)
    {
        normaliseLow /= 1000f; normaliseHigh /= 1000f; // Normalises user input to a value between 0 - 1f.
        RegisterHeightMapUndo(terrain, "Normalise HeightMap");

        if (terrain == TerrainType.Land)
            Land.terrainData.SetHeights(0, 0, Array.Normalise(GetHeightMap(), normaliseLow, normaliseHigh, dmns));
        else
            Water.terrainData.SetHeights(0, 0, Array.Normalise(GetHeightMap(TerrainType.Water), normaliseLow, normaliseHigh, dmns));
    }

    /// <summary>Increases or decreases the HeightMap by the offset.</summary>
    /// <param name="offset">The amount to offset by. Negative values offset down.</param>
    /// <param name="clampOffset">Check if offsetting the HeightMap would exceed the min-max values.</param>
    public static void OffsetHeightMap(float offset, bool clampOffset, TerrainType terrain = TerrainType.Land, Area dmns = null)
    {
        offset /= 1000f; // Normalises user input to a value between 0 - 1f.
        RegisterHeightMapUndo(terrain, "Offset HeightMap");

        if (terrain == TerrainType.Land)
            Land.terrainData.SetHeights(0, 0, Array.Offset(GetHeightMap(), offset, clampOffset, dmns));
        else
            Water.terrainData.SetHeights(0, 0, Array.Offset(GetHeightMap(TerrainType.Water), offset, clampOffset, dmns));
    }

    /// <summary>Sets the HeightMap level to the minimum if it's below.</summary>
    /// <param name="minimumHeight">The minimum height to set.</param>
    /// <param name="maximumHeight">The maximum height to set.</param>
    public static void ClampHeightMap(float minimumHeight, float maximumHeight, TerrainType terrain = TerrainType.Land, Area dmns = null)
    {
        minimumHeight /= 1000f; maximumHeight /= 1000f; // Normalises user input to a value between 0 - 1f.
        RegisterHeightMapUndo(terrain, "Clamp HeightMap");

        if (terrain == TerrainType.Land)
            Land.terrainData.SetHeights(0, 0, Array.ClampValues(GetHeightMap(), minimumHeight, maximumHeight, dmns));
        else
            Water.terrainData.SetHeights(0, 0, Array.ClampValues(GetHeightMap(TerrainType.Water), minimumHeight, maximumHeight, dmns));
    }

    /// <summary>Terraces the HeightMap.</summary>
    /// <param name="featureSize">The height of each terrace.</param>
    /// <param name="interiorCornerWeight">The weight of the terrace effect.</param>
    public static void TerraceErodeHeightMap(float featureSize, float interiorCornerWeight)
    {
        RegisterHeightMapUndo(TerrainType.Land, "Erode HeightMap");

        Material mat = new Material((Shader)AssetDatabase.LoadAssetAtPath("Packages/com.unity.terrain-tools/Shaders/TerraceErosion.shader", typeof(Shader)));
        UnityEngine.TerrainTools.BrushTransform brushXform = UnityEngine.TerrainTools.TerrainPaintUtility.CalculateBrushTransform(Land, HeightMapCentre, Land.terrainData.size.x, 0.0f);
        UnityEngine.TerrainTools.PaintContext paintContext = UnityEngine.TerrainTools.TerrainPaintUtility.BeginPaintHeightmap(Land, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(1.0f, featureSize, interiorCornerWeight, 0.0f);
        mat.SetTexture("_BrushTex", FilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        UnityEngine.TerrainTools.TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        UnityEngine.TerrainTools.TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - TerraceErosion");
    }

    /// <summary>Smooths the HeightMap.</summary>
    /// <param name="filterStrength">The strength of the smoothing.</param>
    /// <param name="blurDirection">The direction the smoothing should preference. Between -1f - 1f.</param>
    public static void SmoothHeightMap(float filterStrength, float blurDirection)
    {
        RegisterHeightMapUndo(TerrainType.Land, "Smooth HeightMap");

        Material mat = UnityEngine.TerrainTools.TerrainPaintUtility.GetBuiltinPaintMaterial();
        UnityEngine.TerrainTools.BrushTransform brushXform = UnityEngine.TerrainTools.TerrainPaintUtility.CalculateBrushTransform(Land, HeightMapCentre, Land.terrainData.size.x, 0.0f);
        UnityEngine.TerrainTools.PaintContext paintContext = UnityEngine.TerrainTools.TerrainPaintUtility.BeginPaintHeightmap(Land, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(filterStrength, 0.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", FilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        Vector4 smoothWeights = new Vector4(Mathf.Clamp01(1.0f - Mathf.Abs(blurDirection)), Mathf.Clamp01(-blurDirection), Mathf.Clamp01(blurDirection), 0.0f);
        mat.SetVector("_SmoothWeights", smoothWeights);
        UnityEngine.TerrainTools.TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)UnityEngine.TerrainTools.TerrainBuiltinPaintMaterialPasses.SmoothHeights);
        UnityEngine.TerrainTools.TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - Smooth Heights");
    }

    /// <summary>Callback for whenever the heightmap is updated.</summary>
    private static void HeightMapChanged(Terrain terrain, RectInt heightRegion, bool synched)
    {
        if (terrain.Equals(Land))
            ResetHeightCache();

        Callbacks.InvokeHeightMapUpdated(terrain.Equals(Land) ? TerrainType.Land : TerrainType.Water);
    }
    #endregion
    #endregion

    #region Terrains
    #region Fields
    /// <summary>The Land terrain in the scene.</summary>
    public static Terrain Land { get; private set; }
    /// <summary>The Water terrain in the scene.</summary>
    public static Terrain Water { get; private set; }
    /// <summary>The material used by the Water terrain object.</summary>
    public static Material WaterMaterial { get; private set; }
    /// <summary>The size of the Land and Water terrains in the scene.</summary>
    public static Vector3 TerrainSize { get => Land.terrainData.size; }
    /// <summary>The offset of the terrain from World Space.</summary>
    public static Vector3 MapOffset { get => 0.5f * TerrainSize; }
    /// <summary>The condition of the current terrain.</summary>
    /// <value>True = Terrain is loading / False = Terrain is loaded.</value>
    public static bool IsLoading { get; private set; } = false;

    /// <summary>Enum of the 2 different terrains in scene. (Land, Water). Required to reference the terrain objects across the Editor.</summary>
    public enum TerrainType
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

    /// <summary>Loads and sets up the terrain and associated splatmaps.</summary>
    /// <param name="mapInfo">Struct containing all info about the map to initialise.</param>
    public static void Load(MapInfo mapInfo, int progressID)
    {
        if (!IsLoading)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.Load(mapInfo, progressID));
    }
    #endregion
    #endregion

    #region Terrain Layers
    /// <summary>The Terrain layers used by the terrain for paint operations</summary>
    private static TerrainLayer[] GroundLayers = null, BiomeLayers = null, TopologyLayers = null;

    #region Methods
    /// <summary>Sets the unity terrain references if not already set, and returns the current terrain layers.</summary>
    /// <returns>Array of TerrainLayers currently displayed on the Land Terrain.</returns>
    public static TerrainLayer[] GetTerrainLayers()
    {
        if (GroundLayers == null || BiomeLayers == null || TopologyLayers == null)
            SetTerrainLayers();

        return CurrentLayerType switch
        {
            LayerType.Ground => GroundLayers,
            LayerType.Biome => BiomeLayers,
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
        TerrainLayer[] textures = new TerrainLayer[5];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arid.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arid");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Temperate.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/temperate");
        textures[2] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Tundra.terrainlayer");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/tundra");
        textures[3] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arctic.terrainlayer");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arctic");
        textures[4] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Jungle.terrainlayer");
        textures[4].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/jungle");
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
    /// <summary>The LayerType currently being displayed on the terrain.</summary>
    public static LayerType CurrentLayerType { get; private set; }
    /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LayerType is set to topology.</summary>
    public static TerrainTopology.Enum TopologyLayerEnum { get; private set; }
    /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LayerType is set to topology.</summary>
    public static int TopologyLayer { get => TerrainTopology.TypeToIndex((int)TopologyLayerEnum); }
    /// <summary>The state of the current layer data.</summary>
    /// <value>True = Layer has been modified and not saved / False = Layer has not been modified since last saved.</value>
    public static bool LayerDirty { get; private set; } = false;
    /// <summary>The amount of TerrainLayers used on the current LayerType.</summary>
    public static int Layers => LayerCount(CurrentLayerType);

    public enum LayerType
    {
        Ground,
        Biome,
        Alpha,
        Topology
    }
    #endregion

    #region Methods
    /// <summary>Saves any changes made to the Alphamaps, including paint operations.</summary>
    public static void SaveLayer()
    {
        SetSplatMap(GetSplatMap(CurrentLayerType, TopologyLayer), CurrentLayerType, TopologyLayer);
        Callbacks.InvokeLayerSaved(CurrentLayerType, TopologyLayer);
    }

    /// <summary>Changes the active Land and Topology Layers.</summary>
    /// <param name="layer">The LayerType to change to.</param>
    /// <param name="topology">The Topology layer to change to.</param>
    public static void ChangeLayer(LayerType layer, int topology = -1)
    {
        if (layer == LayerType.Alpha)
            return;
        if (layer == LayerType.Topology && (topology < 0 || topology >= TerrainTopology.COUNT))
        {
            Debug.LogError($"ChangeLayer({layer}, {topology}) topology parameter out of bounds. Should be between 0 - {TerrainTopology.COUNT - 1}");
            return;
        }

        if (LayerDirty)
            SaveLayer();

        CurrentLayerType = layer;
        TopologyLayerEnum = (TerrainTopology.Enum)TerrainTopology.IndexToType(topology);
        SetSplatMap(GetSplatMap(layer, topology), layer, topology);
        ClearSplatMapUndo();

        Callbacks.InvokeLayerChanged(layer, topology);
    }

    /// <summary>Layer count in layer chosen, used for determining the size of the splatmap array.</summary>
    /// <param name="layer">The LayerType to return the texture count from. (Ground, Biome or Topology)</param>
    public static int LayerCount(LayerType layer)
    {
        return layer switch
        {
            LayerType.Ground => 8,
            LayerType.Biome => 5,
            _ => 2
        };
    }
    #endregion
    #endregion

    #region Other
    /// <summary>Registers changes made to the HeightMap after the function is called.</summary>
    /// <param name="terrain">HeightMap to record.</param>
    /// <param name="name">Name of the Undo object on the stack.</param>
    public static void RegisterHeightMapUndo(TerrainType terrain, string name)
    {
        Undo.RegisterCompleteObjectUndo(terrain == TerrainType.Land ? Land.terrainData.heightmapTexture : Water.terrainData.heightmapTexture, name);
    }

    /// <summary>Registers changes made to the SplatMap after the function is called.</summary>
    /// <param name="terrain">SplatMap to record.</param>
    /// <param name="name">Name of the Undo object on the stack.</param>
    public static void RegisterSplatMapUndo(string name) => Undo.RegisterCompleteObjectUndo(Land.terrainData.alphamapTextures, name);

    /// <summary>Clears all undo operations on the currently displayed SplatMap.</summary>
    public static void ClearSplatMapUndo()
    {
        foreach (var tex in Land.terrainData.alphamapTextures)
            Undo.ClearUndo(tex);
    }
    #endregion

    private class Coroutines
    {
        /// <summary>Loads and sets up the terrain and associated splatmaps.</summary>
        /// <param name="mapInfo">Struct containing all info about the map to initialise.</param>
        public static IEnumerator Load(MapInfo mapInfo, int progressID)
        {
            IsLoading = true;
            yield return SetTerrains(mapInfo, progressID);
            yield return SetSplatMaps(mapInfo, progressID);
            ClearSplatMapUndo();
            AreaManager.Reset();
            Progress.Report(progressID, .99f, "Loaded Terrain.");
            IsLoading = false;
        }

        /// <summary>Loads and sets the Land and Water terrain objects.</summary>
        private static IEnumerator SetTerrains(MapInfo mapInfo, int progressID)
        {
            HeightMapRes = mapInfo.terrainRes;

            yield return SetupTerrain(mapInfo, Water);
            Progress.Report(progressID, .2f, "Loaded: Water.");
            yield return SetupTerrain(mapInfo, Land);
            Progress.Report(progressID, .5f, "Loaded: Land.");
        }

        /// <summary>Sets up the inputted terrain's terraindata.</summary>
        private static IEnumerator SetupTerrain(MapInfo mapInfo, Terrain terrain)
        {
            if (terrain.terrainData.size != mapInfo.size)
            {
                terrain.terrainData.heightmapResolution = mapInfo.terrainRes;
                terrain.terrainData.size = mapInfo.size;
                terrain.terrainData.alphamapResolution = mapInfo.splatRes;
                terrain.terrainData.baseMapResolution = mapInfo.splatRes;
            }
            terrain.terrainData.SetHeights(0, 0, terrain.Equals(Land) ? mapInfo.land.heights : mapInfo.water.heights);
            yield return null;
        }

        /// <summary>Sets and initialises the Splat/AlphaMaps of all layers from MapInfo. Called when first loading/creating a map.</summary>
        private static IEnumerator SetSplatMaps(MapInfo mapInfo, int progressID)
        {
            SplatMapRes = mapInfo.splatRes;

            SetSplatMap(mapInfo.splatMap, LayerType.Ground);
            SetSplatMap(mapInfo.biomeMap, LayerType.Biome);
            SetAlphaMap(mapInfo.alphaMap);
            yield return null;
            Progress.Report(progressID, .8f, "Loaded: Splats.");

            TopologyData.Set(mapInfo.topology);
            Parallel.For(0, TerrainTopology.COUNT, i =>
            {
                if (CurrentLayerType != LayerType.Topology || TopologyLayer != i)
                    SetSplatMap(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(i)), LayerType.Topology, i);
            });
            SetSplatMap(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(TopologyLayer)), LayerType.Topology, TopologyLayer);
            Progress.Report(progressID, .9f, "Loaded: Topologies.");
        }
    }
}