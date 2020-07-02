using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;
using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace RustMapEditor.Data
{
    public static class TerrainManager
    {
        /// <summary>The Ground textures of the map. [Res, Res, Textures(8)].</summary>
        public static float[,,] GroundArray { get; private set; }

        /// <summary>The Biome textures of the map. [Res, Res, Textures(4)</summary>
        public static float[,,] BiomeArray { get; private set; }

        /// <summary>The Topology layers, and textures of the map. [31][Res, Res, Textures(2)]</summary>
        public static float[][,,] TopologyArray { get; private set; }

        /// <summary>The Terrain layers used by the terrain for paint operations</summary>
        private static TerrainLayer[] groundTextures = null, biomeTextures = null, miscTextures = null;

        /// <summary>The current slopearray of the terrain.</summary>
        private static float[,] SlopeArray;

        /// <summary>The LandLayer currently being displayed on the terrain.</summary>
        public static LandLayers LandLayer { get; private set; }

        /// <summary>The Topology layer currently being displayed/to be displayed on the terrain when the LandLayer is set to topology.</summary>
        public static TerrainTopology.Enum TopologyLayer { get; private set; }

        /// <summary>The previously selected topology layer. Used to save the Topology layer before displaying the new one.</summary>
        private static int lastTopologyLayer = 0;

        /// <summary>The terrain pieces in the scene.</summary>
        public static Terrain land, water;

        public static bool LayerSet { get; private set; }

        private static Coroutines Coroutine;

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
            TopologyArray = new float[TerrainTopology.COUNT][,,];
            Coroutine = new Coroutines();
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

        /// <summary>Gets the size of each splat relative to the terrain size it covers.</summary>
        public static float GetSplatSize()
        {
            return land.terrainData.size.x / GetSplatMapResolution();
        }

        public static float[,] GetSlopes()
        {
            if (SlopeArray != null)
            {
                return SlopeArray;
            }
            SlopeArray = new float[GetHeightMapResolution(), GetHeightMapResolution()];
            for (int i = 0; i < land.terrainData.alphamapHeight; i++)
            {
                for (int j = 0; j < land.terrainData.alphamapHeight; j++)
                {
                    SlopeArray[j, i] = land.terrainData.GetSteepness((float)i / (float)land.terrainData.alphamapHeight, (float)j / (float)land.terrainData.alphamapHeight);
                }
            }
            return SlopeArray;
        }

        public static void SetTerrainReferences()
        {
            water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
            land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        }

        /// <summary>Callback for whenever the heightmap is updated.</summary>
        private static void HeightmapChanged(Terrain terrain, RectInt heightRegion, bool synched)
        {
            if (terrain == land)
            {
                SlopeArray = null;
            }
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
            if (layer == LandLayers.Alpha)
                return;
            if (layer == LandLayer)
                SaveLayer(lastTopologyLayer);
            SetLayer(layer, topology);
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
            return land.terrainData.GetHoles(0, 0, GetSplatMapResolution(), GetSplatMapResolution());
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
            switch (layer)
            {
                case LandLayers.Alpha:
                    land.terrainData.SetHoles(0, 0, array);
                    break;
            }
        }

        /// <summary>Sets the terrain alphamaps to the LandLayer.</summary>
        /// <param name="layer">The LandLayer to set.</param>
        /// <param name="topology">The Topology layer to set.</param>
        public static void SetLayer(LandLayers layer, int topology = 0)
        {
            LayerSet = false;
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutine.SetLayer(layer, topology));
        }

        /// <summary>Saves any changes made to the Alphamaps, like the paint brush.</summary>
        /// <param name="topologyLayer">The Topology layer, if active.</param>
        public static void SaveLayer(int topologyLayer = 0)
        {
            if (LayerSet == false)
            {
                Debug.LogError("Saving Layer before layer is set");
                return;
            }

            switch (LandLayer)
            {
                case LandLayers.Ground:
                    GroundArray = land.terrainData.GetAlphamaps(0, 0, land.terrainData.alphamapWidth, land.terrainData.alphamapHeight);
                    break;
                case LandLayers.Biome:
                    BiomeArray = land.terrainData.GetAlphamaps(0, 0, land.terrainData.alphamapWidth, land.terrainData.alphamapHeight);
                    break;
                case LandLayers.Topology:
                    TopologyArray[topologyLayer] = land.terrainData.GetAlphamaps(0, 0, land.terrainData.alphamapWidth, land.terrainData.alphamapHeight);
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

        private class Coroutines
        {
            public IEnumerator SetLayer(LandLayers layer, int topology = 0)
            {
                yield return EditorCoroutineUtility.StartCoroutineOwnerless(SetLayerCoroutine(layer, topology));
                LayerSet = true;
            }

            private IEnumerator SetLayerCoroutine(LandLayers layer, int topology = 0)
            {
                if (groundTextures == null || biomeTextures == null || miscTextures == null)
                    GetTextures();
                
                switch (layer)
                {
                    case LandLayers.Ground:
                        land.terrainData.terrainLayers = groundTextures;
                        land.terrainData.SetAlphamaps(0, 0, GroundArray);
                        LandLayer = layer;
                        break;
                    case LandLayers.Biome:
                        land.terrainData.terrainLayers = biomeTextures;
                        land.terrainData.SetAlphamaps(0, 0, BiomeArray);
                        LandLayer = layer;
                        break;
                    case LandLayers.Topology:
                        lastTopologyLayer = topology;
                        land.terrainData.terrainLayers = miscTextures;
                        land.terrainData.SetAlphamaps(0, 0, TopologyArray[topology]);
                        LandLayer = layer;
                        break;
                }
                TopologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(topology);
                yield return null;
            }
        }
    }
}