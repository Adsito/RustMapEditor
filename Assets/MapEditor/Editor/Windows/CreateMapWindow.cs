using UnityEngine;
using UnityEditor;
using RustMapEditor.UI;
using RustMapEditor.Variables;

public class CreateMapWindow : EditorWindow
{
    int mapSize = 1000;
    Layers layers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate };
    float landHeight = 505;

    static Rect ScenePos { get => SceneView.lastActiveSceneView.position; }

    public static void Init()
    {
        CreateMapWindow window = CreateInstance<CreateMapWindow>();
        window.position = new Rect(ScenePos.x + ScenePos.width / 2 - 75f, ScenePos.y + ScenePos.height / 2, 250f, 123f);
        window.ShowPopup();
    }

    public void OnGUI()
    {
        Elements.BoldLabel(ToolTips.createMapLabel);

        Functions.NewMapOptions(ref mapSize, ref landHeight, ref layers, this);
    }
}
