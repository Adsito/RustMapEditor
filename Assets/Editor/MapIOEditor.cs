using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapIO))]
public class MapIOEditor : Editor
{
    string loadFile = "";

    string saveFile = "";
    string mapName = "";

    int mapSize = 1000, mainMenuOptions = 0, toolsOptions = 0, mapToolsOptions = 0, heightMapOptions = 0;
    float heightToSet = 450f, scale = 0f, offset = 0f;
    bool top = false, left = false, right = false, bottom = false, checkHeight = true, setWaterMap = false;
    bool allLayers = false, ground = false, biome = false, alpha = false, topology = false, heightmap = false, prefabs = false, paths = false;
    public LayerOptionEditor optionEditor;

    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;

    public override void OnInspectorGUI()
    {
        MapIO script = (MapIO)target;

        GUIContent[] mainMenu = new GUIContent[3];
        mainMenu[0] = new GUIContent("Main Menu");
        mainMenu[1] = new GUIContent("Tools");
        mainMenu[2] = new GUIContent("Prefabs");
        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu);

        #region Main Menu
        switch (mainMenuOptions)
        {
            case 0:
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New Map", GUILayout.MaxWidth(75)))
                {
                if (mapSize < 1000)
                    {
                        Debug.LogError("Use a map size greater than 1000");
                        return;
                    }
                    script.newEmptyTerrain(mapSize);
                }
                GUILayout.Label("Map Size", GUILayout.MaxWidth(60));
                mapSize = EditorGUILayout.IntField(mapSize, GUILayout.MaxWidth(40));
                EditorGUILayout.EndHorizontal();
                    
                    
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Load Map", GUILayout.MaxWidth(75)))
                {
                    loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");

                    var blob = new WorldSerialization();
                    Debug.Log("Importing map " + loadFile);
                    if (loadFile == "")
                    {
                        Debug.LogError("Empty load path");
                        return;
                    }
                    EditorUtility.DisplayProgressBar("Loading Map", "Loading Land Heightmap Data ", 0.2f);
                    blob.Load(loadFile);
                    EditorUtility.DisplayProgressBar("Loading Map", "Loading Land Heightmap Data ", 0.3f);
                    script.Load(blob);
                }
                if (GUILayout.Button("Save Map", GUILayout.MaxWidth(75)))
                {
                    saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
                    if (saveFile == "")
                    {
                        Debug.LogError("Empty save path");
                    }
                    Debug.Log("Exported map " + saveFile);
                    script.Save(saveFile);
                }
                if (GUILayout.Button("Select Bundle File", GUILayout.MaxWidth(125)))
                {
                    script.bundleFile = UnityEditor.EditorUtility.OpenFilePanel("Select Bundle File", script.bundleFile, "");
                }
                GUILayout.EndHorizontal();
                break;
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
                                GUIContent[] heightMapMenu = new GUIContent[3];
                                heightMapMenu[0] = new GUIContent("Offset");
                                heightMapMenu[1] = new GUIContent("Heights");
                                heightMapMenu[2] = new GUIContent("Debug Tools");
                                heightMapOptions = GUILayout.Toolbar(heightMapOptions, heightMapMenu);

                                switch (heightMapOptions)
                                {
                                    case 0:
                                        GUILayout.Label("Heightmap Offset (Increase or Decrease)");
                                        EditorGUILayout.BeginHorizontal();
                                        offset = EditorGUILayout.FloatField(offset, GUILayout.MaxWidth(35));
                                        checkHeight = EditorGUILayout.ToggleLeft("In bounds", checkHeight, GUILayout.MaxWidth(75));
                                        setWaterMap = EditorGUILayout.ToggleLeft("Water Heightmap", setWaterMap, GUILayout.MaxWidth(125));
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button("Offset Heightmap"))
                                        {
                                            script.offsetHeightmap(offset, checkHeight, setWaterMap);
                                        }
                                        break;
                                    case 1:
                                        EditorGUILayout.BeginHorizontal();
                                        heightToSet = EditorGUILayout.FloatField(heightToSet, GUILayout.MaxWidth(35));
                                        if (GUILayout.Button("Set Minimum Height", GUILayout.MaxWidth(130)))
                                        {
                                            script.setMinimumHeight(heightToSet);
                                        }
                                        if (GUILayout.Button("Set Maximum Height", GUILayout.MaxWidth(130)))
                                        {
                                            script.setMaximumHeight(heightToSet);
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Label("Edges to set:");
                                        EditorGUILayout.BeginHorizontal();
                                        top = EditorGUILayout.ToggleLeft("Top ", top, GUILayout.MaxWidth(60));
                                        left = EditorGUILayout.ToggleLeft("Left ", left, GUILayout.MaxWidth(60));
                                        bottom = EditorGUILayout.ToggleLeft("Bottom ", bottom, GUILayout.MaxWidth(60));
                                        right = EditorGUILayout.ToggleLeft("Right ", right, GUILayout.MaxWidth(60));
                                        EditorGUILayout.EndHorizontal();

                                        if (GUILayout.Button("Set Edge Pixel Height"))
                                        {
                                            script.setEdgePixel(heightToSet, top, left, right, bottom);
                                        }
                                        break;
                                    case 2:
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button("Debug Alpha"))
                                        {
                                            script.changeLayer("Ground");
                                            script.alphaDebug("Ground");
                                        }
                                        if (GUILayout.Button("Debug Water"))
                                        {
                                            script.debugWaterLevel();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button("Remove Broken Prefabs"))
                                        {
                                            script.removeBrokenPrefabs();
                                        }
                                        break;
                                }
                                
                                break;
                            #endregion
                            #region Textures
                            case 2:
                                GUILayout.Label("Copies the texture selected, and paints the selected \n texture in wherever the texture is active."); //ToDo: Have this make sense and not look like a chimp wrote it.
                                string[] layerFromList = { "Ground", "Biome", "Topology" };
                                landLayerFrom = EditorGUILayout.Popup("Land Layer To Copy From:", landLayerFrom, layerFromList);
                                switch (landLayerFrom) // Get texture list from the currently selected landLayer.
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        script.groundLayerFrom = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture To Copy From:", script.groundLayerFrom);
                                        textureFrom = 0;
                                        break;
                                    case 1:
                                        script.biomeLayerFrom = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture To Copy From:", script.biomeLayerFrom);
                                        textureFrom = 1;
                                        break;
                                    case 2:
                                        script.topologyLayerFrom = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Texture To Copy From:", script.topologyLayerFrom);
                                        textureFrom = 2;
                                        break;
                                }
                                string[] layerToList = { "Ground", "Biome", "Topology" };
                                landLayerToPaint = EditorGUILayout.Popup("Land Layer To Paint to:", landLayerToPaint, layerToList);
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
                                        script.topologyLayerToPaint = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", script.topologyLayerToPaint);
                                        textureToPaint = 2;
                                        break;
                                }
                                if (GUILayout.Button("Copy textures to new layer"))
                                {
                                    script.textureCopy(layerFromList[landLayerFrom], layerToList[landLayerToPaint], textureFrom, textureToPaint);
                                }
                                break;
                            #endregion
                        }
                        break;
                    default:
                        toolsOptions = 0;
                        break;
                    #endregion
                    #region Layer Tools
                    case 1:
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
                        scale = EditorGUILayout.Slider(scale, 1f, 2000f);
                        GUILayout.Label("Scale of the heightmap generation, \n the further left the less smoothed the terrain will be");
                        if (GUILayout.Button("Generate Perlin Heightmap"))
                        {
                            script.generatePerlinHeightmap(scale);
                        }
                        break;
                        #endregion
                }
                break;
            #endregion
            default:
                mainMenuOptions = 0;
                break;
        }
        #endregion
    }
}
