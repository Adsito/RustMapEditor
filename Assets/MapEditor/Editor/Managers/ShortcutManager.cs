using UnityEditor;
using UnityEditor.ShortcutManagement;
using RustMapEditor.UI;
using RustMapEditor.Data;
using RustMapEditor.Variables;
using UnityEngine;
using UnityEditor.EditorTools;

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
        SceneManager.CentreSceneView(SceneView.lastActiveSceneView);
    }

    [Shortcut("RustMapEditor/Clear Map Prefabs")]
    public static void ClearMapPrefabs()
    {
        PrefabManager.DeletePrefabs(PrefabManager.CurrentMapPrefabs);
    }

    [Shortcut("RustMapEditor/Clear Map Paths")]
    public static void ClearMapPaths()
    {
        PathManager.DeletePaths(PathManager.CurrentMapPaths);
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

    [Shortcut("RustMapEditor/Move Tool", KeyCode.W, ShortcutModifiers.Shift)]
    public static void SelectMoveTool()
    {
        EditorTools.SetActiveTool(typeof(MoveToolCentred));
    }

    [Shortcut("RustMapEditor/Rotate Tool", KeyCode.E, ShortcutModifiers.Shift)]
    public static void SelectRotateTool()
    {
        EditorTools.SetActiveTool(typeof(RotateToolCentred));
    }

    [Shortcut("RustMapEditor/Scale Tool", KeyCode.R, ShortcutModifiers.Shift)]
    public static void SelectScaleTool()
    {
        EditorTools.SetActiveTool(typeof(ScaleToolCentred));
    }

    [Shortcut("RustMapEditor/Enable Hide Flags")]
    public static void EnableHideFlags() => SceneManager.ToggleHideFlags(true);

    [Shortcut("RustMapEditor/Disable Hide Flags")]
    public static void DisableHideFlags() => SceneManager.ToggleHideFlags(false);

    [Shortcut("RustMapEditor/Open Settings Menu"), MenuItem("Rust Map Editor/Settings", false, 0)]
    public static void OpenSettings() => SettingsWindow.Init();
}