using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;
using static TerrainManager;
using static RustMapEditor.Maths.Array;
using static WorldConverter;
using Unity.EditorCoroutines.Editor;
using System.Collections;

public static class MapManager
{
    public static class Callbacks
    {
        public delegate void MapManagerCallback(string mapName = "");

        /// <summary>Called after a map has been loaded. Calls on both map loaded and map created.</summary>
        public static event MapManagerCallback MapLoaded;

        /// <summary>Called after map has been saved and written to disk.</summary>
        public static event MapManagerCallback MapSaved;

        public static void OnMapLoaded(string mapName = "") => MapLoaded?.Invoke(mapName);
        public static void OnMapSaved(string mapName = "") => MapSaved?.Invoke(mapName);
    }

    public static Texture terrainFilterTexture;

    [InitializeOnLoadMethod]
    static void Init()
    {
        terrainFilterTexture = Resources.Load<Texture>("Textures/Brushes/White128");
        EditorApplication.update += OnProjectLoad;
    }

    /// <summary>Executes once when the project finished loading.</summary>
    static void OnProjectLoad()
    {
        if (Land != null)
        {
            EditorApplication.update -= OnProjectLoad;
            if (!EditorApplication.isPlaying)
                CreateMap(1000);
        }
    }
    
    public static List<int> GetEnumSelection<T>(T enumGroup)
    {
        var selectedEnums = new List<int>();
        for (int i = 0; i < System.Enum.GetValues(typeof(T)).Length; i++)
        {
            int layer = 1 << i;
            if ((Convert.ToInt32(enumGroup) & layer) != 0)
                selectedEnums.Add(i);
        }
        return selectedEnums;
    }

    public static void RotateMap(Selections.Objects objectSelection, bool CW)
    {
        foreach (var item in GetEnumSelection(objectSelection))
        {
            switch (item)
            {
                case 0:
                case 1:
                case 2:
                    RotateLayer((LandLayers) item, CW);
                    break;
                case 3:
                    RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, CW);
                    break;
                case 4:
                    RotateHeightMap(CW);
                    break;
                case 5:
                    RotateHeightMap(CW, TerrainType.Water);
                    break;
                case 6:
                    PrefabManager.RotatePrefabs(CW);
                    break;
                case 7:
                    PathManager.RotatePaths(CW);
                    break;
            }
        }
    }

    #region SplatMap Methods
    /// <summary>Rotates the selected layer.</summary>
    /// <param name="landLayerToPaint">The LandLayer to rotate. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void RotateLayer(LandLayers landLayerToPaint, bool CW, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetSplatMap(Rotate(GetSplatMap(landLayerToPaint, topology), CW), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                SetAlphaMap(Rotate(GetAlphaMap(), CW));
                break;
        }
    }

    /// <summary>Rotates the selected topologies.</summary>
    /// <param name="topologyLayers">The Topology layers to rotate.</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateTopologyLayers(TerrainTopology.Enum topologyLayers, bool CW)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);

        int progressId = Progress.Start("Rotating Topologies", null, Progress.Options.Sticky);
        for (int i = 0; i < topologyElements.Count; i++)
        {
            Progress.Report(progressId, (float)i / topologyElements.Count, "Rotating: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString());
            RotateLayer(LandLayers.Topology, CW, i);
        }
        Progress.Finish(progressId);
    }

    /// <summary>Paints if all the conditions passed in are true.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to paint.</param>
    /// <param name="conditions">The conditions to check.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintConditional(LandLayers landLayerToPaint, int texture, Conditions conditions, int topology = 0)
    {
        int splatRes = SplatMapRes;
        bool[,] conditionsMet = new bool[splatRes, splatRes]; // Paints wherever the conditionsmet is false.

        int progressId = Progress.Start("Conditional Paint");
        for (int i = 0; i < TerrainSplat.COUNT; i++)
            if (conditions.GroundConditions.CheckLayer[i])
                conditionsMet = CheckConditions(GetSplatMap(LandLayers.Ground), conditionsMet, i, conditions.GroundConditions.Weight[i]);

        Progress.Report(progressId, 0.2f, "Checking Biome");
        for (int i = 0; i < TerrainBiome.COUNT; i++)
            if (conditions.BiomeConditions.CheckLayer[i])
                conditionsMet = CheckConditions(GetSplatMap(LandLayers.Biome), conditionsMet, i, conditions.BiomeConditions.Weight[i]);

        Progress.Report(progressId, 0.3f, "Checking Alpha");
        if (conditions.AlphaConditions.CheckAlpha)
            conditionsMet = CheckConditions(GetAlphaMap(), conditionsMet, (conditions.AlphaConditions.Texture == 0) ? true : false);

        Progress.Report(progressId, 0.5f, "Checking Topology");
        for (int i = 0; i < TerrainTopology.COUNT; i++)
            if (conditions.TopologyConditions.CheckLayer[i])
                conditionsMet = CheckConditions(GetSplatMap(LandLayers.Topology, i), conditionsMet, (int)conditions.TopologyConditions.Texture[i], 0.5f);

        Progress.Report(progressId, 0.7f, "Checking Heights");
        if (conditions.TerrainConditions.CheckHeights)
            conditionsMet = CheckConditions(GetHeights(), conditionsMet, conditions.TerrainConditions.Heights.HeightLow, conditions.TerrainConditions.Heights.HeightHigh);

        Progress.Report(progressId, 0.8f, "Checking Slopes");
        if (conditions.TerrainConditions.CheckSlopes)
            conditionsMet = CheckConditions(GetSlopes(), conditionsMet, conditions.TerrainConditions.Slopes.SlopeLow, conditions.TerrainConditions.Slopes.SlopeHigh);

        Progress.Report(progressId, 0.8f, "Painting");
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                float[,,] splatMapToPaint = GetSplatMap(landLayerToPaint, topology);
                int textureCount = LayerCount(landLayerToPaint);
                Parallel.For(0, splatRes, i =>
                {
                    for (int j = 0; j < splatRes; j++)
                        if (conditionsMet[i, j] == false)
                        {
                            for (int k = 0; k < textureCount; k++)
                                splatMapToPaint[i, j, k] = 0f;
                            splatMapToPaint[i, j, texture] = 1f;
                        }
                });
                SetSplatMap(splatMapToPaint, landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                bool[,] alphaMapToPaint = GetAlphaMap();
                Parallel.For(0, splatRes, i =>
                {
                    for (int j = 0; j < splatRes; j++)
                        alphaMapToPaint[i, j] = (conditionsMet[i, j] == false) ? conditionsMet[i, j] : alphaMapToPaint[i, j];
                });
                SetAlphaMap(alphaMapToPaint);
                break;
        }
        Progress.Finish(progressId);
    }

    /// <summary>Paints the layer wherever the height conditions are met.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintHeight(LandLayers landLayerToPaint, float heightLow, float heightHigh, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetSplatMap(SetRange(GetSplatMap(landLayerToPaint, topology), GetHeights(), t, heightLow, heightHigh), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetAlphaMap(SetRange(GetAlphaMap(), GetHeights(), value, heightLow, heightHigh));
                break;
        }
    }

    /// <summary>Paints the layer wherever the height conditions are met with a weighting determined by the range the height falls in.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="minBlendLow">The minimum height to start to paint. The texture weight will increase as it gets closer to the heightlow.</param>
    /// <param name="maxBlendHigh">The maximum height to start to paint. The texture weight will increase as it gets closer to the heighthigh.</param>
    /// <param name="t">The texture to paint.</param>
    public static void PaintHeightBlend(LandLayers landLayerToPaint, float heightLow, float heightHigh, float minBlendLow, float maxBlendHigh, int t)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
                SetSplatMap(SetRangeBlend(GetSplatMap(landLayerToPaint), GetHeights(), t, heightLow, heightHigh, minBlendLow, maxBlendHigh), landLayerToPaint);
                break;
        }
    }

    /// <summary>Sets whole layer to the active texture.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintLayer(LandLayers landLayerToPaint, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetSplatMap(SetValues(GetSplatMap(landLayerToPaint), t), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                SetAlphaMap(SetValues(GetAlphaMap(), true));
                break;
        }
    }

    /// <summary>Paints the selected Topology layers.</summary>
    /// <param name="topologyLayers">The Topology layers to clear.</param>
    public static void PaintTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);

        int progressId = Progress.Start("Paint Topologies");
        for (int i = 0; i < topologyElements.Count; i++)
        {
            Progress.Report(progressId, (float)i / topologyElements.Count, "Painting: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString());
            PaintLayer(LandLayers.Topology, 0, i);
        }
        Progress.Finish(progressId);
    }

    /// <summary>Sets whole layer to the inactive texture. Alpha and Topology only.</summary>
    /// <param name="landLayerToPaint">The LandLayer to clear. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void ClearLayer(LandLayers landLayerToPaint, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Topology:
                SetSplatMap(SetValues(GetSplatMap(landLayerToPaint, topology), 1), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                SetAlphaMap(SetValues(GetAlphaMap(), false));
                break;
        }
    }

    /// <summary>Clears the selected Topology layers.</summary>
    /// <param name="topologyLayers">The Topology layers to clear.</param>
    public static void ClearTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);

        int progressId = Progress.Start("Clear Topologies");
        for (int i = 0; i < topologyElements.Count; i++)
        {
            Progress.Report(progressId, (float)i / topologyElements.Count, "Clearing: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString());
            ClearLayer(LandLayers.Topology, i);
        }
        Progress.Finish(progressId);
    }

    /// <summary>Inverts the active and inactive textures. Alpha and Topology only.</summary>
    /// <param name="landLayerToPaint">The LandLayer to invert. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void InvertLayer(LandLayers landLayerToPaint, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Topology:
                SetSplatMap(Invert(GetSplatMap(landLayerToPaint, topology)), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                SetAlphaMap(Invert(GetAlphaMap()));
                break;
        }
    }

    /// <summary>Inverts the selected Topology layers.</summary>
    /// <param name="topologyLayers">The Topology layers to invert.</param>
    public static void InvertTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);

        int progressId = Progress.Start("Invert Topologies");
        for (int i = 0; i < topologyElements.Count; i++)
        {
            Progress.Report(progressId, (float)i / topologyElements.Count, "Inverting: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString());
            InvertLayer(LandLayers.Topology, i);
        }
        Progress.Finish(progressId);
    }

    /// <summary>Paints the layer wherever the slope conditions are met. Includes option to blend.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="slopeLow">The minimum slope to paint at 100% weight.</param>
    /// <param name="slopeHigh">The maximum slope to paint at 100% weight.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintSlope(LandLayers landLayerToPaint, float slopeLow, float slopeHigh, int t, int topology = 0) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetSplatMap(SetRange(GetSplatMap(landLayerToPaint, topology), GetSlopes(), t, slopeLow, slopeHigh), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetAlphaMap(SetRange(GetAlphaMap(), GetSlopes(), value, slopeLow, slopeHigh));
                break;
        }
    }

    /// <summary> Paints the layer wherever the slope conditions are met. Includes option to blend.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="slopeLow">The minimum slope to paint at 100% weight.</param>
    /// <param name="slopeHigh">The maximum slope to paint at 100% weight.</param>
    /// <param name="minBlendLow">The minimum slope to start to paint. The texture weight will increase as it gets closer to the slopeLow.</param>
    /// <param name="maxBlendHigh">The maximum slope to start to paint. The texture weight will increase as it gets closer to the slopeHigh.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintSlopeBlend(LandLayers landLayerToPaint, float slopeLow, float slopeHigh, float minBlendLow, float maxBlendHigh, int t) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
                SetSplatMap(SetRangeBlend(GetSplatMap(landLayerToPaint), GetSlopes(), t, slopeLow, slopeHigh, minBlendLow, maxBlendHigh), landLayerToPaint);
                break;
        }
    }

    /// <summary>Paints the splats wherever the water is above 500 and is above the terrain.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="aboveTerrain">Check if the watermap is above the terrain before painting.</param>
    /// <param name="tex">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintRiver(LandLayers landLayerToPaint, bool aboveTerrain, int tex, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetSplatMap(SetRiver(GetSplatMap(landLayerToPaint, topology), GetHeights(), GetHeights(TerrainManager.TerrainType.Water), aboveTerrain, tex), landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                SetAlphaMap(SetRiver(GetAlphaMap(), GetHeights(), GetHeights(TerrainManager.TerrainType.Water), aboveTerrain, tex == 0));
                break;
            
        }
    }
    #endregion

    /// <summary>Centres the Prefab and Path parent objects.</summary>
    static void CentreSceneObjects(MapInfo mapInfo)
    {
        PrefabManager.PrefabParent.GetComponent<LockObject>().SetPosition(new Vector3(mapInfo.size.x / 2, 500, mapInfo.size.z / 2));
        PathManager.PathParent.GetComponent<LockObject>().SetPosition(new Vector3(mapInfo.size.x / 2, 500, mapInfo.size.z / 2));
    }

    /// <summary>Loads and sets up the map.</summary>
    public static void Load(MapInfo mapInfo, string loadPath = "")
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.Load(mapInfo, loadPath));
    }

    /// <summary>Saves the map.</summary>
    /// <param name="path">The path to save to.</param>
    public static void Save(string path)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.Save(path));
    }

    /// <summary>Creates a new flat terrain.</summary>
    /// <param name="size">The size of the terrain.</param>
    public static void CreateMap(int size, int ground = 4, int biome = 1, float landHeight = 503f)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.CreateMap(size, ground, biome, landHeight));
    }

    private class Coroutines
    {
        public static IEnumerator Load(MapInfo mapInfo, string path = "")
        {
            ProgressManager.RemoveProgressBars("Load:");

            int progressID = Progress.Start("Load: " + path.Split('/').Last(), "Preparing Map", Progress.Options.Sticky);
            int delPrefab = Progress.Start("Prefabs", null, Progress.Options.Sticky, progressID);
            int spwPrefab = Progress.Start("Prefabs", null, Progress.Options.Sticky, progressID);
            int delPath = Progress.Start("Paths", null, Progress.Options.Sticky, progressID);
            int spwPath = Progress.Start("Paths", null, Progress.Options.Sticky, progressID);
            int terrainID = Progress.Start("Terrain", null, Progress.Options.Sticky, progressID);

            PrefabManager.DeletePrefabs(PrefabManager.CurrentMapPrefabs, delPrefab);
            PathManager.DeletePaths(PathManager.CurrentMapPaths, delPath);
            CentreSceneObjects(mapInfo);
            TerrainManager.Load(mapInfo, terrainID);
            PrefabManager.SpawnPrefabs(mapInfo.prefabData, spwPrefab);
            PathManager.SpawnPaths(mapInfo.pathData, spwPath);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            while (Progress.GetProgressById(spwPrefab).running)
            {
                if (sw.Elapsed.TotalMilliseconds > 0.05f)
                {
                    sw.Restart();
                    yield return null;
                }
            }

            Progress.Report(progressID, 0.99f, "Loaded");
            Progress.Finish(terrainID, Progress.Status.Succeeded);
            Progress.Finish(progressID, Progress.Status.Succeeded);

            Callbacks.OnMapLoaded(path);
        }

        public static IEnumerator Save(string path)
        {
            ProgressManager.RemoveProgressBars("Save:");

            int progressID = Progress.Start("Save: " + path.Split('/').Last(), "Saving Map", Progress.Options.Sticky);
            int prefabID = Progress.Start("Prefabs", null, Progress.Options.Sticky, progressID);
            int pathID = Progress.Start("Paths", null, Progress.Options.Sticky, progressID);
            int terrainID = Progress.Start("Terrain", null, Progress.Options.Sticky, progressID);

            SaveLayer();
            yield return null;
            TerrainToWorld(Land, Water, (prefabID, pathID, terrainID)).Save(path);

            Progress.Report(progressID, 0.99f, "Saved");
            Progress.Finish(prefabID, Progress.Status.Succeeded);
            Progress.Finish(pathID, Progress.Status.Succeeded);
            Progress.Finish(terrainID, Progress.Status.Succeeded);
            Progress.Finish(progressID, Progress.Status.Succeeded);

            Callbacks.OnMapSaved(path);
        }

        public static IEnumerator CreateMap(int size, int ground = 4, int biome = 1, float landHeight = 503f)
        {
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(Load(EmptyMap(size, landHeight), "New Map"));
        }
    }
}