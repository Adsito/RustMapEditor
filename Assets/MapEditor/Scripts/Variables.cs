namespace RustMapEditor.Variables
{
    public struct Conditions
    {
        public GroundConditions GroundConditions;
        public BiomeConditions BiomeConditions;
        public AlphaConditions AlphaConditions;
        public TopologyConditions TopologyConditions;
        public TerrainConditions TerrainConditions;
        public AreaConditions AreaConditions;
    }
    public struct GroundConditions
    {
        public GroundConditions(TerrainSplat.Enum layer)
        {
            Layer = layer;
            Weight = new float[TerrainSplat.COUNT];
            CheckLayer = new bool[TerrainSplat.COUNT];
        }
        public TerrainSplat.Enum Layer;
        public float[] Weight;
        public bool[] CheckLayer;
    }
    public struct BiomeConditions
    {
        public BiomeConditions(TerrainBiome.Enum layer)
        {
            Layer = layer;
            Weight = new float[TerrainBiome.COUNT];
            CheckLayer = new bool[TerrainBiome.COUNT];
        }
        public TerrainBiome.Enum Layer;
        public float[] Weight;
        public bool[] CheckLayer;
    }
    public struct AlphaConditions
    {
        public AlphaConditions(AlphaTextures texture)
        {
            Texture = texture;
            CheckAlpha = false;
        }
        public AlphaTextures Texture;
        public bool CheckAlpha;
    }
    public struct TopologyConditions
    {
        public TopologyConditions(TerrainTopology.Enum layer)
        {
            Layer = layer;
            Texture = new TopologyTextures[TerrainTopology.COUNT];
            CheckLayer = new bool[TerrainTopology.COUNT];
        }
        public TerrainTopology.Enum Layer;
        public TopologyTextures[] Texture;
        public bool[] CheckLayer;
    }
    public struct TerrainConditions
    {
        public HeightsInfo Heights;
        public bool CheckHeights;
        public SlopesInfo Slopes;
        public bool CheckSlopes;
    }
    public struct AreaConditions
    {
        public Dimensions Area;
        public bool CheckArea;
    }
    public struct TopologyLayers
    {
        public float[,,] Topologies
        {
            get; set;
        }
    }
    public class Dimensions
    {
        public int x0;
        public int x1;
        public int z0;
        public int z1;

        public Dimensions(int x0, int x1, int z0, int z1)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.z0 = z0;
            this.z1 = z1;
        }

        public static Dimensions HeightMapDimensions()
        {
            return new Dimensions(0, TerrainManager.HeightMapRes, 0, TerrainManager.HeightMapRes);
        }
    }
    public enum LandLayers
    {
        Ground = 0,
        Biome = 1,
        Alpha = 2,
        Topology = 3,
    }
    public enum AlphaTextures
    {
        Visible = 0,
        InVisible = 1,
    }
    public enum TopologyTextures
    {
        Active = 0,
        InActive = 1,
    }
    public struct SlopesInfo
    {
        public bool BlendSlopes;
        public float SlopeBlendLow;
        public float SlopeLow;
        public float SlopeHigh;
        public float SlopeBlendHigh;
    }
    public struct HeightsInfo
    {
        public bool BlendHeights;
        public float HeightBlendLow;
        public float HeightLow;
        public float HeightHigh;
        public float HeightBlendHigh;
    }
    public class Selections
    {
        public enum Objects
        {
            Ground = 1 << 0,
            Biome = 1 << 1,
            Alpha = 1 << 2,
            Topology = 1 << 3,
            Heightmap = 1 << 4,
            Watermap = 1 << 5,
            Prefabs = 1 << 6,
            Paths = 1 << 7,
        }
        public enum Terrains
        {
            Land = 1 << 0,
            Water = 1 << 1,
        }
        public enum Layers
        {
            Ground = 1 << 0,
            Biome = 1 << 1,
            Alpha = 1 << 2,
            Topology = 1 << 3,
        }
    }
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
    public class Layers
    {
        public TerrainSplat.Enum Ground
        {
            get; set;
        }
        public TerrainBiome.Enum Biome
        {
            get; set;
        }
        public TerrainTopology.Enum Topologies
        {
            get; set;
        }
        public LandLayers LandLayer
        {
            get; set;
        }
        public AlphaTextures AlphaTexture
        {
            get; set;
        }
        public TopologyTextures TopologyTexture
        {
            get; set;
        }
    }
}