﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapIO))]
public class MapIOEditor : Editor
{
    string loadFile = "";

    string saveFile = "";
    string mapName = "";
    string prefabSaveFile = "";
    //Todo: Clean this up. It's coarse and rough and irritating and it gets everywhere.
    int mapSize = 1000, mainMenuOptions = 0, toolsOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0;
    float heightToSet = 450f, scale = 50f, offset = 0f;
    bool top = false, left = false, right = false, bottom = false, checkHeight = true, setWaterMap = false;
    bool allLayers = false, ground = false, biome = false, alpha = false, topology = false, heightmap = false, prefabs = false, paths = false;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float minBlendLow = 25f, maxBlendLow = 40f, minBlendHigh = 60f, maxBlendHigh = 75f, blendStrength = 5f;
    float minBlendLowHeight = 0f, maxBlendHighHeight = 1000f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;
    int layerConditionalInt, texture = 0;
    bool AlphaVisible = false, AlphaInvisible = false;
    bool TopoActive = false, TopoInactive = false;

    bool checkHeightCndtl = false, checkSlopeCndtl = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f;
    float heightLowCndtl = 500f, heightHighCndtl = 600f;

    public TerrainBiome.Enum biomeLayerToPaint;
    public TerrainBiome.Enum biomeLayerConditional;
    public TerrainSplat.Enum groundLayerToPaint;
    public TerrainSplat.Enum groundLayerConditional;

    bool layerSet = false;
    bool[] groundTxtCndtl = new bool[8] { true, true, true, true, true, true, true, true };
    bool[] biomeTxtCndtl = new bool[4] { true, true, true, true };
    bool[] alphaTxtCndtl = new bool[2] { true, true };
    bool[] topoTxtCndtl = new bool[2] { true, true };
    string[] landLayersCndtl = new string[4] { "Ground", "Biome", "Alpha", "Topology" };
    int[] topoLayersCndtl = new int[] { };

    public override void OnInspectorGUI()
    {
        MapIO script = (MapIO)target;

        if (layerSet == false)
        {
            groundLayerToPaint = TerrainSplat.Enum.Grass;
            biomeLayerToPaint = TerrainBiome.Enum.Temperate;
            layerSet = true;
        }

        GUIContent[] mainMenu = new GUIContent[3];
        mainMenu[0] = new GUIContent("Main Menu");
        mainMenu[1] = new GUIContent("Tools");
        mainMenu[2] = new GUIContent("Prefabs");
        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu);

        #region Menu
        switch (mainMenuOptions)
        {
            #region Main Menu
            case 0:
                GUILayout.Label("Map Options", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Load", "Opens a file viewer to find and open a Rust .map file."), GUILayout.MaxWidth(45)))
                {
                    loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");

                    var blob = new WorldSerialization();
                    if (loadFile == "")
                    {
                        return;
                    }
                    EditorUtility.DisplayProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.1f);
                    blob.Load(loadFile);
                    script.loadPath = loadFile;
                    EditorUtility.DisplayProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.2f);
                    script.Load(blob);
                }
                if (GUILayout.Button(new GUIContent("Save", "Opens a file viewer to find and save a Rust .map file."), GUILayout.MaxWidth(45)))
                {
                    saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
                    if (saveFile == "")
                    {
                        return;
                    }
                    Debug.Log("Exported map " + saveFile);
                    script.savePath = saveFile;
                    prefabSaveFile = saveFile;
                    EditorUtility.DisplayProgressBar("Saving Map: " + saveFile, "Saving Heightmap ", 0.1f);
                    script.Save(saveFile);
                }
                if (GUILayout.Button(new GUIContent("New", "Creates a new map " + mapSize.ToString() + " metres in size."), GUILayout.MaxWidth(45)))
                {
                    int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Exit", "Save and Create New Map");
                    if (mapSize < 1000 & mapSize > 6000)
                    {
                        EditorUtility.DisplayDialog("Error", "Map size must be between 1000 - 6000", "Ok");
                        return;
                    }
                    switch (newMap)
                    {
                        case 0:
                            script.loadPath = "New Map";
                            script.newEmptyTerrain(mapSize);
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
                            script.Save(saveFile);
                            script.loadPath = "New Map";
                            script.newEmptyTerrain(mapSize);
                            break;
                        default:
                            Debug.Log("Create New Map option outofbounds");
                            break;
                    }
                }
                GUILayout.Label(new GUIContent("Size", "The size of the Rust Map to create upon new map."), GUILayout.MaxWidth(30));
                mapSize = EditorGUILayout.IntField(mapSize, GUILayout.MaxWidth(45));
                
                EditorGUILayout.EndHorizontal();
                
                
                if (GUILayout.Button(new GUIContent("Export LootCrates", "Exports all lootcrates that don't yet respawn in Rust to a JSON for use with the LootCrateRespawn plugin." +
                    "NOTE: Currently on map load the lootcrates will be spawned twice, due to them not being deleted before saving the map. This will be fixed soon :)")))
                {
                    prefabSaveFile = UnityEditor.EditorUtility.SaveFilePanel("Export LootCrates", prefabSaveFile, "LootCrateData", "json");
                    if (prefabSaveFile == "")
                    {
                        return;
                    }
                    script.exportLootCrates(prefabSaveFile);
                }
            break;
            #endregion
            #region Tools
            case 1:
                GUIContent[] toolsOptionsMenu = new GUIContent[3];
                toolsOptionsMenu[0] = new GUIContent("Map Tools");
                toolsOptionsMenu[1] = new GUIContent("Layer Tools");
                toolsOptionsMenu[2] = new GUIContent("Generation");
                toolsOptions = GUILayout.Toolbar(toolsOptions, toolsOptionsMenu);

                switch (toolsOptions)
                {
                    #region Map Tools
                    case 0:
                        GUIContent[] mapToolsMenu = new GUIContent[3];
                        mapToolsMenu[0] = new GUIContent("Rotate Map");
                        mapToolsMenu[1] = new GUIContent("HeightMap");
                        mapToolsMenu[2] = new GUIContent("Textures");
                        mapToolsOptions = GUILayout.Toolbar(mapToolsOptions, mapToolsMenu);

                        switch (mapToolsOptions)
                        {
                            #region Rotate Map
                            case 0:
                                GUILayout.Label("Layers to Rotate", EditorStyles.boldLabel);
                                EditorGUILayout.BeginHorizontal();
                                allLayers = EditorGUILayout.ToggleLeft("Rotate All", allLayers, GUILayout.MaxWidth(75));

                                if (GUILayout.Button("Rotate 90°", GUILayout.MaxWidth(90))) // Calls every rotate function from MapIO. Rotates 90 degrees.
                                {
                                    if (heightmap == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Heightmap ", 0.05f);
                                        script.rotateHeightmap(true);
                                    }
                                    if (paths == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Paths ", 0.075f);
                                        script.rotatePaths(true);
                                    }
                                    if (prefabs == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Prefabs ", 0.1f);
                                        script.rotatePrefabs(true);
                                    }
                                    if (ground == true)
                                    {
                                        script.changeLayer("Ground");
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ground Textures ", 0.15f);
                                        script.rotateGroundmap(true);
                                    }
                                    if (biome == true)
                                    {
                                        script.changeLayer("Biome");
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Biome Textures ", 0.2f);
                                        script.rotateBiomemap(true);
                                    }
                                    if (alpha == true)
                                    {
                                        script.changeLayer("Alpha");
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alpha Textures ", 0.25f);
                                        script.rotateAlphamap(true);
                                    }
                                    if (topology == true)
                                    {
                                        script.changeLayer("Topology");
                                        script.rotateAllTopologymap(true);
                                    };
                                    EditorUtility.DisplayProgressBar("Rotating Map", "Finished ", 1f);
                                    EditorUtility.ClearProgressBar();
                                }
                                if (GUILayout.Button("Rotate 270°", GUILayout.MaxWidth(90))) // Calls every rotate function from MapIO. Rotates 270 degrees.
                                {
                                    if (heightmap == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Heightmap ", 0.05f);
                                        script.rotateHeightmap(false);
                                    }
                                    if (paths == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Paths ", 0.075f);
                                        script.rotatePaths(false);
                                    }
                                    if (prefabs == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Prefabs ", 0.1f);
                                        script.rotatePrefabs(false);
                                    }
                                    if (ground == true)
                                    {
                                        script.changeLayer("Ground");
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ground Textures ", 0.15f);
                                        script.rotateGroundmap(false);
                                    }
                                    if (biome == true)
                                    {
                                        script.changeLayer("Biome");
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Biome Textures ", 0.2f);
                                        script.rotateBiomemap(false);
                                    }
                                    if (alpha == true)
                                    {
                                        script.changeLayer("Alpha");
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alpha Textures ", 0.25f);
                                        script.rotateAlphamap(false);
                                    }
                                    if (topology == true)
                                    {
                                        script.changeLayer("Topology");
                                        script.rotateAllTopologymap(false);
                                    };
                                    EditorUtility.DisplayProgressBar("Rotating Map", "Finished ", 1f);
                                    EditorUtility.ClearProgressBar();
                                }
                                EditorGUILayout.EndHorizontal();
                                if (allLayers == true)
                                {
                                    ground = true; biome = true; alpha = true; topology = true; heightmap = true; paths = true; prefabs = true;
                                }

                                EditorGUILayout.BeginHorizontal();
                                ground = EditorGUILayout.ToggleLeft("Ground", ground, GUILayout.MaxWidth(60));
                                biome = EditorGUILayout.ToggleLeft("Biome", biome, GUILayout.MaxWidth(60));
                                alpha = EditorGUILayout.ToggleLeft("Alpha", alpha, GUILayout.MaxWidth(60));
                                topology = EditorGUILayout.ToggleLeft("Topology", topology, GUILayout.MaxWidth(75));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                paths = EditorGUILayout.ToggleLeft("Paths", paths, GUILayout.MaxWidth(60));
                                prefabs = EditorGUILayout.ToggleLeft("Prefabs", prefabs, GUILayout.MaxWidth(60));
                                heightmap = EditorGUILayout.ToggleLeft("HeightMap", heightmap, GUILayout.MaxWidth(80));
                                EditorGUILayout.EndHorizontal();
                                break;
                            #endregion
                            #region HeightMap
                            case 1:
                                GUIContent[] heightMapMenu = new GUIContent[2];
                                heightMapMenu[0] = new GUIContent("Heights");
                                heightMapMenu[1] = new GUIContent("Debug Tools");
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
                                            script.offsetHeightmap(offset, checkHeight, setWaterMap);
                                        }
                                        GUILayout.Label("Heightmap Minimum/Maximum Height", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        heightToSet = EditorGUILayout.FloatField(heightToSet, GUILayout.MaxWidth(40));
                                        if (GUILayout.Button(new GUIContent("Set Minimum Height", "Raises any of the land below " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
                                            " metres."), GUILayout.MaxWidth(130)))
                                        {
                                            script.setMinimumHeight(heightToSet);
                                        }
                                        if (GUILayout.Button(new GUIContent("Set Maximum Height", "Lowers any of the land above " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
                                            " metres."), GUILayout.MaxWidth(130)))
                                        {
                                            script.setMaximumHeight(heightToSet);
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Label("Edge of Map Height", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        top = EditorGUILayout.ToggleLeft("Top ", top, GUILayout.MaxWidth(60));
                                        left = EditorGUILayout.ToggleLeft("Left ", left, GUILayout.MaxWidth(60));
                                        bottom = EditorGUILayout.ToggleLeft("Bottom ", bottom, GUILayout.MaxWidth(60));
                                        right = EditorGUILayout.ToggleLeft("Right ", right, GUILayout.MaxWidth(60));
                                        EditorGUILayout.EndHorizontal();

                                        if (GUILayout.Button(new GUIContent("Set Edge Height", "Sets the very edge of the map to " + heightToSet.ToString() + " metres on any of the sides selected.")))
                                        {
                                            script.setEdgePixel(heightToSet, top, left, right, bottom);
                                        }
                                        break;
                                    case 1:
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button(new GUIContent("Debug Alpha", "Sets the ground texture to rock wherever the terrain is invisible. Prevents the floating grass effect.")))
                                        {
                                            script.changeLayer("Ground");
                                            script.alphaDebug("Ground");
                                        }
                                        if (GUILayout.Button(new GUIContent("Debug Water", "Raises the water heightmap to 500 metres if it is below.")))
                                        {
                                            script.debugWaterLevel();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Remove Broken Prefabs", "Removes any prefabs known to prevent maps from loading. Use this is you are having" +
                                            " errors loading a map on a server.")))
                                        {
                                            script.removeBrokenPrefabs();
                                        }
                                        break;
                                }
                                break;
                            #endregion
                            #region Textures
                            case 2:
                                GUILayout.Label("Copy Textures", EditorStyles.boldLabel);
                                string[] layerList = { "Ground", "Biome", "Topology" };
                                landLayerFrom = EditorGUILayout.Popup("Layer:", landLayerFrom, layerList);
                                switch (landLayerFrom) // Get texture list from the currently selected landLayer.
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        script.groundLayerFrom = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Copy:", script.groundLayerFrom);
                                        textureFrom = 0;
                                        break;
                                    case 1:
                                        script.biomeLayerFrom = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Copy:", script.biomeLayerFrom);
                                        textureFrom = 1;
                                        break;
                                    case 2:
                                        script.topologyLayerFrom = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology To Copy:", script.topologyLayerFrom);
                                        textureFrom = 2;
                                        break;
                                }
                                landLayerToPaint = EditorGUILayout.Popup("Layer:", landLayerToPaint, layerList);
                                switch (landLayerToPaint) // Get texture list from the currently selected landLayer.
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        script.groundLayerToPaint = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", script.groundLayerToPaint);
                                        textureToPaint = 0;
                                        break;
                                    case 1:
                                        script.biomeLayerToPaint = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", script.biomeLayerToPaint);
                                        textureToPaint = 1;
                                        break;
                                    case 2:
                                        script.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology To Paint:", script.topologyLayerToPaint);
                                        textureToPaint = 2;
                                        break;
                                }
                                if (GUILayout.Button(new GUIContent("Copy textures to new layer", "Copies the Texture from the " + layerList[landLayerFrom] + " layer and " +
                                    "paints it on the " + layerList[landLayerToPaint] + " layer.")))
                                {
                                    script.textureCopy(layerList[landLayerFrom], layerList[landLayerToPaint], textureFrom, textureToPaint);
                                }
                                GUILayout.Label("Conditional Paint", EditorStyles.boldLabel);
                                string[] landLayerList = { "Ground", "Biome", "Alpha", "Topology" };
                                string[] activeTextureAlpha = { "Visible", "Invisible" };
                                string[] activeTextureTopo = { "Active", "Inactive" };
                                int[] values = { 0, 1 };

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
                                        script.conditionalGround = (TerrainSplat.Enum)EditorGUILayout.EnumFlagsField(script.conditionalGround);
                                        break;
                                    case 1: // Biome
                                        GUILayout.Label("Biome Texture", EditorStyles.boldLabel);
                                        script.conditionalBiome = (TerrainBiome.Enum)EditorGUILayout.EnumFlagsField(script.conditionalBiome);
                                        break;
                                    case 2: // Alpha
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button("ALL", GUILayout.MaxWidth(30)))
                                        {
                                            alphaTxtCndtl = new bool[] { true, true };
                                            AlphaVisible = true; AlphaInvisible = true;
                                        }
                                        if (GUILayout.Button("NONE", GUILayout.MaxWidth(45)))
                                        {
                                            alphaTxtCndtl = new bool[] { false, false };
                                            AlphaVisible = false; AlphaInvisible = false;
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.BeginHorizontal();
                                        AlphaVisible = EditorGUILayout.ToggleLeft("Visible", AlphaVisible, GUILayout.MaxWidth(60));
                                        AlphaInvisible = EditorGUILayout.ToggleLeft("Invisible", AlphaInvisible, GUILayout.MaxWidth(65));
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUI.EndChangeCheck();
                                        if (GUI.changed)
                                        {
                                            alphaTxtCndtl[0] = AlphaVisible;
                                            alphaTxtCndtl[1] = AlphaInvisible;
                                        }
                                        break;
                                    case 3: // Topology
                                        GUILayout.Label("Topology Layer", EditorStyles.boldLabel);
                                        script.conditionalTopology = (TerrainTopology.Enum)EditorGUILayout.EnumFlagsField(script.conditionalTopology);
                                        EditorGUILayout.Space();
                                        GUILayout.Label("Topology Texture", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button("ALL", GUILayout.MaxWidth(30)))
                                        {
                                            topoTxtCndtl = new bool[] { true, true };
                                            TopoActive = true; TopoInactive = true;
                                        }
                                        if (GUILayout.Button("NONE", GUILayout.MaxWidth(45)))
                                        {
                                            topoTxtCndtl = new bool[] { false, false };
                                            TopoActive = false; TopoInactive = false;
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.BeginHorizontal();
                                        TopoActive = EditorGUILayout.ToggleLeft("Active", TopoActive, GUILayout.MaxWidth(60));
                                        TopoInactive = EditorGUILayout.ToggleLeft("Inactive", TopoInactive, GUILayout.MaxWidth(65));
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUI.EndChangeCheck();
                                        if (GUI.changed)
                                        {
                                            topoTxtCndtl[0] = TopoActive;
                                            topoTxtCndtl[1] = TopoInactive;
                                        }
                                        break;
                                    case 4: // Terrain
                                        GUILayout.Label("Slope Range", EditorStyles.boldLabel);
                                        checkSlopeCndtl = EditorGUILayout.Toggle("Check Slopes:", checkSlopeCndtl);
                                        if (checkSlopeCndtl == true)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Label("From: " + slopeLowCndtl.ToString() + "°", EditorStyles.boldLabel);
                                            GUILayout.Label("To: " + slopeHighCndtl.ToString() + "°", EditorStyles.boldLabel);
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.MinMaxSlider(ref slopeLowCndtl, ref slopeHighCndtl, 0f, 90f);
                                        }
                                        GUILayout.Label("Height Range", EditorStyles.boldLabel);
                                        checkHeightCndtl = EditorGUILayout.Toggle("Check Heights:", checkHeightCndtl);
                                        if (checkHeightCndtl == true)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Label("From: " + heightLowCndtl.ToString(), EditorStyles.boldLabel);
                                            GUILayout.Label("To: " + heightHighCndtl.ToString(), EditorStyles.boldLabel);
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.MinMaxSlider(ref heightLowCndtl, ref heightHighCndtl, 0f, 1000f);
                                        }
                                        break;
                                }
                                GUILayout.Label("Texture To Paint:", EditorStyles.boldLabel);
                                layerConditionalInt = EditorGUILayout.Popup("Layer:", layerConditionalInt, landLayerList);
                                switch (layerConditionalInt)
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        groundLayerToPaint = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", groundLayerToPaint);
                                        texture = TerrainSplat.TypeToIndex((int)groundLayerToPaint);
                                        break;
                                    case 1:
                                        biomeLayerToPaint = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", biomeLayerToPaint);
                                        texture = TerrainBiome.TypeToIndex((int)biomeLayerToPaint);
                                        break;
                                    case 2:
                                        texture = EditorGUILayout.IntPopup("Texture To Paint:", texture, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        script.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology Layer:", script.topologyLayerToPaint);
                                        texture = EditorGUILayout.IntPopup("Texture To Paint:", texture, activeTextureTopo, values);
                                        break;
                                }
                                if (GUILayout.Button(new GUIContent("Paint Conditional", "Paints the selected texture if it matches all of the conditions set.")))
                                {
                                    List<Conditions> conditions = new List<Conditions>();
                                    conditions.Add(new Conditions()
                                    {
                                        LandLayers = landLayersCndtl,
                                        AlphaTextures = alphaTxtCndtl,
                                        TopologyLayers = script.conditionalTopology,
                                        TopologyTextures = topoTxtCndtl,
                                        SlopeLow = slopeLowCndtl,
                                        SlopeHigh = slopeHighCndtl,
                                        HeightLow = heightLowCndtl,
                                        HeightHigh = heightHighCndtl,
                                        CheckHeight = checkHeightCndtl,
                                        CheckSlope = checkSlopeCndtl
                                    });
                                    script.paintConditional(landLayerList[layerConditionalInt], texture, conditions);
                                }
                                break;
                            #endregion
                        }
                        break;
                    #endregion
                    #region Layer Tools
                    case 1:
                        GUILayout.Label("Layer Tools", EditorStyles.boldLabel);

                        string oldLandLayer = script.landLayer;
                        string[] options = { "Ground", "Biome", "Alpha", "Topology" };
                        script.landSelectIndex = EditorGUILayout.Popup("Layer:", script.landSelectIndex, options);
                        script.landLayer = options[script.landSelectIndex];
                        if (script.landLayer != oldLandLayer)
                        {
                            script.changeLandLayer();
                            Repaint();
                        }
                        if (minBlendHigh > maxBlendHigh)
                        {
                            maxBlendHigh = minBlendHigh + 0.25f;
                            if (maxBlendHigh > 90f)
                            {
                                maxBlendHigh = 90f;
                            }
                        }
                        if (minBlendLow > maxBlendLow)
                        {
                            minBlendLow = maxBlendLow - 0.25f;
                            if (minBlendLow < 0f)
                            {
                                minBlendLow = 0f;
                            }
                        }
                        maxBlendLow = slopeLow;
                        minBlendHigh = slopeHigh;
                        if (blendSlopes == false)
                        {
                            minBlendLow = maxBlendLow;
                            maxBlendHigh = minBlendHigh;
                        }
                        if (blendHeights == false)
                        {
                            minBlendLowHeight = heightLow;
                            maxBlendHighHeight = heightHigh;
                        }
                        #region Ground Layer
                        if (script.landLayer.Equals("Ground"))
                        {
                            script.terrainLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint: ", script.terrainLayer);
                            if (GUILayout.Button("Paint Whole Layer"))
                            {
                                script.paintLayer("Ground", 0);
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                script.rotateGroundmap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                script.rotateGroundmap(false);
                            }
                            EditorGUILayout.EndHorizontal();
                            aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
                            if (GUILayout.Button("Paint Rivers"))
                            {
                                script.paintRiver("Ground", aboveTerrain, 0);
                            }
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            blendSlopes = EditorGUILayout.ToggleLeft("Toggle Blend Slopes", blendSlopes);
                            // Todo: Toggle for check between heightrange.
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (blendSlopes == true)
                            {
                                GUILayout.Label("Blend Low: " + minBlendLow + "°");
                                EditorGUILayout.MinMaxSlider(ref minBlendLow, ref maxBlendLow, 0f, 90f);
                                GUILayout.Label("Blend High: " + maxBlendHigh + "°");
                                EditorGUILayout.MinMaxSlider(ref minBlendHigh, ref maxBlendHigh, 0f, 90f);
                                GUILayout.Label("Blend Strength: ");
                                blendStrength = EditorGUILayout.Slider(blendStrength, 0f, 10f);
                            }
                            if (GUILayout.Button("Paint slopes"))
                            {
                                script.paintSlope("Ground", slopeLow, slopeHigh, minBlendLow, maxBlendHigh, 0, blendStrength);
                            }
                            GUILayout.Label("Custom height range");
                            blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
                            if (blendHeights == true)
                            {
                                minBlendLowHeight = EditorGUILayout.FloatField("Bottom Blend", minBlendLowHeight);
                            }
                            heightLow = EditorGUILayout.FloatField("Bottom", heightLow);
                            heightHigh = EditorGUILayout.FloatField("Top", heightHigh);
                            if (blendHeights == true)
                            {
                                maxBlendHighHeight = EditorGUILayout.FloatField("Top Blend", maxBlendHighHeight);
                                GUILayout.Label("Blend Strength: ");
                                blendStrength = EditorGUILayout.Slider(blendStrength, 0f, 10f);
                            }
                            if (GUILayout.Button("Paint Heights"))
                            {
                                script.paintHeight("Ground", heightLow, heightHigh, minBlendLowHeight, maxBlendHighHeight, 0, blendStrength);
                            }
                            
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Ground textures");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 10f, 2000f);
                            if (GUILayout.Button("Generate random Ground map"))
                            {
                                script.generateEightLayersNoise("Ground", scale);
                            }
                        }
                        #endregion

                        #region Biome Layer
                        if (script.landLayer.Equals("Biome"))
                        {
                            script.biomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Biome To Paint:", script.biomeLayer);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                script.rotateBiomemap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                script.rotateBiomemap(false);
                            }
                            EditorGUILayout.EndHorizontal();
                            aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
                            if (GUILayout.Button("Paint Rivers"))
                            {
                                script.paintRiver("Biome", aboveTerrain, 0);
                            }
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (GUILayout.Button("Paint slopes"))
                            {
                                script.paintSlope("Biome", slopeLow, slopeHigh, slopeLow, slopeHigh, 0, 1);
                            }
                            GUILayout.Label("Custom height range");
                            heightLow = EditorGUILayout.FloatField("Bottom", heightLow);
                            heightHigh = EditorGUILayout.FloatField("Top", heightHigh);
                            if (GUILayout.Button("Paint range"))
                            {
                                script.paintHeight("Biome", heightLow, heightHigh, minBlendLowHeight, maxBlendHighHeight, 0, 1);
                            }
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            if (GUILayout.Button("Paint Area"))
                            {
                                script.paintArea("Biome", z1, z2, x1, x2, 0);
                            }
                            if (GUILayout.Button("Paint Whole Layer"))
                            {
                                script.paintLayer("Biome", 0);
                            }
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Biomes");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 500f, 5000f);
                            if (GUILayout.Button("Generate random Biome map"))
                            {
                                script.generateFourLayersNoise("Biome", scale);
                            }
                        }
                        #endregion

                        #region Alpha Layer
                        if (script.landLayer.Equals("Alpha"))
                        {
                            GUILayout.Label("Green = Terrain Visible, Purple = Terrain Invisible", EditorStyles.boldLabel);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                script.rotateAlphamap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                script.rotateAlphamap(false);
                            }
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Label("Custom height range");
                            heightLow = EditorGUILayout.FloatField("bottom", heightLow);
                            heightHigh = EditorGUILayout.FloatField("top", heightHigh);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint range"))
                            {
                                script.paintHeight("Alpha", heightLow, heightHigh, heightLow, heightHigh, 0, 1);
                            }
                            if (GUILayout.Button("Erase range"))
                            {
                                script.paintHeight("Alpha", heightLow, heightHigh, heightLow, heightHigh, 1, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Area"))
                            {
                                script.paintArea("Alpha", z1, z2, x1, x2, 0);
                            }
                            if (GUILayout.Button("Erase Area"))
                            {
                                script.paintArea("Alpha", z1, z2, x1, x2, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Layer"))
                            {
                                script.paintLayer("Alpha", 0);
                            }
                            if (GUILayout.Button("Clear Layer"))
                            {
                                script.clearLayer("Alpha");
                            }
                            if (GUILayout.Button("Invert Layer"))
                            {
                                script.invertLayer("Alpha");
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        #endregion

                        #region Topology Layer
                        if (script.landLayer.Equals("Topology"))
                        {
                            GUILayout.Label("Green = Topology Active, Purple = Topology Inactive", EditorStyles.boldLabel);
                            script.oldTopologyLayer = script.topologyLayer;
                            script.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology Layer:", script.topologyLayer);
                            if (script.topologyLayer != script.oldTopologyLayer)
                            {
                                script.changeLandLayer();
                                Repaint();
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                script.rotateTopologymap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                script.rotateTopologymap(false);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate all Topology layers 90° ClockWise.")))
                            {
                                script.rotateAllTopologymap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate all Topology layers 90° Counter ClockWise.")))
                            {
                                script.rotateAllTopologymap(false);
                            }
                            EditorGUILayout.EndHorizontal();
                            aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
                            if (GUILayout.Button("Paint Rivers"))
                            {
                                script.paintRiver("Topology", aboveTerrain, 0);
                            }
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (GUILayout.Button("Paint slopes"))
                            {
                                script.paintSlope("Topology", slopeLow, slopeHigh, slopeLow, slopeHigh, 0, 1);
                            }
                            GUILayout.Label("Custom height range");
                            heightLow = EditorGUILayout.FloatField("Bottom", heightLow);
                            heightHigh = EditorGUILayout.FloatField("Top", heightHigh);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint range"))
                            {
                                script.paintHeight("Topology", heightLow, heightHigh, heightLow, heightHigh, 0, 1);
                            }
                            if (GUILayout.Button("Erase range"))
                            {
                                script.paintHeight("Topology", heightLow, heightHigh, heightLow, heightHigh, 1, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Area"))
                            {
                                script.paintArea("Topology", z1, z2, x1, x2, 0);
                            }
                            if (GUILayout.Button("Erase Area"))
                            {
                                script.paintArea("Topology", z1, z2, x1, x2, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Layer"))
                            {
                                script.paintLayer("Topology", 0);
                            }
                            if (GUILayout.Button("Clear Layer"))
                            {
                                script.clearLayer("Topology");
                            }
                            if (GUILayout.Button("Invert Layer"))
                            {
                                script.invertLayer("Topology");
                            }
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Topology");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 10f, 500f);
                            if (GUILayout.Button("Generate random topology map"))
                            {
                                script.generateTwoLayersNoise("Topology", scale, 1, 0);
                            }
                        }
                        break;
                    #endregion
                    default:
                        toolsOptions = 0;
                        break;
                    #endregion
                    #region Generation
                    case 2:
                        if (GUILayout.Button("Default Topologies"))
                        {
                            script.autoGenerateTopology(false);
                        }
                        if (GUILayout.Button("Wipe & Default Topologies"))
                        {
                            script.autoGenerateTopology(true);
                        }
                        if (GUILayout.Button("Default Ground Textures"))
                        {
                            script.autoGenerateGround();
                        }
                        /*
                        scale = EditorGUILayout.Slider(scale, 1f, 2000f);
                        GUILayout.Label("Scale of the heightmap generation, \n the further left the less smoothed the terrain will be");
                        if (GUILayout.Button("Generate Perlin Heightmap"))
                        {
                            script.generatePerlinHeightmap(scale);
                        }
                        */
                        break;
                        #endregion
                }
                break;
            #endregion
            #region Prefabs
            case 2:
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Load", "Loads all the prefabs from the Rust Asset Bundle for use in the editor. Prefabs paths to be loaded can be changed in " +
                    "AssetList.txt in the root directory"), GUILayout.MaxWidth(100)))
                {
                    script.StartPrefabLookup();
                }
                if (GUILayout.Button(new GUIContent("Unload", "Unloads all the prefabs from the Rust Asset Bundle."), GUILayout.MaxWidth(100)))
                {
                    if (script.getPrefabLookUp() != null)
                    {
                        script.getPrefabLookUp().Dispose();
                        script.setPrefabLookup(null);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("ERROR: Can't unload prefabs", "No prefabs loaded.", "Ok");
                    }
                }
                if (GUILayout.Button(new GUIContent("Replace", "Replaces all of the placeholder cube prefabs on the map with the Rust Assets"), GUILayout.MaxWidth(100)))
                {
                    if (script.getPrefabLookUp() != null)
                    {
                        if (script.loadPath != "")
                        {
                            script.ReplacePrefabs();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("ERROR: Can't replace prefabs", "No map loaded.", "Ok");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("ERROR: Can't replace prefabs", "No prefabs loaded.", "Ok");
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent("Select Bundle File", "Select the bundles files located in your steamapps/common/Rust/Bundles directory."), GUILayout.MaxWidth(125)))
                {
                    script.bundleFile = UnityEditor.EditorUtility.OpenFilePanel("Select Bundle File", script.bundleFile, "");
                }
                GUILayout.TextArea(script.bundleFile);
                break;
            #endregion
            default:
                mainMenuOptions = 0;
                break;
        }
        #endregion
    }
}
