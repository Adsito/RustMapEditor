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
        MapManager.CentreSceneView(SceneView.lastActiveSceneView);
    }

    [Shortcut("RustMapEditor/Clear Progress Bar")]
    public static void ClearProgressBar()
    {
        ProgressBarManager.Clear();
    }

    [Shortcut("RustMapEditor/Clear Map Prefabs")]
    public static void ClearMapPrefabs()
    {
        MapManager.RemoveMapObjects(true);
    }

    [Shortcut("RustMapEditor/Clear Map Paths")]
    public static void ClearMapPaths()
    {
        MapManager.RemoveMapObjects(false, true);
    }

    [Shortcut("RustMapEditor/Clear Layer")]
    public static void ClearLayer()
    {
        MapManager.ClearLayer(TerrainManager.LandLayer, TerrainTopology.TypeToIndex((int)TerrainManager.TopologyLayer));
    }

    [Shortcut("RustMapEditor/Invert Layer")]
    public static void InvertLayer()
    {
        MapManager.InvertLayer(TerrainManager.LandLayer, TerrainTopology.TypeToIndex((int)TerrainManager.TopologyLayer));
    }

    [Shortcut("RustMapEditor/Invert Land")]
    public static void InvertLand()
    {
        MapManager.InvertHeightmap(Selections.Terrains.Land);
    }
}