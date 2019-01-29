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
    public LayerOptionEditor optionEditor;

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
            if(mapSize < 1000)
            {
                Debug.LogError("Use a map size greater than 1000");
                return;
            }
            script.newEmptyTerrain(mapSize);   
        }

        GUILayout.Label("Load Map", EditorStyles.boldLabel);
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

        GUILayout.Label("Save Map", EditorStyles.boldLabel);
        if (GUILayout.Button("Export .map file"))
        {
            saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
            if(saveFile == "")
            {
                Debug.LogError("Empty save path");
            }
            Debug.Log("Exported map " + saveFile);
            script.Save(saveFile);
        }


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



        GUILayout.Label("Heightmap Options", EditorStyles.boldLabel);

        GUILayout.Label("Land Heightmap Offset (Move Land to correct position)");
        if (GUILayout.Button("Click here to bake heightmap values"))
        {
            script.offsetHeightmap();
        }

        GUILayout.Label("Land Heightmap Scale");
        script.scale = float.Parse(GUILayout.TextField(script.scale + ""));
        script.scale = GUILayout.HorizontalSlider(script.scale, 0.1f, 2);
        if (GUILayout.Button("Scale Map"))
        {
            script.scaleHeightmap();
            script.scale = 1f;
        }

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
            script.rotateTopologymap(true);
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
            script.rotateTopologymap(false);
        }

        if (GUILayout.Button("Flip"))
        {
            script.flipHeightmap();
        }
        if (GUILayout.Button("Transpose"))
        {
            script.transposeHeightmap();
        }
        EditorGUILayout.EndHorizontal();
        /*if (GUILayout.Button("Generate Default Topologies"))
        {
            script.autoGenerateTopology(false);
        }*/
        if (GUILayout.Button("Wipe Layers then Auto Generate"))
        {
            script.autoGenerateTopology(true);
        }
    }
}
