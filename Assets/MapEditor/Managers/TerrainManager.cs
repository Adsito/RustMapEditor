using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using static WorldConverter;
using System.Threading.Tasks;

namespace RustMapEditor.Data
{
    public static class TerrainManager
    {
        /// <summary>The Ground textures of the map. [Res, Res, Textures(8)].</summary>
        public static float[,,] GroundArray { get; private set; }

        /// <summary>The Biome textures of the map. [Res, Res, Textures(4)</summary>
        public static float[,,] BiomeArray { get; private set; }

        /// <summary>The Topology layers, and textures of the map. [31][Res, Res, Textures(2)]</summary>
        public static float[][,,] TopologyArray { get; private set; } = new float[TerrainTopology.COUNT][,,];

        /// <summary>The Terrain layers used by the terrain for paint operations</summary>
        public static TerrainLayer[] GroundTextures { get; private set; } = null;

        /// <summary>The Terrain layers used by the terrain for paint operations</summary>
        public static TerrainLayer[] BiomeTextures { get; private set; } = null;

        /// <summary>The Terrain layers used by the terrain for paint operations</summary>
        public static TerrainLayer[] MiscTextures { get; private set; } = null;

        /// <summary>The current slopearray of the terrain.</summary>
        public static float[,] SlopeArray { get; private set; }

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

        /// <summary>The state of the layer being applied to the terrain.</summary>
        public static bool LayerSet { get; private set; }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            TerrainCallbacks.textureChanged += TextureChanged;
            TerrainCallbacks.heightmapChanged += HeightmapChanged;
            EditorApplication.update += ProjectLoaded;
        }

        private static void ProjectLoaded()
        {
            EditorApplication.update -= ProjectLoaded;
            SetTerrainReferences();
        }

        public static void SetWaterTransparency(float alpha)
        {
            Color _color = WaterMaterial.color;
            _color.a = alpha;
            WaterMaterial.color = _color;
        }

        /// <summary>Gets the size of each splat relative to the terrain size it covers.</summary>
        public static float GetSplatSize()
        {
            return Land.terrainData.size.x / SplatMapRes;
        }

        public static float[,] GetSlopes()
        {
            if (SlopeArray != null)
                return SlopeArray;

            SlopeArray = new float[HeightMapRes, HeightMapRes];
            for (int i = 0; i < Land.terrainData.alphamapHeight; i++)
                for (int j = 0; j < Land.terrainData.alphamapHeight; j++)
                    SlopeArray[j, i] = Land.terrainData.GetSteepness((float)i / (float)Land.terrainData.alphamapHeight, (float)j / (float)Land.terrainData.alphamapHeight);

            return SlopeArray;
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
            if (terrain == Land)
                SlopeArray = null;
        }

        /// <summary>Callback for whenever the alphamap is updated.</summary>
        private static void TextureChanged(Terrain terrain, string textureName, RectInt texelRegion, bool synched)
        {

        }

        /// <summary>Changes the active Land and Topology Layers.</summary>
        /// <param name="layer">The LandLayer to change to.</param>
        /// <param name="topology">The Topology layer to change to.</param>
        public static void ChangeLandLayer(LandLayers layer, int topology = 0)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ChangeLayer(layer, topology));
        }

        /// <summary>Returns the SplatMap at the selected LandLayer.</summary>
        /// <param name="landLayer">The LandLayer to return. (Ground, Biome, Topology)</param>
        /// <param name="topology">The Topology layer, if selected.</param>
        public static float[,,] GetSplatMap(LandLayers landLayer, int topology = 0)
        {
            switch (landLayer)
            {
                case LandLayers.Ground:
                    return GroundArray;
                case LandLayers.Biome:
                    return BiomeArray;
                case LandLayers.Topology:
                    return TopologyArray[topology];
            }
            return null;
        }

        /// <summary>Returns the current maps alphaArray.</summary>
        public static bool[,] GetAlphaMap()
        {
            return Land.terrainData.GetHoles(0, 0, SplatMapRes, SplatMapRes);
        }

        /// <summary>Sets the array data of LandLayer.</summary>
        /// <param name="layer">The layer to set the data to.</param>
        /// <param name="topology">The topology layer if the landlayer is topology.</param>
        public static void SetData(float[,,] array, LandLayers layer, int topology = 0)
        {
            switch (layer)
            {
                case LandLayers.Ground:
                    GroundArray = array;
                    break;
                case LandLayers.Biome:
                    BiomeArray = array;
                    break;
                case LandLayers.Topology:
                    TopologyArray[topology] = array;
                    break;
            }
        }

        /// <summary>Sets the array data of LandLayer.</summary>
        /// <param name="layer">The layer to set the data to.</param>
        public static void SetData(bool[,] array, LandLayers layer)
        {
            if (layer == LandLayers.Alpha)
                Land.terrainData.SetHoles(0, 0, array);
        }

        /// <summary>Sets the terrain alphamaps to the LandLayer.</summary>
        /// <param name="layer">The LandLayer to set.</param>
        /// <param name="topology">The Topology layer to set.</param>
        public static void SetLayer(LandLayers layer, int topology = 0)
        {
            LayerSet = false;
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SetLayer(layer, topology));
        }

        /// <summary>Saves any changes made to the Alphamaps, like the paint brush.</summary>
        /// <param name="topology">The Topology layer, if active.</param>
        public static void SaveLayer()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SaveLayer());
        }

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
            TopologyData.InitMesh(mapInfo.topology);
            SetData(mapInfo.splatMap, LandLayers.Ground);
            SetData(mapInfo.biomeMap, LandLayers.Biome);
            Parallel.For(0, TerrainTopology.COUNT, i =>
            {
                SetData(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(i)), LandLayers.Topology, i);
            });
        }

        private static void GetTextures()
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
            textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Tundra.terrainlayer");
            textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/tundra");
            textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Temperate.terrainlayer");
            textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/temperate");
            textures[2] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arid.terrainlayer");
            textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arid");
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
                LayerSet = true;
            }

            public static IEnumerator SetLayer(LandLayers layer, int topology = 0)
            {
                if (GroundTextures == null || BiomeTextures == null || MiscTextures == null)
                    GetTextures();
                
                switch (layer)
                {
                    case LandLayers.Ground:
                        Land.terrainData.terrainLayers = GroundTextures;
                        Land.terrainData.SetAlphamaps(0, 0, GroundArray);
                        LandLayer = layer;
                        break;
                    case LandLayers.Biome:
                        Land.terrainData.terrainLayers = BiomeTextures;
                        Land.terrainData.SetAlphamaps(0, 0, BiomeArray);
                        LandLayer = layer;
                        break;
                    case LandLayers.Topology:
                        LastTopologyLayer = topology;
                        Land.terrainData.terrainLayers = MiscTextures;
                        Land.terrainData.SetAlphamaps(0, 0, TopologyArray[topology]);
                        LandLayer = layer;
                        break;
                }
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
                        GroundArray = Land.terrainData.GetAlphamaps(0, 0, Land.terrainData.alphamapWidth, Land.terrainData.alphamapHeight);
                        break;
                    case LandLayers.Biome:
                        BiomeArray = Land.terrainData.GetAlphamaps(0, 0, Land.terrainData.alphamapWidth, Land.terrainData.alphamapHeight);
                        break;
                    case LandLayers.Topology:
                        TopologyArray[LastTopologyLayer] = Land.terrainData.GetAlphamaps(0, 0, Land.terrainData.alphamapWidth, Land.terrainData.alphamapHeight);
                        break;
                }
                foreach (var item in Land.terrainData.alphamapTextures)
                    Undo.ClearUndo(item);

                yield return null;
            }
        }
    }
}