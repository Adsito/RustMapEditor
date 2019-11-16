namespace RustMapEditor.Variables
{
    public struct Conditions
    {
        public TerrainSplat.Enum GroundConditions
        {
            get; set;
        }
        public TerrainBiome.Enum BiomeConditions
        {
            get; set;
        }
        public TerrainTopology.Enum TopologyLayers
        {
            get; set;
        }
        public AlphaTextures AlphaTextures
        {
            get; set;
        }
        public TopologyTextures TopologyTextures
        {
            get; set;
        }
        public bool CheckAlpha
        {
            get; set;
        }
        public int AlphaTexture
        {
            get; set;
        }
        public int TopologyTexture
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
        public Dimensions Dimensions
        {
            get; set;
        }
    }
    public struct TopologyLayers
    {
        public float[,,] Topologies
        {
            get; set;
        }
    }
    public struct GroundTextures
    {
        public int Texture
        {
            get; set;
        }
    }
    public struct BiomeTextures
    {
        public int Texture
        {
            get; set;
        }
    }
    public class Dimensions
    {
        public int x0 { get; set; }
        public int x1 { get; set; }
        public int z0 { get; set; }
        public int z1 { get; set; }

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
        public bool BlendSlopes { get; set; }
        public float SlopeBlendLow { get; set; }
        public float SlopeLow { get; set; }
        public float SlopeHigh { get; set; }
        public float SlopeBlendHigh { get; set; }
    }
    public struct HeightsInfo
    {
        public bool BlendHeights { get; set; }
        public float HeightBlendLow { get; set; }
        public float HeightLow { get; set; }
        public float HeightHigh { get; set; }
        public float HeightBlendHigh { get; set; }
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