using System;
using Rotorz.ReorderableList;
using UnityEngine;
using UnityEditor;

public static class EditorUIFunctions 
{
    #region Menu Items
    [MenuItem("Rust Map Editor/Main Menu", false, 0)]
    public static void OpenMainMenu()
    {
        MapIOEditor window = (MapIOEditor)EditorWindow.GetWindow(typeof(MapIOEditor), false, "Rust Map Editor");
    }
    [MenuItem("Rust Map Editor/Terrain Tools", false, 1)]
    public static void OpenTerrainTools()
    {
        Selection.activeGameObject = MapIO.terrain.gameObject;
    }
    [MenuItem("Rust Map Editor/Links/Wiki", false, 2)]
    public static void OpenWiki()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/wiki");
    }
    [MenuItem("Rust Map Editor/Links/Discord", false, 3)]
    public static void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/HPmTWVa");
    }
    [MenuItem("Rust Map Editor/Links/RoadMap", false, 3)]
    public static void OpenRoadMap()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/projects/1");
    }
    [MenuItem("Rust Map Editor/Links/Report Bug", false, 4)]
    public static void OpenReportBug()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
    }
    [MenuItem("Rust Map Editor/Links/Request Feature", false, 5)]
    public static void OpenRequestFeature()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
    }
    #endregion
    #region MainMenu
    public static void EditorIO(ref string loadFile, ref string saveFile, ref int mapSize, string mapName = "custommap")
    {
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.loadMap))
        {
            loadFile = EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
            if (loadFile == "")
            {
                return;
            }
            var world = new WorldSerialization();
            MapIO.ProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.1f);
            world.Load(loadFile);
            MapIO.ProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.2f);
            MapIO.Load(world);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.saveMap))
        {
            saveFile = EditorUtility.SaveFilePanel("Save Map File", saveFile, mapName, "map");
            if (saveFile == "")
            {
                return;
            }
            Debug.Log("Saved map " + saveFile);
            MapIO.ProgressBar("Saving Map: " + saveFile, "Saving Heightmap ", 0.1f);
            MapIO.Save(saveFile);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.newMap))
        {
            int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Close", "Save and Create New Map");
            switch (newMap)
            {
                case 0:
                    MapIO.CreateNewMap(mapSize);
                    break;
                case 2:
                    saveFile = EditorUtility.SaveFilePanel("Save Map File", saveFile, mapName, "map");
                    if (saveFile == "")
                    {
                        EditorUtility.DisplayDialog("Error", "Save Path is Empty", "Ok");
                        return;
                    }
                    Debug.Log("Saved map " + saveFile);
                    MapIO.Save(saveFile);
                    MapIO.CreateNewMap(mapSize);
                    break;
            }
        }
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.mapSize);
        mapSize = Mathf.Clamp(EditorUIElements.ToolbarDelayedIntField(mapSize), 1000, 6000);
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void MapInfo()
    {
        if (MapIO.terrain != null)
        {
            EditorUIElements.BoldLabel(EditorVars.ToolTips.mapInfo);
            GUILayout.Label("Size: " + MapIO.terrain.terrainData.size.x);
            GUILayout.Label("HeightMap: " + MapIO.terrain.terrainData.heightmapResolution + "x" + MapIO.terrain.terrainData.heightmapResolution);
            GUILayout.Label("SplatMap: " + MapIO.terrain.terrainData.alphamapResolution + "x" + MapIO.terrain.terrainData.alphamapResolution);
        }
    }
    public static void EditorInfo()
    {
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.editorInfo);
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.systemOS);
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.systemRAM);
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.unityVersion);
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.editorVersion);
    }
    public static void EditorLinks()
    {
        EditorUIElements.BoldLabel(EditorVars.ToolTips.editorLinks);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.reportBug))
        {
            OpenReportBug();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.requestFeature))
        {
            OpenRequestFeature();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.roadMap))
        {
            OpenRoadMap();
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.wiki))
        {
            OpenWiki();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.discord))
        {
            OpenDiscord();
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void EditorSettings()
    {
        EditorUIElements.BoldLabel(EditorVars.ToolTips.editorSettings);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.saveSettings))
        {
            MapEditorSettings.SaveSettings();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.discardSettings))
        {
            MapEditorSettings.LoadSettings();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.defaultSettings))
        {
            MapEditorSettings.SetDefaultSettings();
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.rustDirectory);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.browseRustDirectory))
        {
            var returnDirectory = EditorUtility.OpenFolderPanel("Browse Rust Directory", MapEditorSettings.rustDirectory, "Rust");
            MapEditorSettings.rustDirectory = String.IsNullOrEmpty(returnDirectory) ? MapEditorSettings.rustDirectory : returnDirectory;
        }
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.rustDirectoryPath);
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.objectQuality);

        EditorUIElements.BeginToolbarHorizontal();
        MapEditorSettings.objectQuality = EditorUIElements.ToolbarIntSlider(EditorVars.ToolTips.objectQuality, MapEditorSettings.objectQuality, 0, 200);
        EditorUIElements.EndToolbarHorizontal();
    }
    #endregion
    #region Prefabs
    public static void PrefabTools(ref bool deleteOnExport, string lootCrateSaveFile = "", string mapPrefabSaveFile = "")
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.tools);

        deleteOnExport = EditorUIElements.ToolbarToggle(EditorVars.ToolTips.deleteOnExport, deleteOnExport);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.exportMapLootCrates))
        {
            lootCrateSaveFile = EditorUtility.SaveFilePanel("Export LootCrates", lootCrateSaveFile, "LootCrateData", "json");
            if (!String.IsNullOrEmpty(lootCrateSaveFile))
            {
                MapIO.ExportLootCrates(lootCrateSaveFile, deleteOnExport);
            }
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.exportMapPrefabs))
        {
            mapPrefabSaveFile = EditorUtility.SaveFilePanel("Export Map Prefabs", mapPrefabSaveFile, "MapData", "json");
            if (!String.IsNullOrEmpty(mapPrefabSaveFile))
            {
                MapIO.ExportMapPrefabs(mapPrefabSaveFile, deleteOnExport);
            }
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.hidePrefabsInRustEdit))
        {
            MapIO.HidePrefabsInRustEdit();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.breakRustEditPrefabs))
        {
            MapIO.BreakRustEditCustomPrefabs();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.groupRustEditPrefabs))
        {
            MapIO.GroupRustEditCustomPrefabs();
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.deleteMapPrefabs))
        {
            MapIO.RemoveMapObjects(true, false);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.deleteMapPaths))
        {
            MapIO.RemoveMapObjects(false, true);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void AssetBundle()
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.assetBundle);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.loadBundle))
        {
            PrefabManager.LoadBundle(MapEditorSettings.rustDirectory + MapEditorSettings.bundlePathExt);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.unloadBundle))
        {
            PrefabManager.DisposeBundle();
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    #endregion
    #region Generation Tools
    public static void NodePresets(Vector2 presetScrollPos)
    {
        EditorUIElements.BoldLabel(EditorVars.ToolTips.presets);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.refreshPresets))
        {
            MapIO.RefreshPresetsList();
        }
        EditorUIElements.EndToolbarHorizontal();

        presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
        ReorderableListGUI.Title("Node Presets");
        ReorderableListGUI.ListField(MapIO.generationPresetList, NodePresetDrawer, DrawEmpty);
        GUILayout.EndScrollView();
    }
    public static void DrawEmpty()
    {
        EditorUIElements.MiniLabel(EditorVars.ToolTips.noPresets);
    }
    #endregion
    #region MapTools
    #region HeightMap
    public static void TerraceMap(ref float terraceErodeFeatureSize, ref float terraceErodeInteriorCornerWeight)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.terrace);

        terraceErodeFeatureSize = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.featureSize, terraceErodeFeatureSize, 2f, 1000f);
        terraceErodeInteriorCornerWeight = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.cornerWeight, terraceErodeInteriorCornerWeight, 0f, 1f);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.terraceMap))
        {
            MapIO.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void SmoothMap(ref float filterStrength, ref float blurDirection, ref int smoothPasses)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.smooth);

        filterStrength = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.smoothStrength, filterStrength, 0f, 1f);
        blurDirection = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.blurDirection, blurDirection, -1f, 1f);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.smoothMap))
        {
            for (int i = 0; i < smoothPasses; i++)
            {
                MapIO.SmoothHeightmap(filterStrength, blurDirection);
            }
        }
        smoothPasses = EditorGUILayout.IntSlider(smoothPasses, 1, 1000);
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void NormaliseMap(ref float normaliseLow, ref float normaliseHigh, ref bool autoUpdate)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.normalise);

        EditorGUI.BeginChangeCheck();
        normaliseLow = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.normaliseLow, normaliseLow, 0f, normaliseHigh);
        normaliseHigh = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.normaliseHigh, normaliseHigh, normaliseLow, 1000f);
        if (EditorGUI.EndChangeCheck() && autoUpdate == true)
        {
            MapIO.NormaliseHeightmap(normaliseLow, normaliseHigh);
        }

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.normaliseMap))
        {
            MapIO.NormaliseHeightmap(normaliseLow, normaliseHigh);
        }
        autoUpdate = EditorUIElements.ToolbarToggle(EditorVars.ToolTips.autoUpdateNormalise, autoUpdate);
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void EdgeHeight() // Use area tools to manipulate area of heightmap.
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
    public static void SetHeight()
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
    public static void MinMaxHeight()
    {
        GUILayout.Label("Heightmap Minimum/Maximum Height", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
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
    public static void OffsetMap()
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
    public static void InvertMap()
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
    public static void ConditionalPaint()
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
    public static void CopyTextures()
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
    public static void RotateMap()
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
    #endregion
    #endregion
    #region LayerTools
    public static void TextureSelect(int index)
    {
        GUILayout.Label("Texture Select", EditorStyles.miniBoldLabel);

        switch (index)
        {
            case 0:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.groundTextureSelect);
                MapIO.groundLayer = (TerrainSplat.Enum)EditorUIElements.ToolbarEnumPopup(MapIO.groundLayer);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 1:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.biomeTextureSelect);
                MapIO.biomeLayer = (TerrainBiome.Enum)EditorUIElements.ToolbarEnumPopup(MapIO.biomeLayer);
                EditorUIElements.EndToolbarHorizontal();
                break;
        }
    }
    public static void TopologyLayerSelect()
    {
        GUILayout.Label("Layer Select", EditorStyles.miniBoldLabel);

        EditorUIElements.BeginToolbarHorizontal();
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.topologyLayerSelect);
        EditorGUI.BeginChangeCheck();
        LandData.topologyLayer = (TerrainTopology.Enum)EditorUIElements.ToolbarEnumPopup(LandData.topologyLayer);
        EditorUIElements.EndToolbarHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            LandData.ChangeLandLayer("topology", TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
        }
    }
    public static void SlopeTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Slope Tools", EditorStyles.miniBoldLabel);

        if (index < 2)
        {
            EditorUIElements.ToolbarToggleMinMax(EditorVars.ToolTips.toggleBlend, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref blendSlopes, ref slopeLow, ref slopeHigh, 0f, 90f);
            if (blendSlopes)
            {
                EditorUIElements.ToolbarMinMax(EditorVars.ToolTips.blendLow, EditorVars.ToolTips.blendHigh, ref slopeBlendLow, ref slopeBlendHigh, 0f, 90f);

                EditorUIElements.BeginToolbarHorizontal();
                if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintSlopes))
                {
                    MapIO.PaintSlopeBlend(landLayers[index], slopeLow, slopeHigh, slopeBlendLow, slopeBlendHigh, texture);
                }
                EditorUIElements.EndToolbarHorizontal();
            }
            else
            {
                EditorUIElements.BeginToolbarHorizontal();
                if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintSlopes))
                {
                    MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, texture);
                }
                EditorUIElements.EndToolbarHorizontal();
            }
        }
        else
        {
            EditorUIElements.ToolbarMinMax(EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref slopeLow, ref slopeHigh, 0f, 90f);

            EditorUIElements.BeginToolbarHorizontal();
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintSlopes))
            {
                MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, texture, topology);
            }
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.eraseSlopes))
            {
                MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, erase, topology);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
    }
    public static void HeightTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Height Tools", EditorStyles.miniBoldLabel);

        if (index < 2)
        {
            EditorUIElements.ToolbarToggleMinMax(EditorVars.ToolTips.toggleBlend, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref blendHeights, ref heightLow, ref heightHigh, 0f, 1000f);
            if (blendHeights)
            {
                EditorUIElements.ToolbarMinMax(EditorVars.ToolTips.blendLow, EditorVars.ToolTips.blendHigh, ref heightBlendLow, ref heightBlendHigh, 0f, 1000f);

                EditorUIElements.BeginToolbarHorizontal();
                if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintHeights))
                {
                    MapIO.PaintHeightBlend(landLayers[index], heightLow, heightHigh, heightBlendLow, heightBlendHigh, texture, topology);
                }
                EditorUIElements.EndToolbarHorizontal();
            }
            else
            {
                if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintHeights))
                {
                    MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, texture);
                }
            }
        }
        else
        {
            EditorUIElements.ToolbarMinMax(EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref heightLow, ref heightHigh, 0f, 1000f);

            EditorUIElements.BeginToolbarHorizontal();
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintHeights))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, texture, topology);
            }
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.eraseHeights))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, erase, topology);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
    }
    public static void RotateTools(int index, int topology = 0)
    {
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotate90))
        {
            MapIO.RotateLayer(landLayers[index], true);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotate270))
        {
            MapIO.RotateLayer(landLayers[index], false);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void TopologyTools()
    {
        EditorUIElements.BeginToolbarHorizontal();
        if (GUILayout.Button(new GUIContent("Rotate All 90°", "Rotate all Topology layers 90°"), EditorStyles.toolbarButton))
        {
            MapIO.RotateAllTopologyLayers(true);
        }
        if (GUILayout.Button(new GUIContent("Rotate All 270°", "Rotate all Topology layers 270°"), EditorStyles.toolbarButton))
        {
            MapIO.RotateAllTopologyLayers(false);
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.BeginToolbarHorizontal();
        if (GUILayout.Button(new GUIContent("Invert All", "Invert all Topology layers."), EditorStyles.toolbarButton))
        {
            MapIO.InvertAllTopologyLayers();
        }
        if (GUILayout.Button(new GUIContent("Clear All", "Clear all Topology layers."), EditorStyles.toolbarButton))
        {
            MapIO.ClearAllTopologyLayers();
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void AreaTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Area Tools", EditorStyles.miniBoldLabel);

        EditorUIElements.ToolbarMinMaxInt(EditorVars.ToolTips.fromZ, EditorVars.ToolTips.toZ, ref z1, ref z2, 0f, MapIO.terrain.terrainData.alphamapResolution);
        EditorUIElements.ToolbarMinMaxInt(EditorVars.ToolTips.fromX, EditorVars.ToolTips.toX, ref x1, ref x2, 0f, MapIO.terrain.terrainData.alphamapResolution);

        if (index > 1) // Alpha and Topology
        {
            EditorUIElements.BeginToolbarHorizontal();
            if (GUILayout.Button("Paint Area", EditorStyles.toolbarButton))
            {
                MapIO.PaintArea(landLayers[index], (int)z1, (int)z2, (int)x1, (int)x2, texture, topology);
            }
            if (GUILayout.Button("Erase Area", EditorStyles.toolbarButton))
            {
                MapIO.PaintArea(landLayers[index], (int)z1, (int)z2, (int)x1, (int)x2, erase, topology);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
        else
        {
            EditorUIElements.BeginToolbarHorizontal();
            if (GUILayout.Button("Paint Area", EditorStyles.toolbarButton))
            {
                MapIO.PaintArea(landLayers[index], (int)z1, (int)z2, (int)x1, (int)x2, texture);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    public static void RiverTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("River Tools", EditorStyles.miniBoldLabel);

        if (index > 1)
        {
            EditorUIElements.BeginToolbarHorizontal();
            EditorUIElements.ToolbarToggle(EditorVars.ToolTips.aboveTerrain, ref aboveTerrain);

            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintRivers))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, texture, topology);
            }
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.eraseRivers))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, erase, topology);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
        else
        {
            EditorUIElements.BeginToolbarHorizontal();
            EditorUIElements.ToolbarToggle(EditorVars.ToolTips.aboveTerrain, ref aboveTerrain);

            if (GUILayout.Button("Paint Rivers", EditorStyles.toolbarButton))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, texture);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
    }
    public static void PaintTools(int index, int texture, int erase = 0, int topology = 0)
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
    #region PrefabData
    public static void PrefabCategory(PrefabDataHolder target)
    {
        EditorUIElements.BeginToolbarHorizontal();
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.prefabCategory);
        target.prefabData.category = EditorUIElements.ToolbarTextField(target.prefabData.category);
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void PrefabID(PrefabDataHolder target)
    {
        EditorUIElements.BeginToolbarHorizontal();
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.prefabID);
        target.prefabData.id = (uint)EditorUIElements.ToolbarIntField((int)target.prefabData.id);
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void SnapToGround(PrefabDataHolder target)
    {
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.snapToGround))
        {
            target.SnapToGround();
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    #endregion
    #region Functions
    public static string NodePresetDrawer(Rect position, string itemValue)
    {
        position.width -= 39;
        GUI.Label(position, itemValue);
        position.x = position.xMax;
        position.width = 39;
        if (GUI.Button(position, EditorVars.ToolTips.openPreset, EditorStyles.toolbarButton))
        {
            MapIO.RefreshPresetsList();
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
    public static void SetLandLayer(int landIndex, int topology = 0)
    {
        LandData.ChangeLandLayer(((EditorEnums.Layers.LandLayers)landIndex).ToString(), topology);
    }
    #endregion
}