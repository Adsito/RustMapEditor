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
    public static Vector2 heightmapCentre = new Vector2(0.5f, 0.5f);

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
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(T)).Length; i++)
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
                    RotateLayer(LandLayers.Ground, CW);
                    break;
                case 1:
                    RotateLayer(LandLayers.Biome, CW);
                    break;
                case 2:
                    RotateLayer(LandLayers.Alpha, CW);
                    break;
                case 3:
                    RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, CW);
                    break;
                case 4:
                    RotateTerrains(CW, Selections.Terrains.Land);
                    break;
                case 5:
                    RotateTerrains(CW, Selections.Terrains.Water);
                    break;
                case 6:
                    RotatePrefabs(CW);
                    break;
                case 7:
                    RotatePaths(CW);
                    break;
            }
        }
    }

    /// <summary>Rotates prefabs 90°.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePrefabs(bool CW)
    {
        var prefabRotate = GameObject.FindGameObjectWithTag("Prefabs");
        if (CW)
        {
            prefabRotate.transform.Rotate(0, 90, 0, Space.World);
            prefabRotate.GetComponent<LockObject>().UpdateTransform();
        }
        else
        {
            prefabRotate.transform.Rotate(0, -90, 0, Space.World);
            prefabRotate.GetComponent<LockObject>().UpdateTransform();
        }
    }

    /// <summary>Rotates paths 90°.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePaths(bool CW)
    {
        var pathRotate = GameObject.FindGameObjectWithTag("Paths");
        if (CW)
        {
            pathRotate.transform.Rotate(0, 90, 0, Space.World);
            pathRotate.GetComponent<LockObject>().UpdateTransform();
        }
        else
        {
            pathRotate.transform.Rotate(0, -90, 0, Space.World);
            pathRotate.GetComponent<LockObject>().UpdateTransform();
        }
    }

    /// <summary>Rotates the selected terrains.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateTerrains(bool CW, Selections.Terrains terrains, Dimensions dmns = null)
    {
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    Land.terrainData.SetHeights(0, 0, Rotate(Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), CW, dmns));
                    break;
                case 1:
                    Water.terrainData.SetHeights(0, 0, Rotate(Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), CW, dmns));
                    break;
            }
        }
    }

    /// <summary>Sets the selected terrains to the height set.</summary>
    /// <param name="height">The height to set.</param>
    public static void SetHeightmap(float height, Selections.Terrains terrains, Dimensions dmns = null)
    {
        height /= 1000f;
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    Land.terrainData.SetHeights(0, 0, SetValues(Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), height, dmns));
                    break;
                case 1:
                    Water.terrainData.SetHeights(0, 0, SetValues(Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), height, dmns));
                    break;
            }
        }
    }

    /// <summary>Inverts the selected terrains.</summary>
    public static void InvertHeightmap(Selections.Terrains terrains, Dimensions dmns = null)
    {
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    Land.terrainData.SetHeights(0, 0, Invert(Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), dmns));
                    break;
                case 1:
                    Water.terrainData.SetHeights(0, 0, Invert(Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), dmns));
                    break;
            }
        }
    }

    /// <summary> Normalises the terrain between two heights.</summary>
    /// <param name="normaliseLow">The lowest height the HeightMap should be.</param>
    /// <param name="normaliseHigh">The highest height the HeightMap should be.</param>
    public static void NormaliseHeightmap(float normaliseLow, float normaliseHigh, Selections.Terrains terrains, Dimensions dmns = null)
    {
        normaliseLow /= 1000f; normaliseHigh /= 1000f;
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    Land.terrainData.SetHeights(0, 0, Normalise(Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), normaliseLow, normaliseHigh, dmns));
                    break;
                case 1:
                    Water.terrainData.SetHeights(0, 0, Normalise(Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), normaliseLow, normaliseHigh, dmns));
                    break;
            }
        }
    }

    /// <summary>Terraces the HeightMap.</summary>
    /// <param name="featureSize">The height of each terrace.</param>
    /// <param name="interiorCornerWeight">The weight of the terrace effect.</param>
    public static void TerraceErodeHeightmap(float featureSize, float interiorCornerWeight)
    {
        Material mat = new Material((Shader)AssetDatabase.LoadAssetAtPath("Packages/com.unity.terrain-tools/Shaders/TerraceErosion.shader", typeof(Shader)));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(Land, heightmapCentre, Land.terrainData.size.x, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(Land, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(1.0f, featureSize, interiorCornerWeight, 0.0f);
        mat.SetTexture("_BrushTex", terrainFilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - TerraceErosion");
    }

    /// <summary>Smooths the terrain.</summary>
    /// <param name="filterStrength">The strength of the smoothing.</param>
    /// <param name="blurDirection">The direction the smoothing should preference. Between -1f - 1f.</param>
    public static void SmoothHeightmap(float filterStrength, float blurDirection)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(Land, heightmapCentre, Land.terrainData.size.x, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(Land, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(filterStrength, 0.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", terrainFilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        Vector4 smoothWeights = new Vector4(Mathf.Clamp01(1.0f - Mathf.Abs(blurDirection)), Mathf.Clamp01(-blurDirection), Mathf.Clamp01(blurDirection), 0.0f);
        mat.SetVector("_SmoothWeights", smoothWeights);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SmoothHeights);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - Smooth Heights");
    }

    /// <summary>Increases or decreases the terrain by the offset.</summary>
    /// <param name="offset">The amount to offset by. Negative values offset down.</param>
    /// <param name="clampOffset">Check if offsetting the heightmap would exceed the min-max values.</param>
    public static void OffsetHeightmap(float offset, bool clampOffset, Selections.Terrains terrains, Dimensions dmns = null)
    {
        offset /= 1000f;
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    Land.terrainData.SetHeights(0, 0, Offset(Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), offset, clampOffset, dmns));
                    break;
                case 1:
                    Water.terrainData.SetHeights(0, 0, Offset(Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), offset, clampOffset, dmns));
                    break;
            }
        }
    }

    /// <summary>Sets the HeightMap level to the minimum if it's below.</summary>
    /// <param name="minimumHeight">The minimum height to set.</param>
    /// <param name="maximumHeight">The maximum height to set.</param>
    public static void ClampHeightmap(float minimumHeight, float maximumHeight, Selections.Terrains terrains, Dimensions dmns = null)
    {
        minimumHeight /= 1000f; maximumHeight /= 1000f;
        switch (terrains)
        {
            case Selections.Terrains.Land:
                Land.terrainData.SetHeights(0, 0, ClampValues(Land.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), minimumHeight, maximumHeight, dmns));
                break;
            case Selections.Terrains.Water:
                Water.terrainData.SetHeights(0, 0, ClampValues(Water.terrainData.GetHeights(0, 0, HeightMapRes, HeightMapRes), minimumHeight, maximumHeight, dmns));
                break;
        }
    }

    /// <summary>Returns the height of the HeightMap at the selected coords.</summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public static float GetHeight(int x, int z)
    {
        float xNorm = (float)x / (float)Land.terrainData.alphamapHeight;
        float yNorm = (float)z / (float)Land.terrainData.alphamapHeight;
        float height = Land.terrainData.GetInterpolatedHeight(xNorm, yNorm);
        return height;
    }

    /// <summary>Returns a 2D array of the height values.</summary>
    public static float[,] GetHeights()
    {
        return Land.terrainData.GetInterpolatedHeights(0, 0, Land.terrainData.alphamapHeight, Land.terrainData.alphamapHeight, 1f / (float)Land.terrainData.alphamapHeight, 1f / (float)Land.terrainData.alphamapHeight);
    }

    public static float[,] GetWaterHeights()
    {
        return Water.terrainData.GetInterpolatedHeights(0, 0, Water.terrainData.alphamapHeight, Water.terrainData.alphamapHeight, 1f / (float)Water.terrainData.alphamapHeight, 1f / (float)Water.terrainData.alphamapHeight);
    }

    /// <summary>Returns the slope of the HeightMap at the selected coords.</summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public static float GetSlope(int x, int z)
    {
        float xNorm = (float)x / Land.terrainData.alphamapHeight;
        float yNorm = (float)z / Land.terrainData.alphamapHeight;
        float slope = Land.terrainData.GetSteepness(xNorm, yNorm);
        return slope;
    }

    #region SplatMap Methods
    /// <summary>Texture count in layer chosen, used for determining the size of the splatmap array.</summary>
    /// <param name="landLayer">The LandLayer to return the texture count from. (Ground, Biome, Alpha, Topology)</param>
    public static int TextureCount(LandLayers landLayer)
    {
        switch (landLayer)
        {
            case LandLayers.Ground:
                return 8;
            case LandLayers.Biome:
                return 4;
            default:
                return 2;
        }
    }

    /// <summary>Returns the value of a texture at the selected coords.</summary>
    /// <param name="landLayer">The LandLayer of the texture. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to get.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static float GetTexture(LandLayers landLayer, int texture, int x, int z, int topology = 0)
    {
        return GetSplatMap(landLayer, topology)[x, z, texture];
    }

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
                SetData(Rotate(GetSplatMap(landLayerToPaint, topology), CW), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(Rotate(AlphaArray, CW), landLayerToPaint);
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
            conditionsMet = CheckConditions(AlphaArray, conditionsMet, (conditions.AlphaConditions.Texture == 0) ? true : false);

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
                int textureCount = TextureCount(landLayerToPaint);
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
                SetData(splatMapToPaint, landLayerToPaint, topology);
                SetLayer(landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                bool[,] alphaMapToPaint = AlphaArray;
                Parallel.For(0, splatRes, i =>
                {
                    for (int j = 0; j < splatRes; j++)
                        alphaMapToPaint[i, j] = (conditionsMet[i, j] == false) ? conditionsMet[i, j] : alphaMapToPaint[i, j];
                });
                SetData(alphaMapToPaint, landLayerToPaint);
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
                SetData(SetRange(GetSplatMap(landLayerToPaint, topology), GetHeights(), t, heightLow, heightHigh), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetRange(AlphaArray, GetHeights(), value, heightLow, heightHigh), landLayerToPaint);
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
                SetData(SetRangeBlend(GetSplatMap(landLayerToPaint), GetHeights(), t, heightLow, heightHigh, minBlendLow, maxBlendHigh), landLayerToPaint);
                SetLayer(LandLayer);
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
                SetData(SetValues(GetSplatMap(landLayerToPaint), t), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(SetValues(AlphaArray, true), landLayerToPaint);
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
                SetData(SetValues(GetSplatMap(landLayerToPaint, topology), 1), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(SetValues(AlphaArray, false), landLayerToPaint);
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
                SetData(Invert(GetSplatMap(landLayerToPaint, topology)), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(Invert(AlphaArray), landLayerToPaint);
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
                SetData(SetRange(GetSplatMap(landLayerToPaint, topology), GetSlopes(), t, slopeLow, slopeHigh), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetRange(AlphaArray, GetSlopes(), value, slopeLow, slopeHigh), landLayerToPaint);
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
                SetData(SetRangeBlend(GetSplatMap(landLayerToPaint), GetSlopes(), t, slopeLow, slopeHigh, minBlendLow, maxBlendHigh), landLayerToPaint);
                SetLayer(LandLayer);
                break;
        }
    }

    /// <summary>Paints the splats wherever the water is above 500 and is above the terrain.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="aboveTerrain">Check if the watermap is above the terrain before painting.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintRiver(LandLayers landLayerToPaint, bool aboveTerrain, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(SetRiver(GetSplatMap(landLayerToPaint, topology), GetHeights(), GetWaterHeights(), aboveTerrain, t), landLayerToPaint, topology);
                SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetRiver(AlphaArray, GetHeights(), GetWaterHeights(), aboveTerrain, value), landLayerToPaint);
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

            var splatMapTask = Task.Run(() => SetSplatMaps(mapInfo));

            PrefabManager.DeletePrefabs(PrefabManager.CurrentMapPrefabs, delPrefab);
            PathManager.DeletePaths(PathManager.CurrentMapPaths, delPath);
            CentreSceneObjects(mapInfo);
            SetTerrain(mapInfo, terrainID);
            PrefabManager.SpawnPrefabs(mapInfo.prefabData, spwPrefab);
            PathManager.SpawnPaths(mapInfo.pathData, spwPath);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (!splatMapTask.IsCompleted)
            {
                if (sw.Elapsed.TotalMilliseconds > 0.05f)
                {
                    sw.Restart();
                    yield return null;
                }
            }

            SetLayer(LandLayer, TerrainTopology.TypeToIndex((int)TopologyLayer)); // Sets the alphamaps to the currently selected.

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
            PaintLayer(LandLayers.Ground, ground);
            PaintLayer(LandLayers.Biome, biome);
        }
    }
}