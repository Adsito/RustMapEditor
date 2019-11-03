using System;
using UnityEditor;
using UnityEngine;

public class MapIOEditor : EditorWindow
{
    #region Values
    string loadFile = "", saveFile = "", mapName = "", prefabSaveFile = "", mapPrefabSaveFile = "";
    int mapSize = 1000, mainMenuOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0, advancedOptions = 0, layerIndex = 0;
    float heightToSet = 450f, offset = 0f, heightSet = 500f;
    bool clampOffset = true;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float slopeBlendLow = 25f, slopeBlendHigh = 75f;
    float heightBlendLow = 0f, heightBlendHigh = 1000f;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    float z1 = 0, z2 = 256, x1 = 0, x2 = 256;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    Conditions conditions = new Conditions() { CheckAlpha = true};
    EditorVars.Layers layers = new EditorVars.Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field};
    ArrayOperations.Dimensions dimensions = new ArrayOperations.Dimensions() { x0 = 0, x1 = 256, z0 = 0, z1 = 256 };
    int layerConditionalInt, texture = 0, topologyTexture = 0, alphaTexture;
    bool deletePrefabs = false, autoUpdate = false;
    bool checkHeightCndtl = false, checkSlopeCndtl = false, checkAlpha = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f, heightLowCndtl = 500f, heightHighCndtl = 600f;
    Vector2 scrollPos = new Vector2(0, 0), presetScrollPos = new Vector2(0, 0);
    EditorVars.Selections.Objects rotateSelection;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f;
    float blurDirection = 0f, filterStrength = 1f;
    int smoothPasses = 0;
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
        {
            EditorUIFunctions.SetLandLayer(layerIndex);
        }

        #region Menu
        switch (mainMenuOptions)
        {
            #region File
            case 0:
                EditorUIFunctions.EditorIO(ref loadFile, ref saveFile, ref mapSize);
                EditorUIFunctions.EditorInfo();
                EditorUIFunctions.MapInfo();
                EditorUIFunctions.EditorLinks();
                EditorUIFunctions.EditorSettings();
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
                        EditorUIFunctions.AssetBundle();
                        break;
                    case 1:
                        EditorUIFunctions.PrefabTools(ref deletePrefabs, prefabSaveFile, mapPrefabSaveFile);
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
                    EditorUIFunctions.SetLandLayer(layerIndex);
                }
                ClampValues();

                switch (layerIndex)
                {
                    case 0:
                        EditorUIFunctions.TextureSelect((EditorVars.LandLayers)layerIndex, ref layers);
                        EditorUIFunctions.LayerTools((EditorVars.LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground));
                        EditorUIFunctions.RotateTools((EditorVars.LandLayers)layerIndex);
                        EditorUIFunctions.RiverTools((EditorVars.LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground), ref aboveTerrain);
                        //EditorUIFunctions.SlopeTools((EditorVars.LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground));
                        //EditorUIFunctions.HeightTools((EditorVars.LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground));
                        EditorUIFunctions.AreaTools((EditorVars.LandLayers)layerIndex, TerrainSplat.TypeToIndex((int)layers.Ground), dimensions);
                        break;
                    case 1:
                        EditorUIFunctions.TextureSelect((EditorVars.LandLayers)layerIndex, ref layers);
                        EditorUIFunctions.LayerTools((EditorVars.LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome));
                        EditorUIFunctions.RotateTools((EditorVars.LandLayers)layerIndex);
                        EditorUIFunctions.RiverTools((EditorVars.LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome), ref aboveTerrain);
                        //EditorUIFunctions.SlopeTools((EditorVars.LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome));
                        //EditorUIFunctions.HeightTools((EditorVars.LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome));
                        EditorUIFunctions.AreaTools((EditorVars.LandLayers)layerIndex, TerrainBiome.TypeToIndex((int)layers.Biome), dimensions);
                        break;
                    case 2:
                        EditorUIFunctions.LayerTools((EditorVars.LandLayers)layerIndex, 0, 1);
                        EditorUIFunctions.RotateTools((EditorVars.LandLayers)layerIndex);
                        EditorUIFunctions.RiverTools((EditorVars.LandLayers)layerIndex, 1, ref aboveTerrain, 0);
                        //EditorUIFunctions.SlopeTools((EditorVars.LandLayers)layerIndex, 1, 0);
                        //EditorUIFunctions.HeightTools((EditorVars.LandLayers)layerIndex, 1, 0);
                        EditorUIFunctions.AreaTools((EditorVars.LandLayers)layerIndex, 1, dimensions, 0);
                        break;
                    case 3:
                        EditorUIFunctions.TopologyLayerSelect(ref layers);
                        EditorUIFunctions.LayerTools((EditorVars.LandLayers)layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        EditorUIFunctions.RotateTools((EditorVars.LandLayers)layerIndex, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        EditorUIFunctions.TopologyTools();
                        EditorUIFunctions.RiverTools((EditorVars.LandLayers)layerIndex, 0, ref aboveTerrain, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        //EditorUIFunctions.SlopeTools((EditorVars.LandLayers)layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        //EditorUIFunctions.HeightTools((EditorVars.LandLayers)layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        EditorUIFunctions.AreaTools((EditorVars.LandLayers)layerIndex, 0, dimensions, 1, TerrainTopology.TypeToIndex((int)layers.Topologies));
                        break;
                }
                break;
            #endregion
            #region Advanced
            case 3:
                GUIContent[] advancedOptionsMenu = new GUIContent[2];
                advancedOptionsMenu[0] = new GUIContent("Generation");
                advancedOptionsMenu[1] = new GUIContent("Map Tools");

                EditorGUI.BeginChangeCheck();
                advancedOptions = GUILayout.Toolbar(advancedOptions, advancedOptionsMenu, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck() && advancedOptions == 0)
                {
                    MapIO.RefreshPresetsList();
                }

                switch (advancedOptions)
                {
                    #region Generation
                    case 0:
                        EditorUIFunctions.NodePresets(presetScrollPos);
                        break;
                    #endregion
                    #region Map Tools
                    case 1:
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
                                        EditorUIElements.BoldLabel(EditorVars.ToolTips.heightsLabel);
                                        EditorUIFunctions.OffsetMap(ref offset, ref clampOffset);
                                        EditorUIFunctions.SetHeight(ref heightSet);
                                        EditorUIFunctions.MinMaxHeight(ref heightToSet);
                                        EditorUIElements.BoldLabel(EditorVars.ToolTips.miscLabel);
                                        EditorUIFunctions.InvertMap();
                                        break;
                                    case 1:
                                        EditorUIFunctions.NormaliseMap(ref normaliseLow, ref normaliseHigh, ref autoUpdate);
                                        EditorUIFunctions.SmoothMap(ref filterStrength, ref blurDirection, ref smoothPasses);
                                        EditorUIFunctions.TerraceMap(ref terraceErodeFeatureSize, ref terraceErodeInteriorCornerWeight);
                                        break;
                                }
                                break;
                            #endregion
                            #region Textures
                            case 1:
                                EditorUIFunctions.ConditionalPaint(ref conditionalPaintOptions, ref layerConditionalInt, ref texture, ref conditions, ref layers);
                                break;
                            #endregion
                            #region Misc
                            case 2:
                                EditorUIFunctions.RotateMap(rotateSelection);
                                break;
                                #endregion
                        }
                        break;
                        #endregion
                }
                break;
            #endregion
        }
        #endregion
        #region InspectorGUIInput
        Event e = Event.current;
        #endregion
        EditorGUILayout.EndScrollView();
    }
    /// <summary>
    /// Clamps all the Height and Slope tool values.
    /// </summary>
    private void ClampValues()
    {
        slopeLow = Mathf.Clamp(slopeLow, 0f, slopeHigh);
        slopeHigh = Mathf.Clamp(slopeHigh, slopeLow, 90f);
        slopeBlendLow = Mathf.Clamp(slopeBlendLow, 0f, slopeLow);
        slopeBlendHigh = Mathf.Clamp(slopeBlendHigh, slopeHigh, 90f);

        heightLow = Mathf.Clamp(heightLow, 0f, heightHigh);
        heightHigh = Mathf.Clamp(heightHigh, heightLow, 1000f);
        heightBlendLow = Mathf.Clamp(heightBlendLow, 0f, heightLow);
        heightBlendHigh = Mathf.Clamp(heightBlendHigh, heightHigh, 1000f);
    }
}