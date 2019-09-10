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
        EditorSettings editorSettings = new EditorSettings
        {
            ObjectQuality = 200,
            RustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust"
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
                RustDirectory = rustDirectory
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
        }
    }
    /// <summary>
    /// Sets the EditorSettings back to default values.
    /// </summary>
    public static void SetDefaultSettings()
    {
        rustDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Rust";
        objectQuality = 200;
    }
}
[Serializable]
public struct EditorSettings
{
    public string RustDirectory;
    public int ObjectQuality;
}