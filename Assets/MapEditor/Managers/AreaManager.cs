using RustMapEditor.Variables;

public static class AreaManager
{
    public static Dimensions Area = new Dimensions(0, 512, 0, 512);

    public static bool AreaActive { get; private set; }

    public static void Reset()
    {
        Area = new Dimensions(0, TerrainManager.SplatMapRes, 0, TerrainManager.SplatMapRes);
    }
}