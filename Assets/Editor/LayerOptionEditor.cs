using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayerOptions))]
public class LayerOptionEditor : Editor
{
    MapIO mapIO;
    float y1 = 0f, y2 = 500f, opacity = 0.50f, slopeLow = 0.995f, slopeHigh = 0.995f, scale = 50f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    #region All Layers
    public override void OnInspectorGUI()
    {
        LayerOptions script = (LayerOptions)target;
        if (mapIO == null)
            mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

        GUILayout.Label("Land Options", EditorStyles.boldLabel);
        LayerOptions.showBounds = EditorGUILayout.Toggle("Show Bounds", LayerOptions.showBounds);

        string oldLandLayer = mapIO.landLayer;
        string[] options = { "Ground", "Biome", "Alpha", "Topology" };
        mapIO.landSelectIndex = EditorGUILayout.Popup("Select Land Layer:", mapIO.landSelectIndex, options);
        mapIO.landLayer = options[mapIO.landSelectIndex];
        if (mapIO.landLayer != oldLandLayer)
        {
            mapIO.changeLandLayer();
            Repaint();
        }
        if (slopeLow > slopeHigh)
        {
            slopeHigh += 0.00005f;
            slopeLow = slopeHigh - 0.00005f;
            if (slopeHigh > 1f)
            {
                slopeHigh = 1f;
            }
            if (slopeLow < 0f)
            {
                slopeLow = 0f;
            }
        }
        #endregion
        
        #region Ground Layer
        if (mapIO.landLayer.Equals("Ground"))
        {
            GUILayout.Label("Ground Layer Paint", EditorStyles.boldLabel);
            mapIO.terrainLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Select terrain texture to paint:", mapIO.terrainLayer);
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
            GUILayout.Label("Slope threshhold:");
            slopeLow = GUILayout.HorizontalSlider(slopeLow, 0.99f, 1f);
            slopeHigh = GUILayout.HorizontalSlider(slopeHigh, 0.99f, 1f);
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Ground", slopeLow, slopeHigh, 0);
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
            GUILayout.Label("Slope threshhold:");
            slopeLow = GUILayout.HorizontalSlider(slopeLow, 0.99f, 1f);
            slopeHigh = GUILayout.HorizontalSlider(slopeHigh, 0.99f, 1f);
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Biome", slopeLow, slopeHigh, 0);
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
            GUILayout.Label("Slope threshhold:");
            slopeLow = GUILayout.HorizontalSlider(slopeLow, 0.99f, 1f);
            slopeHigh = GUILayout.HorizontalSlider(slopeHigh, 0.99f, 1f);
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Topology", slopeLow, slopeHigh, 0);
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
            GUILayout.Label("Noise scale, the futher left the smaller the blobs");
            scale = GUILayout.HorizontalSlider(scale, 10f, 500f);
            if (GUILayout.Button("Generate random topology map"))
            {
                mapIO.generateTwoLayersNoise("Topology", scale, 0);
            }
        }
        #endregion
    }
}
