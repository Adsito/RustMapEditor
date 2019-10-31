using System;
using UnityEditor;
using UnityEngine;
using Rotorz.ReorderableList;

public class MapIOEditor : EditorWindow
{
    #region Values
    string editorVersion = "v2.2-prerelease";
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
            SetLandLayer(layerIndex);
        }

        #region Menu
        switch (mainMenuOptions)
        {
            #region File
            case 0:
                EditorIO();
                EditorInfo();
                MapInfo();
                EditorLinks();
                EditorSettings();
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
                        AssetBundle();
                        break;
                    case 1:
                        GUILayout.Label("Tools", EditorStyles.miniBoldLabel);

                        deletePrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Delete on Export.", "Deletes prefabs/lootcrates after exporting."), deletePrefabs, GUILayout.MaxWidth(300));
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        if (GUILayout.Button(new GUIContent("Export LootCrates", "Exports all lootcrates that don't yet respawn in Rust to a JSON for use with the LootCrateRespawn plugin." +
                            "If you don't delete them after export they will duplicate on first map load."), EditorStyles.toolbarButton))
                        {
                            prefabSaveFile = EditorUtility.SaveFilePanel("Export LootCrates", prefabSaveFile, "LootCrateData", "json");
                            if (prefabSaveFile == "")
                            {
                                return;
                            }
                            MapIO.ExportLootCrates(prefabSaveFile, deletePrefabs);
                        }
                        if (GUILayout.Button(new GUIContent("Export Map Prefabs", "Exports all map prefabs to plugin data."), EditorStyles.toolbarButton))
                        {
                            mapPrefabSaveFile = EditorUtility.SaveFilePanel("Export Map Prefabs", prefabSaveFile, "MapData", "json");
                            if (mapPrefabSaveFile == "")
                            {
                                return;
                            }
                            MapIO.ExportMapPrefabs(mapPrefabSaveFile, deletePrefabs);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        if (GUILayout.Button(new GUIContent("Hide Prefabs in RustEdit", "Changes all the prefab categories to a semi-colon. Hides all of the prefabs from " +
                            "appearing in RustEdit. Use the break RustEdit Custom Prefabs button to undo."), EditorStyles.toolbarButton))
                        {
                            MapIO.HidePrefabsInRustEdit();
                        }
                        if (GUILayout.Button(new GUIContent("Break RustEdit Custom Prefabs", "Breaks down all custom prefabs saved in the map file."), EditorStyles.toolbarButton))
                        {
                            MapIO.BreakRustEditCustomPrefabs();
                        }
                        EditorGUILayout.EndHorizontal();
                        if (GUILayout.Button(new GUIContent("Group RustEdit Custom Prefabs", "Groups all custom prefabs saved in the map file."), EditorStyles.toolbarButton))
                        {
                            MapIO.GroupRustEditCustomPrefabs();
                        }
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                        if (GUILayout.Button(new GUIContent("Delete All Map Prefabs", "Removes all the prefabs from the map."), EditorStyles.toolbarButton))
                        {
                            MapIO.RemoveMapObjects(true, false);
                        }
                        if (GUILayout.Button(new GUIContent("Delete All Map Paths", "Removes all the paths from the map."), EditorStyles.toolbarButton))
                        {
                            MapIO.RemoveMapObjects(false, true);
                        }
                        EditorGUILayout.EndHorizontal();
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
                    SetLandLayer(layerIndex);
                }
                ClampValues();

                switch (layerIndex)
                {
                    #region Ground Layer
                    case 0:
                        TextureSelect(layerIndex);
                        PaintTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        RotateTools(layerIndex);
                        RiverTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        SlopeTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        HeightTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        AreaTools(layerIndex, TerrainSplat.TypeToIndex((int)MapIO.groundLayer));
                        break;
                    #endregion
                    #region Biome Layer
                    case 1:
                        TextureSelect(layerIndex);
                        PaintTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        RotateTools(layerIndex);
                        RiverTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        SlopeTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        HeightTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        AreaTools(layerIndex, TerrainBiome.TypeToIndex((int)MapIO.biomeLayer));
                        break;
                    #endregion
                    #region Alpha Layer
                    case 2:
                        PaintTools(layerIndex, 1, 0);
                        RotateTools(layerIndex);
                        RiverTools(layerIndex, 1, 0);
                        SlopeTools(layerIndex, 1, 0);
                        HeightTools(layerIndex, 1, 0);
                        AreaTools(layerIndex, 1, 0);
                        break;
                    #endregion
                    #region Topology Layer
                    case 3:
                        TopologyLayerSelect();
                        PaintTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        RotateTools(layerIndex, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        TopologyTools();
                        RiverTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        SlopeTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        HeightTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
                        AreaTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
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
                    MapIO.RefreshAssetList();
                }

                switch (advancedOptions)
                {
                    #region Generation
                    case 0:
                        NodePresets();
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
                                        OffsetMap();
                                        EdgeHeight();
                                        SetHeight();
                                        MinMaxHeight();
                                        GUILayout.Label("Misc", EditorStyles.boldLabel);
                                        InvertMap();
                                        break;
                                    case 1:
                                        NormaliseMap();
                                        SmoothMap();
                                        TerraceMap();
                                        break;
                                }
                                break;
                            #endregion
                            #region Textures
                            case 1:
                                CopyTextures();
                                ConditionalPaint();
                                break;
                            #endregion
                            #region Misc
                            case 2:
                                RotateMap();
                                DebugMap();
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
    #region Menu Items
    [MenuItem("Rust Map Editor/Main Menu", false, 0)]
    static void OpenMainMenu()
    {
        MapIOEditor window = (MapIOEditor)EditorWindow.GetWindow(typeof(MapIOEditor), false, "Rust Map Editor");
    }
    [MenuItem("Rust Map Editor/Terrain Tools", false, 1)]
    static void OpenTerrainTools()
    {
        Selection.activeGameObject = GameObject.FindGameObjectWithTag("Land");
    }
    [MenuItem("Rust Map Editor/Links/Wiki", false, 2)]
    static void OpenWiki()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/wiki");
    }
    [MenuItem("Rust Map Editor/Links/Discord", false, 3)]
    static void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/HPmTWVa");
    }
    [MenuItem("Rust Map Editor/Links/RoadMap", false, 3)]
    static void OpenRoadMap()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/projects/1");
    }
    [MenuItem("Rust Map Editor/Links/Report Bug", false, 4)]
    static void OpenReportBug()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
    }
    [MenuItem("Rust Map Editor/Links/Request Feature", false, 5)]
    static void OpenRequestFeature()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
    }
    #endregion
    #region Methods
    #region Prefabs
    private void AssetBundle()
    {
        GUILayout.Label("Asset Bundle", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Load", "Loads all the prefabs from the Rust Asset Bundle for use in the editor. Prefabs paths to be loaded can be changed in " +
            "AssetList.txt in the root directory"), EditorStyles.toolbarButton))
        {
            PrefabManager.LoadBundle(MapEditorSettings.rustDirectory + MapEditorSettings.bundlePathExt);
        }
        if (GUILayout.Button(new GUIContent("Unload", "Unloads the loaded bundle and prefabs."), EditorStyles.toolbarButton))
        {
            PrefabManager.DisposeBundle();
        }
        EditorGUILayout.EndHorizontal();
    }
    #endregion
    #region Functions
    private string NodePresetDrawer(Rect position, string itemValue)
    {
        position.width -= 39;
        GUI.Label(position, itemValue);
        position.x = position.xMax;
        position.width = 39;
        if (GUI.Button(position, new GUIContent("Open", "Opens the Node Editor for the preset."), EditorStyles.toolbarButton))
        {
            MapIO.RefreshAssetList();
            MapIO.nodePresetLookup.TryGetValue(itemValue, out UnityEngine.Object preset);
            if (preset != null)
            {
                AssetDatabase.OpenAsset(preset.GetInstanceID());
            }
            else
            {
                Debug.LogError("The preset you are trying to open is null.");
            }
        }
        return itemValue;
    }
    /// <summary>
    /// Sets the active landLayer to the index.
    /// </summary>
    /// <param name="landIndex">The landLayer to change to.</param>
    /// <param name="topology">The Topology layer to set.</param>
    private void SetLandLayer(int landIndex, int topology = 0)
    {
        LandData.ChangeLandLayer(landLayers[landIndex], topology);
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
    #endregion
    #region Generation Tools
    private void NodePresets()
    {
        GUILayout.Label(new GUIContent("Node Presets", "List of all the node presets in the project."), EditorStyles.boldLabel);

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Refresh presets list.", "Refreshes the list of all the Node Presets in the project."), EditorStyles.toolbarButton))
        {
            MapIO.RefreshAssetList();
        }
        GUILayout.EndHorizontal();

        presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
        ReorderableListGUI.Title("Node Presets");
        ReorderableListGUI.ListField(MapIO.generationPresetList, NodePresetDrawer, DrawEmpty);
        GUILayout.EndScrollView();
    }
    #endregion
    #region MapTools
    #region HeightMap
    private void TerraceMap()
    {
        GUILayout.Label(new GUIContent("Terrace", "Terrace the entire terrain."), EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.LabelField(new GUIContent("Feature Size", "The higher the value the more terrace levels generated."), GUILayout.MaxWidth(85));
        terraceErodeFeatureSize = EditorGUILayout.Slider(terraceErodeFeatureSize, 2f, 1000f);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.LabelField(new GUIContent("Corner Weight", "The strength of the corners of the terrace."), GUILayout.MaxWidth(85));
        terraceErodeInteriorCornerWeight = EditorGUILayout.Slider(terraceErodeInteriorCornerWeight, 0f, 1f);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Terrace Map", "Terraces the heightmap."), EditorStyles.toolbarButton))
        {
            MapIO.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void SmoothMap()
    {
        GUILayout.Label(new GUIContent("Smooth", "Smooth the entire terrain."), EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.LabelField(new GUIContent("Strength", "The strength of the smoothing operation."), GUILayout.MaxWidth(85));
        filterStrength = EditorGUILayout.Slider(filterStrength, 0f, 1f);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.LabelField(new GUIContent("Blur Direction", "The direction the terrain should blur towards. Negative is down, " +
            "positive is up."), GUILayout.MaxWidth(85));
        blurDirection = EditorGUILayout.Slider(blurDirection, -1f, 1f);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Smooth Map", "Smoothes the heightmap " + smoothPasses + " times."), EditorStyles.toolbarButton))
        {
            for (int i = 0; i < smoothPasses; i++)
            {
                MapIO.SmoothHeightmap(filterStrength, blurDirection);
            }
        }
        smoothPasses = EditorGUILayout.IntSlider(smoothPasses, 1, 1000);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    private void NormaliseMap()
    {
        GUILayout.Label(new GUIContent("Normalise", "Moves the heightmap heights to between the two heights."), EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.LabelField(new GUIContent("Low", "The lowest point on the map after being normalised."), GUILayout.MaxWidth(40));
        EditorGUI.BeginChangeCheck();
        normaliseLow = EditorGUILayout.Slider(normaliseLow, 0f, 1000f);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.LabelField(new GUIContent("High", "The highest point on the map after being normalised."), GUILayout.MaxWidth(40));
        normaliseHigh = EditorGUILayout.Slider(normaliseHigh, 0f, 1000f);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (EditorGUI.EndChangeCheck() && autoUpdate == true)
        {
            MapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Normalise", "Normalises the heightmap between these heights."), EditorStyles.toolbarButton))
        {
            MapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f);
        }
        autoUpdate = EditorGUILayout.ToggleLeft(new GUIContent("Auto Update", "Automatically applies the changes to the heightmap on value change."), autoUpdate);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    private void EdgeHeight()
    {
        GUILayout.Label("Edge Height", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        edgeHeight = EditorGUILayout.FloatField(edgeHeight, EditorStyles.toolbarTextField, GUILayout.MaxWidth(50));
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        sides[0] = EditorGUILayout.ToggleLeft("Top ", sides[0], GUILayout.MaxWidth(40));
        sides[3] = EditorGUILayout.ToggleLeft("Left ", sides[3], GUILayout.MaxWidth(40));
        sides[2] = EditorGUILayout.ToggleLeft("Bottom ", sides[2], GUILayout.MaxWidth(58));
        sides[1] = EditorGUILayout.ToggleLeft("Right ", sides[1], GUILayout.MaxWidth(50));
        
        if (GUILayout.Button(new GUIContent("Set Edge Height", "Sets the very edge of the map to " + edgeHeight.ToString() + 
            " metres on any of the sides selected."), EditorStyles.toolbarButton))
        {
            MapIO.SetEdgePixel(edgeHeight, sides);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void SetHeight()
    {
        GUILayout.Label("Set Height", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        heightSet = EditorGUILayout.FloatField(heightSet, EditorStyles.toolbarTextField, GUILayout.MaxWidth(50));
        if (GUILayout.Button(new GUIContent("Set Land Height", "Sets the land height to " + heightSet.ToString() + " m."), EditorStyles.toolbarButton))
        {
            MapIO.SetHeightmap(heightSet, EditorEnums.Selections.Terrains.Land);
        }
        if (GUILayout.Button(new GUIContent("Set Water Height", "Sets the water height to " + heightSet.ToString() + " m."), EditorStyles.toolbarButton))
        {
            MapIO.SetHeightmap(heightSet, EditorEnums.Selections.Terrains.Water);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void MinMaxHeight()
    {
        GUILayout.Label("Heightmap Minimum/Maximum Height", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        heightToSet = EditorGUILayout.FloatField(heightToSet, EditorStyles.toolbarTextField, GUILayout.MaxWidth(50));
        if (GUILayout.Button(new GUIContent("Set Minimum Height", "Raises any of the land below " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
            " metres."), EditorStyles.toolbarButton))
        {
            MapIO.SetMinimumHeight(heightToSet);
        }
        if (GUILayout.Button(new GUIContent("Set Maximum Height", "Lowers any of the land above " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
            " metres."), EditorStyles.toolbarButton))
        {
            MapIO.SetMaximumHeight(heightToSet);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void OffsetMap()
    {
        GUILayout.Label("Heightmap Offset", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        offset = EditorGUILayout.FloatField(offset, EditorStyles.toolbarTextField, GUILayout.MaxWidth(50));
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        checkHeight = EditorGUILayout.ToggleLeft(new GUIContent("Check", "Prevents the flattening effect if you raise or lower the heightmap" +
            " by too large a value."), checkHeight, GUILayout.MaxWidth(55));
        setWaterMap = EditorGUILayout.ToggleLeft(new GUIContent("Water", "If toggled it will raise or lower the water heightmap as well as the " +
            "land heightmap."), setWaterMap, GUILayout.MaxWidth(55));
        if (GUILayout.Button(new GUIContent("Offset Heightmap", "Raises or lowers the height of the entire heightmap by " + offset.ToString() + " metres. " +
            "A positive offset will raise the heightmap, a negative offset will lower the heightmap."), EditorStyles.toolbarButton))
        {
            MapIO.OffsetHeightmap(offset, checkHeight, setWaterMap);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void InvertMap()
    {
        GUILayout.Label("Invert", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Invert", "Inverts the heightmap in on itself."), EditorStyles.toolbarButton))
        {
            MapIO.InvertHeightmap();
        }
        EditorGUILayout.EndHorizontal();
    }
    #endregion
    #region Textures
    private void ConditionalPaint()
    {
        GUILayout.Label("Conditional Paint", EditorStyles.boldLabel);

        GUIContent[] conditionalPaintMenu = new GUIContent[5];
        conditionalPaintMenu[0] = new GUIContent("Ground");
        conditionalPaintMenu[1] = new GUIContent("Biome");
        conditionalPaintMenu[2] = new GUIContent("Alpha");
        conditionalPaintMenu[3] = new GUIContent("Topology");
        conditionalPaintMenu[4] = new GUIContent("Terrain");
        conditionalPaintOptions = GUILayout.Toolbar(conditionalPaintOptions, conditionalPaintMenu, EditorStyles.toolbarButton);

        GUILayout.Label(new GUIContent("Conditions"), EditorStyles.miniBoldLabel);

        switch (conditionalPaintOptions)
        {
            case 0: // Ground
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Textures To Check:", "The Ground textures to check."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.conditionalGround = (TerrainSplat.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalGround, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
                break;
            case 1: // Biome
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Textures To Check:", "The Biome textures to check."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.conditionalBiome = (TerrainBiome.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalBiome, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
                break;
            case 2: // Alpha
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Check Alpha:", "If toggled the Alpha will be checked on the selected texture."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                checkAlpha = EditorGUILayout.Toggle(checkAlpha);
                GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
                EditorGUILayout.EndHorizontal();

                if (checkAlpha)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GUILayout.Label(new GUIContent("Texture To Check:", "The Alpha texture to check."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                    alphaTexture = EditorGUILayout.IntPopup(alphaTexture, activeTextureAlpha, values, EditorStyles.toolbarDropDown);
                    EditorGUILayout.EndHorizontal();
                }
                break;
            case 3: // Topology
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Layers:", "The Topology layers to check."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.conditionalTopology = (TerrainTopology.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalTopology, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Check:", "The Topology texture to check."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                topologyTexture = EditorGUILayout.IntPopup(topologyTexture, activeTextureTopo, values, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
                break;
            case 4: // Terrain
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Check Slopes:", "If toggled the Slopes will be checked within the selected range."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                checkSlopeCndtl = EditorGUILayout.Toggle(checkSlopeCndtl);
                EditorGUILayout.EndHorizontal();

                if (checkSlopeCndtl == true)
                {
                    if (slopeLowCndtl > slopeHighCndtl)
                    {
                        slopeLowCndtl = slopeHighCndtl - 0.05f;
                    }
                    if (slopeLowCndtl < 0)
                    {
                        slopeLowCndtl = 0f;
                    }
                    if (slopeHighCndtl > 90f)
                    {
                        slopeHighCndtl = 90f;
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    slopeLowCndtl = EditorGUILayout.FloatField(slopeLowCndtl, EditorStyles.toolbarTextField);
                    slopeHighCndtl = EditorGUILayout.FloatField(slopeHighCndtl, EditorStyles.toolbarTextField);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.MinMaxSlider(ref slopeLowCndtl, ref slopeHighCndtl, 0f, 90f);
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Check Heights:", "If toggled the Height will be checked within the selected range."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                checkHeightCndtl = EditorGUILayout.Toggle(checkHeightCndtl);
                EditorGUILayout.EndHorizontal();

                if (checkHeightCndtl == true)
                {
                    if (heightLowCndtl > heightHighCndtl)
                    {
                        heightLowCndtl = heightHighCndtl - 0.05f;
                    }
                    if (heightLowCndtl < 0)
                    {
                        heightLowCndtl = 0f;
                    }
                    if (heightHighCndtl > 1000f)
                    {
                        heightHighCndtl = 1000f;
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    heightLowCndtl = EditorGUILayout.FloatField(heightLowCndtl, EditorStyles.toolbarTextField);
                    heightHighCndtl = EditorGUILayout.FloatField(heightHighCndtl, EditorStyles.toolbarTextField);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.MinMaxSlider(ref heightLowCndtl, ref heightHighCndtl, 0f, 1000f);
                }
                break;
        }
        GUILayout.Label(new GUIContent("Texture To Paint"), EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(new GUIContent("Layer:", "The layer to paint to."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
        layerConditionalInt = EditorGUILayout.Popup(layerConditionalInt, landLayers, EditorStyles.toolbarDropDown);
        EditorGUILayout.EndHorizontal();
        
        switch (layerConditionalInt)
        {
            case 0:
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Ground texture to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.groundLayerToPaint = (TerrainSplat.Enum)EditorGUILayout.EnumPopup(MapIO.groundLayerToPaint, EditorStyles.toolbarDropDown);
                texture = TerrainSplat.TypeToIndex((int)MapIO.groundLayerToPaint);
                EditorGUILayout.EndHorizontal();
                break;
            case 1:
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Biome texture to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.biomeLayerToPaint = (TerrainBiome.Enum)EditorGUILayout.EnumPopup(MapIO.biomeLayerToPaint, EditorStyles.toolbarButton);
                texture = TerrainBiome.TypeToIndex((int)MapIO.biomeLayerToPaint);
                EditorGUILayout.EndHorizontal();
                break;
            case 2:
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Alpha texture to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                texture = EditorGUILayout.IntPopup(texture, activeTextureAlpha, values, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
                break;
            case 3:
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Topology Layer:", "The Topology layer to paint to."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup(MapIO.topologyLayerToPaint, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Topology texture to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                texture = EditorGUILayout.IntPopup(texture, activeTextureTopo, values, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
                break;
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Paint Conditional", "Paints the selected texture if it matches all of the conditions set."), EditorStyles.toolbarButton))
        {
            Conditions conditions = new Conditions();
            conditions.GroundConditions = MapIO.conditionalGround;
            conditions.BiomeConditions = MapIO.conditionalBiome;
            conditions.CheckAlpha = checkAlpha;
            conditions.AlphaTexture = alphaTexture;
            conditions.TopologyLayers = MapIO.conditionalTopology;
            conditions.TopologyTexture = topologyTexture;
            conditions.SlopeLow = slopeLowCndtl;
            conditions.SlopeHigh = slopeHighCndtl;
            conditions.HeightLow = heightLowCndtl;
            conditions.HeightHigh = heightHighCndtl;
            conditions.CheckHeight = checkHeightCndtl;
            conditions.CheckSlope = checkSlopeCndtl;
            MapIO.PaintConditional(landLayers[layerConditionalInt], texture, conditions);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void CopyTextures()
    {
        GUILayout.Label(new GUIContent("Copy Textures", "Copies the texture selected, and paints it with the selected texture."), EditorStyles.boldLabel);

        GUILayout.Label(new GUIContent("Texture To Copy"), EditorStyles.miniBoldLabel);

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(new GUIContent("Layer:", "The layer to copy from."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
        EditorGUI.BeginChangeCheck();
        landLayerFrom = (EditorEnums.Layers.LandLayers)EditorGUILayout.EnumPopup(landLayerFrom, EditorStyles.toolbarDropDown);
        if (EditorGUI.EndChangeCheck() && (int)landLayerFrom > 1 && textureFrom > 1)
        {
            textureFrom = 0;
        }
        GUILayout.EndHorizontal();

        switch ((int)landLayerFrom) // Get texture list from the currently selected landLayer.
        {
            case 0:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Copy:", "The Ground texture which will be copied."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.groundLayerFrom = (TerrainSplat.Enum)EditorGUILayout.EnumPopup(MapIO.groundLayerFrom, EditorStyles.toolbarDropDown);
                textureFrom = TerrainSplat.TypeToIndex((int)MapIO.groundLayerFrom);
                GUILayout.EndHorizontal();
                break;
            case 1:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Copy:", "The Biome which will be copied."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.biomeLayerFrom = (TerrainBiome.Enum)EditorGUILayout.EnumPopup(MapIO.biomeLayerFrom, EditorStyles.toolbarDropDown);
                textureFrom = TerrainBiome.TypeToIndex((int)MapIO.biomeLayerFrom);
                GUILayout.EndHorizontal();
                break;
            case 2:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Copy:", "The active/inactive alpha which will be copied."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                textureFrom = EditorGUILayout.IntPopup(textureFrom, activeTextureAlpha, values, EditorStyles.toolbarDropDown);
                GUILayout.EndHorizontal();
                break;
            case 3:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Topology Layer:", "The Topology layer to copy from."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.topologyLayerFrom = (TerrainTopology.Enum)EditorGUILayout.EnumPopup(MapIO.topologyLayerFrom, EditorStyles.toolbarDropDown);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Copy:", "The active/inactive topology which will be copied."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                textureFrom = EditorGUILayout.IntPopup(textureFrom, activeTextureTopo, values, EditorStyles.toolbarDropDown);
                GUILayout.EndHorizontal();
                break;
        }

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(new GUIContent("Layer:", "The layer to copy to."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
        EditorGUI.BeginChangeCheck();
        landLayerToPaint = (EditorEnums.Layers.LandLayers)EditorGUILayout.EnumPopup(landLayerToPaint, EditorStyles.toolbarDropDown);
        if (EditorGUI.EndChangeCheck() && (int)landLayerToPaint > 1 && textureToPaint > 1)
        {
            textureToPaint = 0;
        }
        GUILayout.EndHorizontal();

        GUILayout.Label(new GUIContent("Texture To Paste"), EditorStyles.miniBoldLabel);

        switch ((int)landLayerToPaint) // Get texture list from the currently selected landLayer.
        {
            case 0:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Ground texture to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.groundLayerToPaint = (TerrainSplat.Enum)EditorGUILayout.EnumPopup(MapIO.groundLayerToPaint, EditorStyles.toolbarDropDown);
                textureToPaint = TerrainSplat.TypeToIndex((int)MapIO.groundLayerToPaint);
                GUILayout.EndHorizontal();
                break;
            case 1:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Biome to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.biomeLayerToPaint = (TerrainBiome.Enum)EditorGUILayout.EnumPopup(MapIO.biomeLayerToPaint, EditorStyles.toolbarDropDown);
                textureToPaint = TerrainBiome.TypeToIndex((int)MapIO.biomeLayerToPaint);
                GUILayout.EndHorizontal();
                break;
            case 2:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Alpha to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                textureToPaint = EditorGUILayout.IntPopup(textureToPaint, activeTextureAlpha, values, EditorStyles.toolbarDropDown);
                GUILayout.EndHorizontal();
                break;
            case 3:
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Topology Layer:", "The Topology layer to paint to."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                MapIO.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup(MapIO.topologyLayerToPaint, EditorStyles.toolbarDropDown);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(new GUIContent("Texture To Paint:", "The Topology texture to paint."), EditorStyles.toolbarButton, GUILayout.MaxWidth(120));
                textureToPaint = EditorGUILayout.IntPopup(textureToPaint, activeTextureTopo, values, EditorStyles.toolbarDropDown);
                GUILayout.EndHorizontal();
                break;
        }

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Copy textures to new layer", "Copies the Texture from the " + landLayers[(int)landLayerFrom] + " layer and " +
            "paints it on the " + landLayers[(int)landLayerToPaint] + " layer."), EditorStyles.toolbarButton))
        {
            MapIO.CopyTexture(landLayers[(int)landLayerFrom], landLayers[(int)landLayerToPaint], textureFrom, textureToPaint, TerrainTopology.TypeToIndex((int)MapIO.topologyLayerFrom), TerrainTopology.TypeToIndex((int)MapIO.topologyLayerToPaint));
        }
        GUILayout.EndHorizontal();
    }
    #endregion
    #region Misc
    private void RotateMap()
    {
        GUILayout.Label("Rotate Map", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(new GUIContent("Rotation Selection: ", "The items to rotate."), EditorStyles.toolbarButton);
        rotateSelection = (EditorEnums.Selections.ObjectSelection)EditorGUILayout.EnumFlagsField(new GUIContent(), rotateSelection, EditorStyles.toolbarDropDown);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Rotate 90°", EditorStyles.toolbarButton))
        {
            MapIO.RotateMap(rotateSelection, true);
        }
        if (GUILayout.Button("Rotate 270°", EditorStyles.toolbarButton))
        {
            MapIO.RotateMap(rotateSelection, false);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void DebugMap()
    {
        GUILayout.Label("Debug", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Debug Water", "Raises the water heightmap to 500 metres if it is below."), EditorStyles.toolbarButton))
        {
            MapIO.DebugWaterLevel();
        }
        EditorGUILayout.EndHorizontal();
    }
    #endregion
    #endregion
    #region LayerTools
    private void TextureSelect(int index)
    {
        GUILayout.Label("Texture Select", EditorStyles.miniBoldLabel);

        switch (index)
        {
            case 0:
                EditorUI.BeginToolbarHorizontal();
                EditorUI.ToolbarLabel(EditorVars.ToolTips.groundTextureSelect);
                MapIO.groundLayer = (TerrainSplat.Enum)EditorUI.ToolbarEnumPopup(MapIO.groundLayer);
                EditorUI.EndToolbarHorizontal();
                break;
            case 1:
                EditorUI.BeginToolbarHorizontal();
                EditorUI.ToolbarLabel(EditorVars.ToolTips.biomeTextureSelect);
                MapIO.biomeLayer = (TerrainBiome.Enum)EditorUI.ToolbarEnumPopup(MapIO.biomeLayer);
                EditorUI.EndToolbarHorizontal();
                break;
        }
    }
    private void TopologyLayerSelect()
    {
        GUILayout.Label("Layer Select", EditorStyles.miniBoldLabel);

        EditorUI.BeginToolbarHorizontal();
        EditorUI.ToolbarLabel(EditorVars.ToolTips.topologyLayerSelect);
        EditorGUI.BeginChangeCheck();
        LandData.topologyLayer = (TerrainTopology.Enum)EditorUI.ToolbarEnumPopup(LandData.topologyLayer);
        EditorUI.EndToolbarHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            LandData.ChangeLandLayer("topology", TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
        }
    }
    private void SlopeTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Slope Tools", EditorStyles.miniBoldLabel);

        if (index < 2)
        {
            EditorUI.ToolbarToggleMinMax(EditorVars.ToolTips.toggleBlend, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref blendSlopes, ref slopeLow, ref slopeHigh, 0f, 90f);
            if (blendSlopes)
            {
                EditorUI.ToolbarMinMax(EditorVars.ToolTips.blendLow, EditorVars.ToolTips.blendHigh, ref slopeBlendLow, ref slopeBlendHigh, 0f, 90f);

                EditorUI.BeginToolbarHorizontal();
                if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintSlopes))
                {
                    MapIO.PaintSlopeBlend(landLayers[index], slopeLow, slopeHigh, slopeBlendLow, slopeBlendHigh, texture);
                }
                EditorUI.EndToolbarHorizontal();
            }
            else
            {
                EditorUI.BeginToolbarHorizontal();
                if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintSlopes))
                {
                    MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, texture);
                }
                EditorUI.EndToolbarHorizontal();
            }
        }
        else
        {
            EditorUI.ToolbarMinMax(EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref slopeLow, ref slopeHigh, 0f, 90f);

            EditorUI.BeginToolbarHorizontal();
            if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintSlopes))
            {
                MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, texture, topology);
            }
            if (EditorUI.ToolbarButton(EditorVars.ToolTips.eraseSlopes))
            {
                MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, erase, topology);
            }
            EditorUI.EndToolbarHorizontal();
        }
    }
    private void HeightTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Height Tools", EditorStyles.miniBoldLabel);

        if (index < 2)
        {
            EditorUI.ToolbarToggleMinMax(EditorVars.ToolTips.toggleBlend, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref blendHeights, ref heightLow, ref heightHigh, 0f, 1000f);
            if (blendHeights)
            {
                EditorUI.ToolbarMinMax(EditorVars.ToolTips.blendLow, EditorVars.ToolTips.blendHigh, ref heightBlendLow, ref heightBlendHigh, 0f, 1000f);

                EditorUI.BeginToolbarHorizontal();
                if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintHeights))
                {
                    MapIO.PaintHeightBlend(landLayers[index], heightLow, heightHigh, heightBlendLow, heightBlendHigh, texture, topology);
                }
                EditorUI.EndToolbarHorizontal();
            }
            else
            {
                if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintHeights))
                {
                    MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, texture);
                }
            }
        }
        else
        {
            EditorUI.ToolbarMinMax(EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref heightLow, ref heightHigh, 0f, 1000f);

            EditorUI.BeginToolbarHorizontal();
            if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintHeights))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, texture, topology);
            }
            if (EditorUI.ToolbarButton(EditorVars.ToolTips.eraseHeights))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, erase, topology);
            }
            EditorUI.EndToolbarHorizontal();
        }
    }
    private void RotateTools(int index, int topology = 0)
    {
        EditorUI.BeginToolbarHorizontal();
        if (EditorUI.ToolbarButton(EditorVars.ToolTips.rotate90))
        {
            MapIO.RotateLayer(landLayers[index], true);
        }
        if (EditorUI.ToolbarButton(EditorVars.ToolTips.rotate270))
        {
            MapIO.RotateLayer(landLayers[index], false);
        }
        EditorUI.EndToolbarHorizontal();
    }
    private void TopologyTools()
    {
        EditorUI.BeginToolbarHorizontal();
        if (GUILayout.Button(new GUIContent("Rotate All 90°", "Rotate all Topology layers 90°"), EditorStyles.toolbarButton))
        {
            MapIO.RotateAllTopologyLayers(true);
        }
        if (GUILayout.Button(new GUIContent("Rotate All 270°", "Rotate all Topology layers 270°"), EditorStyles.toolbarButton))
        {
            MapIO.RotateAllTopologyLayers(false);
        }
        EditorUI.EndToolbarHorizontal();

        EditorUI.BeginToolbarHorizontal();
        if (GUILayout.Button(new GUIContent("Invert All", "Invert all Topology layers."), EditorStyles.toolbarButton))
        {
            MapIO.InvertAllTopologyLayers();
        }
        if (GUILayout.Button(new GUIContent("Clear All", "Clear all Topology layers."), EditorStyles.toolbarButton))
        {
            MapIO.ClearAllTopologyLayers();
        }
        EditorUI.EndToolbarHorizontal();
    }
    private void AreaTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Area Tools", EditorStyles.miniBoldLabel);

        EditorUI.ToolbarMinMaxInt(EditorVars.ToolTips.fromZ, EditorVars.ToolTips.toZ, ref z1, ref z2, 0f, MapIO.terrain.terrainData.alphamapResolution);
        EditorUI.ToolbarMinMaxInt(EditorVars.ToolTips.fromX, EditorVars.ToolTips.toX, ref x1, ref x2, 0f, MapIO.terrain.terrainData.alphamapResolution);

        if (index > 1) // Alpha and Topology
        {
            EditorUI.BeginToolbarHorizontal();
            if (GUILayout.Button("Paint Area", EditorStyles.toolbarButton))
            {
                MapIO.PaintArea(landLayers[index], (int)z1, (int)z2, (int)x1, (int)x2, texture, topology);
            }
            if (GUILayout.Button("Erase Area", EditorStyles.toolbarButton))
            {
                MapIO.PaintArea(landLayers[index], (int)z1, (int)z2, (int)x1, (int)x2, erase, topology);
            }
            EditorUI.EndToolbarHorizontal();
        }
        else
        {
            EditorUI.BeginToolbarHorizontal();
            if (GUILayout.Button("Paint Area", EditorStyles.toolbarButton))
            {
                MapIO.PaintArea(landLayers[index], (int)z1, (int)z2, (int)x1, (int)x2, texture);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    private void RiverTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("River Tools", EditorStyles.miniBoldLabel);

        if (index > 1)
        {
            EditorUI.BeginToolbarHorizontal();
            EditorUI.ToolbarToggle(EditorVars.ToolTips.aboveTerrain, ref aboveTerrain);

            if (EditorUI.ToolbarButton(EditorVars.ToolTips.paintRivers))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, texture, topology);
            }
            if (EditorUI.ToolbarButton(EditorVars.ToolTips.eraseRivers))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, erase, topology);
            }
            EditorUI.EndToolbarHorizontal();
        }
        else
        {
            EditorUI.BeginToolbarHorizontal();
            EditorUI.ToolbarToggle(EditorVars.ToolTips.aboveTerrain, ref aboveTerrain);

            if (GUILayout.Button("Paint Rivers", EditorStyles.toolbarButton))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, texture);
            }
            EditorUI.EndToolbarHorizontal();
        }
    }
    private void PaintTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Layer Tools", EditorStyles.miniBoldLabel);
        if (index > 1)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Paint Layer", EditorStyles.toolbarButton))
            {
                MapIO.PaintLayer(landLayers[index], texture, topology);
            }
            if (GUILayout.Button("Clear Layer", EditorStyles.toolbarButton))
            {
                MapIO.PaintLayer(landLayers[index], erase, topology);
            }
            if (GUILayout.Button("Invert Layer", EditorStyles.toolbarButton))
            {
                MapIO.InvertLayer(landLayers[index], topology);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Paint Layer", EditorStyles.toolbarButton))
            {
                MapIO.PaintLayer(landLayers[index], texture);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion
    #region MainMenu
    private void EditorIO()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Load", "Opens a file viewer to find and open a Rust .map file."), EditorStyles.toolbarButton))
        {
            loadFile = EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
            if (loadFile == "")
            {
                return;
            }
            var world = new WorldSerialization();
            MapIO.ProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.1f);
            world.Load(loadFile);
            MapIO.loadPath = loadFile;
            MapIO.ProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.2f);
            MapIO.Load(world);
        }
        if (GUILayout.Button(new GUIContent("Save", "Opens a file viewer to find and save a Rust .map file."), EditorStyles.toolbarButton))
        {
            saveFile = EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
            if (saveFile == "")
            {
                return;
            }
            Debug.Log("Exported map " + saveFile);
            MapIO.savePath = saveFile;
            prefabSaveFile = saveFile;
            MapIO.ProgressBar("Saving Map: " + saveFile, "Saving Heightmap ", 0.1f);
            MapIO.Save(saveFile);
        }
        if (GUILayout.Button(new GUIContent("New", "Creates a new map " + mapSize.ToString() + " metres in size."), EditorStyles.toolbarButton))
        {
            int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Close", "Save and Create New Map");
            switch (newMap)
            {
                case 0:
                    MapIO.loadPath = "New Map";
                    MapIO.CreateNewMap(mapSize);
                    break;
                case 1:
                    // User cancelled
                    break;
                case 2:
                    saveFile = EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
                    if (saveFile == "")
                    {
                        EditorUtility.DisplayDialog("Error", "Save Path is Empty", "Ok");
                        return;
                    }
                    Debug.Log("Exported map " + saveFile);
                    MapIO.Save(saveFile);
                    MapIO.loadPath = "New Map";
                    MapIO.CreateNewMap(mapSize);
                    break;
            }
        }
        GUILayout.Label(new GUIContent("Size", "The size of the Rust Map to create upon new map. (1000-6000)"), EditorStyles.toolbarButton);
        mapSize = Mathf.Clamp(EditorGUILayout.IntField(mapSize, EditorStyles.toolbarTextField, GUILayout.MaxWidth(60)), 1000, 6000);
        GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    private void MapInfo()
    {
        if (MapIO.terrain != null)
        {
            GUILayout.Label("Map Info", EditorStyles.boldLabel, GUILayout.MaxWidth(75));
            GUILayout.Label("Size: " + MapIO.terrain.terrainData.size.x);
            GUILayout.Label("HeightMap: " + MapIO.terrain.terrainData.heightmapResolution + "x" + MapIO.terrain.terrainData.heightmapResolution);
            GUILayout.Label("SplatMap: " + MapIO.terrain.terrainData.alphamapResolution + "x" + MapIO.terrain.terrainData.alphamapResolution);
        }
    }
    private void EditorInfo()
    {
        GUILayout.Label("Editor Info", EditorStyles.boldLabel, GUILayout.MaxWidth(75));
        GUILayout.Label("OS: " + SystemInfo.operatingSystem);
        GUILayout.Label("Unity Version: " + Application.unityVersion);
        GUILayout.Label("Editor Version: " + editorVersion);
    }
    private void EditorLinks()
    {
        GUILayout.Label(new GUIContent("Links"), EditorStyles.boldLabel, GUILayout.MaxWidth(60));

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Report Bug", "Opens up the editor bug report in GitHub."), EditorStyles.toolbarButton))
        {
            OpenReportBug();
        }
        if (GUILayout.Button(new GUIContent("Request Feature", "Opens up the editor feature request in GitHub."), EditorStyles.toolbarButton))
        {
            OpenRequestFeature();
        }
        if (GUILayout.Button(new GUIContent("RoadMap", "Opens up the editor roadmap in GitHub."), EditorStyles.toolbarButton))
        {
            OpenRoadMap();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Wiki", "Opens up the editor wiki in GitHub."), EditorStyles.toolbarButton))
        {
            OpenWiki();
        }
        if (GUILayout.Button(new GUIContent("Discord", "Discord invitation link."), EditorStyles.toolbarButton))
        {
            OpenDiscord();
        }
        EditorGUILayout.EndHorizontal();
    }
    private void EditorSettings()
    {
        GUILayout.Label(new GUIContent("Settings"), EditorStyles.boldLabel, GUILayout.MaxWidth(60));

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Save Changes", "Sets and saves the current settings."), EditorStyles.toolbarButton))
        {
            MapEditorSettings.SaveSettings();
        }
        if (GUILayout.Button(new GUIContent("Discard", "Discards the changes to the settings."), EditorStyles.toolbarButton))
        {
            MapEditorSettings.LoadSettings();
        }
        if (GUILayout.Button(new GUIContent("Default", "Sets the settings back to the default."), EditorStyles.toolbarButton))
        {
            MapEditorSettings.SetDefaultSettings();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label(new GUIContent("Rust Directory", @"The base install directory of Rust. Normally located at steamapps\common\Rust"), EditorStyles.miniBoldLabel, GUILayout.MaxWidth(95));

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Browse", "Browse and select the base directory of Rust."), EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
        {
            var returnDirectory = EditorUtility.OpenFolderPanel("Browse Rust Directory", MapEditorSettings.rustDirectory, "Rust");
            MapEditorSettings.rustDirectory = String.IsNullOrEmpty(returnDirectory) ? MapEditorSettings.rustDirectory : returnDirectory;
        }
        GUILayout.Label(new GUIContent(MapEditorSettings.rustDirectory, "The install directory of Rust on the local PC."), EditorStyles.toolbarButton);
        EditorGUILayout.EndHorizontal();

        GUILayout.Label(new GUIContent("Object Quality", "Controls the object render distance the exact same as ingame. Between 0-200"), EditorStyles.miniBoldLabel, GUILayout.MaxWidth(95));

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        MapEditorSettings.objectQuality = EditorGUILayout.IntSlider(MapEditorSettings.objectQuality, 0, 200);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    #endregion
    private void DrawEmpty()
    {
        GUILayout.Label("No presets in list.", EditorStyles.miniLabel);
    }
    #endregion
}