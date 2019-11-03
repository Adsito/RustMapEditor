using System;
using Rotorz.ReorderableList;
using UnityEngine;
using UnityEditor;

public static class EditorUIFunctions 
{
    #region Layers
    private static EditorVars.AlphaTextures alphaTextures;
    private static EditorVars.TopologyTextures topologyTextures;
    private static EditorVars.LandLayers landLayers;
    #endregion
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
            if (String.IsNullOrEmpty(loadFile))
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
            if (String.IsNullOrEmpty(saveFile))
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
                    if (String.IsNullOrEmpty(saveFile))
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
            EditorUIElements.BoldLabel(EditorVars.ToolTips.mapInfoLabel);
            GUILayout.Label("Size: " + MapIO.terrain.terrainData.size.x);
            GUILayout.Label("HeightMap: " + MapIO.terrain.terrainData.heightmapResolution + "x" + MapIO.terrain.terrainData.heightmapResolution);
            GUILayout.Label("SplatMap: " + MapIO.terrain.terrainData.alphamapResolution + "x" + MapIO.terrain.terrainData.alphamapResolution);
        }
    }
    public static void EditorInfo()
    {
        EditorUIElements.BoldLabel(EditorVars.ToolTips.editorInfoLabel);
        EditorUIElements.Label(EditorVars.ToolTips.systemOS);
        EditorUIElements.Label(EditorVars.ToolTips.systemRAM);
        EditorUIElements.Label(EditorVars.ToolTips.unityVersion);
        EditorUIElements.Label(EditorVars.ToolTips.editorVersion);
    }
    public static void EditorLinks()
    {
        EditorUIElements.BoldLabel(EditorVars.ToolTips.editorLinksLabel);

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
        EditorUIElements.BoldLabel(EditorVars.ToolTips.editorSettingsLabel);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.saveSettings))
        {
            MapEditorSettings.SaveSettings();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.discardSettings))
        {
            MapEditorSettings.LoadSettings();
            EditorVars.ToolTips.rustDirectoryPath.text = MapEditorSettings.rustDirectory;
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.defaultSettings))
        {
            MapEditorSettings.SetDefaultSettings();
            EditorVars.ToolTips.rustDirectoryPath.text = MapEditorSettings.rustDirectory;
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.rustDirectory);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.browseRustDirectory))
        {
            var returnDirectory = EditorUtility.OpenFolderPanel("Browse Rust Directory", MapEditorSettings.rustDirectory, "Rust");
            MapEditorSettings.rustDirectory = String.IsNullOrEmpty(returnDirectory) ? MapEditorSettings.rustDirectory : returnDirectory;
            EditorVars.ToolTips.rustDirectoryPath.text = MapEditorSettings.rustDirectory;
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
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.toolsLabel);

        EditorUIElements.BeginToolbarHorizontal();
        deleteOnExport = EditorUIElements.ToolbarToggle(EditorVars.ToolTips.deleteOnExport, deleteOnExport);
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
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.assetBundleLabel);

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
        EditorUIElements.BoldLabel(EditorVars.ToolTips.presetsLabel);

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
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.terraceLabel);

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
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.smoothLabel);

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
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.normaliseLabel);

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
    public static void SetHeight(ref float height)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.setHeightLabel);
        
        height = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.heightToSet, height, 0f, 1000f);
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.setLandHeight))
        {
            MapIO.SetHeightmap(height, EditorVars.Selections.Terrains.Land);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.setWaterHeight))
        {
            MapIO.SetHeightmap(height, EditorVars.Selections.Terrains.Water);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void MinMaxHeight(ref float height)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.setHeightLabel);

        height = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.heightToSet, height, 0f, 1000f);
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.setMinHeight))
        {
            MapIO.SetMinimumHeight(height);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.setMaxHeight))
        {
            MapIO.SetMaximumHeight(height);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void OffsetMap(ref float offset, ref bool clampOffset)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.setHeightLabel);

        offset = EditorUIElements.ToolbarSlider(EditorVars.ToolTips.offsetHeight, offset, 0f, 1000f);
        EditorUIElements.BeginToolbarHorizontal();
        clampOffset = EditorUIElements.ToolbarToggle(EditorVars.ToolTips.clampOffset, clampOffset);
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.offsetLand))
        {
            MapIO.OffsetHeightmap(offset, clampOffset, EditorVars.Selections.Terrains.Land);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.offsetWater))
        {
            MapIO.OffsetHeightmap(offset, clampOffset, EditorVars.Selections.Terrains.Water);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void InvertMap()
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.setHeightLabel);

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.invertLand))
        {
            MapIO.InvertHeightmap(EditorVars.Selections.Terrains.Land);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.invertWater))
        {
            MapIO.InvertHeightmap(EditorVars.Selections.Terrains.Water);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    #endregion
    public static void ConditionalPaint(ref int cndsOptions, ref int paintOptions, ref int texture, ref Conditions cnds, ref EditorVars.Layers layers)
    {
        EditorUIElements.BoldLabel(EditorVars.ToolTips.conditionalPaintLabel);

        GUIContent[] conditionalPaintMenu = new GUIContent[5];
        conditionalPaintMenu[0] = new GUIContent("Ground");
        conditionalPaintMenu[1] = new GUIContent("Biome");
        conditionalPaintMenu[2] = new GUIContent("Alpha");
        conditionalPaintMenu[3] = new GUIContent("Topology");
        conditionalPaintMenu[4] = new GUIContent("Terrain");
        cndsOptions = GUILayout.Toolbar(cndsOptions, conditionalPaintMenu, EditorStyles.toolbarButton);

        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.conditionsLabel);

        switch (cndsOptions)
        {
            case 0: // Ground
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureCheck);
                cnds.GroundConditions = (TerrainSplat.Enum)EditorUIElements.ToolbarEnumFlagsField(cnds.GroundConditions);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 1: // Biome
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureCheck);
                cnds.BiomeConditions = (TerrainBiome.Enum)EditorUIElements.ToolbarEnumFlagsField(cnds.BiomeConditions);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 2: // Alpha
                EditorUIElements.BeginToolbarHorizontal();
                cnds.CheckAlpha = EditorUIElements.ToolbarToggle(EditorVars.ToolTips.checkAlpha, cnds.CheckAlpha);
                cnds.AlphaTexture = (int)(EditorVars.AlphaTextures)EditorUIElements.ToolbarEnumPopup(alphaTextures);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 3: // Topology
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.topologyLayerSelect);
                cnds.TopologyLayers = (TerrainTopology.Enum)EditorUIElements.ToolbarEnumFlagsField(cnds.TopologyLayers);
                EditorUIElements.EndToolbarHorizontal();

                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureCheck);
                cnds.TopologyTexture = (int)(EditorVars.TopologyTextures)EditorUIElements.ToolbarEnumPopup(topologyTextures);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 4: // Terrain
                float tempSlopeLow = cnds.SlopeLow, tempSlopeHigh = cnds.SlopeHigh;
                cnds.CheckSlope = EditorUIElements.ToolbarToggleMinMax(EditorVars.ToolTips.checkSlopes, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, cnds.CheckSlope, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                cnds.SlopeLow = tempSlopeLow; cnds.SlopeHigh = tempSlopeHigh;

                float tempHeightLow = cnds.HeightLow, tempHeightHigh = cnds.HeightHigh;
                cnds.CheckHeight = EditorUIElements.ToolbarToggleMinMax(EditorVars.ToolTips.checkHeights, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, cnds.CheckHeight, ref tempHeightLow, ref tempHeightHigh, 0f, 1000f);
                cnds.HeightLow = tempHeightLow; cnds.HeightHigh = tempHeightHigh;
                break;
        }
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.textureToPaintLabel);

        EditorUIElements.BeginToolbarHorizontal();
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.layerSelect);
        paintOptions = (int)(EditorVars.LandLayers)EditorUIElements.ToolbarEnumPopup(landLayers);
        EditorUIElements.EndToolbarHorizontal();

        switch (paintOptions)
        {
            case 0:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureSelect);
                texture = (int)(TerrainSplat.Enum)EditorUIElements.ToolbarEnumPopup(layers.Ground);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 1:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureSelect);
                texture = (int)(TerrainBiome.Enum)EditorUIElements.ToolbarEnumPopup(layers.Biome);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 2:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureSelect);
                texture = (int)(EditorVars.AlphaTextures)EditorUIElements.ToolbarEnumPopup(layers.AlphaTexture);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case 3:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.topologyLayerSelect);
                layers.Topologies = (TerrainTopology.Enum)EditorUIElements.ToolbarEnumPopup(layers.Topologies);
                EditorUIElements.EndToolbarHorizontal();

                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureSelect);
                texture = (int)(EditorVars.TopologyTextures)EditorUIElements.ToolbarEnumPopup(layers.TopologyTexture);
                EditorUIElements.EndToolbarHorizontal();
                break;
        }

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintConditional))
        {
            MapIO.PaintConditional((EditorVars.LandLayers)paintOptions, texture, cnds, TerrainTopology.TypeToIndex((int)layers.Topologies));
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void RotateMap(EditorVars.Selections.Objects selection)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.rotateMapLabel);

        EditorUIElements.BeginToolbarHorizontal();
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.rotateSelection);
        selection = (EditorVars.Selections.Objects)EditorUIElements.ToolbarEnumFlagsField(selection);
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotate90))
        {
            MapIO.RotateMap(selection, true);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotate270))
        {
            MapIO.RotateMap(selection, false);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    #endregion
    #region LayerTools
    public static void TextureSelect(EditorVars.LandLayers landLayer, ref EditorVars.Layers layers)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.textureSelectLabel);

        switch (landLayer)
        {
            case EditorVars.LandLayers.Ground:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureSelect);
                layers.Ground = (TerrainSplat.Enum)EditorUIElements.ToolbarEnumPopup(layers.Ground);
                EditorUIElements.EndToolbarHorizontal();
                break;
            case EditorVars.LandLayers.Biome:
                EditorUIElements.BeginToolbarHorizontal();
                EditorUIElements.ToolbarLabel(EditorVars.ToolTips.textureSelect);
                layers.Biome = (TerrainBiome.Enum)EditorUIElements.ToolbarEnumPopup(layers.Biome);
                EditorUIElements.EndToolbarHorizontal();
                break;
        }
    }
    public static void TopologyLayerSelect(ref EditorVars.Layers layers)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.layerSelect);

        EditorUIElements.BeginToolbarHorizontal();
        EditorUIElements.ToolbarLabel(EditorVars.ToolTips.topologyLayerSelect);
        EditorGUI.BeginChangeCheck();
        layers.Topologies = (TerrainTopology.Enum)EditorUIElements.ToolbarEnumPopup(layers.Topologies);
        EditorUIElements.EndToolbarHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            LandData.ChangeLandLayer(EditorVars.LandLayers.Topology, TerrainTopology.TypeToIndex((int)layers.Topologies));
        }
    }
    /*
    public static void SlopeTools(EditorVars.LandLayers layer, int texture, bool blendSlopes, int erase = 0, int topology = 0)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.slopeToolsLabel);

        if ((int)layer < 2)
        {
            EditorUIElements.ToolbarToggleMinMax(EditorVars.ToolTips.toggleBlend, EditorVars.ToolTips.rangeLow, EditorVars.ToolTips.rangeHigh, ref blendSlopes, ref slopeLow, ref slopeHigh, 0f, 90f);
            if (blendSlopes)
            {
                EditorUIElements.ToolbarMinMax(EditorVars.ToolTips.blendLow, EditorVars.ToolTips.blendHigh, ref slopeBlendLow, ref slopeBlendHigh, 0f, 90f);

                EditorUIElements.BeginToolbarHorizontal();
                if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintSlopes))
                {
                    MapIO.PaintSlopeBlend(layer, slopeLow, slopeHigh, slopeBlendLow, slopeBlendHigh, texture);
                }
                EditorUIElements.EndToolbarHorizontal();
            }
            else
            {
                EditorUIElements.BeginToolbarHorizontal();
                if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintSlopes))
                {
                    MapIO.PaintSlope(layer, slopeLow, slopeHigh, texture);
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
    */
    public static void TopologyTools()
    {
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotateAll90))
        {
            MapIO.RotateAllTopologyLayers(true);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotateAll270))
        {
            MapIO.RotateAllTopologyLayers(false);
        }
        EditorUIElements.EndToolbarHorizontal();

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.invertAll))
        {
            MapIO.InvertAllTopologyLayers();
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.clearAll))
        {
            MapIO.ClearAllTopologyLayers();
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void AreaTools(EditorVars.LandLayers landLayer, int texture, ArrayOperations.Dimensions dmns, int erase = 0, int topology = 0)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.areaToolsLabel);

        float tmpz0 = dmns.z0; float tmpz1 = dmns.z1; float tmpx0 = dmns.x0; float tmpx1 = dmns.x1;
        EditorUIElements.ToolbarMinMaxInt(EditorVars.ToolTips.fromZ, EditorVars.ToolTips.toZ, ref tmpz0, ref tmpz1, 0f, MapIO.terrain.terrainData.alphamapResolution);
        EditorUIElements.ToolbarMinMaxInt(EditorVars.ToolTips.fromX, EditorVars.ToolTips.toX, ref tmpx0, ref tmpx1, 0f, MapIO.terrain.terrainData.alphamapResolution);
        dmns.z0 = (int)tmpz0; dmns.z1 = (int)tmpz1; dmns.x0 = (int)tmpx0; dmns.x1 = (int)tmpx1;

        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintArea))
        {
            MapIO.PaintArea(landLayer, dmns, texture, topology);
        }
        if ((int)landLayer > 1)
        {
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.eraseArea))
            {
                MapIO.PaintArea(landLayer, dmns, erase, topology);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    public static void RiverTools(EditorVars.LandLayers landLayer, int texture, ref bool aboveTerrain, int erase = 0, int topology = 0)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.riverToolsLabel);

        if ((int)landLayer > 1)
        {
            EditorUIElements.BeginToolbarHorizontal();
            aboveTerrain = EditorUIElements.ToolbarToggle(EditorVars.ToolTips.aboveTerrain, aboveTerrain);
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintRivers))
            {
                MapIO.PaintRiver(landLayers, aboveTerrain, texture, topology);
            }
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.eraseRivers))
            {
                MapIO.PaintRiver(landLayers, aboveTerrain, erase, topology);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
        else
        {
            EditorUIElements.BeginToolbarHorizontal();
            EditorUIElements.ToolbarToggle(EditorVars.ToolTips.aboveTerrain, aboveTerrain);
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintRivers))
            {
                MapIO.PaintRiver(landLayers, aboveTerrain, texture);
            }
            EditorUIElements.EndToolbarHorizontal();
        }
    }
    public static void RotateTools(EditorVars.LandLayers landLayer, int topology = 0)
    {
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotate90))
        {
            MapIO.RotateLayer(landLayer, true, topology);
        }
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.rotate270))
        {
            MapIO.RotateLayer(landLayer, false, topology);
        }
        EditorUIElements.EndToolbarHorizontal();
    }
    public static void LayerTools(EditorVars.LandLayers landLayer, int texture, int erase = 0, int topology = 0)
    {
        EditorUIElements.MiniBoldLabel(EditorVars.ToolTips.layerToolsLabel);
        
        EditorUIElements.BeginToolbarHorizontal();
        if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.paintLayer))
        {
            MapIO.PaintLayer(landLayer, texture, topology);
        }
        if ((int)landLayer > 1)
        {
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.clearLayer))
            {
                MapIO.PaintLayer(landLayer, erase, topology);
            }
            if (EditorUIElements.ToolbarButton(EditorVars.ToolTips.invertLayer))
            {
                MapIO.InvertLayer(landLayer, topology);
            }
        }
        EditorUIElements.EndToolbarHorizontal();
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
        LandData.ChangeLandLayer((EditorVars.LandLayers)landIndex, topology);
    }
    /// <summary>
    /// Sets the active landLayer to the index.
    /// </summary>
    /// <param name="landIndex">The landLayer to change to.</param>
    /// <param name="topology">The Topology layer to set.</param>
    public static void SetLandLayer(EditorVars.LandLayers landIndex, int topology = 0)
    {
        LandData.ChangeLandLayer(landIndex, topology);
    }
    #endregion
}