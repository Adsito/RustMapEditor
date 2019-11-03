using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MapEditorSettings
{
    public const string settingsPath = "EditorSettings.json";
    public const string bundlePathExt = @"\Bundles\Bundles";

    public static string rustDirectory;
    public static int objectQuality;
    public static string[] prefabPaths;

    [InitializeOnLoadMethod]
    static void Init()
    {
        if (!File.Exists(settingsPath))
        {
            using (StreamWriter write = new StreamWriter(settingsPath, false))
            {
                write.Write(JsonUtility.ToJson(DefaultSettings()));
            }
        }
        LoadSettings();
    }
    /// <summary>
    /// Returns a new EditorSettings struct set with the default settings.
    /// </summary>
    /// <returns></returns>
    static EditorSettings DefaultSettings()
    {
        SetDefaultSettings();
        EditorSettings editorSettings = new EditorSettings
        {
            ObjectQuality = objectQuality,
            RustDirectory = rustDirectory,
            PrefabPaths = prefabPaths
        };
        return editorSettings;
    }
    /// <summary>
    /// Saves the current EditorSettings to a JSON file.
    /// </summary>
    public static void SaveSettings()
    {
        using (StreamWriter write = new StreamWriter(settingsPath, false))
        {
            EditorSettings editorSettings = new EditorSettings()
            {
                ObjectQuality = objectQuality,
                RustDirectory = rustDirectory,
                PrefabPaths = prefabPaths
            };
            write.Write(JsonUtility.ToJson(editorSettings));
        }
    }
    /// <summary>
    /// Loads and sets the current EditorSettings from a JSON file.
    /// </summary>
    public static void LoadSettings()
    {
        using (StreamReader reader = new StreamReader(settingsPath))
        {
            EditorSettings editorSettings = JsonUtility.FromJson<EditorSettings>(reader.ReadToEnd());
            rustDirectory = editorSettings.RustDirectory;
            objectQuality = editorSettings.ObjectQuality;
            prefabPaths = editorSettings.PrefabPaths;
        }
    }
    /// <summary>
    /// Sets the EditorSettings back to default values.
    /// </summary>
    public static void SetDefaultSettings()
    {
        rustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust";
        objectQuality = 200;
        SetDefaultPrefabPaths();
    }
    /// <summary>
    /// Sets the spawnable prefab paths to default paths.
    /// </summary>
    public static void SetDefaultPrefabPaths()
    {
        prefabPaths = new string[]
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
    public string RustDirectory;
    public int ObjectQuality;
    public string[] PrefabPaths;
}