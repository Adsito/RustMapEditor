using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Rotorz.ReorderableList;

public class MapIOEditor : EditorWindow
{
    string editorVersion = "v2.0-prerelease";
    string[] landLayers = { "Ground", "Biome", "Alpha", "Topology" };
    string loadFile = "", saveFile = "", mapName = "", prefabSaveFile = "", mapPrefabSaveFile = "";
    int mapSize = 1000, mainMenuOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0, advancedOptions = 0, layerIndex = 0;
    float heightToSet = 450f, offset = 0f;
    bool[] sides = new bool[4]; 
    bool checkHeight = true, setWaterMap = false;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float slopeMinBlendLow = 25f, slopeMaxBlendLow = 40f, slopeMinBlendHigh = 60f, slopeMaxBlendHigh = 75f;
    float heightMinBlendLow = 0f, heightMaxBlendLow = 500f, heightMinBlendHigh = 500f, heightMaxBlendHigh = 1000f;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;
    int layerConditionalInt, texture = 0, topologyTexture = 0, alphaTexture;
    bool deletePrefabs = false;
    bool checkHeightCndtl = false, checkSlopeCndtl = false, checkAlpha = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f;
    float heightLowCndtl = 500f, heightHighCndtl = 600f;
    bool autoUpdate = false;
    Vector2 scrollPos = new Vector2(0, 0), presetScrollPos = new Vector2(0, 0);
    EditorSelections.ObjectSeletion rotateSelection;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f;
    float blurDirection = 0f, filterStrength = 1f;

    int[] values = { 0, 1 };
    string[] activeTextureAlpha = { "Visible", "Invisible" };
    string[] activeTextureTopo = { "Active", "Inactive" };

    [MenuItem("Rust Map Editor/Main Menu", false, 0)]
    static void Initialize()
    {
        MapIOEditor window = (MapIOEditor)EditorWindow.GetWindow(typeof(MapIOEditor), false, "Rust Map Editor");
    }
    public void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIContent[] mainMenu = new GUIContent[4];
        mainMenu[0] = new GUIContent("File");
        mainMenu[1] = new GUIContent("Prefabs");
        mainMenu[2] = new GUIContent("Layers");
        mainMenu[3] = new GUIContent("Advanced");
        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu);

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
                GUIContent[] prefabsOptionsMenu = new GUIContent[3];
                prefabsOptionsMenu[0] = new GUIContent("Asset Bundle");
                prefabsOptionsMenu[1] = new GUIContent("Prefab Tools");
                prefabsOptionsMenu[2] = new GUIContent("Spawn Prefabs");
                prefabOptions = GUILayout.Toolbar(prefabOptions, prefabsOptionsMenu);

                switch (prefabOptions)
                {
                    case 0:
                        EditorGUILayout.LabelField("THIS IS AN EARLY PREVIEW. Currently waiting for SDK updates.", EditorStyles.boldLabel);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Load", "Loads all the prefabs from the Rust Asset Bundle for use in the editor. Prefabs paths to be loaded can be changed in " +
                            "AssetList.txt in the root directory"), GUILayout.MaxWidth(100)))
                        {
                            MapIO.StartPrefabLookup();
                        }
                        if (GUILayout.Button(new GUIContent("Unload", "Unloads all the prefabs from the Rust Asset Bundle."), GUILayout.MaxWidth(100)))
                        {
                            if (MapIO.GetPrefabLookUp() != null)
                            {
                                MapIO.GetPrefabLookUp().Dispose();
                                MapIO.SetPrefabLookup(null);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("ERROR: Can't unload prefabs", "No prefabs loaded.", "Ok");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                    case 2:
                        if (GUILayout.Button(new GUIContent("Prefab List", "Opens a window to drag and drop prefabs onto the map."), GUILayout.MaxWidth(125)))
                        {
                            PrefabHierachyEditor.ShowWindow();
                        }
                        break;
                    case 1:
                        if (GUILayout.Button(new GUIContent("Remove Broken Prefabs", "Removes any prefabs known to prevent maps from loading. Use this is you are having" +
                                    " errors loading a map on a server.")))
                        {
                            MapIO.RemoveBrokenPrefabs();
                        }
                        deletePrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Delete on Export.", "Deletes the prefabs after exporting them."), deletePrefabs, GUILayout.MaxWidth(300));
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Export LootCrates", "Exports all lootcrates that don't yet respawn in Rust to a JSON for use with the LootCrateRespawn plugin." +
                            "If you don't delete them after export they will duplicate on first map load.")))
                        {
                            prefabSaveFile = EditorUtility.SaveFilePanel("Export LootCrates", prefabSaveFile, "LootCrateData", "json");
                            if (prefabSaveFile == "")
                            {
                                return;
                            }
                            MapIO.ExportLootCrates(prefabSaveFile, deletePrefabs);
                        }
                        if (GUILayout.Button(new GUIContent("Export Map Prefabs", "Exports all map prefabs to plugin data.")))
                        {
                            mapPrefabSaveFile = EditorUtility.SaveFilePanel("Export Map Prefabs", prefabSaveFile, "MapData", "json");
                            if (mapPrefabSaveFile == "")
                            {
                                return;
                            }
                            MapIO.ExportMapPrefabs(mapPrefabSaveFile, deletePrefabs);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Hide Prefabs in RustEdit", "Changes all the prefab categories to a semi-colon. Hides all of the prefabs from " +
                            "appearing in RustEdit. Use the break RustEdit Custom Prefabs button to undo.")))
                        {
                            MapIO.HidePrefabsInRustEdit();
                        }
                        if (GUILayout.Button(new GUIContent("Break RustEdit Custom Prefabs", "Breaks down all custom prefabs saved in the map file.")))
                        {
                            MapIO.BreakRustEditCustomPrefabs();
                        }
                        EditorGUILayout.EndHorizontal();
                        if (GUILayout.Button(new GUIContent("Group RustEdit Custom Prefabs", "Groups all custom prefabs saved in the map file.")))
                        {
                            MapIO.GroupRustEditCustomPrefabs();
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Delete All Map Prefabs", "Removes all the prefabs from the map.")))
                        {
                            MapIO.RemoveMapObjects(true, false);
                        }
                        if (GUILayout.Button(new GUIContent("Delete All Map Paths", "Removes all the paths from the map.")))
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
                layerIndex = GUILayout.Toolbar(layerIndex, layersOptionsMenu);

                ClampValues();
                SetLandLayer(layerIndex);

                switch (layerIndex)
                {
                    #region Ground Layer
                    case 0:
                        MapIO.groundLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint: ", MapIO.groundLayer);
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
                        MapIO.biomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Biome To Paint:", MapIO.biomeLayer);
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
                        GUILayout.Label("Green = Terrain Visible \nPurple = Terrain Invisible", EditorStyles.boldLabel);
                        PaintTools(layerIndex, 1, 0);
                        RotateTools(layerIndex);
                        SlopeTools(layerIndex, 1, 0);
                        HeightTools(layerIndex, 1, 0);
                        AreaTools(layerIndex, 1, 0);
                        break;
                    #endregion
                    #region Topology Layer
                    case 3:
                        GUILayout.Label("Green = Active \nPurple = Inactive", EditorStyles.boldLabel);
                        MapIO.oldTopologyLayer = MapIO.topologyLayer;
                        MapIO.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology Layer:", MapIO.topologyLayer);
                        if (MapIO.topologyLayer != MapIO.oldTopologyLayer)
                        {
                            MapIO.ChangeLandLayer();
                            Repaint();
                        }
                        PaintTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)MapIO.topologyLayer));
                        RotateTools(layerIndex, TerrainTopology.TypeToIndex((int)MapIO.topologyLayer));
                        TopologyTools();
                        RiverTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)MapIO.topologyLayer));
                        SlopeTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)MapIO.topologyLayer));
                        HeightTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)MapIO.topologyLayer));
                        AreaTools(layerIndex, 0, 1, TerrainTopology.TypeToIndex((int)MapIO.topologyLayer));
                        break;
                    #endregion
                }
                break;
            #endregion
            #region Advanced
            case 3:
                GUIContent[] advancedOptionsMenu = new GUIContent[3];
                advancedOptionsMenu[0] = new GUIContent("Generation");
                advancedOptionsMenu[1] = new GUIContent("Map Tools");
                advancedOptionsMenu[2] = new GUIContent("");
                advancedOptions = GUILayout.Toolbar(advancedOptions, advancedOptionsMenu);
                
                switch (advancedOptions)
                {
                    #region Generation
                    case 0:
                        GUILayout.Label(new GUIContent("Node Presets", "List of all the node presets in the project."), EditorStyles.boldLabel);
                        if (GUILayout.Button(new GUIContent("Refresh presets list.", "Refreshes the list of all the Node Presets in the project.")))
                        {
                            MapIO.RefreshAssetList();
                        }
                        presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
                        ReorderableListGUI.Title("Node Presets");
                        ReorderableListGUI.ListField(MapIO.generationPresetList, NodePresetDrawer, DrawEmpty);
                        GUILayout.EndScrollView();
                        break;
                    #endregion
                    case 1:
                        GUIContent[] mapToolsMenu = new GUIContent[3];
                        mapToolsMenu[0] = new GUIContent("HeightMap");
                        mapToolsMenu[1] = new GUIContent("Textures");
                        mapToolsMenu[2] = new GUIContent("Misc");
                        mapToolsOptions = GUILayout.Toolbar(mapToolsOptions, mapToolsMenu);

                        switch (mapToolsOptions)
                        {
                            #region HeightMap
                            case 0:
                                GUIContent[] heightMapMenu = new GUIContent[2];
                                heightMapMenu[0] = new GUIContent("Heights");
                                heightMapMenu[1] = new GUIContent("Filters");
                                heightMapOptions = GUILayout.Toolbar(heightMapOptions, heightMapMenu);

                                switch (heightMapOptions)
                                {
                                    case 0:
                                        GUILayout.Label("Heightmap Offset (Increase or Decrease)", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        offset = EditorGUILayout.FloatField(offset, GUILayout.MaxWidth(40));
                                        checkHeight = EditorGUILayout.ToggleLeft(new GUIContent("Prevent Flattening", "Prevents the flattening effect if you raise or lower the heightmap" +
                                            " by too large a value."), checkHeight, GUILayout.MaxWidth(125));
                                        setWaterMap = EditorGUILayout.ToggleLeft(new GUIContent("Water Heightmap", "If toggled it will raise or lower the water heightmap as well as the " +
                                            "land heightmap."), setWaterMap, GUILayout.MaxWidth(125));
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Offset Heightmap", "Raises or lowers the height of the entire heightmap by " + offset.ToString() + " metres. " +
                                            "A positive offset will raise the heightmap, a negative offset will lower the heightmap.")))
                                        {
                                            MapIO.OffsetHeightmap(offset, checkHeight, setWaterMap);
                                        }
                                        GUILayout.Label("Heightmap Minimum/Maximum Height", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        heightToSet = EditorGUILayout.FloatField(heightToSet, GUILayout.MaxWidth(40));
                                        if (GUILayout.Button(new GUIContent("Set Minimum Height", "Raises any of the land below " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
                                            " metres."), GUILayout.MaxWidth(130)))
                                        {
                                            MapIO.SetMinimumHeight(heightToSet);
                                        }
                                        if (GUILayout.Button(new GUIContent("Set Maximum Height", "Lowers any of the land above " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
                                            " metres."), GUILayout.MaxWidth(130)))
                                        {
                                            MapIO.SetMaximumHeight(heightToSet);
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Label("Edge of Map Height", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        sides[0] = EditorGUILayout.ToggleLeft("Top ", sides[0], GUILayout.MaxWidth(60));
                                        sides[3] = EditorGUILayout.ToggleLeft("Left ", sides[3], GUILayout.MaxWidth(60));
                                        sides[2] = EditorGUILayout.ToggleLeft("Bottom ", sides[2], GUILayout.MaxWidth(60));
                                        sides[1] = EditorGUILayout.ToggleLeft("Right ", sides[1], GUILayout.MaxWidth(60));
                                        EditorGUILayout.EndHorizontal();

                                        if (GUILayout.Button(new GUIContent("Set Edge Height", "Sets the very edge of the map to " + heightToSet.ToString() + " metres on any of the sides selected.")))
                                        {
                                            MapIO.SetEdgePixel(heightToSet, sides);
                                        }

                                        break;
                                    case 1:
                                        GUILayout.Label("Flip, Invert and Scale", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        //EditorGUILayout.LabelField("Scale", GUILayout.MaxWidth(60));
                                        //mapScale = EditorGUILayout.Slider(mapScale, 0.01f, 10f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        /*
                                        if (GUILayout.Button(new GUIContent("Rescale", "Scales the heightmap by " + mapScale.ToString() + " %.")))
                                        {
                                            script.scaleHeightmap(mapScale);
                                        }*/
                                        if (GUILayout.Button(new GUIContent("Invert", "Inverts the heightmap in on itself.")))
                                        {
                                            MapIO.InvertHeightmap();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        GUILayout.Label(new GUIContent("Normalise", "Moves the heightmap heights to between the two heights."), EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Low", "The lowest point on the map after being normalised."), GUILayout.MaxWidth(40));
                                        EditorGUI.BeginChangeCheck();
                                        normaliseLow = EditorGUILayout.Slider(normaliseLow, 0f, 1000f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("High", "The highest point on the map after being normalised."), GUILayout.MaxWidth(40));
                                        normaliseHigh = EditorGUILayout.Slider(normaliseHigh, 0f, 1000f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        if (EditorGUI.EndChangeCheck() && autoUpdate == true)
                                        {
                                            MapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button(new GUIContent("Normalise", "Normalises the heightmap between these heights.")))
                                        {
                                            MapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f);
                                        }
                                        autoUpdate = EditorGUILayout.ToggleLeft(new GUIContent("Auto Update", "Automatically applies the changes to the heightmap on value change."), autoUpdate);
                                        EditorGUILayout.EndHorizontal();
                                        GUILayout.Label(new GUIContent("Smooth", "Smooth the entire terrain."), EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Strength", "The strength of the smoothing operation."), GUILayout.MaxWidth(85));
                                        filterStrength = EditorGUILayout.Slider(filterStrength, 0f, 1f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Blur Direction", "The direction the terrain should blur towards. Negative is down, " +
                                            "positive is up."), GUILayout.MaxWidth(85));
                                        blurDirection = EditorGUILayout.Slider(blurDirection, -1f, 1f);
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Smooth Map", "Smoothes the heightmap.")))
                                        {
                                            MapIO.SmoothHeightmap(filterStrength, blurDirection);
                                        }
                                        GUILayout.Label(new GUIContent("Terrace", "Terrace the entire terrain."), EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Feature Size", "The higher the value the more terrace levels generated."), GUILayout.MaxWidth(85));
                                        terraceErodeFeatureSize = EditorGUILayout.Slider(terraceErodeFeatureSize, 2f, 1000f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Corner Weight", "The strength of the corners of the terrace."), GUILayout.MaxWidth(85));
                                        terraceErodeInteriorCornerWeight = EditorGUILayout.Slider(terraceErodeInteriorCornerWeight, 0f, 1f);
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Terrace Map", "Terraces the heightmap.")))
                                        {
                                            MapIO.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
                                        }
                                        break;
                                }
                                break;
                            #endregion
                            #region Textures
                            case 1:
                                GUILayout.Label("Copy Textures", EditorStyles.boldLabel);
                                landLayerFrom = EditorGUILayout.Popup("Layer:", landLayerFrom, landLayers);
                                switch (landLayerFrom) // Get texture list from the currently selected landLayer.
                                {
                                    case 0:
                                        MapIO.groundLayerFrom = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Copy:", MapIO.groundLayerFrom);
                                        textureFrom = TerrainSplat.TypeToIndex((int)MapIO.groundLayerFrom);
                                        break;
                                    case 1:
                                        MapIO.biomeLayerFrom = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Copy:", MapIO.biomeLayerFrom);
                                        textureFrom = TerrainBiome.TypeToIndex((int)MapIO.biomeLayerFrom);
                                        break;
                                    case 2:
                                        textureFrom = EditorGUILayout.IntPopup("Texture To Copy:", textureFrom, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        MapIO.topologyLayerFrom = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology To Copy:", MapIO.topologyLayerFrom);
                                        textureFrom = EditorGUILayout.IntPopup("Texture To Copy:", textureFrom, activeTextureTopo, values);
                                        break;
                                }
                                landLayerToPaint = EditorGUILayout.Popup("Layer:", landLayerToPaint, landLayers);
                                switch (landLayerToPaint) // Get texture list from the currently selected landLayer.
                                {
                                    case 0:
                                        MapIO.groundLayerToPaint = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.groundLayerToPaint);
                                        textureToPaint = TerrainSplat.TypeToIndex((int)MapIO.groundLayerToPaint);
                                        break;
                                    case 1:
                                        MapIO.biomeLayerToPaint = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.biomeLayerToPaint);
                                        textureToPaint = TerrainBiome.TypeToIndex((int)MapIO.biomeLayerToPaint);
                                        break;
                                    case 2:
                                        textureToPaint = EditorGUILayout.IntPopup("Texture To Paint:", textureToPaint, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        MapIO.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology To Paint:", MapIO.topologyLayerToPaint);
                                        textureToPaint = EditorGUILayout.IntPopup("Texture To Paint:", textureToPaint, activeTextureTopo, values);
                                        break;
                                }
                                if (GUILayout.Button(new GUIContent("Copy textures to new layer", "Copies the Texture from the " + landLayers[landLayerFrom] + " layer and " +
                                    "paints it on the " + landLayers[landLayerToPaint] + " layer.")))
                                {
                                    MapIO.CopyTexture(landLayers[landLayerFrom], landLayers[landLayerToPaint], textureFrom, textureToPaint, TerrainTopology.TypeToIndex((int)MapIO.topologyLayerFrom), TerrainTopology.TypeToIndex((int)MapIO.topologyLayerToPaint));
                                }
                                GUILayout.Label("Conditional Paint", EditorStyles.boldLabel);

                                GUIContent[] conditionalPaintMenu = new GUIContent[5];
                                conditionalPaintMenu[0] = new GUIContent("Ground");
                                conditionalPaintMenu[1] = new GUIContent("Biome");
                                conditionalPaintMenu[2] = new GUIContent("Alpha");
                                conditionalPaintMenu[3] = new GUIContent("Topology");
                                conditionalPaintMenu[4] = new GUIContent("Terrain");
                                conditionalPaintOptions = GUILayout.Toolbar(conditionalPaintOptions, conditionalPaintMenu);

                                switch (conditionalPaintOptions)
                                {
                                    case 0: // Ground
                                        GUILayout.Label("Ground Texture", EditorStyles.boldLabel);
                                        MapIO.conditionalGround = (TerrainSplat.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalGround);
                                        break;
                                    case 1: // Biome
                                        GUILayout.Label("Biome Texture", EditorStyles.boldLabel);
                                        MapIO.conditionalBiome = (TerrainBiome.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalBiome);
                                        break;
                                    case 2: // Alpha
                                        checkAlpha = EditorGUILayout.Toggle("Check Alpha:", checkAlpha);
                                        if (checkAlpha)
                                        {
                                            alphaTexture = EditorGUILayout.IntPopup("Alpha Texture:", alphaTexture, activeTextureAlpha, values);
                                        }
                                        break;
                                    case 3: // Topology
                                        GUILayout.Label("Topology Layer", EditorStyles.boldLabel);
                                        MapIO.conditionalTopology = (TerrainTopology.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalTopology);
                                        EditorGUILayout.Space();
                                        GUILayout.Label("Topology Texture", EditorStyles.boldLabel);
                                        topologyTexture = EditorGUILayout.IntPopup("Topology Texture:", topologyTexture, activeTextureTopo, values);
                                        break;
                                    case 4: // Terrain
                                        GUILayout.Label("Slope Range", EditorStyles.boldLabel);
                                        checkSlopeCndtl = EditorGUILayout.Toggle("Check Slopes:", checkSlopeCndtl);
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
                                            EditorGUILayout.BeginHorizontal();
                                            slopeLowCndtl = EditorGUILayout.FloatField(slopeLowCndtl);
                                            slopeHighCndtl = EditorGUILayout.FloatField(slopeHighCndtl);
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.MinMaxSlider(ref slopeLowCndtl, ref slopeHighCndtl, 0f, 90f);
                                        }
                                        GUILayout.Label("Height Range", EditorStyles.boldLabel);
                                        checkHeightCndtl = EditorGUILayout.Toggle("Check Heights:", checkHeightCndtl);
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
                                            EditorGUILayout.BeginHorizontal();
                                            heightLowCndtl = EditorGUILayout.FloatField(heightLowCndtl);
                                            heightHighCndtl = EditorGUILayout.FloatField(heightHighCndtl);
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.MinMaxSlider(ref heightLowCndtl, ref heightHighCndtl, 0f, 1000f);
                                        }
                                        break;
                                }
                                GUILayout.Label("Texture To Paint:", EditorStyles.boldLabel);
                                layerConditionalInt = EditorGUILayout.Popup("Layer:", layerConditionalInt, landLayers);
                                switch (layerConditionalInt)
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        MapIO.groundLayerToPaint = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.groundLayerToPaint);
                                        texture = TerrainSplat.TypeToIndex((int)MapIO.groundLayerToPaint);
                                        break;
                                    case 1:
                                        MapIO.biomeLayerToPaint = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.biomeLayerToPaint);
                                        texture = TerrainBiome.TypeToIndex((int)MapIO.biomeLayerToPaint);
                                        break;
                                    case 2:
                                        texture = EditorGUILayout.IntPopup("Texture To Paint:", texture, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        MapIO.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology Layer:", MapIO.topologyLayerToPaint);
                                        texture = EditorGUILayout.IntPopup("Texture To Paint:", texture, activeTextureTopo, values);
                                        break;
                                }
                                if (GUILayout.Button(new GUIContent("Paint Conditional", "Paints the selected texture if it matches all of the conditions set.")))
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
                                break;
                            #endregion
                            #region Misc
                            case 2:
                                GUILayout.Label("Rotate Map", EditorStyles.boldLabel);
                                rotateSelection = (EditorSelections.ObjectSeletion)EditorGUILayout.EnumFlagsField(new GUIContent("Rotation Selection: ", "The items to rotate."), rotateSelection);

                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button("Rotate 90°", GUILayout.MaxWidth(90)))
                                {
                                    MapIO.ParseRotateEnumFlags(rotateSelection, true);
                                }
                                if (GUILayout.Button("Rotate 270°", GUILayout.MaxWidth(90)))
                                {
                                    MapIO.ParseRotateEnumFlags(rotateSelection, false);
                                }
                                EditorGUILayout.EndHorizontal();

                                GUILayout.Label("Debug", EditorStyles.boldLabel);
                                if (GUILayout.Button(new GUIContent("Debug Water", "Raises the water heightmap to 500 metres if it is below.")))
                                {
                                    MapIO.DebugWaterLevel();
                                }
                                break;
                                #endregion
                        }
                        break;
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
    #region OtherMenus
    [MenuItem("Rust Map Editor/Terrain Tools", false, 1)]
    static void OpenTerrainTools()
    {
        Selection.activeGameObject = GameObject.FindGameObjectWithTag("Land");
    }
    [MenuItem("Rust Map Editor/Wiki", false, 2)]
    static void OpenWiki()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/wiki");
    }
    [MenuItem("Rust Map Editor/Discord", false, 3)]
    static void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/HPmTWVa");
    }
    #endregion
    #region Methods
    private string NodePresetDrawer(Rect position, string itemValue)
    {
        position.width -= 39;
        GUI.Label(position, itemValue);
        position.x = position.xMax;
        position.width = 39;
        if (GUI.Button(position, new GUIContent("Open", "Opens the Node Editor for the preset.")))
        {
            MapIO.RefreshAssetList();
            MapIO.nodePresetLookup.TryGetValue(itemValue, out Object preset);
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
    /// <param name="index">The landLayer to change to.</param>
    private void SetLandLayer(int index)
    {
        MapIO.landSelectIndex = index;
        string oldLandLayer = MapIO.landLayer;
        MapIO.landLayer = landLayers[MapIO.landSelectIndex];
        if (MapIO.landLayer != oldLandLayer)
        {
            MapIO.ChangeLandLayer();
            Repaint();
        }
    }
    /// <summary>
    /// Clamps all the Height and Slope tool values.
    /// </summary>
    private void ClampValues()
    {
        slopeLow = Mathf.Clamp(slopeLow, 0f, 89.99f);
        slopeMinBlendLow = Mathf.Clamp(slopeMinBlendLow, 0f, slopeLow);
        slopeMinBlendHigh = Mathf.Clamp(slopeMinBlendHigh, slopeMinBlendLow, slopeLow);
        slopeHigh = Mathf.Clamp(slopeHigh, 0.01f, 90f);
        slopeMaxBlendHigh = Mathf.Clamp(slopeMaxBlendHigh, slopeHigh, 90f);
        if (slopeLow > slopeHigh)
        {
            slopeLow = slopeHigh - 0.01f;
        }
        slopeMaxBlendLow = slopeLow;
        slopeMinBlendHigh = slopeHigh;
        if (blendSlopes == false)
        {
            slopeMaxBlendHigh = slopeHigh;
            slopeMinBlendLow = slopeLow;
        }
        heightLow = Mathf.Clamp(heightLow, 0f, 999.99f);
        heightMinBlendLow = Mathf.Clamp(heightMinBlendLow, 0f, heightLow);
        heightMinBlendHigh = Mathf.Clamp(heightMinBlendHigh, heightMinBlendLow, heightLow);
        heightHigh = Mathf.Clamp(heightHigh, 0.01f, 1000f);
        heightMaxBlendHigh = Mathf.Clamp(heightMaxBlendHigh, heightHigh, 1000f);
        if (heightLow > heightHigh)
        {
            heightLow = heightHigh - 0.01f;
        }
        heightMaxBlendLow = heightLow;
        heightMinBlendHigh = heightHigh;
        if (blendHeights == false)
        {
            heightMaxBlendHigh = heightHigh;
            heightMinBlendLow = heightLow;
        }
    }
    private void SlopeTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
        if (index < 2)
        {
            blendSlopes = EditorGUILayout.ToggleLeft("Toggle Blend Slopes", blendSlopes);
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
        GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
        if (blendSlopes == true)
        {
            GUILayout.Label("Blend Low: " + slopeMinBlendLow + "°");
            EditorGUILayout.MinMaxSlider(ref slopeMinBlendLow, ref slopeMaxBlendLow, 0f, 90f);
            GUILayout.Label("Blend High: " + slopeMaxBlendHigh + "°");
            EditorGUILayout.MinMaxSlider(ref slopeMinBlendHigh, ref slopeMaxBlendHigh, 0f, 90f);
        }
        if (index > 1) // Alpha and Topology
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the terrain on the " + MapIO.landLayer + " layer within the slope range.")))
            {
                MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, slopeLow, slopeHigh, texture, topology);
            }
            if (GUILayout.Button("Erase Slopes"))
            {
                MapIO.PaintSlope(landLayers[index], heightLow, heightHigh, heightLow, heightHigh, erase, topology);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the terrain on the " + MapIO.landLayer + " layer within the slope range.")))
            {
                MapIO.PaintSlope(landLayers[index], slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, texture);
            }
        }
    }
    private void HeightTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Height Tools", EditorStyles.boldLabel);
        if (index < 2)
        {
            blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
        GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
        if (blendHeights == true)
        {
            GUILayout.Label("Blend Low: " + heightMinBlendLow + "m");
            EditorGUILayout.MinMaxSlider(ref heightMinBlendLow, ref heightMaxBlendLow, 0f, 1000f);
            GUILayout.Label("Blend High: " + heightMaxBlendHigh + "m");
            EditorGUILayout.MinMaxSlider(ref heightMinBlendHigh, ref heightMaxBlendHigh, 0f, 1000f);
        }
        if (index > 1) // Alpha and Topology
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Paint Heights", "Paints the terrain on the " + MapIO.landLayer + " layer within the height range.")))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, heightLow, heightHigh, texture, topology);
            }
            if (GUILayout.Button("Erase Heights"))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, heightLow, heightHigh, erase, topology);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button(new GUIContent("Paint Heights", "Paints the terrain on the " + MapIO.landLayer + " layer within the height range.")))
            {
                MapIO.PaintHeight(landLayers[index], heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, texture);
            }
        }
    }
    private void RotateTools(int index, int topology = 0)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate the " + landLayers[index] + " layer 90°.")))
        {
            MapIO.RotateLayer(landLayers[index], true);
        }
        if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate the " + landLayers[index] + " layer 270°.")))
        {
            MapIO.RotateLayer(landLayers[index], false);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void TopologyTools()
    {
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Rotate All 90°", "Rotate all Topology layers 90°")))
        {
            MapIO.RotateAllTopologyLayers(true);
        }
        if (GUILayout.Button(new GUIContent("Rotate All 270°", "Rotate all Topology layers 270°")))
        {
            MapIO.RotateAllTopologyLayers(false);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Invert All", "Invert all Topology layers.")))
        {
            MapIO.InvertAllTopologyLayers();
        }
        if (GUILayout.Button(new GUIContent("Clear All", "Clear all Topology layers.")))
        {
            MapIO.ClearAllTopologyLayers();
        }
        EditorGUILayout.EndHorizontal();
    }
    private void AreaTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Area Tools", EditorStyles.boldLabel);
        z1 = Mathf.Clamp(EditorGUILayout.IntField("From Z ", z1), 0, z2);
        z2 = Mathf.Clamp(EditorGUILayout.IntField("To Z ", z2), z1, MapIO.terrain.terrainData.alphamapResolution);
        x1 = Mathf.Clamp(EditorGUILayout.IntField("From X ", x1), 0, x2);
        x2 = Mathf.Clamp(EditorGUILayout.IntField("To X ", x2), x1, MapIO.terrain.terrainData.alphamapResolution);
        if (index > 1) // Alpha and Topology
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Area"))
            {
                MapIO.PaintArea(landLayers[index], z1, z2, x1, x2, texture, topology);
            }
            if (GUILayout.Button("Erase Area"))
            {
                MapIO.PaintArea(landLayers[index], z1, z2, x1, x2, erase, topology);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Paint Area"))
            {
                MapIO.PaintArea(landLayers[index], z1, z2, x1, x2, texture);
            }
        }
    }
    private void RiverTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("River Tools", EditorStyles.boldLabel);
        aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
        if (index > 1)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Rivers"))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, texture, topology);
            }
            if (GUILayout.Button("Erase Rivers"))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, erase, topology);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Paint Rivers"))
            {
                MapIO.PaintRiver(landLayers[index], aboveTerrain, texture);
            }
        }
    }
    private void PaintTools(int index, int texture, int erase = 0, int topology = 0)
    {
        GUILayout.Label("Layer Tools", EditorStyles.boldLabel);
        if (index > 1)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Layer"))
            {
                MapIO.PaintLayer(landLayers[index], texture, topology);
            }
            if (GUILayout.Button("Clear Layer"))
            {
                MapIO.PaintLayer(landLayers[index], erase, topology);
            }
            if (GUILayout.Button("Invert Layer"))
            {
                MapIO.InvertLayer(landLayers[index], topology);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Paint Layer"))
            {
                MapIO.PaintLayer(landLayers[index], texture);
            }
        }
    }
    private void EditorIO()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Load", "Opens a file viewer to find and open a Rust .map file."), EditorStyles.toolbarButton, GUILayout.MaxWidth(45)))
        {
            loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
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
        if (GUILayout.Button(new GUIContent("Save", "Opens a file viewer to find and save a Rust .map file."), EditorStyles.toolbarButton, GUILayout.MaxWidth(45)))
        {
            saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
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
        if (GUILayout.Button(new GUIContent("New", "Creates a new map " + mapSize.ToString() + " metres in size."), EditorStyles.toolbarButton, GUILayout.MaxWidth(45)))
        {
            int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Exit", "Save and Create New Map");
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
                    saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
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
        GUILayout.Label(new GUIContent("Size", "The size of the Rust Map to create upon new map. (1000-6000)"), GUILayout.MaxWidth(30));
        mapSize = Mathf.Clamp(EditorGUILayout.IntField(mapSize, GUILayout.MaxWidth(45)), 1000, 6000);
        EditorGUILayout.EndHorizontal();
    }
    private void MapInfo()
    {
        GUILayout.Label("Map Info", EditorStyles.boldLabel, GUILayout.MaxWidth(75));
        GUILayout.Label("Size: " + MapIO.terrain.terrainData.size.x);
        GUILayout.Label("HeightMap: " + MapIO.terrain.terrainData.heightmapResolution + "x" + MapIO.terrain.terrainData.heightmapResolution);
        GUILayout.Label("SplatMap: " + MapIO.terrain.terrainData.alphamapResolution + "x" + MapIO.terrain.terrainData.alphamapResolution);
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
        if (GUILayout.Button(new GUIContent("Report Bug", "Opens up the editor bug report in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(75)))
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
        }
        if (GUILayout.Button(new GUIContent("Request Feature", "Opens up the editor feature request in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(105)))
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
        }
        if (GUILayout.Button(new GUIContent("RoadMap", "Opens up the editor roadmap in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(65)))
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/projects/1");
        }
        if (GUILayout.Button(new GUIContent("Wiki", "Opens up the editor wiki in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(65)))
        {
            Application.OpenURL("https://github.com/RustMapMaking/Editor/wiki");
        }
        EditorGUILayout.EndHorizontal();
    }
    private void EditorSettings()
    {
        GUILayout.Label(new GUIContent("Settings"), EditorStyles.boldLabel, GUILayout.MaxWidth(60));

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(new GUIContent("Save Changes", "Sets and saves the current settings."), EditorStyles.toolbarButton, GUILayout.MaxWidth(82)))
        {
            MapEditorSettings.SaveSettings();
        }
        if (GUILayout.Button(new GUIContent("Discard", "Discards the changes to the settings."), EditorStyles.toolbarButton, GUILayout.MaxWidth(82)))
        {
            MapEditorSettings.LoadSettings();
        }
        if (GUILayout.Button(new GUIContent("Default", "Sets the settings back to the default."), EditorStyles.toolbarButton, GUILayout.MaxWidth(82)))
        {
            MapEditorSettings.SetDefaultSettings();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label(new GUIContent("Rust Directory", @"The base install directory of Rust. Normally located at steamapps\common\Rust"), GUILayout.MaxWidth(95));
        MapEditorSettings.rustDirectory = EditorGUILayout.TextArea(MapEditorSettings.rustDirectory);

        GUILayout.Label(new GUIContent("Object Quality", "Controls the object render distance the exact same as ingame. Between 0-200"), GUILayout.MaxWidth(95));
        MapEditorSettings.objectQuality = EditorGUILayout.IntSlider(MapEditorSettings.objectQuality, 0, 200);
    }
    private void DrawEmpty()
    {
        GUILayout.Label("No presets in list.", EditorStyles.miniLabel);
    }
    #endregion
}
public class PrefabHierachyEditor : EditorWindow
{
    [SerializeField] TreeViewState m_TreeViewState;

    PrefabHierachy m_TreeView;
    SearchField m_SearchField;

    void OnEnable()
    {
        if (m_TreeViewState == null)
            m_TreeViewState = new TreeViewState();

        m_TreeView = new PrefabHierachy(m_TreeViewState);
        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
    }
    void OnGUI()
    {
        DoToolbar();
        DoTreeView();
    }
    void DoToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Space(100);
        GUILayout.FlexibleSpace();
        m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
        GUILayout.EndHorizontal();
    }
    void DoTreeView()
    {
        Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
        m_TreeView.OnGUI(rect);
    }
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabHierachyEditor>();
        window.titleContent = new GUIContent("Prefabs");
        window.Show();
    }
}
