using RustMapEditor.UI;
using RustMapEditor.Variables;
using UnityEditor;
using UnityEngine;

public class LayersWindow : EditorWindow
{
    int layerIndex = (int)TerrainManager.LandLayer;
    bool aboveTerrain = false;
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
        GUIContent[] layersOptionsMenu = new GUIContent[4];
        layersOptionsMenu[0] = new GUIContent("Ground");
        layersOptionsMenu[1] = new GUIContent("Biome");
        layersOptionsMenu[2] = new GUIContent("Alpha");
        layersOptionsMenu[3] = new GUIContent("Topology");

        EditorGUI.BeginChangeCheck();
        layerIndex = GUILayout.Toolbar(layerIndex, layersOptionsMenu, EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck())
            Functions.SetLandLayer((LandLayers)layerIndex, TerrainTopology.TypeToIndex((int)layers.Topologies));

        switch (TerrainManager.LandLayer)
        {
            case LandLayers.Ground:
                Functions.TextureSelect(TerrainManager.LandLayer, ref layers);
                Functions.AreaSelect();
                Functions.LayerTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground));
                Functions.RotateTools(TerrainManager.LandLayer);
                Functions.RiverTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground), ref aboveTerrain);
                Functions.SlopeTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground), ref slopesInfo);
                Functions.HeightTools(TerrainManager.LandLayer, TerrainSplat.TypeToIndex((int)layers.Ground), ref heightsInfo);
                break;
            case LandLayers.Biome:
                Functions.TextureSelect(TerrainManager.LandLayer, ref layers);
                Functions.AreaSelect();
                Functions.LayerTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome));
                Functions.RotateTools(TerrainManager.LandLayer);
                Functions.RiverTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome), ref aboveTerrain);
                Functions.SlopeTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome), ref slopesInfo);
                Functions.HeightTools(TerrainManager.LandLayer, TerrainBiome.TypeToIndex((int)layers.Biome), ref heightsInfo);
                break;
            case LandLayers.Alpha:
                Functions.LayerTools(TerrainManager.LandLayer, 0, 1);
                Functions.AreaSelect();
                Functions.RotateTools(TerrainManager.LandLayer);
                Functions.RiverTools(TerrainManager.LandLayer, 0, ref aboveTerrain, 1);
                Functions.SlopeTools(TerrainManager.LandLayer, 0, ref slopesInfo, 1);
                Functions.HeightTools(TerrainManager.LandLayer, 0, ref heightsInfo, 1);
                break;
            case LandLayers.Topology:
                Functions.TopologyLayerSelect(ref layers);
                Functions.AreaSelect();
                Functions.LayerTools(TerrainManager.LandLayer, 0, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.RotateTools(TerrainManager.LandLayer, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.TopologyTools();
                Functions.RiverTools(TerrainManager.LandLayer, 0, ref aboveTerrain, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.SlopeTools(TerrainManager.LandLayer, 0, ref slopesInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                Functions.HeightTools(TerrainManager.LandLayer, 0, ref heightsInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                break;
        }
    }
}
