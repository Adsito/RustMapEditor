/*
 * File: IslandGeneratorEditor.cs
 * Author: Juuso Tenhunen
 * Date: 24.3.2016
 * Modified: 24.3.2016
 * 
 * Summary:
 * Custom Editor script for Unity3D to be
 * used by my IslandGenerator script IslandGenerator.cs
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//Disable obsolete warnings for Undo.RegisterUndo().
//New Undo.RecordObject() crashes Unity if used on Terrain.
#pragma warning disable 0618

[CustomEditor(typeof(IslandGenerator))]
public class IslandGeneratorEditor : Editor {
    private int terrainReso;

    //This is for checking if component is attached to a GameObject with Terrain
    private bool isTerrain = true;

    //List to store last used Seeds for Randomization.
    private List<string> usedSeeds;
    //Index of selected item on the Used Seeds list.
    private int selected = 0;

    //Boolean to check if shores are finished calculating.
    private bool shoresCalculated = false;

    private GUIStyle centerLabel = new GUIStyle();

    //Objects for TerrainData to be used in Undo
    private static Terrain t;
    private Object terraini;

    //GUIContents for buttons/textfields/intfields, so they can have Tooltips.
    private GUIContent seedContent;
    private GUIContent useSeedContent;
    private GUIContent randomFillContent;
    private GUIContent maxRadiusContent;
    private GUIContent smoothTimesContent;
    private GUIContent generateContent;
    private GUIContent calculateShoresContent;
    private GUIContent addShoresContent;
    private GUIContent smoothOnceContent;
    private GUIContent heightContent;
    private GUIContent scaleContent;
    private GUIContent addPerlinContent;
    private GUIContent lowerSeaContent;

    /// <summary>
    /// This is run everytime the inspector is updated.
    /// </summary>
    void OnEnable()
    {
        //Here we check whether or not there is a Terrain component with TerrainData Asset in the GameObject.
        if (t == null || terraini == null || !isTerrain)
        {
            //This will catch an Exception if there is no Terrain component.
            //However, it will pass if the component doesn't have TerrainData Asset. The if-statement handles that.
            try
            {
                t = (Selection.activeGameObject as GameObject).GetComponent<Terrain>() as Terrain;
                terraini = t.terrainData as Object;
                if (terraini == null)
                {
                    Debug.LogError("Terrain doesn't have TerrainData Asset! Please add TerrainData to Terrain Component!");
                    isTerrain = false;
                }
                else
                {
                    terrainReso = t.terrainData.heightmapResolution;
                    isTerrain = true;
                }

            }
            catch (System.Exception e)
            {
                if (e is UnityEngine.MissingComponentException)
                {
                    isTerrain = false;
                    Debug.LogError(e.GetType() + ": NO TERRAIN FOUND. Please add this Component to Terrain Gameobject");
                }
                if (e is System.NullReferenceException)
                {
                    Debug.LogWarning("Unknown Exception if PLAY is hit. Doesn't seem to affect the Island Generation, so this can be ignored.\nException details: "+e);
                }
            }
        }

        //GUIStyle with center alignment and a slightly bigger font for Titles,
        centerLabel.alignment = TextAnchor.UpperCenter;
        centerLabel.fontSize = 12;

        //Tooltips for the different layout components.
        seedContent = new GUIContent("Seed", "The Seed used for Random Generation");
        useSeedContent = new GUIContent("Use Random Seed", "Whether to random the seed or use your own seed");
        randomFillContent = new GUIContent("Random Fill Percent", "How dense the initial noise is. Lower values equal larger islands. Prefered values 40-60");
        maxRadiusContent = new GUIContent("Shore Steepnes", "How steep the shores are. Lower values have steep shores and higher values have more gentle shores. Higher values affect calculation time");
        smoothTimesContent = new GUIContent("Smooth Times", "How many iterations of overall Smooth-function is performed when shores are added");
        generateContent = new GUIContent("Generate Map", "Generate the basic formation of island(s)");
        calculateShoresContent = new GUIContent("Calculate Shores", "Begin to calculate shores to your base map");
        addShoresContent = new GUIContent("Add Shores", "Adds shores to the map");
        smoothOnceContent = new GUIContent("Smooth Once", "Performs a single Smooth-function");
        heightContent = new GUIContent("Perlin noise height", "Height of perlin noise bumps. Recommended values under 0.05");
        scaleContent = new GUIContent("Perlin noise scale", "How dense the perlin noise bumps are. Higher values mean more bumps");
        addPerlinContent = new GUIContent("Add Perlin Noise", "Apply the Perlin Noise with the parameters above");
        lowerSeaContent = new GUIContent("Lower Seafloor", "Adding multiple Perlin Noises can raise the overall height of the map. This button resets the lowest point of the map back to 0 level. Multiple uses can be used to lower the whole map");
        
    }

    /// <summary>
    /// This overrides the default InspectorGUI
    /// </summary>
    public override void OnInspectorGUI()
    {
        //Here we check once more if the Terrain has a TerrainData attached to it.
        //OnEnable() doesn't trigger if you just drag&drop the data to the inspector and
        //the Island Generator GUI doesn't show up. OnInspectorGUI() triggers more often,
        //so here we'll start checking whether or not Terrain has TerrainData asset in it.
        if (terraini == null && t != null && !isTerrain)
        {
            terraini = t.terrainData as Object;

            //Terraindata detected, we can draw the GUI.
            if (terraini != null)
            {
                terrainReso = t.terrainData.heightmapResolution;
                isTerrain = true;
            }

        }

        //If there is no Terrain or TerrainData, this draws an error message in the Inspector
        //rather than the actual Generator GUI
        if (!isTerrain)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("No Terrain Found!", centerLabel);
            EditorGUILayout.LabelField("Please add Terrain Component\nto this Game Object.", centerLabel);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        //If the Terrain AND TerrainData is found, the actual Generator GUI is drawn
        else
        {

            terrainReso = t.terrainData.heightmapResolution;

            //This is the actual Generator script where we can input and output values from the GUI
            IslandGenerator script = (IslandGenerator)target;

            /***BEGIN GENERATOR BOX***/
            EditorGUILayout.BeginVertical("box");

            //If the Thread th1 is still running, it means shore calculation is still in progress,
            //so further map generation should be disabled for that time
            if (script.th1 != null && script.th1.ThreadState == System.Threading.ThreadState.Running)
                GUI.enabled = false;

            EditorGUILayout.LabelField("Generate Island", centerLabel);

            usedSeeds = script.usedSeeds;

            //This checks if user selects a seed from the Used Seeds list and replaces the current seed with it.
            EditorGUI.BeginChangeCheck();
            selected = EditorGUILayout.Popup("Used Seeds", selected, usedSeeds.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                script.seed = usedSeeds[selected];
            }

            script.seed = EditorGUILayout.TextField(seedContent, script.seed);
            script.useRandomSeed = EditorGUILayout.Toggle(useSeedContent, script.useRandomSeed);
            script.randomFillPercent = EditorGUILayout.IntSlider(randomFillContent, script.randomFillPercent, 30, 70);

            //If the shore radius changes, shores needs to be calculated again before adding them,
            //so the Add Shores Button will be disabled if the value here changes.
            EditorGUI.BeginChangeCheck();
            script.maxRadius = EditorGUILayout.IntSlider(maxRadiusContent, script.maxRadius, 25, 200);
            if (EditorGUI.EndChangeCheck())
            {
                shoresCalculated = false;
                script.prog = "0%";
            }

            script.mapSmoothTimes = EditorGUILayout.IntField(smoothTimesContent, script.mapSmoothTimes);

            //Button to Generate basemap.
            if (GUILayout.Button(generateContent))
            {
                if (terrainReso < 129)
                {
                    Debug.LogWarning("Terrain Resolution is too small! Please use resolutions of 129 and higher.");
                }
                else
                {
                    Undo.RegisterUndo(terraini, "Generated New Map");
                    shoresCalculated = false;
                    script.StartGeneration();
                    script.prog = "0%";

                    //If the Used Seeds list doesn't contains current seed already
                    //we add that seed to the list.
                    if (!usedSeeds.Contains(script.seed))
                        usedSeeds.Insert(0, script.seed);
                    //Else if the seed exists in the list (e.g. same custom seed is used twice)
                    //we move the seed to the top.
                    else
                    {
                        usedSeeds.RemoveAt(usedSeeds.IndexOf(script.seed));
                        usedSeeds.Insert(0, script.seed);
                    }

                    //If there is more than 10 seeds, remove the oldest seed.
                    if (usedSeeds.Count > 10)
                        usedSeeds.RemoveAt(usedSeeds.Count - 1);

                    //After the button is pressed, change focus of the list to the latest seed.
                    selected = 0;
                }
            }

            EditorGUILayout.LabelField("Status", script.prog);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(calculateShoresContent))
            {
                script.CalculateShores();
            }

            //Progress of shore calculation is handled in the actual script, and when it reaches 100%
            //the "Add Shores" button can be enabled.
            if (script.prog == "100%")
                shoresCalculated = true;

            if (shoresCalculated && GUILayout.Button(addShoresContent))
            {
                Undo.RegisterUndo(terraini, "Added Shores");
                script.SmoothShores();

                //Smooths the map as many times as the user defines.
                for (int i = 0; i < script.mapSmoothTimes; i++)
                {
                    script.BlendHeights();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(smoothOnceContent))
            {
                Undo.RegisterUndo(terraini, "Applied Smooth");
                script.BlendHeights();
            }

            GUI.enabled = true;

            if ((script.th1 != null && script.th1.ThreadState == System.Threading.ThreadState.Running) && GUILayout.Button("Abort"))
            {
                script.aborted = true;
                shoresCalculated = false;
            }

            EditorGUILayout.EndVertical();
            /***END GENERATOR BOX***/

            EditorGUILayout.Space();

            /***BEGIN PERLIN NOISE BOX***/
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Add Perlin Noise", centerLabel);
            script.perlinHeight = EditorGUILayout.FloatField(heightContent, script.perlinHeight);
            script.perlinScale = EditorGUILayout.FloatField(scaleContent, script.perlinScale);

            if (GUILayout.Button(addPerlinContent))
            {
                Undo.RegisterUndo(terraini, "Applied Perlin Noise");
                script.PerlinNoise();
            }
            if (GUILayout.Button(lowerSeaContent))
            {
                Undo.RegisterUndo(terraini, "Lowered Heightmap");
                script.ResetSeaFloor();
            }
            EditorGUILayout.EndVertical();
            /***END PERLIN NOISE BOX***/

            EditorGUILayout.Space();

            /***BEGIN PRESETS BOX***/
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Some preset noises  ", centerLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Many Low Bumps"))
            {
                Undo.RegisterUndo(terraini, "Applied Perlin Noise");
                script.perlinHeight = 0.001f;
                script.perlinScale = 75;
                script.PerlinNoise();
            }
            if (GUILayout.Button("Many High Bumps"))
            {
                Undo.RegisterUndo(terraini, "Applied Perlin Noise");
                script.perlinHeight = 0.005f;
                script.perlinScale = 100;
                script.PerlinNoise();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Few Low Bumps"))
            {
                Undo.RegisterUndo(terraini, "Applied Perlin Noise");
                script.perlinHeight = 0.01f;
                script.perlinScale = 20;
                script.PerlinNoise();
            }
            if (GUILayout.Button("Few High Bumps"))
            {
                Undo.RegisterUndo(terraini, "Applied Perlin Noise");
                script.perlinHeight = 0.015f;
                script.perlinScale = 25;
                script.PerlinNoise();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            /***END BOX***/
        }
    }
}
