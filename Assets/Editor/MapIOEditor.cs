using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Rotorz.ReorderableList;

[CustomEditor(typeof(MapIO))]
public class MapIOInspector : Editor
{
    public override void OnInspectorGUI()
    {
        //Only override this to stop the MapIO public variables from being exposed when the object is selected.
    }
}
public class MapIOEditor : EditorWindow
{
    string editorVersion = "v1.4-prerelease";

    string loadFile = "";
    string saveFile = "";
    string mapName = "";
    string prefabSaveFile = "";
    //Todo: Clean this up. It's coarse and rough and irritating and it gets everywhere.
    int mapSize = 1000, mainMenuOptions = 0, toolsOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0;
    float heightToSet = 450f, scale = 50f, offset = 0f;
    //float mapScale = 1f; Comment back in when used.
    bool[] sides = new bool[4]; 
    bool checkHeight = true, setWaterMap = false;
    bool allLayers = false, ground = false, biome = false, alpha = false, topology = false, heightmap = false, prefabs = false, paths = false;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float slopeMinBlendLow = 25f, slopeMaxBlendLow = 40f, slopeMinBlendHigh = 60f, slopeMaxBlendHigh = 75f;
    float heightMinBlendLow = 0f, heightMaxBlendLow = 500f, heightMinBlendHigh = 500f, heightMaxBlendHigh = 1000f;
    float normaliseLow = 450f, normaliseHigh = 1000f, normaliseBlend = 1f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;
    int layerConditionalInt, texture = 0;
    bool AlphaVisible = false, AlphaInvisible = false;
    bool TopoActive = false, TopoInactive = false;
    bool deletePrefabs = false;
    bool checkHeightCndtl = false, checkSlopeCndtl = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f;
    float heightLowCndtl = 500f, heightHighCndtl = 600f;
    bool autoUpdate = false, itemValueSet = false;
    string itemValueOld = "", assetDirectory = "Assets/AutoGenPresets/";
    Vector2 scrollPos = new Vector2(0, 0);
    Vector2 presetScrollPos = new Vector2(0, 0);

    float filterStrength = 1f;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f;
    float blurDirection = 1f;

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

    [MenuItem("Rust Map Editor/Main Menu")]
    static void Initialize()
    {
        MapIOEditor window = (MapIOEditor)EditorWindow.GetWindow(typeof(MapIOEditor), false, "Rust Map Editor");
    }
    public void OnGUI()
    {
        MapIO script = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

        if (layerSet == false)
        {
            groundLayerToPaint = TerrainSplat.Enum.Grass;
            biomeLayerToPaint = TerrainBiome.Enum.Temperate;
            layerSet = true;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIContent[] mainMenu = new GUIContent[2];
        mainMenu[0] = new GUIContent("Main Menu");
        mainMenu[1] = new GUIContent("Tools");
        //mainMenu[2] = new GUIContent("Prefabs");
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

                GUILayout.Label("Editor Info", EditorStyles.boldLabel);
                GUILayout.Label("OS: " + SystemInfo.operatingSystem);
                GUILayout.Label("Unity Version: " + Application.unityVersion);
                GUILayout.Label("Editor Version: " + editorVersion);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Report Bug", "Opens up the editor bug report in GitHub."), GUILayout.MaxWidth(75)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/issues/new?assignees=Adsitoz&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
                }
                if (GUILayout.Button(new GUIContent("Request Feature", "Opens up the editor feature request in GitHub."), GUILayout.MaxWidth(105)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/issues/new?assignees=Adsitoz&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
                }
                if (GUILayout.Button(new GUIContent("RoadMap", "Opens up the editor roadmap in GitHub."), GUILayout.MaxWidth(65)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/projects/1");
                }
                EditorGUILayout.EndHorizontal();
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
                        GUIContent[] mapToolsMenu = new GUIContent[4];
                        mapToolsMenu[0] = new GUIContent("Transform");
                        mapToolsMenu[1] = new GUIContent("HeightMap");
                        mapToolsMenu[2] = new GUIContent("Textures");
                        mapToolsMenu[3] = new GUIContent("Misc");
                        mapToolsOptions = GUILayout.Toolbar(mapToolsOptions, mapToolsMenu);

                        switch (mapToolsOptions)
                        {
                            #region Rotate Map
                            case 0:
                                GUILayout.Label("Rotate Map", EditorStyles.boldLabel);
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
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ground Textures ", 0.15f);
                                        script.rotateGroundmap(true);
                                    }
                                    if (biome == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Biome Textures ", 0.2f);
                                        script.rotateBiomemap(true);
                                    }
                                    if (alpha == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alpha Textures ", 0.25f);
                                        script.rotateAlphamap(true);
                                    }
                                    if (topology == true)
                                    {
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
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ground Textures ", 0.15f);
                                        script.rotateGroundmap(false);
                                    }
                                    if (biome == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Biome Textures ", 0.2f);
                                        script.rotateBiomemap(false);
                                    }
                                    if (alpha == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alpha Textures ", 0.25f);
                                        script.rotateAlphamap(false);
                                    }
                                    if (topology == true)
                                    {
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
                                        sides[0] = EditorGUILayout.ToggleLeft("Top ", sides[0], GUILayout.MaxWidth(60));
                                        sides[3] = EditorGUILayout.ToggleLeft("Left ", sides[3], GUILayout.MaxWidth(60));
                                        sides[2] = EditorGUILayout.ToggleLeft("Bottom ", sides[2], GUILayout.MaxWidth(60));
                                        sides[1] = EditorGUILayout.ToggleLeft("Right ", sides[1], GUILayout.MaxWidth(60));
                                        EditorGUILayout.EndHorizontal();

                                        if (GUILayout.Button(new GUIContent("Set Edge Height", "Sets the very edge of the map to " + heightToSet.ToString() + " metres on any of the sides selected.")))
                                        {
                                            script.setEdgePixel(heightToSet, sides);
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
                                            script.flipHeightmap();
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
                                        EditorGUILayout.LabelField(new GUIContent("Blend", "The amount of blending to occur during normalisation. The higher the value the" +
                                            "smoother the result will be."), GUILayout.MaxWidth(40));
                                        normaliseBlend = EditorGUILayout.Slider(normaliseBlend, 0f, 1f);
                                        if (EditorGUI.EndChangeCheck() && autoUpdate == true)
                                        {
                                            script.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f, normaliseBlend);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button(new GUIContent("Normalise", "Normalises the heightmap between these heights.")))
                                        {
                                            script.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f, normaliseBlend);
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
                                            script.SmoothHeightmap(filterStrength, blurDirection);
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
                                            script.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
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
                            #region Misc
                            case 3:
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
                        if (slopeMinBlendHigh > slopeMaxBlendHigh)
                        {
                            slopeMaxBlendHigh = slopeMinBlendHigh + 0.25f;
                            if (slopeMaxBlendHigh > 90f)
                            {
                                slopeMaxBlendHigh = 90f;
                            }
                        }
                        if (slopeMinBlendLow > slopeMaxBlendLow)
                        {
                            slopeMinBlendLow = slopeMaxBlendLow - 0.25f;
                            if (slopeMinBlendLow < 0f)
                            {
                                slopeMinBlendLow = 0f;
                            }
                        }
                        if (heightMinBlendLow > heightMaxBlendLow)
                        {
                            heightMinBlendLow = heightMaxBlendLow - 0.25f;
                            if (heightMinBlendLow < 0f)
                            {
                                heightMinBlendLow = 0f;
                            }
                        }
                        if (heightMinBlendHigh > heightMaxBlendHigh)
                        {
                            heightMaxBlendHigh = heightMinBlendHigh + 0.25f;
                            if (heightMaxBlendHigh > 1000f)
                            {
                                heightMaxBlendHigh = 1000f;
                            }
                        }
                        slopeMaxBlendLow = slopeLow;
                        slopeMinBlendHigh = slopeHigh;
                        heightMaxBlendLow = heightLow;
                        heightMinBlendHigh = heightHigh;
                        if (blendSlopes == false)
                        {
                            slopeMinBlendLow = slopeMaxBlendLow;
                            slopeMaxBlendHigh = slopeMinBlendHigh;
                        }
                        if (blendHeights == false)
                        {
                            heightMinBlendLow = heightLow;
                            heightMaxBlendHigh = heightHigh;
                        }
                        #region Ground Layer
                        if (script.landLayer.Equals("Ground"))
                        {
                            script.terrainLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Paint: ", script.terrainLayer);
                            if (GUILayout.Button("Paint Whole Layer"))
                            {
                                script.PaintLayer("Ground", TerrainSplat.TypeToIndex((int)script.terrainLayer));
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
                                script.paintRiver("Ground", aboveTerrain, TerrainSplat.TypeToIndex((int)script.terrainLayer));
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
                                GUILayout.Label("Blend Low: " + slopeMinBlendLow + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendLow, ref slopeMaxBlendLow, 0f, 90f);
                                GUILayout.Label("Blend High: " + slopeMaxBlendHigh + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendHigh, ref slopeMaxBlendHigh, 0f, 90f);
                            }
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the terrain on the " + script.landLayer + " layer within the slope range.")))
                            {
                                script.PaintSlope("Ground", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainSplat.TypeToIndex((int)script.terrainLayer));
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 90
                            GUILayout.Label("Custom height range");
                            blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
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
                            if (GUILayout.Button(new GUIContent("Paint Heights", "Paints the terrain on the " + script.landLayer + " layer within the height range.")))
                            {
                                script.PaintHeight("Ground", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainSplat.TypeToIndex((int)script.terrainLayer));
                            }
                            /*
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Ground textures");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 10f, 2000f);
                            if (GUILayout.Button("Generate random Ground map"))
                            {
                                script.generateEightLayersNoise("Ground", scale);
                            }*/
                        }
                        #endregion

                        #region Biome Layer
                        if (script.landLayer.Equals("Biome"))
                        {
                            script.biomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Biome To Paint:", script.biomeLayer);
                            if (GUILayout.Button("Paint Whole Layer"))
                            {
                                script.PaintLayer("Biome", TerrainBiome.TypeToIndex((int)script.biomeLayer));
                            }
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
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            blendSlopes = EditorGUILayout.ToggleLeft("Toggle Blend Slopes", blendSlopes);
                            // Todo: Toggle for check between heightrange.
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (blendSlopes == true)
                            {
                                GUILayout.Label("Blend Low: " + slopeMinBlendLow + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendLow, ref slopeMaxBlendLow, 0f, 90f);
                                GUILayout.Label("Blend High: " + slopeMaxBlendHigh + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendHigh, ref slopeMaxBlendHigh, 0f, 90f);
                            }
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the terrain on the " + script.landLayer + " layer within the slope range.")))
                            {
                                script.PaintSlope("Biome", slopeLow, slopeHigh, slopeLow, slopeHigh, TerrainSplat.TypeToIndex((int)script.biomeLayer));
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 1000
                            GUILayout.Label("Custom Height Range");
                            blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
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
                            if (GUILayout.Button(new GUIContent("Paint Heights", "Paints the terrain on the " + script.landLayer + " layer within the height range.")))
                            {
                                script.PaintHeight("Biome", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainBiome.TypeToIndex((int)script.biomeLayer));
                            }
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            if (GUILayout.Button("Paint Area"))
                            {
                                script.paintArea("Biome", z1, z2, x1, x2, TerrainBiome.TypeToIndex((int)script.biomeLayer));
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
                            GUILayout.Label("Green = Terrain Visible \nPurple = Terrain Invisible", EditorStyles.boldLabel);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Layer"))
                            {
                                script.PaintLayer("Alpha", 1);
                            }
                            if (GUILayout.Button("Clear Layer"))
                            {
                                script.PaintLayer("Alpha", 0);
                            }
                            if (GUILayout.Button("Invert Layer"))
                            {
                                script.InvertLayer("Alpha");
                            }
                            EditorGUILayout.EndHorizontal();
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
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the slopes on the " + script.landLayer + " layer within the slope range.")))
                            {
                                script.PaintSlope("Alpha", slopeLow, slopeHigh, slopeLow, slopeHigh, 0);
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 1000
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint range"))
                            {
                                script.PaintHeight("Alpha", heightLow, heightHigh, heightLow, heightHigh, 0);
                            }
                            if (GUILayout.Button("Erase range"))
                            {
                                script.PaintHeight("Alpha", heightLow, heightHigh, heightLow, heightHigh, 1);
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
                        }
                        #endregion

                        #region Topology Layer
                        if (script.landLayer.Equals("Topology"))
                        {
                            GUILayout.Label("Green = Active \nPurple = Inactive", EditorStyles.boldLabel);
                            script.oldTopologyLayer = script.topologyLayer;
                            script.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology Layer:", script.topologyLayer);
                            if (script.topologyLayer != script.oldTopologyLayer)
                            {
                                script.changeLandLayer();
                                Repaint();
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Layer"))
                            {
                                script.PaintLayer("Topology", 0);
                            }
                            if (GUILayout.Button("Clear Layer"))
                            {
                                script.ClearLayer("Topology");
                            }
                            if (GUILayout.Button("Invert Layer"))
                            {
                                script.InvertLayer("Topology");
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                script.rotateTopologymap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                script.rotateTopologymap(false);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate All 90°", "Rotate all Topology layers 90° ClockWise.")))
                            {
                                script.rotateAllTopologymap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate All 270°", "Rotate all Topology layers 90° Counter ClockWise.")))
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
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the slopes on the " + script.landLayer + " layer within the slope range.")))
                            {
                                script.PaintSlope("Topology", slopeLow, slopeHigh, slopeLow, slopeHigh, 0);
                            }
                            if (GUILayout.Button(new GUIContent("Erase Slopes", "Paints the slopes within the slope range with the INACTIVE topology texture.")))
                            {
                                script.PaintSlope("Topology", slopeLow, slopeHigh, slopeLow, slopeHigh, 1);
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 1000
                            GUILayout.Label("Custom Height Range");
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Paint Range", "Paints the slopes within the height range with the ACTIVE topology texture.")))
                            {
                                script.PaintHeight("Topology", heightLow, heightHigh, heightLow, heightHigh, 0);
                            }
                            if (GUILayout.Button(new GUIContent("Erase Range", "Paints the slopes within the height range with the INACTIVE topology texture.")))
                            {
                                script.PaintHeight("Topology", heightLow, heightHigh, heightLow, heightHigh, 1);
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
                        if (GUILayout.Button(new GUIContent("Default Topologies", "Generates default topologies on the map and paints over existing topologies without wiping them.")))
                        {
                            script.autoGenerateTopology(false);
                        }
                        if (GUILayout.Button(new GUIContent("Wipe & Default Topologies", "Generates default topologies on the map and paints over existing topologies after wiping them.")))
                        {
                            script.autoGenerateTopology(true);
                        }
                        if (GUILayout.Button(new GUIContent("Default Ground Textures", "Generates default ground textures and paints over existing textures after wiping them.")))
                        {
                            script.AutoGenerateGround();
                        }
                        if (GUILayout.Button(new GUIContent("Default Biome Textures", "Generates default biome textures and paints over existing textures after wiping them.")))
                        {
                            script.AutoGenerateBiome();
                        }
                        scale = EditorGUILayout.Slider(scale, 1f, 2000f);
                        GUILayout.Label("Scale of the heightmap generation, \n the further left the less smoothed the terrain will be");
                        if (GUILayout.Button(new GUIContent("Generate Perlin Heightmap", "Really basic perlin doesn't do much rn.")))
                        {
                            script.generatePerlinHeightmap(scale);
                        }
                        /*
                        GUILayout.Label(new GUIContent("Auto Generation Presets", "List of all the auto generation presets in the project."), EditorStyles.boldLabel);
                        if (GUILayout.Button(new GUIContent("Refresh presets list.", "Refreshes the list of all the Generation Presets in the project.")))
                        {
                            script.RefreshAssetList();
                        }
                        presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
                        ReorderableListGUI.Title("Generation Presets");
                        ReorderableListGUI.ListField(script.generationPresetList, AutoGenerationPresetDrawer, DrawEmpty);
                        GUILayout.EndScrollView();*/
                        break;
                        #endregion
                }
                break;
            #endregion
            #region Prefabs
            case 2:
                GUIContent[] prefabsOptionsMenu = new GUIContent[2];
                prefabsOptionsMenu[0] = new GUIContent("Asset Bundle");
                //prefabsOptionsMenu[1] = new GUIContent("Spawn Prefabs");
                prefabsOptionsMenu[1] = new GUIContent("Prefab Tools");
                prefabOptions = GUILayout.Toolbar(prefabOptions, prefabsOptionsMenu);

                switch (prefabOptions)
                {
                    case 0:
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Load", "Loads all the prefabs from the Rust Asset Bundle for use in the editor. Prefabs paths to be loaded can be changed in " +
                            "AssetList.txt in the root directory"), GUILayout.MaxWidth(100)))
                        {
                            if (!script.bundleFile.Contains(@"steamapps/common/Rust/Bundles/Bundles"))
                            {
                                script.bundleFile = script.bundleFile = EditorUtility.OpenFilePanel("Select Bundle File", script.bundleFile, "");
                                if (script.bundleFile == "")
                                {
                                    return;
                                }
                                if (!script.bundleFile.Contains(@"steamapps/common/Rust/Bundles/Bundles"))
                                {
                                    EditorUtility.DisplayDialog("ERROR: Bundle File Invalid", @"Bundle file path invalid. It should be located within steamapps\common\Rust\Bundles", "Ok");
                                    return;
                                }
                            }
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
                        EditorGUILayout.EndHorizontal();
                        script.bundleFile = GUILayout.TextArea(script.bundleFile);
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
                            script.removeBrokenPrefabs();
                        }
                        deletePrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Delete LootCrates on Export.", "Deletes the lootcrates after exporting them. Stops lootcrates from originally spawning" +
                            "on first map load."), deletePrefabs, GUILayout.MaxWidth(300));
                        if (GUILayout.Button(new GUIContent("Export LootCrates", "Exports all lootcrates that don't yet respawn in Rust to a JSON for use with the LootCrateRespawn plugin." +
                            "If you don't delete them after export they will duplicate on first map load.")))
                        {
                            prefabSaveFile = UnityEditor.EditorUtility.SaveFilePanel("Export LootCrates", prefabSaveFile, "LootCrateData", "json");
                            if (prefabSaveFile == "")
                            {
                                return;
                            }
                            script.exportLootCrates(prefabSaveFile, deletePrefabs);
                        }
                        break;
                    default:
                        prefabOptions = 0;
                        break;
                }
                break;
            #endregion
            default:
                mainMenuOptions = 0;
                break;
        }
        #endregion
        #region InspectorGUIInput
        Event e = Event.current;
        #endregion
        EditorGUILayout.EndScrollView();
    }
    #region OtherMenus
    [MenuItem("Rust Map Editor/Wiki")]
    static void OpenWiki()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/wiki");
    }
    [MenuItem("Rust Map Editor/Discord")]
    static void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/HPmTWVa");
    }
    #endregion
    #region Methods
    private string AutoGenerationPresetDrawer(Rect position, string itemValue)
    {
        MapIO script = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        if (itemValueSet == false)
        {
            itemValueOld = itemValue;
            itemValueSet = true;
        }
        if (itemValue == null)
        {
            itemValue = "";
            itemValueOld = itemValue;
        }
        position.width -= 67;
        EditorGUI.BeginChangeCheck();
        itemValue = EditorGUI.TextField(position, itemValue);
        if (EditorGUI.EndChangeCheck())
        {
            if (itemValue != "" && script.generationPresetList.Contains(itemValue) == false) // Case for renaming an asset to empty path.
            {
                if (!itemValue.EndsWith(".asset"))
                {
                    AssetDatabase.RenameAsset(assetDirectory + itemValueOld + ".asset", itemValue);
                }
                else
                {
                    AssetDatabase.RenameAsset(assetDirectory + itemValueOld, itemValue);
                }
                script.RefreshAssetList();
                itemValueSet = false;
            }
        }
        position.x = position.xMax + 5;
        position.width = 38;
        if (GUI.Button(position, new GUIContent("Open", "Opens the Node Editor for the preset.")))
        {
            script.RefreshAssetList();
            script.generationPresetLookup.TryGetValue(itemValue, out Object preset);
            if (preset != null)
            {
                AssetDatabase.OpenAsset(preset.GetInstanceID());
            }
            else
            {
                Debug.LogError("The preset you are trying to open is null.");
            }
        }
        position.x = position.x + 40;
        position.width = 30;
        if (GUI.Button(position, "Run"))
        {
            script.generationPresetLookup.TryGetValue(itemValue, out Object preset);
            if (preset != null)
            {
                var graph = (XNode.NodeGraph)AssetDatabase.LoadAssetAtPath(assetDirectory + itemValue + ".asset", typeof(XNode.NodeGraph));
                MapIO.ParseNodeGraph(graph);
            }
        }
        return itemValue;
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
