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
    public static bool LoadBundleOnLaunch { get; set; }
    public static bool TerrainTextureSet { get; set; }
    public static string[] PrefabPaths { get; private set; }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        if (!File.Exists(SettingsPath))
            using (StreamWriter write = new StreamWriter(SettingsPath, false))
                write.Write(JsonUtility.ToJson(new EditorSettings(), true));

        LoadSettings();
    }

    /// <summary>Saves the current EditorSettings to a JSON file.</summary>
    public static void SaveSettings()
    {
        using (StreamWriter write = new StreamWriter(SettingsPath, false))
        {
            EditorSettings editorSettings = new EditorSettings
            (
                RustDirectory, PrefabRenderDistance, PathRenderDistance, WaterTransparency, LoadBundleOnLaunch
            );
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
            LoadBundleOnLaunch = editorSettings.loadbundleonlaunch;
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
        LoadBundleOnLaunch = false;
    }
}

[Serializable]
public struct EditorSettings
{
    public string rustDirectory;
    public float prefabRenderDistance;
    public float pathRenderDistance;
    public float waterTransparency;
    public bool loadbundleonlaunch;
    public bool terrainTextureSet;
    public string[] prefabPaths;

    public EditorSettings
    (
        string rustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust", float prefabRenderDistance = 700f, float pathRenderDistance = 200f, 
        float waterTransparency = 0.2f, bool loadbundleonlaunch = false, bool terrainTextureSet = false)
        {
            this.rustDirectory = rustDirectory;
            this.prefabRenderDistance = prefabRenderDistance;
            this.pathRenderDistance = pathRenderDistance;
            this.waterTransparency = waterTransparency;
            this.loadbundleonlaunch = loadbundleonlaunch;
            this.terrainTextureSet = terrainTextureSet;
            this.prefabPaths = SettingsManager.PrefabPaths;
        }
}