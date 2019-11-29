using System;
using Rotorz.ReorderableList;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;
using static RustMapEditor.Data.LandData;

namespace RustMapEditor.UI
{
    public static class Functions
    {
        #region Menu Items
        [MenuItem("Rust Map Editor/Main Menu", false, 0)]
        public static void OpenMainMenu()
        {
            MapIOEditor window = (MapIOEditor)EditorWindow.GetWindow(typeof(MapIOEditor), false, "Rust Map Editor");
        }
        [MenuItem("Rust Map Editor/Hierachy/Prefabs", false, 1)]
        static void OpenPrefabHierachy()
        {
            PrefabHierachyWindow window = (PrefabHierachyWindow)EditorWindow.GetWindow(typeof(PrefabHierachyWindow), false, "Prefab Hierachy");
        }
        [MenuItem("Rust Map Editor/Hierachy/Paths", false, 1)]
        static void OpenPathHierachy()
        {
            PathHierachyWindow window = (PathHierachyWindow)EditorWindow.GetWindow(typeof(PathHierachyWindow), false, "Path Hierachy");
        }
        [MenuItem("Rust Map Editor/Terrain Tools", false, 2)]
        public static void OpenTerrainTools()
        {
            Selection.activeGameObject = land.gameObject;
        }
        [MenuItem("Rust Map Editor/Links/Wiki", false, 10)]
        public static void OpenWiki()
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/wiki");
        }
        [MenuItem("Rust Map Editor/Links/Discord", false, 10)]
        public static void OpenDiscord()
        {
            Application.OpenURL("https://discord.gg/HPmTWVa");
        }
        [MenuItem("Rust Map Editor/Links/RoadMap", false, 10)]
        public static void OpenRoadMap()
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/projects/1");
        }
        [MenuItem("Rust Map Editor/Links/Report Bug", false, 10)]
        public static void OpenReportBug()
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
        }
        [MenuItem("Rust Map Editor/Links/Request Feature", false, 10)]
        public static void OpenRequestFeature()
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
        }
        #endregion
        #region MainMenu
        public static void EditorIO(ref string loadFile, ref string saveFile, ref int mapSize, string mapName = "custommap")
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.loadMap))
            {
                loadFile = EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
                if (String.IsNullOrEmpty(loadFile))
                {
                    return;
                }
                var world = new WorldSerialization();
                world.Load(loadFile);
                MapIO.Load(world);
            }
            if (Elements.ToolbarButton(ToolTips.saveMap))
            {
                saveFile = EditorUtility.SaveFilePanel("Save Map File", saveFile, mapName, "map");
                if (String.IsNullOrEmpty(saveFile))
                {
                    return;
                }
                MapIO.ProgressBar("Saving Map: " + saveFile, "Saving Heightmap ", 0.1f);
                MapIO.Save(saveFile);
            }
            if (Elements.ToolbarButton(ToolTips.newMap))
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
                        MapIO.Save(saveFile);
                        MapIO.CreateNewMap(mapSize);
                        break;
                }
            }
            Elements.ToolbarLabel(ToolTips.mapSize);
            mapSize = Mathf.Clamp(Elements.ToolbarIntField(mapSize), 1000, 6000);
            Elements.EndToolbarHorizontal();
        }
        public static void MapInfo()
        {
            if (land != null)
            {
                Elements.BoldLabel(ToolTips.mapInfoLabel);
                GUILayout.Label("Size: " + land.terrainData.size.x);
                GUILayout.Label("HeightMap: " + land.terrainData.heightmapResolution + "x" + land.terrainData.heightmapResolution);
                GUILayout.Label("SplatMap: " + land.terrainData.alphamapResolution + "x" + land.terrainData.alphamapResolution);
            }
        }
        public static void EditorInfo()
        {
            Elements.BoldLabel(ToolTips.editorInfoLabel);
            Elements.Label(ToolTips.systemOS);
            Elements.Label(ToolTips.systemRAM);
            Elements.Label(ToolTips.unityVersion);
            Elements.Label(ToolTips.editorVersion);
        }
        public static void EditorLinks()
        {
            Elements.BoldLabel(ToolTips.editorLinksLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.reportBug))
            {
                OpenReportBug();
            }
            if (Elements.ToolbarButton(ToolTips.requestFeature))
            {
                OpenRequestFeature();
            }
            if (Elements.ToolbarButton(ToolTips.roadMap))
            {
                OpenRoadMap();
            }
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.wiki))
            {
                OpenWiki();
            }
            if (Elements.ToolbarButton(ToolTips.discord))
            {
                OpenDiscord();
            }
            Elements.EndToolbarHorizontal();
        }
        public static void EditorSettings()
        {
            Elements.BoldLabel(ToolTips.editorSettingsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.saveSettings))
            {
                MapEditorSettings.SaveSettings();
            }
            if (Elements.ToolbarButton(ToolTips.discardSettings))
            {
                MapEditorSettings.LoadSettings();
                ToolTips.rustDirectoryPath.text = MapEditorSettings.rustDirectory;
            }
            if (Elements.ToolbarButton(ToolTips.defaultSettings))
            {
                MapEditorSettings.SetDefaultSettings();
            }
            Elements.EndToolbarHorizontal();

            Elements.MiniBoldLabel(ToolTips.rustDirectory);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.browseRustDirectory))
            {
                var returnDirectory = EditorUtility.OpenFolderPanel("Browse Rust Directory", MapEditorSettings.rustDirectory, "Rust");
                MapEditorSettings.rustDirectory = String.IsNullOrEmpty(returnDirectory) ? MapEditorSettings.rustDirectory : returnDirectory;
                ToolTips.rustDirectoryPath.text = MapEditorSettings.rustDirectory;
            }
            Elements.ToolbarLabel(ToolTips.rustDirectoryPath);
            Elements.EndToolbarHorizontal();

            Elements.MiniBoldLabel(ToolTips.renderDistanceLabel);
            EditorGUI.BeginChangeCheck();
            MapEditorSettings.prefabRenderDistance = Elements.ToolbarSlider(ToolTips.prefabRenderDistance, MapEditorSettings.prefabRenderDistance, 0, 5000f);
            MapEditorSettings.pathRenderDistance = Elements.ToolbarSlider(ToolTips.pathRenderDistance, MapEditorSettings.pathRenderDistance, 0, 5000f);
            if (EditorGUI.EndChangeCheck())
            {
                MapIO.SetCullingDistances(MapIO.GetLastSceneView().camera, MapEditorSettings.prefabRenderDistance, MapEditorSettings.pathRenderDistance);
            }

            //MapEditorSettings.objectQuality = Elements.ToolbarIntSlider(ToolTips.objectQuality, MapEditorSettings.objectQuality, 0, 200);
        }
        #endregion
        #region Prefabs
        public static void PrefabTools(ref bool deleteOnExport, string lootCrateSaveFile = "", string mapPrefabSaveFile = "")
        {
            Elements.MiniBoldLabel(ToolTips.toolsLabel);

            Elements.BeginToolbarHorizontal();
            deleteOnExport = Elements.ToolbarToggle(ToolTips.deleteOnExport, deleteOnExport);
            if (Elements.ToolbarButton(ToolTips.exportMapLootCrates))
            {
                lootCrateSaveFile = EditorUtility.SaveFilePanel("Export LootCrates", lootCrateSaveFile, "LootCrateData", "json");
                if (!String.IsNullOrEmpty(lootCrateSaveFile))
                {
                    MapIO.ExportLootCrates(lootCrateSaveFile, deleteOnExport);
                }
            }
            if (Elements.ToolbarButton(ToolTips.exportMapPrefabs))
            {
                mapPrefabSaveFile = EditorUtility.SaveFilePanel("Export Map Prefabs", mapPrefabSaveFile, "MapData", "json");
                if (!String.IsNullOrEmpty(mapPrefabSaveFile))
                {
                    MapIO.ExportMapPrefabs(mapPrefabSaveFile, deleteOnExport);
                }
            }
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.hidePrefabsInRustEdit))
            {
                MapIO.HidePrefabsInRustEdit();
            }
            if (Elements.ToolbarButton(ToolTips.breakRustEditPrefabs))
            {
                MapIO.BreakRustEditCustomPrefabs();
            }
            if (Elements.ToolbarButton(ToolTips.groupRustEditPrefabs))
            {
                MapIO.GroupRustEditCustomPrefabs();
            }
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.deleteMapPrefabs))
            {
                MapIO.RemoveMapObjects(true, false);
            }
            if (Elements.ToolbarButton(ToolTips.deleteMapPaths))
            {
                MapIO.RemoveMapObjects(false, true);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void AssetBundle()
        {
            Elements.MiniBoldLabel(ToolTips.assetBundleLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.loadBundle))
            {
                PrefabManager.LoadBundle(MapEditorSettings.rustDirectory + MapEditorSettings.bundlePathExt);
            }
            if (Elements.ToolbarButton(ToolTips.unloadBundle))
            {
                PrefabManager.DisposeBundle();
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion
        #region Generation Tools
        public static void NodePresets(Vector2 presetScrollPos)
        {
            Elements.BoldLabel(ToolTips.presetsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.refreshPresets))
            {
                MapIO.RefreshPresetsList();
            }
            Elements.EndToolbarHorizontal();

            presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
            ReorderableListGUI.Title("Node Presets");
            ReorderableListGUI.ListField(MapIO.generationPresetList, NodePresetDrawer, DrawEmpty);
            GUILayout.EndScrollView();
        }
        public static void DrawEmpty()
        {
            Elements.MiniLabel(ToolTips.noPresets);
        }
        #endregion
        #region MapTools
        #region HeightMap
        public static void TerraceMap(ref float terraceErodeFeatureSize, ref float terraceErodeInteriorCornerWeight)
        {
            Elements.MiniBoldLabel(ToolTips.terraceLabel);

            terraceErodeFeatureSize = Elements.ToolbarSlider(ToolTips.featureSize, terraceErodeFeatureSize, 2f, 1000f);
            terraceErodeInteriorCornerWeight = Elements.ToolbarSlider(ToolTips.cornerWeight, terraceErodeInteriorCornerWeight, 0f, 1f);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.terraceMap))
            {
                MapIO.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void SmoothMap(ref float filterStrength, ref float blurDirection, ref int smoothPasses)
        {
            Elements.MiniBoldLabel(ToolTips.smoothLabel);

            filterStrength = Elements.ToolbarSlider(ToolTips.smoothStrength, filterStrength, 0f, 1f);
            blurDirection = Elements.ToolbarSlider(ToolTips.blurDirection, blurDirection, -1f, 1f);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.smoothMap))
            {
                for (int i = 0; i < smoothPasses; i++)
                {
                    MapIO.SmoothHeightmap(filterStrength, blurDirection);
                }
            }
            smoothPasses = EditorGUILayout.IntSlider(smoothPasses, 1, 100);
            Elements.EndToolbarHorizontal();
        }
        public static void NormaliseMap(ref float normaliseLow, ref float normaliseHigh, ref bool autoUpdate)
        {
            Elements.MiniBoldLabel(ToolTips.normaliseLabel);

            EditorGUI.BeginChangeCheck();
            normaliseLow = Elements.ToolbarSlider(ToolTips.normaliseLow, normaliseLow, 0f, normaliseHigh);
            normaliseHigh = Elements.ToolbarSlider(ToolTips.normaliseHigh, normaliseHigh, normaliseLow, 1000f);
            if (EditorGUI.EndChangeCheck() && autoUpdate == true)
            {
                MapIO.NormaliseHeightmap(normaliseLow, normaliseHigh, Selections.Terrains.Land);
            }

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.normaliseMap))
            {
                MapIO.NormaliseHeightmap(normaliseLow, normaliseHigh, Selections.Terrains.Land);
            }
            autoUpdate = Elements.ToolbarToggle(ToolTips.autoUpdateNormalise, autoUpdate);
            Elements.EndToolbarHorizontal();
        }
        public static void SetHeight(ref float height)
        {
            Elements.MiniBoldLabel(ToolTips.setHeightLabel);

            height = Elements.ToolbarSlider(ToolTips.heightToSet, height, 0f, 1000f);
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.setLandHeight))
            {
                MapIO.SetHeightmap(height, Selections.Terrains.Land);
            }
            if (Elements.ToolbarButton(ToolTips.setWaterHeight))
            {
                MapIO.SetHeightmap(height, Selections.Terrains.Water);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void ClampHeight(ref float heightLow, ref float heightHigh)
        {
            Elements.MiniBoldLabel(ToolTips.clampHeightLabel);

            Elements.ToolbarMinMax(ToolTips.minHeight, ToolTips.maxHeight, ref heightLow, ref heightHigh, 0f, 1000f);
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.setMinHeight))
            {
                MapIO.ClampHeightmap(heightLow, 1000f, Selections.Terrains.Land);
            }
            if (Elements.ToolbarButton(ToolTips.setMaxHeight))
            {
                MapIO.ClampHeightmap(0f, heightHigh, Selections.Terrains.Land);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void OffsetMap(ref float offset, ref bool clampOffset)
        {
            Elements.MiniBoldLabel(ToolTips.offsetLabel);

            offset = Elements.ToolbarSlider(ToolTips.offsetHeight, offset, -1000f, 1000f);
            Elements.BeginToolbarHorizontal();
            clampOffset = Elements.ToolbarToggle(ToolTips.clampOffset, clampOffset);
            if (Elements.ToolbarButton(ToolTips.offsetLand))
            {
                MapIO.OffsetHeightmap(offset, clampOffset, Selections.Terrains.Land);
            }
            if (Elements.ToolbarButton(ToolTips.offsetWater))
            {
                MapIO.OffsetHeightmap(offset, clampOffset, Selections.Terrains.Water);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void InvertMap()
        {
            Elements.MiniBoldLabel(ToolTips.invertLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.invertLand))
            {
                MapIO.InvertHeightmap(Selections.Terrains.Land);
            }
            if (Elements.ToolbarButton(ToolTips.invertWater))
            {
                MapIO.InvertHeightmap(Selections.Terrains.Water);
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion
        public static void ConditionalPaintConditions(ref Conditions cnds, ref int cndsOptions)
        {
            Elements.BoldLabel(ToolTips.conditionalPaintLabel);

            GUIContent[] conditionalPaintMenu = new GUIContent[5];
            conditionalPaintMenu[0] = new GUIContent("Ground");
            conditionalPaintMenu[1] = new GUIContent("Biome");
            conditionalPaintMenu[2] = new GUIContent("Alpha");
            conditionalPaintMenu[3] = new GUIContent("Topology");
            conditionalPaintMenu[4] = new GUIContent("Terrain");
            cndsOptions = GUILayout.Toolbar(cndsOptions, conditionalPaintMenu, EditorStyles.toolbarButton);

            Elements.MiniBoldLabel(ToolTips.conditionsLabel);

            switch (cndsOptions)
            {
                case 0: // Ground
                    Elements.BeginToolbarHorizontal();
                    cnds.GroundConditions.CheckLayer[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.GroundConditions.CheckLayer[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)]);
                    cnds.GroundConditions.Layer = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(cnds.GroundConditions.Layer);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    cnds.GroundConditions.Weight[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)] = Elements.ToolbarSlider(ToolTips.conditionalTextureWeight, cnds.GroundConditions.Weight[TerrainSplat.TypeToIndex((int)cnds.GroundConditions.Layer)], 0.01f, 1f);
                    Elements.EndToolbarHorizontal();
                    break;
                case 1: // Biome
                    Elements.BeginToolbarHorizontal();
                    cnds.BiomeConditions.CheckLayer[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.BiomeConditions.CheckLayer[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)]);
                    cnds.BiomeConditions.Layer = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(cnds.BiomeConditions.Layer);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    cnds.BiomeConditions.Weight[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)] = Elements.ToolbarSlider(ToolTips.conditionalTextureWeight, cnds.BiomeConditions.Weight[TerrainBiome.TypeToIndex((int)cnds.BiomeConditions.Layer)], 0.01f, 1f);
                    Elements.EndToolbarHorizontal();
                    break;
                case 2: // Alpha
                    Elements.BeginToolbarHorizontal();
                    cnds.AlphaConditions.CheckAlpha = Elements.ToolbarToggle(ToolTips.checkTexture, cnds.AlphaConditions.CheckAlpha);
                    cnds.AlphaConditions.Texture = (AlphaTextures)Elements.ToolbarEnumPopup(cnds.AlphaConditions.Texture);
                    Elements.EndToolbarHorizontal();
                    break;
                case 3: // Topology
                    Elements.BeginToolbarHorizontal();
                    cnds.TopologyConditions.CheckLayer[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)] = Elements.ToolbarToggle(ToolTips.checkTopologyLayer, cnds.TopologyConditions.CheckLayer[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)]);
                    cnds.TopologyConditions.Layer = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(cnds.TopologyConditions.Layer);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.checkTexture);
                    cnds.TopologyConditions.Texture[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)] = (TopologyTextures)Elements.ToolbarEnumPopup(cnds.TopologyConditions.Texture[TerrainTopology.TypeToIndex((int)cnds.TopologyConditions.Layer)]);
                    Elements.EndToolbarHorizontal();
                    break;
                case 4: // Terrain
                    float tempSlopeLow = cnds.TerrainConditions.Slopes.SlopeLow, tempSlopeHigh = cnds.TerrainConditions.Slopes.SlopeHigh;
                    cnds.TerrainConditions.CheckSlopes = Elements.ToolbarToggleMinMax(ToolTips.checkSlopes, ToolTips.rangeLow, ToolTips.rangeHigh, cnds.TerrainConditions.CheckSlopes, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                    cnds.TerrainConditions.Slopes.SlopeLow = tempSlopeLow; cnds.TerrainConditions.Slopes.SlopeHigh = tempSlopeHigh;

                    float tempHeightLow = cnds.TerrainConditions.Heights.HeightLow, tempHeightHigh = cnds.TerrainConditions.Heights.HeightHigh;
                    cnds.TerrainConditions.CheckHeights = Elements.ToolbarToggleMinMax(ToolTips.checkHeights, ToolTips.rangeLow, ToolTips.rangeHigh, cnds.TerrainConditions.CheckHeights, ref tempHeightLow, ref tempHeightHigh, 0f, 1000f);
                    cnds.TerrainConditions.Heights.HeightLow = tempHeightLow; cnds.TerrainConditions.Heights.HeightHigh = tempHeightHigh;
                    break;
            }
        }
        public static void ConditionalPaintLayerSelect(ref Conditions cnds, ref Layers layers, ref int texture)
        {
            Elements.MiniBoldLabel(ToolTips.textureToPaintLabel);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.layerSelect);
            layers.LandLayer = (LandLayers)Elements.ToolbarEnumPopup(layers.LandLayer);
            Elements.EndToolbarHorizontal();

            switch (layers.LandLayer)
            {
                case LandLayers.Ground:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
                    texture = TerrainSplat.TypeToIndex((int)layers.Ground);
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Biome:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
                    texture = TerrainBiome.TypeToIndex((int)layers.Biome);
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Alpha:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.AlphaTexture = (AlphaTextures)Elements.ToolbarEnumPopup(layers.AlphaTexture);
                    texture = (int)layers.AlphaTexture;
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Topology:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.topologyLayerSelect);
                    layers.Topologies = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(layers.Topologies);
                    Elements.EndToolbarHorizontal();

                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.TopologyTexture = (TopologyTextures)Elements.ToolbarEnumPopup(layers.TopologyTexture);
                    texture = (int)layers.TopologyTexture;
                    Elements.EndToolbarHorizontal();
                    break;
            }

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintConditional))
            {
                MapIO.PaintConditional(layers.LandLayer, texture, cnds, TerrainTopology.TypeToIndex((int)layers.Topologies));
            }
            Elements.EndToolbarHorizontal();
        }
        public static void ConditionalPaint(ref int cndsOptions, ref int texture, ref Conditions cnds, ref Layers layers)
        {
            ConditionalPaintConditions(ref cnds, ref cndsOptions);
            ConditionalPaintLayerSelect(ref cnds, ref layers, ref texture);
        }
        public static void RotateMap(ref Selections.Objects selection)
        {
            Elements.MiniBoldLabel(ToolTips.rotateMapLabel);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.rotateSelection);
            selection = (Selections.Objects)Elements.ToolbarEnumFlagsField(selection);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotate90))
            {
                MapIO.RotateMap(selection, true);
            }
            if (Elements.ToolbarButton(ToolTips.rotate270))
            {
                MapIO.RotateMap(selection, false);
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion
        #region LayerTools
        public static void TextureSelect(LandLayers landLayer, ref Layers layers)
        {
            Elements.MiniBoldLabel(ToolTips.textureSelectLabel);

            switch (landLayer)
            {
                case LandLayers.Ground:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
                    Elements.EndToolbarHorizontal();
                    break;
                case LandLayers.Biome:
                    Elements.BeginToolbarHorizontal();
                    Elements.ToolbarLabel(ToolTips.textureSelect);
                    layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
                    Elements.EndToolbarHorizontal();
                    break;
            }
        }
        public static void TopologyLayerSelect(ref Layers layers)
        {
            Elements.MiniBoldLabel(ToolTips.layerSelect);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.topologyLayerSelect);
            EditorGUI.BeginChangeCheck();
            layers.Topologies = (TerrainTopology.Enum)Elements.ToolbarEnumPopup(layers.Topologies);
            Elements.EndToolbarHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                ChangeLandLayer(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layers.Topologies));
            }
        }
        public static void SlopeTools(LandLayers landLayer, int texture, ref SlopesInfo slopeInfo, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.slopeToolsLabel);
            slopeInfo = ClampValues(slopeInfo);

            float tempSlopeLow = slopeInfo.SlopeLow; float tempSlopeHigh = slopeInfo.SlopeHigh;
            if ((int)landLayer < 2)
            {
                float tempSlopeBlendLow = slopeInfo.SlopeBlendLow; float tempSlopeBlendHigh = slopeInfo.SlopeBlendHigh;
                slopeInfo.BlendSlopes = Elements.ToolbarToggleMinMax(ToolTips.toggleBlend, ToolTips.rangeLow, ToolTips.rangeHigh, slopeInfo.BlendSlopes, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                slopeInfo.SlopeLow = tempSlopeLow; slopeInfo.SlopeHigh = tempSlopeHigh;

                if (slopeInfo.BlendSlopes)
                {
                    Elements.ToolbarMinMax(ToolTips.blendLow, ToolTips.blendHigh, ref tempSlopeBlendLow, ref tempSlopeBlendHigh, 0f, 90f);
                    slopeInfo.SlopeBlendLow = tempSlopeBlendLow; slopeInfo.SlopeBlendHigh = tempSlopeBlendHigh;
                }

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintSlopes))
                {
                    MapIO.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, texture);
                }
                if (Elements.ToolbarButton(ToolTips.paintSlopesBlend))
                {
                    MapIO.PaintSlopeBlend(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, slopeInfo.SlopeBlendLow, slopeInfo.SlopeBlendHigh, texture);
                }
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.ToolbarMinMax(ToolTips.rangeLow, ToolTips.rangeHigh, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                slopeInfo.SlopeLow = tempSlopeLow; slopeInfo.SlopeHigh = tempSlopeHigh;

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintSlopes))
                {
                    MapIO.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, texture, topology);
                }
                if (Elements.ToolbarButton(ToolTips.eraseSlopes))
                {
                    MapIO.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, erase, topology);
                }
                Elements.EndToolbarHorizontal();
            }
        }
        public static void HeightTools(LandLayers landLayer, int texture, ref HeightsInfo heightInfo, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.heightToolsLabel);
            heightInfo = ClampValues(heightInfo);

            float tempSlopeLow = heightInfo.HeightLow; float tempSlopeHigh = heightInfo.HeightHigh;
            if ((int)landLayer < 2)
            {
                float tempHeightBlendLow = heightInfo.HeightBlendLow; float tempHeightBlendHigh = heightInfo.HeightBlendHigh;
                heightInfo.BlendHeights = Elements.ToolbarToggleMinMax(ToolTips.toggleBlend, ToolTips.rangeLow, ToolTips.rangeHigh, heightInfo.BlendHeights, ref tempSlopeLow, ref tempSlopeHigh, 0f, 1000f);
                heightInfo.HeightLow = tempSlopeLow; heightInfo.HeightHigh = tempSlopeHigh;

                if (heightInfo.BlendHeights)
                {
                    Elements.ToolbarMinMax(ToolTips.blendLow, ToolTips.blendHigh, ref tempHeightBlendLow, ref tempHeightBlendHigh, 0f, 1000f);
                    heightInfo.HeightBlendLow = tempHeightBlendLow; heightInfo.HeightBlendHigh = tempHeightBlendHigh;
                }

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintHeights))
                {
                    MapIO.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, texture);
                }
                if (Elements.ToolbarButton(ToolTips.paintHeightsBlend))
                {
                    MapIO.PaintHeightBlend(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, heightInfo.HeightBlendLow, heightInfo.HeightBlendHigh, texture);
                }
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.ToolbarMinMax(ToolTips.rangeLow, ToolTips.rangeHigh, ref tempSlopeLow, ref tempSlopeHigh, 0f, 1000f);
                heightInfo.HeightLow = tempSlopeLow; heightInfo.HeightHigh = tempSlopeHigh;

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintHeights))
                {
                    MapIO.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, texture, topology);
                }
                if (Elements.ToolbarButton(ToolTips.eraseHeights))
                {
                    MapIO.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, erase, topology);
                }
                Elements.EndToolbarHorizontal();
            }
        }
        public static void TopologyTools()
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotateAll90))
            {
                MapIO.RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, true);
            }
            if (Elements.ToolbarButton(ToolTips.rotateAll270))
            {
                MapIO.RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, false);
            }
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintAll))
            {
                MapIO.PaintTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            }
            if (Elements.ToolbarButton(ToolTips.clearAll))
            {
                MapIO.ClearTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            }
            if (Elements.ToolbarButton(ToolTips.invertAll))
            {
                MapIO.InvertTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void AreaTools(LandLayers landLayer, int texture, Dimensions dmns, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.areaToolsLabel);

            float tmpz0 = dmns.z0; float tmpz1 = dmns.z1; float tmpx0 = dmns.x0; float tmpx1 = dmns.x1;
            Elements.ToolbarMinMaxInt(ToolTips.fromZ, ToolTips.toZ, ref tmpz0, ref tmpz1, 0f, land.terrainData.alphamapResolution);
            Elements.ToolbarMinMaxInt(ToolTips.fromX, ToolTips.toX, ref tmpx0, ref tmpx1, 0f, land.terrainData.alphamapResolution);
            dmns.z0 = (int)tmpz0; dmns.z1 = (int)tmpz1; dmns.x0 = (int)tmpx0; dmns.x1 = (int)tmpx1;

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintArea))
            {
                MapIO.PaintArea(landLayer, dmns, texture, topology);
            }
            if ((int)landLayer > 1)
            {
                if (Elements.ToolbarButton(ToolTips.eraseArea))
                {
                    MapIO.PaintArea(landLayer, dmns, erase, topology);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        public static void RiverTools(LandLayers landLayer, int texture, ref bool aboveTerrain, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.riverToolsLabel);

            if ((int)landLayer > 1)
            {
                Elements.BeginToolbarHorizontal();
                aboveTerrain = Elements.ToolbarToggle(ToolTips.aboveTerrain, aboveTerrain);
                if (Elements.ToolbarButton(ToolTips.paintRivers))
                {
                    MapIO.PaintRiver(landLayer, aboveTerrain, texture, topology);
                }
                if (Elements.ToolbarButton(ToolTips.eraseRivers))
                {
                    MapIO.PaintRiver(landLayer, aboveTerrain, erase, topology);
                }
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.BeginToolbarHorizontal();
                aboveTerrain = Elements.ToolbarToggle(ToolTips.aboveTerrain, aboveTerrain);
                if (Elements.ToolbarButton(ToolTips.paintRivers))
                {
                    MapIO.PaintRiver(landLayer, aboveTerrain, texture);
                }
                Elements.EndToolbarHorizontal();
            }
        }
        public static void RotateTools(LandLayers landLayer, int topology = 0)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotate90))
            {
                MapIO.RotateLayer(landLayer, true, topology);
            }
            if (Elements.ToolbarButton(ToolTips.rotate270))
            {
                MapIO.RotateLayer(landLayer, false, topology);
            }
            Elements.EndToolbarHorizontal();
        }
        public static void LayerTools(LandLayers landLayer, int texture, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.layerToolsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintLayer))
            {
                MapIO.PaintLayer(landLayer, texture, topology);
            }
            if ((int)landLayer > 1)
            {
                if (Elements.ToolbarButton(ToolTips.clearLayer))
                {
                    MapIO.ClearLayer(landLayer, topology);
                }
                if (Elements.ToolbarButton(ToolTips.invertLayer))
                {
                    MapIO.InvertLayer(landLayer, topology);
                }
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion
        #region PrefabData
        public static void PrefabCategory(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabCategory);
            target.prefabData.category = Elements.ToolbarTextField(target.prefabData.category);
            Elements.EndToolbarHorizontal();
        }
        public static void PrefabID(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabID);
            target.prefabData.id = uint.Parse(Elements.ToolbarDelayedTextField(target.prefabData.id.ToString()));
            Elements.EndToolbarHorizontal();
        }
        public static void SnapToGround(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.snapToGround))
            {
                target.SnapToGround();
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion
        #region Functions
        public static string NodePresetDrawer(Rect position, string itemValue)
        {
            position.width -= 39;
            GUI.Label(position, itemValue);
            position.x = position.xMax;
            position.width = 39;
            if (GUI.Button(position, ToolTips.openPreset, EditorStyles.toolbarButton))
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
        /// <summary>Sets the active landLayer to the index.</summary>
        /// <param name="landIndex">The landLayer to change to.</param>
        /// <param name="topology">The Topology layer to set.</param>
        public static void SetLandLayer(LandLayers landIndex, int topology = 0)
        {
            ChangeLandLayer(landIndex, topology);
        }
        public static SlopesInfo ClampValues(SlopesInfo info)
        {
            info.SlopeLow = Mathf.Clamp(info.SlopeLow, 0f, info.SlopeHigh);
            info.SlopeHigh = Mathf.Clamp(info.SlopeHigh, info.SlopeLow, 90f);
            info.SlopeBlendLow = Mathf.Clamp(info.SlopeBlendLow, 0f, info.SlopeLow);
            info.SlopeBlendHigh = Mathf.Clamp(info.SlopeBlendHigh, info.SlopeHigh, 90f);
            return info;
        }
        public static HeightsInfo ClampValues(HeightsInfo info)
        {
            info.HeightLow = Mathf.Clamp(info.HeightLow, 0f, info.HeightHigh);
            info.HeightHigh = Mathf.Clamp(info.HeightHigh, info.HeightLow, 1000f);
            info.HeightBlendLow = Mathf.Clamp(info.HeightBlendLow, 0f, info.HeightLow);
            info.HeightBlendHigh = Mathf.Clamp(info.HeightBlendHigh, info.HeightHigh, 1000f);
            return info;
        }
        #endregion
        #region NodeGraph
        public static void NodeGraphToolbar(XNode.NodeGraph nodeGraph)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.runPreset))
            {
                NodeAsset.Parse(nodeGraph);
            }
            if (Elements.ToolbarButton(ToolTips.deletePreset))
            {
                if (EditorUtility.DisplayDialog("Delete Preset", "Are you sure you wish to delete this preset? Once deleted it can't be undone.", "Ok", "Cancel"))
                {
                    XNodeEditor.NodeEditorWindow.focusedWindow.Close();
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(nodeGraph));
                    MapIO.RefreshPresetsList();
                }
            }
            if (Elements.ToolbarButton(ToolTips.renamePreset))
            {
                XNodeEditor.RenamePreset.Show(nodeGraph);
            }
            if (Elements.ToolbarButton(ToolTips.presetWiki))
            {
                Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/wiki/Node-System");
            }
            Elements.EndToolbarHorizontal();
        }
        #endregion
    }
}