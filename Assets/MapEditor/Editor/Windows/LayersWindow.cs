using RustMapEditor.UI;
using RustMapEditor.Variables;
using UnityEditor;
using UnityEngine;

public class LayersWindow : EditorWindow
{
    int layerIndex = (int)TerrainManager.CurrentLayerType;
    bool aboveTerrain = false;
    Vector2 scrollPos = new Vector2(0, 0);
    Layers layers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field };
    SlopesInfo slopesInfo = new SlopesInfo() { SlopeLow = 40f, SlopeHigh = 60f, SlopeBlendLow = 25f, SlopeBlendHigh = 75f, BlendSlopes = false };
    HeightsInfo heightsInfo = new HeightsInfo() { HeightLow = 400f, HeightHigh = 600f, HeightBlendLow = 300f, HeightBlendHigh = 700f, BlendHeights = false };

    public static void Init()
    {
        if (HasOpenInstances<LayersWindow>())
            return;
        LayersWindow window = CreateInstance<LayersWindow>();
        window.titleContent = new GUIContent("Layers Editor");
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        GUIContent[] layersOptionsMenu = new GUIContent[4];
        layersOptionsMenu[0] = new GUIContent("Ground");
        layersOptionsMenu[1] = new GUIContent("Biome");
        layersOptionsMenu[2] = new GUIContent("Alpha");
        layersOptionsMenu[3] = new GUIContent("Topology");

        EditorGUI.BeginChangeCheck();
        layerIndex = GUILayout.Toolbar(layerIndex, layersOptionsMenu, EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck())
            TerrainManager.ChangeLayer((TerrainManager.LayerType)layerIndex, TerrainTopology.TypeToIndex((int)layers.Topologies));

        if (layerIndex != (int)TerrainManager.CurrentLayerType)
            layerIndex = (int)TerrainManager.CurrentLayerType;

        switch ((TerrainManager.LayerType)layerIndex)
        {
            case TerrainManager.LayerType.Ground:
                Functions.TextureSelect(TerrainManager.CurrentLayerType, ref layers);
                Functions.AreaSelect();
                Functions.LayerTools(TerrainManager.CurrentLayerType, TerrainSplat.TypeToIndex((int)layers.Ground));
                Functions.RotateTools(TerrainManager.CurrentLayerType);
                Functions.RiverTools(TerrainManager.CurrentLayerType, TerrainSplat.TypeToIndex((int)layers.Ground), ref aboveTerrain);
                Functions.SlopeTools(TerrainManager.CurrentLayerType, TerrainSplat.TypeToIndex((int)layers.Ground), ref slopesInfo);
                Functions.HeightTools(TerrainManager.CurrentLayerType, TerrainSplat.TypeToIndex((int)layers.Ground), ref heightsInfo);
                break;
            case TerrainManager.LayerType.Biome:
                Functions.TextureSelect(TerrainManager.CurrentLayerType, ref layers);
                Functions.AreaSelect();
                Functions.LayerTools(TerrainManager.CurrentLayerType, TerrainBiome.TypeToIndex((int)layers.Biome));
                Functions.RotateTools(TerrainManager.CurrentLayerType);
                Functions.RiverTools(TerrainManager.CurrentLayerType, TerrainBiome.TypeToIndex((int)layers.Biome), ref aboveTerrain);
                Functions.SlopeTools(TerrainManager.CurrentLayerType, TerrainBiome.TypeToIndex((int)layers.Biome), ref slopesInfo);
                Functions.HeightTools(TerrainManager.CurrentLayerType, TerrainBiome.TypeToIndex((int)layers.Biome), ref heightsInfo);
                break;
            case TerrainManager.LayerType.Alpha:
                Functions.LayerTools((TerrainManager.LayerType)layerIndex, 0, 1);
                Functions.AreaSelect();
                Functions.RotateTools((TerrainManager.LayerType)layerIndex);
                Functions.RiverTools((TerrainManager.LayerType)layerIndex, 0, ref aboveTerrain, 1);
                Functions.SlopeTools((TerrainManager.LayerType)layerIndex, 0, ref slopesInfo, 1);
                Functions.HeightTools((TerrainManager.LayerType)layerIndex, 0, ref heightsInfo, 1);
                break;
            case TerrainManager.LayerType.Topology:
                Functions.TopologyLayerSelect(ref layers);
                Functions.AreaSelect();
                Functions.LayerTools(TerrainManager.CurrentLayerType, 0, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.RotateTools(TerrainManager.CurrentLayerType, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.TopologyTools();
                Functions.RiverTools(TerrainManager.CurrentLayerType, 0, ref aboveTerrain, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.SlopeTools(TerrainManager.CurrentLayerType, 0, ref slopesInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.HeightTools(TerrainManager.CurrentLayerType, 0, ref heightsInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                break;
        }
        EditorGUILayout.EndScrollView();
    }
}