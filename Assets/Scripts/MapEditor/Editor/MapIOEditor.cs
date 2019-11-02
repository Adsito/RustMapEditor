using System;
using UnityEditor;
using UnityEngine;

public class MapIOEditor : EditorWindow
{
    #region Values
    string[] landLayers = { "Ground", "Biome", "Alpha", "Topology" };
    string loadFile = "", saveFile = "", mapName = "", prefabSaveFile = "", mapPrefabSaveFile = "";
    int mapSize = 1000, mainMenuOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0, advancedOptions = 0, layerIndex = 0;
    float heightToSet = 450f, offset = 0f, heightSet = 500f, edgeHeight = 500f;
    bool[] sides = new bool[4]; 
    bool checkHeight = true, setWaterMap = false;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float slopeBlendLow = 25f, slopeBlendHigh = 75f;
    float heightBlendLow = 0f, heightBlendHigh = 1000f;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    float z1 = 0, z2 = 256, x1 = 0, x2 = 256;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    EditorEnums.Layers.LandLayers landLayerFrom = EditorEnums.Layers.LandLayers.Ground;
    EditorEnums.Layers.LandLayers landLayerToPaint = EditorEnums.Layers.LandLayers.Ground;
    int textureFrom, textureToPaint;
    int layerConditionalInt, texture = 0, topologyTexture = 0, alphaTexture;
    bool deletePrefabs = false;
    bool checkHeightCndtl = false, checkSlopeCndtl = false, checkAlpha = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f;
    float heightLowCndtl = 500f, heightHighCndtl = 600f;
    bool autoUpdate = false;
    Vector2 scrollPos = new Vector2(0, 0), presetScrollPos = new Vector2(0, 0);
    EditorEnums.Selections.ObjectSelection rotateSelection;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f;
    float blurDirection = 0f, filterStrength = 1f;
    int smoothPasses = 0;

    int[] values = { 0, 1 };
    string[] activeTextureAlpha = { "Visible", "Invisible" };
    string[] activeTextureTopo = { "Active", "Inactive" };
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
                    default:
                        prefabOptions = 0;
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
                    #region Ground Layer
                    case 0:
                        EditorUIFunctions.TextureSelect(layerIndex);
                        EditorUIFunctions.PaintTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        EditorUIFunctions.RotateTools(layerIndex);
                        EditorUIFunctions.RiverTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        EditorUIFunctions.SlopeTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        EditorUIFunctions.HeightTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        EditorUIFunctions.AreaTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        break;
                    #endregion
                    #region Biome Layer
                    case 1:
                        EditorUIFunctions.TextureSelect(layerIndex);
                        EditorUIFunctions.PaintTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        EditorUIFunctions.RotateTools(layerIndex);
                        EditorUIFunctions.RiverTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        EditorUIFunctions.SlopeTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        EditorUIFunctions.HeightTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        EditorUIFunctions.AreaTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        break;
                    #endregion
                    #region Alpha Layer
                    case 2:
                        EditorUIFunctions.PaintTools(layerIndex, 1, 0);
                        EditorUIFunctions.RotateTools(layerIndex);
                        EditorUIFunctions.RiverTools(layerIndex, 1, 0);
                        EditorUIFunctions.SlopeTools(layerIndex, 1, 0);
                        EditorUIFunctions.HeightTools(layerIndex, 1, 0);
                        EditorUIFunctions.AreaTools(layerIndex, 1, 0);
                        break;
                    #endregion
                    #region Topology Layer
                    case 3:
                        EditorUIFunctions.TopologyLayerSelect();
                        EditorUIFunctions.PaintTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        EditorUIFunctions.RotateTools(layerIndex, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        EditorUIFunctions.TopologyTools();
                        EditorUIFunctions.RiverTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        EditorUIFunctions.SlopeTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        EditorUIFunctions.HeightTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        EditorUIFunctions.AreaTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        break;
                    #endregion
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
                                        GUILayout.Label("Heights", EditorStyles.boldLabel);
                                        EditorUIFunctions.OffsetMap();
                                        EditorUIFunctions.EdgeHeight();
                                        EditorUIFunctions.SetHeight();
                                        EditorUIFunctions.MinMaxHeight();
                                        GUILayout.Label("Misc", EditorStyles.boldLabel);
                                        EditorUIFunctions.InvertMap();
                                        break;
                                    case 1:
                                        EditorUIFunctions.NormaliseMap();
                                        EditorUIFunctions.SmoothMap();
                                        EditorUIFunctions.TerraceMap();
                                        break;
                                }
                                break;
                            #endregion
                            #region Textures
                            case 1:
                                EditorUIFunctions.CopyTextures();
                                EditorUIFunctions.ConditionalPaint();
                                break;
                            #endregion
                            #region Misc
                            case 2:
                                EditorUIFunctions.RotateMap();
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