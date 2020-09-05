using UnityEditor;
using UnityEditor.ShortcutManagement;
using RustMapEditor.UI;
using RustMapEditor.Variables;
using UnityEngine;
using UnityEditor.EditorTools;

public static class ShortcutManager
{
    [Shortcut("RustMapEditor/Load Map")]
    public static void LoadMap() => Functions.LoadMapPanel();

    [Shortcut("RustMapEditor/Save Map")]
    public static void SaveMap() => Functions.SaveMapPanel();

    [Shortcut("RustMapEditor/New Map")]
    public static void NewMap() => Functions.NewMapPanel();

    [Shortcut("RustMapEditor/Centre Scene View")]
    public static void CentreSceneView() => SceneManager.CentreSceneView(SceneView.lastActiveSceneView);

    [Shortcut("RustMapEditor/Clear Map Prefabs")]
    public static void ClearMapPrefabs() => PrefabManager.DeletePrefabs(PrefabManager.CurrentMapPrefabs);

    [Shortcut("RustMapEditor/Clear Map Paths")]
    public static void ClearMapPaths() => PathManager.DeletePaths(PathManager.CurrentMapPaths);

    [Shortcut("RustMapEditor/Clear Layer")]
    public static void ClearLayer() => MapManager.ClearLayer(TerrainManager.LandLayer, TerrainManager.TopologyLayer);

    [Shortcut("RustMapEditor/Invert Layer")]
    public static void InvertLayer() => MapManager.InvertLayer(TerrainManager.LandLayer, TerrainManager.TopologyLayer);

    [Shortcut("RustMapEditor/Invert Land")]
    public static void InvertLand() => MapManager.InvertHeightmap(Selections.Terrains.Land);

    [Shortcut("RustMapEditor/Select Land")]
    public static void SelectLand() => Selection.activeGameObject = TerrainManager.Land.gameObject;

    [Shortcut("RustMapEditor/Select Water")]
    public static void SelectWater() => Selection.activeGameObject = TerrainManager.Water.gameObject;

    [Shortcut("RustMapEditor/Move Tool", KeyCode.W, ShortcutModifiers.Shift)]
    public static void SelectMoveTool() => EditorTools.SetActiveTool(typeof(MoveToolCentred));

    [Shortcut("RustMapEditor/Rotate Tool", KeyCode.E, ShortcutModifiers.Shift)]
    public static void SelectRotateTool() => EditorTools.SetActiveTool(typeof(RotateToolCentred));

    [Shortcut("RustMapEditor/Scale Tool", KeyCode.R, ShortcutModifiers.Shift)]
    public static void SelectScaleTool() => EditorTools.SetActiveTool(typeof(ScaleToolCentred));

    [Shortcut("RustMapEditor/Enable Hide Flags")]
    public static void EnableHideFlags() => SceneManager.ToggleHideFlags(true);

    [Shortcut("RustMapEditor/Disable Hide Flags")]
    public static void DisableHideFlags() => SceneManager.ToggleHideFlags(false);

    [Shortcut("RustMapEditor/Main Menu"), MenuItem("Rust Map Editor/Main Menu", false, -1)]
    public static void OpenMainMenu() => EditorWindow.GetWindow(typeof(MapManagerWindow), false, "Rust Map Editor");

    [Shortcut("RustMapEditor/Settings Menu"), MenuItem("Rust Map Editor/Settings", false, 0)]
    public static void OpenSettingsMenu() => SettingsWindow.Init();

    [Shortcut("RustMapEditor/Layers Menu"), MenuItem("Rust Map Editor/Layers", false, 0)]
    public static void OpenLayersMenu() => LayersWindow.Init();

    [Shortcut("RustMapEditor/Prefab Hierarchy"), MenuItem("Rust Map Editor/Hierarchy/Prefabs", false, 1)]
    public static void OpenPrefabHierarchy() => EditorWindow.GetWindow(typeof(PrefabHierarchyWindow), false, "Prefab Hierarchy");

    [Shortcut("RustMapEditor/Path Hierarchy"), MenuItem("Rust Map Editor/Hierarchy/Paths", false, 1)]
    public static void OpenPathHierarchy() => EditorWindow.GetWindow(typeof(PathHierarchyWindow), false, "Path Hierarchy");

    [Shortcut("RustMapEditor/Prefab List"), MenuItem("Rust Map Editor/Prefab List", false, 1)]
    public static void OpenPrefabList() => EditorWindow.GetWindow(typeof(PrefabsListWindow), false, "Prefab List");

    [Shortcut("RustMapEditor/Open Wiki")]
    public static void OpenWiki() => Application.OpenURL("https://github.com/RustMapMaking/Editor/wiki");

    [Shortcut("RustMapEditor/Open Discord")]
    public static void OpenDiscord() => Application.OpenURL("https://discord.gg/HPmTWVa");

    [Shortcut("RustMapEditor/Open RoadMap")]
    public static void OpenRoadMap() => Application.OpenURL("https://github.com/RustMapMaking/Editor/projects/1");

    [Shortcut("RustMapEditor/Open Report Bug")]
    public static void OpenReportBug() => Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");

    [Shortcut("RustMapEditor/Open Request Feature")]
    public static void OpenRequestFeature() => Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
}