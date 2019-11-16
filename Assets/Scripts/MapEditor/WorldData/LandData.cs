using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;

namespace RustMapEditor.Data
{
    public static class LandData
    {
        /// <summary>The Ground textures of the map. [Res, Res, Textures(8)].</summary>
        private static float[,,] groundArray;
        /// <summary>The Biome textures of the map. [Res, Res, Textures(4)</summary>
        private static float[,,] biomeArray;
        /// <summary>The Alpha holes of the map. True = Visible, False = Hole. [Res, Res]</summary>
        private static bool[,] alphaArray;
        /// <summary>The Topology layers, and textures of the map. [31][Res, Res, Textures(2)]</summary>
        public static float[][,,] topologyArray = new float[TerrainTopology.COUNT][,,];

        /// <summary>The terrain layers used by the terrain for paint operations</summary>
        private static TerrainLayer[] groundTextures = null, biomeTextures = null, miscTextures = null;

        /// <summary>The LandLayer currently being displayed on the terrain.</summary>
        public static LandLayers landLayer;
        /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LandLayer is set to topology.</summary>
        public static TerrainTopology.Enum topologyLayer;
        /// <summary>The previously selected topology layer. Used to save the Topology layer before displaying the new one.</summary>
        private static int lastTopologyLayer = 0;

        /// <summary>The terrain pieces in the scene.</summary>
        public static Terrain land, water;

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            TerrainCallbacks.textureChanged += TextureChanged;
            TerrainCallbacks.heightmapChanged += HeightmapChanged;
            EditorApplication.update += ProjectLoaded;
        }
        public static Vector3 GetTerrainSize()
        {
            return land.terrainData.size;
        }
        public static Vector3 GetMapOffset()
        {
            return 0.5f * GetTerrainSize();
        }
        public static int GetHeightMapResolution()
        {
            return land.terrainData.heightmapResolution;
        }
        public static int GetSplatMapResolution()
        {
            return land.terrainData.alphamapResolution;
        }
        public static void SetTerrainReferences()
        {
            water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
            land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        }
        private static void ProjectLoaded()
        {
            EditorApplication.update -= ProjectLoaded;
            SetTerrainReferences();
        }
        /// <summary>Callback for whenever the heightmap is updated.</summary>
        private static void HeightmapChanged(Terrain terrain, RectInt heightRegion, bool synched)
        {

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
            if (layer != LandLayers.Alpha && layer != landLayer)
            {
                SetLayer(layer, topology);
            }
            else if (layer != LandLayers.Alpha && layer == LandLayers.Topology)
            {
                SaveLayer(lastTopologyLayer);
                SetLayer(layer, topology);
            }
        }
        /// <summary>Returns the SplatMap at the selected LandLayer.</summary>
        /// <param name="landLayer">The LandLayer to return. (Ground, Biome, Topology)</param>
        /// <param name="topology">The Topology layer, if selected.</param>
        public static float[,,] GetSplatMap(LandLayers landLayer, int topology = 0)
        {
            switch (landLayer)
            {
                case LandLayers.Ground:
                    return groundArray;
                case LandLayers.Biome:
                    return biomeArray;
                case LandLayers.Topology:
                    return topologyArray[topology];
            }
            return null;
        }
        /// <summary>Returns the current maps alphaArray.</summary>
        public static bool[,] GetAlphaMap()
        {
            return alphaArray;
        }
        /// <summary>Sets the array data of LandLayer.</summary>
        /// <param name="layer">The layer to set the data to.</param>
        /// <param name="topology">The topology layer if the landlayer is topology.</param>
        public static void SetData(float[,,] array, LandLayers layer, int topology = 0)
        {
            switch (layer)
            {
                case LandLayers.Ground:
                    groundArray = array;
                    break;
                case LandLayers.Biome:
                    biomeArray = array;
                    break;
                case LandLayers.Topology:
                    topologyArray[topology] = array;
                    break;
            }
        }
        /// <summary>Sets the array data of LandLayer.</summary>
        /// <param name="layer">The layer to set the data to.</param>
        public static void SetData(bool[,] array, LandLayers layer)
        {
            switch (layer)
            {
                case LandLayers.Alpha:
                    alphaArray = array;
                    break;
            }
        }
        /// <summary>Sets the terrain alphamaps to the LandLayer.</summary>
        /// <param name="layer">The LandLayer to set.</param>
        /// <param name="topology">The Topology layer to set.</param>
        public static void SetLayer(LandLayers layer, int topology = 0)
        {
            if (groundTextures == null || biomeTextures == null || miscTextures == null)
            {
                GetTextures();
            }
            switch (layer)
            {
                case LandLayers.Ground:
                    land.terrainData.terrainLayers = groundTextures;
                    land.terrainData.SetAlphamaps(0, 0, groundArray);
                    landLayer = layer;
                    break;
                case LandLayers.Biome:
                    land.terrainData.terrainLayers = biomeTextures;
                    land.terrainData.SetAlphamaps(0, 0, biomeArray);
                    landLayer = layer;
                    break;
                case LandLayers.Alpha:
                    land.terrainData.SetHoles(0, 0, alphaArray);
                    break;
                case LandLayers.Topology:
                    lastTopologyLayer = topology;
                    land.terrainData.terrainLayers = miscTextures;
                    land.terrainData.SetAlphamaps(0, 0, topologyArray[topology]);
                    landLayer = layer;
                    break;
            }
            topologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(topology);
        }
        /// <summary>Updates the stored alphaArray with the current terrain holes.</summary>
        public static void UpdateAlpha()
        {
            alphaArray = land.terrainData.GetHoles(0, 0, land.terrainData.holesResolution, land.terrainData.holesResolution);
        }
        /// <summary>Saves any changes made to the Alphamaps, like the paint brush.</summary>
        /// <param name="topologyLayer">The Topology layer, if active.</param>
        public static void SaveLayer(int topologyLayer = 0)
        {
            switch (landLayer)
            {
                case LandLayers.Ground:
                    groundArray = land.terrainData.GetAlphamaps(0, 0, land.terrainData.alphamapWidth, land.terrainData.alphamapHeight);
                    break;
                case LandLayers.Biome:
                    biomeArray = land.terrainData.GetAlphamaps(0, 0, land.terrainData.alphamapWidth, land.terrainData.alphamapHeight);
                    break;
                case LandLayers.Topology:
                    topologyArray[topologyLayer] = land.terrainData.GetAlphamaps(0, 0, land.terrainData.alphamapWidth, land.terrainData.alphamapHeight);
                    break;
            }
        }
        private static void GetTextures()
        {
            groundTextures = GetGroundTextures();
            biomeTextures = GetBiomeTextures();
            miscTextures = GetMiscTextures();
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
    }
}