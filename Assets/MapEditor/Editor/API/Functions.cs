using System;
using Rotorz.ReorderableList;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;
using static RustMapEditor.Data.TerrainManager;

namespace RustMapEditor.UI
{
    public static class Functions
    {
        #region Menu Items
        [MenuItem("Rust Map Editor/Main Menu", false, 0)]
        public static void OpenMainMenu()
        {
            MapManagerWindow window = (MapManagerWindow)EditorWindow.GetWindow(typeof(MapManagerWindow), false, "Rust Map Editor");
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

        [MenuItem("Rust Map Editor/Prefabs", false, 1)]
        static void OpenPrefabsList()
        {
            PrefabsListWindow window = (PrefabsListWindow)EditorWindow.GetWindow(typeof(PrefabsListWindow), false, "Prefabs List");
        }

        [MenuItem("Rust Map Editor/Terrain Tools", false, 2)]
        public static void OpenTerrainTools()
        {
            Selection.activeGameObject = Land.gameObject;
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
        public static void EditorIO(string mapName = "custommap")
        {
            Elements.BeginToolbarHorizontal();
            LoadMap();
            SaveMap(mapName);
            NewMap();
            Elements.EndToolbarHorizontal();
        }

        public static void LoadMap()
        {
            if (Elements.ToolbarButton(ToolTips.loadMap))
                LoadMapPanel();
        }

        public static void LoadMapPanel()
        {
            string loadFile = "";
            loadFile = EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
            if (string.IsNullOrEmpty(loadFile))
                return;
            var world = new WorldSerialization();
            world.Load(loadFile);
            MapManager.Load(WorldConverter.WorldToTerrain(world), loadFile);
            ReloadTreeViews();
        }

        public static void SaveMap(string mapName = "custommap")
        {
            if (Elements.ToolbarButton(ToolTips.saveMap))
                SaveMapPanel(mapName);
        }

        public static void SaveMapPanel(string mapName = "custommap")
        {
            string saveFile = "";
            saveFile = EditorUtility.SaveFilePanel("Save Map File", saveFile, mapName, "map");
            if (string.IsNullOrEmpty(saveFile))
                return;
            MapManager.Save(saveFile);
        }

        public static void NewMap()
        {
            if (Elements.ToolbarButton(ToolTips.newMap))
                NewMapPanel();
        }

        public static void NewMapPanel()
        {
            CreateMapWindow.Init();
        }

        public static void MapInfo()
        {
            if (Land != null)
            {
                Elements.BoldLabel(ToolTips.mapInfoLabel);
                GUILayout.Label("Size: " + Land.terrainData.size.x);
                GUILayout.Label("HeightMap: " + Land.terrainData.heightmapResolution + "x" + Land.terrainData.heightmapResolution);
                GUILayout.Label("SplatMap: " + Land.terrainData.alphamapResolution + "x" + Land.terrainData.alphamapResolution);
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
                OpenReportBug();
            if (Elements.ToolbarButton(ToolTips.requestFeature))
                OpenRequestFeature();
            if (Elements.ToolbarButton(ToolTips.roadMap))
                OpenRoadMap();
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.wiki))
                OpenWiki();
            if (Elements.ToolbarButton(ToolTips.discord))
                OpenDiscord();
            Elements.EndToolbarHorizontal();
        }
        public static void EditorSettings()
        {
            Elements.BoldLabel(ToolTips.editorSettingsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.saveSettings))
                SettingsManager.SaveSettings();
            if (Elements.ToolbarButton(ToolTips.discardSettings))
            {
                SettingsManager.LoadSettings();
                ToolTips.rustDirectoryPath.text = SettingsManager.RustDirectory;
            }
            if (Elements.ToolbarButton(ToolTips.defaultSettings))
                SettingsManager.SetDefaultSettings();
            Elements.EndToolbarHorizontal();

            Elements.MiniBoldLabel(ToolTips.rustDirectory);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.browseRustDirectory))
            {
                var returnDirectory = EditorUtility.OpenFolderPanel("Browse Rust Directory", SettingsManager.RustDirectory, "Rust");
                SettingsManager.RustDirectory = String.IsNullOrEmpty(returnDirectory) ? SettingsManager.RustDirectory : returnDirectory;
                ToolTips.rustDirectoryPath.text = SettingsManager.RustDirectory;
            }
            Elements.ToolbarLabel(ToolTips.rustDirectoryPath);
            Elements.EndToolbarHorizontal();

            Elements.MiniBoldLabel(ToolTips.renderDistanceLabel);
            EditorGUI.BeginChangeCheck();
            SettingsManager.PrefabRenderDistance = Elements.ToolbarSlider(ToolTips.prefabRenderDistance, SettingsManager.PrefabRenderDistance, 0, 5000f);
            SettingsManager.PathRenderDistance = Elements.ToolbarSlider(ToolTips.pathRenderDistance, SettingsManager.PathRenderDistance, 0, 5000f);

            if (EditorGUI.EndChangeCheck())
                MapManager.SetCullingDistances(SceneView.GetAllSceneCameras(), SettingsManager.PrefabRenderDistance, SettingsManager.PathRenderDistance);

            EditorGUI.BeginChangeCheck();
            SettingsManager.WaterTransparency = Elements.ToolbarSlider(ToolTips.waterTransparency, SettingsManager.WaterTransparency, 0f, 0.5f);
            if (EditorGUI.EndChangeCheck())
                SetWaterTransparency(SettingsManager.WaterTransparency);

            Elements.BeginToolbarHorizontal();
            SettingsManager.LoadBundleOnProjectLoad = Elements.ToolbarCheckBox(ToolTips.loadBundleOnProjectLoad, SettingsManager.LoadBundleOnProjectLoad);
            Elements.EndToolbarHorizontal();

        }
        #endregion

        #region Prefabs
        public static void PrefabTools(ref bool deleteOnExport, string lootCrateSaveFile = "", string mapPrefabSaveFile = "")
        {
            Elements.MiniBoldLabel(ToolTips.toolsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.deleteMapPrefabs))
                PrefabManager.DeletePrefabs(PrefabManager.CurrentMapPrefabs);
            if (Elements.ToolbarButton(ToolTips.deleteMapPaths))
                PathManager.DeletePaths(PathManager.CurrentMapPaths);
            Elements.EndToolbarHorizontal();
        }
        public static void AssetBundle()
        {
            Elements.MiniBoldLabel(ToolTips.assetBundleLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.loadBundle))
                AssetManager.Initialise(SettingsManager.RustDirectory + SettingsManager.BundlePathExt);
            if (Elements.ToolbarButton(ToolTips.unloadBundle))
                AssetManager.Dispose();
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region Generation Tools
        public static void NodePresets(Vector2 presetScrollPos)
        {
            Elements.BoldLabel(ToolTips.presetsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.refreshPresets))
                MapManager.RefreshPresetsList();
            Elements.EndToolbarHorizontal();

            presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
            ReorderableListGUI.Title("Node Presets");
            ReorderableListGUI.ListField(MapManager.generationPresetList, NodePresetDrawer, DrawEmpty);
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
                MapManager.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
            Elements.EndToolbarHorizontal();
        }

        public static void SmoothMap(ref float filterStrength, ref float blurDirection, ref int smoothPasses)
        {
            Elements.MiniBoldLabel(ToolTips.smoothLabel);

            filterStrength = Elements.ToolbarSlider(ToolTips.smoothStrength, filterStrength, 0f, 1f);
            blurDirection = Elements.ToolbarSlider(ToolTips.blurDirection, blurDirection, -1f, 1f);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.smoothMap))
                for (int i = 0; i < smoothPasses; i++)
                    MapManager.SmoothHeightmap(filterStrength, blurDirection);
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
                MapManager.NormaliseHeightmap(normaliseLow, normaliseHigh, Selections.Terrains.Land, Dimensions.HeightMapDimensions());

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.normaliseMap))
                MapManager.NormaliseHeightmap(normaliseLow, normaliseHigh, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            autoUpdate = Elements.ToolbarToggle(ToolTips.autoUpdateNormalise, autoUpdate);
            Elements.EndToolbarHorizontal();
        }

        public static void SetHeight(ref float height)
        {
            Elements.MiniBoldLabel(ToolTips.setHeightLabel);

            height = Elements.ToolbarSlider(ToolTips.heightToSet, height, 0f, 1000f);
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.setLandHeight))
                MapManager.SetHeightmap(height, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.setWaterHeight))
                MapManager.SetHeightmap(height, Selections.Terrains.Water, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }

        public static void ClampHeight(ref float heightLow, ref float heightHigh)
        {
            Elements.MiniBoldLabel(ToolTips.clampHeightLabel);

            Elements.ToolbarMinMax(ToolTips.minHeight, ToolTips.maxHeight, ref heightLow, ref heightHigh, 0f, 1000f);
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.setMinHeight))
                MapManager.ClampHeightmap(heightLow, 1000f, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.setMaxHeight))
                MapManager.ClampHeightmap(0f, heightHigh, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }

        public static void OffsetMap(ref float offset, ref bool clampOffset)
        {
            Elements.MiniBoldLabel(ToolTips.offsetLabel);

            offset = Elements.ToolbarSlider(ToolTips.offsetHeight, offset, -1000f, 1000f);
            Elements.BeginToolbarHorizontal();
            clampOffset = Elements.ToolbarToggle(ToolTips.clampOffset, clampOffset);
            if (Elements.ToolbarButton(ToolTips.offsetLand))
                MapManager.OffsetHeightmap(offset, clampOffset, Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.offsetWater))
                MapManager.OffsetHeightmap(offset, clampOffset, Selections.Terrains.Water, Dimensions.HeightMapDimensions());
            Elements.EndToolbarHorizontal();
        }

        public static void InvertMap()
        {
            Elements.MiniBoldLabel(ToolTips.invertLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.invertLand))
                MapManager.InvertHeightmap(Selections.Terrains.Land, Dimensions.HeightMapDimensions());
            if (Elements.ToolbarButton(ToolTips.invertWater))
                MapManager.InvertHeightmap(Selections.Terrains.Water, Dimensions.HeightMapDimensions());
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
                MapManager.PaintConditional(layers.LandLayer, texture, cnds, TerrainTopology.TypeToIndex((int)layers.Topologies));
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
                MapManager.RotateMap(selection, true);
            if (Elements.ToolbarButton(ToolTips.rotate270))
                MapManager.RotateMap(selection, false);
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
                ChangeLandLayer(LandLayers.Topology, TerrainTopology.TypeToIndex((int)layers.Topologies));
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
                    MapManager.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, texture);
                if (Elements.ToolbarButton(ToolTips.paintSlopesBlend))
                    MapManager.PaintSlopeBlend(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, slopeInfo.SlopeBlendLow, slopeInfo.SlopeBlendHigh, texture);
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.ToolbarMinMax(ToolTips.rangeLow, ToolTips.rangeHigh, ref tempSlopeLow, ref tempSlopeHigh, 0f, 90f);
                slopeInfo.SlopeLow = tempSlopeLow; slopeInfo.SlopeHigh = tempSlopeHigh;

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintSlopes))
                    MapManager.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, texture, topology);
                if (Elements.ToolbarButton(ToolTips.eraseSlopes))
                    MapManager.PaintSlope(landLayer, slopeInfo.SlopeLow, slopeInfo.SlopeHigh, erase, topology);
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
                    MapManager.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, texture);
                if (Elements.ToolbarButton(ToolTips.paintHeightsBlend))
                    MapManager.PaintHeightBlend(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, heightInfo.HeightBlendLow, heightInfo.HeightBlendHigh, texture);
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.ToolbarMinMax(ToolTips.rangeLow, ToolTips.rangeHigh, ref tempSlopeLow, ref tempSlopeHigh, 0f, 1000f);
                heightInfo.HeightLow = tempSlopeLow; heightInfo.HeightHigh = tempSlopeHigh;

                Elements.BeginToolbarHorizontal();
                if (Elements.ToolbarButton(ToolTips.paintHeights))
                    MapManager.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, texture, topology);
                if (Elements.ToolbarButton(ToolTips.eraseHeights))
                    MapManager.PaintHeight(landLayer, heightInfo.HeightLow, heightInfo.HeightHigh, erase, topology);
                Elements.EndToolbarHorizontal();
            }
        }

        public static void TopologyTools()
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotateAll90))
                MapManager.RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, true);
            if (Elements.ToolbarButton(ToolTips.rotateAll270))
                MapManager.RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, false);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintAll))
                MapManager.PaintTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            if (Elements.ToolbarButton(ToolTips.clearAll))
                MapManager.ClearTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            if (Elements.ToolbarButton(ToolTips.invertAll))
                MapManager.InvertTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING);
            Elements.EndToolbarHorizontal();
        }

        public static void AreaSelect()
        {
            Elements.MiniBoldLabel(ToolTips.areaSelectLabel);

            Elements.ToolbarMinMaxInt(ToolTips.fromZ, ToolTips.toZ, ref AreaManager.Area.z0, ref AreaManager.Area.z1, 0, Land.terrainData.alphamapResolution);
            Elements.ToolbarMinMaxInt(ToolTips.fromX, ToolTips.toX, ref AreaManager.Area.x0, ref AreaManager.Area.x1, 0, Land.terrainData.alphamapResolution);

            if (Elements.ToolbarButton(ToolTips.resetArea))
                AreaManager.Reset();
        }

        public static void RiverTools(LandLayers landLayer, int texture, ref bool aboveTerrain, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.riverToolsLabel);

            if ((int)landLayer > 1)
            {
                Elements.BeginToolbarHorizontal();
                aboveTerrain = Elements.ToolbarToggle(ToolTips.aboveTerrain, aboveTerrain);
                if (Elements.ToolbarButton(ToolTips.paintRivers))
                    MapManager.PaintRiver(landLayer, aboveTerrain, texture, topology);
                if (Elements.ToolbarButton(ToolTips.eraseRivers))
                    MapManager.PaintRiver(landLayer, aboveTerrain, erase, topology);
                Elements.EndToolbarHorizontal();
            }
            else
            {
                Elements.BeginToolbarHorizontal();
                aboveTerrain = Elements.ToolbarToggle(ToolTips.aboveTerrain, aboveTerrain);
                if (Elements.ToolbarButton(ToolTips.paintRivers))
                    MapManager.PaintRiver(landLayer, aboveTerrain, texture);
                Elements.EndToolbarHorizontal();
            }
        }

        public static void RotateTools(LandLayers landLayer, int topology = 0)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.rotate90))
                MapManager.RotateLayer(landLayer, true, topology);
            if (Elements.ToolbarButton(ToolTips.rotate270))
                MapManager.RotateLayer(landLayer, false, topology);
            Elements.EndToolbarHorizontal();
        }

        public static void LayerTools(LandLayers landLayer, int texture, int erase = 0, int topology = 0)
        {
            Elements.MiniBoldLabel(ToolTips.layerToolsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.paintLayer))
                MapManager.PaintLayer(landLayer, texture, topology);
            if ((int)landLayer > 1)
            {
                if (Elements.ToolbarButton(ToolTips.clearLayer))
                    MapManager.ClearLayer(landLayer, topology);
                if (Elements.ToolbarButton(ToolTips.invertLayer))
                    MapManager.InvertLayer(landLayer, topology);
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
                target.SnapToGround();
            Elements.EndToolbarHorizontal();
        }

        public static void ToggleLights(PrefabDataHolder target)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.toggleLights))
                target.ToggleLights();
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
                MapManager.RefreshPresetsList();
                MapManager.nodePresetLookup.TryGetValue(itemValue, out UnityEngine.Object preset);
                if (preset != null)
                    AssetDatabase.OpenAsset(preset.GetInstanceID());
                else
                    Debug.LogError("The preset you are trying to open is null.");
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

        public static void ReloadTreeViews()
        {
            PrefabHierachyWindow.ReloadTree();
            PathHierachyWindow.ReloadTree();
        }
        #endregion

        #region NodeGraph
        public static void NodeGraphToolbar(XNode.NodeGraph nodeGraph)
        {
            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.runPreset))
                NodeAsset.Parse(nodeGraph);
            if (Elements.ToolbarButton(ToolTips.deletePreset))
            {
                if (EditorUtility.DisplayDialog("Delete Preset", "Are you sure you wish to delete this preset? Once deleted it can't be undone.", "Ok", "Cancel"))
                {
                    XNodeEditor.NodeEditorWindow.focusedWindow.Close();
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(nodeGraph));
                    MapManager.RefreshPresetsList();
                }
            }
            if (Elements.ToolbarButton(ToolTips.renamePreset))
                XNodeEditor.RenamePreset.Show(nodeGraph);
            if (Elements.ToolbarButton(ToolTips.presetWiki))
                Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/wiki/Node-System");
            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region TreeViews
        public static void DisplayPrefabName(string name)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabName);
            Elements.ToolbarLabel(new GUIContent(name, name));
            Elements.EndToolbarHorizontal();
        }

        public static void DisplayPrefabID(WorldSerialization.PrefabData prefab)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabID);
            Elements.ToolbarLabel(new GUIContent(prefab.id.ToString(), prefab.id.ToString()));
            Elements.EndToolbarHorizontal();
        }

        public static void DisplayPrefabPath(WorldSerialization.PrefabData prefab)
        {
            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.prefabPath);
            Elements.ToolbarLabel(new GUIContent(AssetManager.ToPath(prefab.id), AssetManager.ToPath(prefab.id)));
            Elements.EndToolbarHorizontal();
        }

        public static void SelectPrefabPaths(PrefabsListTreeView treeView, ref bool showAllPrefabs)
        {
            Elements.MiniBoldLabel(ToolTips.optionsLabel);

            Elements.BeginToolbarHorizontal();
            showAllPrefabs = Elements.ToolbarToggle(ToolTips.showAllPrefabs, showAllPrefabs);
            if (Elements.ToolbarButton(ToolTips.treeViewRefresh))
                treeView.RefreshTreeView(showAllPrefabs);
            Elements.EndToolbarHorizontal();
        }

        public static void HierachyOptions(PrefabDataHolder[] prefabs)
        {
            Elements.MiniBoldLabel(ToolTips.hierachyOptionsLabel);

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.hierachyDelete))
                PrefabManager.DeletePrefabs(prefabs);

            Elements.EndToolbarHorizontal();
        }
        #endregion

        #region CreateNewMap
        public static void NewMapOptions(ref int mapSize, ref float landHeight, ref Layers layers, CreateMapWindow window) 
        {
            mapSize = Elements.ToolbarIntSlider(ToolTips.mapSize, mapSize, 1000, 6000);
            landHeight = Elements.ToolbarSlider(ToolTips.newMapHeight, landHeight, 0, 1000);

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.newMapGround);
            layers.Ground = (TerrainSplat.Enum)Elements.ToolbarEnumPopup(layers.Ground);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            Elements.ToolbarLabel(ToolTips.newMapBiome);
            layers.Biome = (TerrainBiome.Enum)Elements.ToolbarEnumPopup(layers.Biome);
            Elements.EndToolbarHorizontal();

            Elements.BeginToolbarHorizontal();
            if (Elements.ToolbarButton(ToolTips.createMap))
            {
                window.Close();
                int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Close", "Save and Create New Map");
                switch (newMap)
                {
                    case 1:
                        return;
                    case 2:
                        SaveMapPanel();
                        break;
                }
                MapManager.CreateMap(mapSize, TerrainSplat.TypeToIndex((int)layers.Ground), TerrainBiome.TypeToIndex((int)layers.Biome), landHeight);
            }
            if (Elements.ToolbarButton(ToolTips.cancel))
                window.Close();
            Elements.EndToolbarHorizontal();
        }
        #endregion
    }
}