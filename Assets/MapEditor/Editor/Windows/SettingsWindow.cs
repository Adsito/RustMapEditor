using UnityEngine;
using UnityEditor;
using RustMapEditor.UI;

public class SettingsWindow : EditorWindow
{
    public static void Init()
    {
        SettingsWindow window = GetWindow<SettingsWindow>();
        window.minSize = new Vector2(300f, 195f);
        window.titleContent = new GUIContent("Settings");
    }

    public void OnGUI()
    {
        Functions.EditorSettings();
    }
}