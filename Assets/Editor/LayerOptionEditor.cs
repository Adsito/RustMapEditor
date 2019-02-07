using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayerOptions))]
public class LayerOptionEditor : Editor
{
    MapIO mapIO;
    float y1 = 0f, y2 = 500f, opacity = 0.50f, slopeLow = 40f, slopeHigh = 60f, scale = 50f;
    float minBlendLow = 25f, maxBlendLow = 40f, minBlendHigh = 60f, maxBlendHigh = 75f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    bool blendSlopes = false;
    #region All Layers
    public override void OnInspectorGUI()
    {
        LayerOptions script = (LayerOptions)target;
        if (mapIO == null)
            mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

        GUILayout.Label("Land Layer To Select", EditorStyles.boldLabel);
        //LayerOptions.showBounds = EditorGUILayout.Toggle("Show Bounds", LayerOptions.showBounds);

        string oldLandLayer = mapIO.landLayer;
        string[] options = { "Ground", "Biome", "Alpha", "Topology" };
        mapIO.landSelectIndex = EditorGUILayout.Popup("Select Land Layer:", mapIO.landSelectIndex, options);
        mapIO.landLayer = options[mapIO.landSelectIndex];
        if (mapIO.landLayer != oldLandLayer)
        {
            mapIO.changeLandLayer();
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
        #endregion
        #region Ground Layer
        if (mapIO.landLayer.Equals("Ground"))
        {
            GUILayout.Label("Ground Layer Paint", EditorStyles.boldLabel);
            mapIO.terrainLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture to paint: ", mapIO.terrainLayer);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rotate 90°"))
            {
                mapIO.rotateGroundmap(true);
            }
            if (GUILayout.Button("Rotate 270°"))
            {
                mapIO.rotateGroundmap(false);
            }
            EditorGUILayout.EndHorizontal();
            //GUILayout.Label("Texture Opacity: " + opacity + " %");
            //opacity = GUILayout.HorizontalSlider(opacity, 0, 1);
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
            }
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Ground", slopeLow, slopeHigh, minBlendLow , maxBlendHigh, 0);
            }
            GUILayout.Label("Custom height range");
            y1 = EditorGUILayout.FloatField("bottom", y1);
            y2 = EditorGUILayout.FloatField("top", y2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight("Ground", y1, y2, opacity, 0);
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Paint Whole Layer"))
            {
                mapIO.paintLayer("Ground", 0);
            }
            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Ground textures");
            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
            scale = GUILayout.HorizontalSlider(scale, 10f, 2000f);
            if (GUILayout.Button("Generate random Ground map"))
            {
                mapIO.generateEightLayersNoise("Ground", scale);
            }
        }
        #endregion

        #region Biome Layer
        if (mapIO.landLayer.Equals("Biome"))
        {
            GUILayout.Label("Biome Layer Paint", EditorStyles.boldLabel);
            mapIO.biomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Select biome to paint:", mapIO.biomeLayer);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rotate 90°"))
            {
                mapIO.rotateBiomemap(true);
            }
            if (GUILayout.Button("Rotate 270°"))
            {
                mapIO.rotateBiomemap(false);
            }
            EditorGUILayout.EndHorizontal();
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
            }
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Biome", slopeLow, slopeHigh, minBlendLow, maxBlendHigh, 0);
            }
            GUILayout.Label("Custom height range");
            y1 = EditorGUILayout.FloatField("bottom", y1);
            y2 = EditorGUILayout.FloatField("top", y2);
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight("Biome", y1, y2, float.MaxValue, 0);
            }
            z1 = EditorGUILayout.IntField("From Z ", z1);
            z2 = EditorGUILayout.IntField("To Z ", z2);
            x1 = EditorGUILayout.IntField("From X ", x1);
            x2 = EditorGUILayout.IntField("To X ", x2);
            if (GUILayout.Button("Paint Area"))
            {
                mapIO.paintArea("Biome", z1, z2, x1, x2, 0);
            }
            if (GUILayout.Button("Paint Whole Layer"))
            {
                mapIO.paintLayer("Biome", 0);
            }
            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Biomes");
            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
            scale = GUILayout.HorizontalSlider(scale, 500f, 5000f);
            if (GUILayout.Button("Generate random Biome map"))
            {
                mapIO.generateFourLayersNoise("Biome", scale);
            }
        }
        #endregion

        #region Alpha Layer
        if (mapIO.landLayer.Equals("Alpha"))
        {
            GUILayout.Label("Alpha Layer Paint", EditorStyles.boldLabel);
            GUILayout.Label("Green = Terrain Visible, Purple = Terrain Invisible", EditorStyles.boldLabel);
            GUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rotate 90°"))
            {
                mapIO.rotateAlphamap(true);
            }
            if (GUILayout.Button("Rotate 270°"))
            {
                mapIO.rotateAlphamap(false);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Custom height range");
            y1 = EditorGUILayout.FloatField("bottom", y1);
            y2 = EditorGUILayout.FloatField("top", y2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight("Alpha", y1, y2, float.MaxValue, 0);
            }
            if (GUILayout.Button("Erase range"))
            {
                mapIO.paintHeight("Alpha", y1, y2, float.MaxValue, 1);
            }
            EditorGUILayout.EndHorizontal();
            z1 = EditorGUILayout.IntField("From Z ", z1);
            z2 = EditorGUILayout.IntField("To Z ", z2);
            x1 = EditorGUILayout.IntField("From X ", x1);
            x2 = EditorGUILayout.IntField("To X ", x2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Area"))
            {
                mapIO.paintArea("Alpha", z1, z2, x1, x2, 0);
            }
            if (GUILayout.Button("Erase Area"))
            {
                mapIO.paintArea("Alpha", z1, z2, x1, x2, 1);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Whole layer"))
            {
                mapIO.paintLayer("Alpha", 0);
            }
            if (GUILayout.Button("Clear Whole layer"))
            {
                mapIO.clearLayer("Alpha");
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Topology Layer
        if (mapIO.landLayer.Equals("Topology"))
        {
            GUILayout.Label("Topology Layer Paint", EditorStyles.boldLabel);
            GUILayout.Label("Green = Topology Active, Purple = Topology Inactive", EditorStyles.boldLabel);
            GUILayout.Space(10f);
            mapIO.oldTopologyLayer = mapIO.topologyLayer;
            mapIO.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Select Topology Layer:", mapIO.topologyLayer);
            if (mapIO.topologyLayer != mapIO.oldTopologyLayer)
            {
                mapIO.changeLandLayer();
                Repaint();
            }
            GUILayout.Label("Rotate seperately or all at once");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rotate 90°"))
            {
                mapIO.rotateTopologymap(true);
            }
            if (GUILayout.Button("Rotate 270°"))
            {
                mapIO.rotateTopologymap(false);
            }
            if (GUILayout.Button("Rotate all 90°"))
            {
                mapIO.rotateAllTopologymap(true);
            }
            if (GUILayout.Button("Rotate all 270°"))
            {
                mapIO.rotateAllTopologymap(true);
            }
            EditorGUILayout.EndHorizontal();
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
            }
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Topology", slopeLow, slopeHigh, minBlendLow, maxBlendHigh, 0);
            }
            GUILayout.Label("Custom height range");
            y1 = EditorGUILayout.FloatField("bottom", y1);
            y2 = EditorGUILayout.FloatField("top", y2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight("Topology", y1, y2, float.MaxValue, 0); 
            }
            if (GUILayout.Button("Erase range"))
            {
                mapIO.paintHeight("Topology", y1, y2, float.MaxValue, 1);
            }
            EditorGUILayout.EndHorizontal();
            z1 = EditorGUILayout.IntField("From Z ", z1);
            z2 = EditorGUILayout.IntField("To Z ", z2);
            x1 = EditorGUILayout.IntField("From X ", x1);
            x2 = EditorGUILayout.IntField("To X ", x2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Area"))
            {
                mapIO.paintArea("Topology", z1, z2, x1, x2, 0);
            }
            if (GUILayout.Button("Erase Area"))
            {
                mapIO.paintArea("Topology", z1, z2, x1, x2, 1);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint Whole layer"))
            {
                mapIO.paintLayer("Topology", 0);
            }
            if (GUILayout.Button("Clear Whole layer"))
            {
                mapIO.clearLayer("Topology");
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Topology");
            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
            scale = GUILayout.HorizontalSlider(scale, 10f, 500f);
            if (GUILayout.Button("Generate random topology map"))
            {
                mapIO.generateTwoLayersNoise("Topology", scale);
            }
        }
        #endregion
    }
}
