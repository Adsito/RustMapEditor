public static class AreaManager
{
    /// <summary>The currently active area that paint operations affect.</summary>
    public static Area ActiveArea = new Area(0, 512, 0, 512);

    public static void Reset()
    {
        ActiveArea = new Area(0, TerrainManager.SplatMapRes, 0, TerrainManager.SplatMapRes);
    }

    public class Area
    {
        public int x0;
        public int x1;
        public int z0;
        public int z1;

        public Area(int x0, int x1, int z0, int z1)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.z0 = z0;
            this.z1 = z1;
        }

        public static Area HeightMapDimensions()
        {
            return new Area(0, TerrainManager.HeightMapRes, 0, TerrainManager.HeightMapRes);
        }
    }
}