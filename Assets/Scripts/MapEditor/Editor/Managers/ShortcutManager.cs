using UnityEditor;
using UnityEditor.ShortcutManagement;
using RustMapEditor.UI;
using RustMapEditor.Data;
using RustMapEditor.Variables;

public static class ShortcutManager
{
    [Shortcut("RustMapEditor/Load Map")]
    public static void LoadMap()
    {
        Functions.LoadMapPanel();
    }

    [Shortcut("RustMapEditor/Save Map")]
    public static void SaveMap()
    {
        Functions.SaveMapPanel();
    }

    [Shortcut("RustMapEditor/New Map")]
    public static void NewMap()
    {
        Functions.NewMapPanel();
    }

    [Shortcut("RustMapEditor/Centre Scene View")]
    public static void CentreSceneView()
    {
        MapIO.CentreSceneView(SceneView.lastActiveSceneView);
    }

    [Shortcut("RustMapEditor/Clear Progress Bar")]
    public static void ClearProgressBar()
    {
        MapIO.ClearProgressBar();
    }

    [Shortcut("RustMapEditor/Clear Map Prefabs")]
    public static void ClearMapPrefabs()
    {
        MapIO.RemoveMapObjects(true);
    }

    [Shortcut("RustMapEditor/Clear Map Paths")]
    public static void ClearMapPaths()
    {
        MapIO.RemoveMapObjects(false, true);
    }

    [Shortcut("RustMapEditor/Clear Layer")]
    public static void ClearLayer()
    {
        MapIO.ClearLayer(LandData.LandLayer, TerrainTopology.TypeToIndex((int)LandData.TopologyLayer));
    }

    [Shortcut("RustMapEditor/Invert Layer")]
    public static void InvertLayer()
    {
        MapIO.InvertLayer(LandData.LandLayer, TerrainTopology.TypeToIndex((int)LandData.TopologyLayer));
    }

    [Shortcut("RustMapEditor/Invert Land")]
    public static void InvertLand()
    {
        MapIO.InvertHeightmap(Selections.Terrains.Land);
    }
}