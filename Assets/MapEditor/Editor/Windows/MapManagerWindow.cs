using UnityEditor;
using UnityEngine;
using RustMapEditor.UI;
using RustMapEditor.Variables;

public class MapManagerWindow : EditorWindow
{
    #region Values
    string prefabSaveFile = "", mapPrefabSaveFile = "";
    int mainMenuOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0, layerIndex = 0;
    float offset = 0f, heightSet = 500f, heightLow = 450f, heightHigh = 750f;
    bool clampOffset = true, aboveTerrain = false;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    Conditions conditions = new Conditions() 
    { 
        GroundConditions = new GroundConditions(TerrainSplat.Enum.Grass), BiomeConditions = new BiomeConditions(TerrainBiome.Enum.Temperate), TopologyConditions = new TopologyConditions(TerrainTopology.Enum.Beach)
    };
    Layers layers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field};
    SlopesInfo slopesInfo = new SlopesInfo() { SlopeLow = 40f, SlopeHigh = 60f, SlopeBlendLow = 25f, SlopeBlendHigh = 75f, BlendSlopes = false };
    HeightsInfo heightsInfo = new HeightsInfo() { HeightLow = 400f, HeightHigh = 600f, HeightBlendLow = 300f, HeightBlendHigh = 700f, BlendHeights = false };
    int texture = 0, smoothPasses = 0;
    bool deletePrefabs = false, autoUpdate = false;
    Vector2 scrollPos = new Vector2(0, 0);
    Selections.Objects rotateSelection;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f, blurDirection = 0f, filterStrength = 1f;
    #endregion

    public void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIContent[] mainMenu = new GUIContent[4];
        mainMenu[0] = new GUIContent("File");
        mainMenu[1] = new GUIContent("Prefabs");
        mainMenu[2] = new GUIContent("Layers");
        mainMenu[3] = new GUIContent("Advanced");

        EditorGUI.BeginChangeCheck();
        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu, EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck() && mainMenuOptions == 2)
            Functions.SetLandLayer((LandLayers)layerIndex);

        #region Menu
        switch (mainMenuOptions)
        {
            #region File
            case 0:
                Functions.EditorIO();
                Functions.EditorInfo();
                Functions.MapInfo();
                Functions.EditorLinks();
                break;
            #endregion
            #region Prefabs
            case 1:
                GUIContent[] prefabsOptionsMenu = new GUIContent[2];
                prefabsOptionsMenu[0] = new GUIContent("Asset Bundle");
                prefabsOptionsMenu[1] = new GUIContent("Prefab Tools");
                prefabOptions = GUILayout.Toolbar(prefabOptions, prefabsOptionsMenu, EditorStyles.toolbarButton);

                switch (prefabOptions)
                {
                    case 0:
                        Functions.AssetBundle();
                        break;
                    case 1:
                        Functions.PrefabTools(ref deletePrefabs, prefabSaveFile, mapPrefabSaveFile);
                        break;
                }
                break;
            #endregion
            #region Layers
            case 2:
                GUIContent[] layersOptionsMenu = new GUIContent[4];
                layersOptionsMenu[0] = new GUIContent("Ground");
                layersOptionsMenu[1] = new GUIContent("Biome");
                layersOptionsMenu[2] = new GUIContent("Alpha");
                layersOptionsMenu[3] = new GUIContent("Topology");

                EditorGUI.BeginChangeCheck();
                layerIndex = GUILayout.Toolbar(layerIndex, layersOptionsMenu, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    Functions.SetLandLayer((LandLayers)layerIndex, TerrainTopology.TypeToIndex((int)layers.Topologies));
                }

                switch (layerIndex)
                {
                    case 0:
                        Functions.TextureSelect((LandLayers)layerIndex, ref layers);
                        Functions.AreaSelect();
                        Functions.LayerTools((LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground));
                        Functions.RotateTools((LandLayers)layerIndex);
                        Functions.RiverTools((LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground), ref aboveTerrain);
                        Functions.SlopeTools((LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground), ref slopesInfo);
                        Functions.HeightTools((LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground), ref heightsInfo);
                        break;
                    case 1:
                        Functions.TextureSelect((LandLayers)layerIndex, ref layers);
                        Functions.AreaSelect();
                        Functions.LayerTools((LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome));
                        Functions.RotateTools((LandLayers)layerIndex);
                        Functions.RiverTools((LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome), ref aboveTerrain);
                        Functions.SlopeTools((LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome), ref slopesInfo);
                        Functions.HeightTools((LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome), ref heightsInfo);
                        break;
                    case 2:
                        Functions.LayerTools((LandLayers)layerIndex, 0, 1);
                        Functions.AreaSelect();
                        Functions.RotateTools((LandLayers)layerIndex);
                        Functions.RiverTools((LandLayers)layerIndex, 0, ref aboveTerrain, 1);
                        Functions.SlopeTools((LandLayers)layerIndex, 0, ref slopesInfo, 1);
                        Functions.HeightTools((LandLayers)layerIndex, 0, ref heightsInfo, 1);
                        break;
                    case 3:
                        Functions.TopologyLayerSelect(ref layers);
                        Functions.AreaSelect();
                        Functions.LayerTools((LandLayers)layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        Functions.RotateTools((LandLayers)layerIndex, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        Functions.TopologyTools();
                        Functions.RiverTools((LandLayers)layerIndex, 0, ref aboveTerrain, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        Functions.SlopeTools((LandLayers)layerIndex, 0, ref slopesInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        Functions.HeightTools((LandLayers)layerIndex, 0, ref heightsInfo, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        break;
                }
                break;
            #endregion
            case 3:
            GUIContent[] mapToolsMenu = new GUIContent[3];
            mapToolsMenu[0] = new GUIContent("HeightMap");
            mapToolsMenu[1] = new GUIContent("Textures");
            mapToolsMenu[2] = new GUIContent("Misc");
            mapToolsOptions = GUILayout.Toolbar(mapToolsOptions, mapToolsMenu, EditorStyles.toolbarButton);
            
            switch (mapToolsOptions)
            {
                #region HeightMap
                case 0:
                    GUIContent[] heightMapMenu = new GUIContent[2];
                    heightMapMenu[0] = new GUIContent("Heights");
                    heightMapMenu[1] = new GUIContent("Filters");
                    heightMapOptions = GUILayout.Toolbar(heightMapOptions, heightMapMenu, EditorStyles.toolbarButton);
            
                    switch (heightMapOptions)
                    {
                        case 0:
                            Elements.BoldLabel(ToolTips.heightsLabel);
                            Functions.OffsetMap(ref offset, ref clampOffset);
                            Functions.SetHeight(ref heightSet);
                            Functions.ClampHeight(ref heightLow, ref heightHigh);
                            Elements.BoldLabel(ToolTips.miscLabel);
                            Functions.InvertMap();
                            break;
                        case 1:
                            Functions.NormaliseMap(ref normaliseLow, ref normaliseHigh, ref autoUpdate);
                            Functions.SmoothMap(ref filterStrength, ref blurDirection, ref smoothPasses);
                            Functions.TerraceMap(ref terraceErodeFeatureSize, ref terraceErodeInteriorCornerWeight);
                            break;
                    }
                    break;
                #endregion

                #region Textures
                case 1:
                    Functions.ConditionalPaint(ref conditionalPaintOptions, ref texture, ref conditions, ref layers);
                    break;
                #endregion

                #region Misc
                case 2:
                    Functions.RotateMap(ref rotateSelection);
                    break;
                    #endregion
            }
            break;
        }
        #endregion
        EditorGUILayout.EndScrollView();
    }
}