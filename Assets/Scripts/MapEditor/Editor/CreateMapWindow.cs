using UnityEngine;
using UnityEditor;
using RustMapEditor.UI;
using RustMapEditor.Variables;

public class CreateMapWindow : EditorWindow
{
    int mapSize = 1000;

    public static void Init()
    {
        int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Close", "Save and Create New Map");

        switch (newMap)
        {
            case 1:
                return;
            case 2:
                Functions.SaveMapPanel();
                break;
        }

        CreateMapWindow window = CreateInstance<CreateMapWindow>();
        window.position = new Rect(ScenePos().x + ScenePos().width / 2 - 75f, ScenePos().y + ScenePos().height / 2, 200f, 60f);
        window.ShowPopup();
    }

    public void OnGUI()
    {
        Elements.BoldLabel(ToolTips.createMapLabel);

        mapSize = Elements.ToolbarIntSlider(ToolTips.mapSize, mapSize, 1000, 6000);

        Elements.BeginToolbarHorizontal();
        if (Elements.ToolbarButton(ToolTips.createMap))
        {
            MapIO.CreateMap(mapSize);
            Close();
        }
        if (Elements.ToolbarButton(ToolTips.cancel))
            Close();
        Elements.EndToolbarHorizontal();
    }

    static Rect ScenePos()
    {
        return SceneView.lastActiveSceneView.position;
    }
}
