public static class EditorEnums
{
    public static class Selections
    {
        public enum ObjectSelection
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
        public enum LandLayers
        {
            Ground = 1 << 0,
            Biome = 1 << 1,
            Alpha = 1 << 2,
            Topology = 1 << 3,
        }
    }
    public static class Layers
    {
        public enum LandLayers
        {
            Ground = 0,
            Biome = 1,
            Alpha = 2,
            Topology = 3,
        }
    }
}