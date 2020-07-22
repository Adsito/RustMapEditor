using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using RustMapEditor.Variables;

public static class SettingsManager
{
    public const string SettingsPath = "EditorSettings.json";
    public const string BundlePathExt = @"\Bundles\Bundles";

    public static string RustDirectory { get; set; }
    public static float PrefabRenderDistance { get; set; }
    public static float PathRenderDistance { get; set; }
    public static float WaterTransparency { get; set; }
    public static bool LoadBundleOnProjectLoad { get; set; }
    public static string[] PrefabPaths { get; private set; }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        if (!File.Exists(SettingsPath))
            using (StreamWriter write = new StreamWriter(SettingsPath, false))
                write.Write(JsonUtility.ToJson(DefaultSettings(), true));

        LoadSettings();
    }

    /// <summary>Returns a new EditorSettings struct set with the default settings.</summary>
    static EditorSettings DefaultSettings()
    {
        SetDefaultSettings();
        EditorSettings editorSettings = new EditorSettings
        {
            rustDirectory = RustDirectory,
            prefabRenderDistance = PrefabRenderDistance,
            pathRenderDistance = PathRenderDistance,
            waterTransparency = WaterTransparency,
            loadbundleonprojectload = LoadBundleOnProjectLoad,
            prefabPaths = PrefabPaths
        };
        return editorSettings;
    }

    /// <summary>Saves the current EditorSettings to a JSON file.</summary>
    public static void SaveSettings()
    {
        using (StreamWriter write = new StreamWriter(SettingsPath, false))
        {
            EditorSettings editorSettings = new EditorSettings()
            {
                rustDirectory = RustDirectory,
                prefabRenderDistance = PrefabRenderDistance,
                pathRenderDistance = PathRenderDistance,
                waterTransparency = WaterTransparency,
                loadbundleonprojectload = LoadBundleOnProjectLoad,
                prefabPaths = PrefabPaths
            };
            write.Write(JsonUtility.ToJson(editorSettings, true));
        }
    }

    /// <summary>Loads and sets the current EditorSettings from a JSON file.</summary>
    public static void LoadSettings()
    {
        using (StreamReader reader = new StreamReader(SettingsPath))
        {
            EditorSettings editorSettings = JsonUtility.FromJson<EditorSettings>(reader.ReadToEnd());
            RustDirectory = editorSettings.rustDirectory;
            PrefabRenderDistance = editorSettings.prefabRenderDistance;
            PathRenderDistance = editorSettings.pathRenderDistance;
            WaterTransparency = editorSettings.waterTransparency;
            LoadBundleOnProjectLoad = editorSettings.loadbundleonprojectload;
            PrefabPaths = editorSettings.prefabPaths;
        }
    }

    /// <summary> Sets the EditorSettings back to default values.</summary>
    public static void SetDefaultSettings()
    {
        RustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust";
        ToolTips.rustDirectoryPath.text = RustDirectory;
        PrefabRenderDistance = 700f;
        PathRenderDistance = 250f;
        WaterTransparency = 0.2f;
        LoadBundleOnProjectLoad = false;
        SetDefaultPrefabPaths();
    }

    /// <summary>Sets the spawnable prefab paths to default paths.</summary>
    public static void SetDefaultPrefabPaths()
    {
        PrefabPaths = new string[]
        {
            @"assets/bundled/prefabs/autospawn/",
            @"assets/bundled/prefabs/bunker_rooms/",
            @"assets/bundled/prefabs/caves/",
            @"assets/bundled/prefabs/hapis/",
            @"assets/bundled/prefabs/modding/",
            @"assets/bundled/prefabs/radtown/",
            @"assets/bundled/prefabs/static/",
            @"assets/content/nature/",
            @"assets/content/building/",
            @"assets/content/props/",
            @"assets/content/structures/",
            @"assets/content/vehicles/",
            @"assets/prefabs/building/",
            @"assets/prefabs/building core/",
            @"assets/prefabs/deployable/",
            @"assets/prefabs/misc/",
            @"assets/prefabs/resource/" 
        };
    }
}

[Serializable]
public struct EditorSettings
{
    public string rustDirectory;
    public float prefabRenderDistance;
    public float pathRenderDistance;
    public float waterTransparency;
    public bool loadbundleonprojectload;
    public string[] prefabPaths;
}