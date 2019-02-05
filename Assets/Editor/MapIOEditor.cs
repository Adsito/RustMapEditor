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

    int mapSize = 2000;
    float heightToSet = 0f, scale = 0f, offset = 0f;
    bool top = false, left = false, right = false, bottom = false, checkHeight = true, setWaterMap = false;
    public LayerOptionEditor optionEditor;

    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint;

    public override void OnInspectorGUI()
    {
        MapIO script = (MapIO)target;

        GUILayout.Label("New Map", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Map Size");
        mapSize = EditorGUILayout.IntField(mapSize, GUILayout.MinWidth(100));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Create new Map (Overwrite current map)"))
        {
            if (mapSize < 1000)
            {
                Debug.LogError("Use a map size greater than 1000");
                return;
            }
            script.newEmptyTerrain(mapSize);
        }

        GUILayout.Label("Import and Export Map", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Import .map file"))
        {
            loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");

            var blob = new WorldSerialization();
            Debug.Log("Importing map " + loadFile);
            if (loadFile == "")
            {
                Debug.LogError("Empty load path");
                return;
            }
            blob.Load(loadFile);
            script.Load(blob);
        }
        if (GUILayout.Button("Export .map file"))
        {
            saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
            if (saveFile == "")
            {
                Debug.LogError("Empty save path");
            }
            Debug.Log("Exported map " + saveFile);
            script.Save(saveFile);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Bundle file", EditorStyles.boldLabel);
        script.bundleFile = GUILayout.TextField(script.bundleFile, GUILayout.MinWidth(100));
        if (GUILayout.Button("Select Bundle File"))
        {
            script.bundleFile = UnityEditor.EditorUtility.OpenFilePanel("Select Bundle File", script.bundleFile, "");
        }

        /*
                GUILayout.Label("Select Bundle file", EditorStyles.boldLabel);

                bundleFile = GUILayout.TextField(bundleFile);
                if (GUILayout.Button("Select Bundle file (Rust\\Bundles\\Bundles)"))
                {
                    bundleFile = UnityEditor.EditorUtility.OpenFilePanel("Select Bundle file (Rust\\Bundles\\Bundles)", bundleFile, "map");
                }
        */



        GUILayout.Label("Whole Map Options", EditorStyles.boldLabel);

        GUILayout.Label("Land Heightmap Offset (Move Land to correct position)");
        if (GUILayout.Button("Move Map"))
        {
            script.moveHeightmap();
        }
        GUILayout.Label("Land Heightmap Offset (Increase or Decrease the entire Heightmap)");
        offset = EditorGUILayout.FloatField(offset);
        EditorGUILayout.BeginHorizontal();
        checkHeight = EditorGUILayout.ToggleLeft("Only raise if offset is within heightmap bounds ", checkHeight);
        setWaterMap = EditorGUILayout.ToggleLeft("Raise watermap ", setWaterMap);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Offset Heightmap"))
        {
            script.offsetHeightmap(offset, checkHeight, setWaterMap);
        }

        /* Hiding this until I fully implement scaling all map components.
        GUILayout.Label("Land Heightmap Scale");
        script.scale = float.Parse(GUILayout.TextField(script.scale + ""));
        script.scale = GUILayout.HorizontalSlider(script.scale, 0.1f, 2);
        if (GUILayout.Button("Scale Map"))
        {
            script.scaleHeightmap();
            script.scale = 1f;
        }
        */
        GUILayout.Label("Rotating maps takes roughly 1 minute to process \n for the largest maps.", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate 90°")) // Calls every rotate function from MapIO. Rotates 90 degrees.
        {
            script.rotateHeightmap(true);
            script.rotateObjects(true);
            script.changeLayer("Ground");
            script.rotateGroundmap(true);
            script.changeLayer("Biome");
            script.rotateBiomemap(true);
            script.changeLayer("Alpha");
            script.rotateAlphamap(true);
            script.changeLayer("Topology");
            script.rotateAllTopologymap(true);
        }
        if (GUILayout.Button("Rotate 270°")) // Calls every rotate function from MapIO. Rotates 270 degrees.
        {
            script.rotateHeightmap(false);
            script.rotateObjects(false);
            script.changeLayer("Ground");
            script.rotateGroundmap(false);
            script.changeLayer("Biome");
            script.rotateBiomemap(false);
            script.changeLayer("Alpha");
            script.rotateAlphamap(false);
            script.changeLayer("Topology");
            script.rotateAllTopologymap(false);
        }

        /* Hiding these until we have everything flip at once.
        if (GUILayout.Button("Flip"))
        {
            script.flipHeightmap();
        }
        if (GUILayout.Button("Transpose"))
        {
            script.transposeHeightmap();
        }*/

        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Scale of the heightmap generation, \n the further left the less smoothed the terrain will be");
        scale = EditorGUILayout.Slider(scale, 1f, 2000f);
        if (GUILayout.Button("Generate Perlin Heightmap"))
        {
            script.generatePerlinHeightmap(scale);
        }
        if (GUILayout.Button("Paint Default Topologies"))
        {
            script.autoGenerateTopology(false);
        }
        if (GUILayout.Button("Wipe Layers & Paint Default Topologies"))
        {
            script.autoGenerateTopology(true);
        }
        if (GUILayout.Button("Paint Default Ground Textures"))
        {
            script.autoGenerateGround();
        }
        GUILayout.Label("This paints the textureToPaint on the layerToPaint \n if the layerFrom has the textureFrom in that coordinate.\n Eg. If layerFrom = Ground, textureToCopyFrom = Snow and \n" +
            "layerToPaint = Topology, textureToPaint = Decor \n it would paint decor wherever there was snow"); //ToDo: Have this make sense and not look like a chimp wrote it.
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

        GUILayout.Label("This paints the ground texture to rock wherever the alpha \n is removing the terrain ingame. \n Used to prevent floating grass.");
        if (GUILayout.Button("Debug Alpha Textures"))
        {
            script.changeLayer("Ground");
            script.alphaDebug("Ground");
        }
        GUILayout.Label("This sets the very edges of the map to this height.");
        heightToSet = EditorGUILayout.FloatField(heightToSet);
        EditorGUILayout.BeginHorizontal();
        top = EditorGUILayout.ToggleLeft("Top ", top);
        left = EditorGUILayout.ToggleLeft("Left ", left);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        bottom = EditorGUILayout.ToggleLeft("Bottom ", bottom);
        right = EditorGUILayout.ToggleLeft("Right ", right);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Set Edge Pixel Height"))
        {
            script.setEdgePixel(heightToSet, top, left, right, bottom);
        }
    }
}
