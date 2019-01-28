using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayerOptions))]
public class LayerOptionEditor : Editor
{
    MapIO mapIO;
    float z1 = 0f, z2 = 500f, opacity = 0.50f, slope = 0f;
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
            slope = GUILayout.HorizontalSlider(slope, 0.99f, 1f);
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Ground", slope, 0);
            }
            GUILayout.Label("Custom height range");
            z1 = EditorGUILayout.FloatField("bottom", z1);
            z2 = EditorGUILayout.FloatField("top", z2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight(z1, z2, opacity, "Ground", 0);
            }
            if (GUILayout.Button("Erase range"))
            {
                mapIO.paintHeight(z1, z2, opacity, "Ground", 1);
            }
            EditorGUILayout.EndHorizontal();
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
            slope = GUILayout.HorizontalSlider(slope, 0.99f, 1f);
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Biome", slope, 0);
            }
            GUILayout.Label("Custom height range");
            z1 = EditorGUILayout.FloatField("bottom", z1);
            z2 = EditorGUILayout.FloatField("top", z2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight(z1, z2, float.MaxValue, "Biome", 0);
            }
            if (GUILayout.Button("Erase range"))
            {
                mapIO.paintHeight(z1, z2, float.MaxValue, "Biome", 1);
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Alpha Layer
        if (mapIO.landLayer.Equals("Alpha"))
        {
            GUILayout.Label("Alpha Layer Paint", EditorStyles.boldLabel);
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
            z1 = EditorGUILayout.FloatField("bottom", z1);
            z2 = EditorGUILayout.FloatField("top", z2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight(z1, z2, float.MaxValue, "Alpha", 0);
            }
            if (GUILayout.Button("Erase range"))
            {
                mapIO.paintHeight(z1, z2, float.MaxValue, "Alpha", 1);
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Clear alpha layer"))
            {
                mapIO.clearLayer("Alpha");
            }
        }
        #endregion

        #region Topology Layer
        if (mapIO.landLayer.Equals("Topology"))
        {
            GUILayout.Label("Topology Layer Paint", EditorStyles.boldLabel);
            mapIO.oldTopologyLayer = mapIO.topologyLayer;
            mapIO.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Select Topology Layer:", mapIO.topologyLayer); 
            if (mapIO.topologyLayer != mapIO.oldTopologyLayer) 
            {
                mapIO.changeLandLayer();
                Repaint();
            }
            GUILayout.Label("Each Topology layer rotates independent of each other");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rotate 90°"))
            {
                mapIO.rotateTopologymap(true);
            }
            if (GUILayout.Button("Rotate 270°"))
            {
                mapIO.rotateTopologymap(false);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Slope threshhold:");
            slope = GUILayout.HorizontalSlider(slope, 0.99f, 1f);
            if (GUILayout.Button("Paint slopes"))
            {
                mapIO.paintSlope("Topology", slope, 0);
            }
            GUILayout.Label("Custom height range");
            z1 = EditorGUILayout.FloatField("bottom", z1);
            z2 = EditorGUILayout.FloatField("top", z2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Paint range"))
            {
                mapIO.paintHeight(z1, z2, float.MaxValue,"Topology", 0); 
            }
            if (GUILayout.Button("Erase range"))
            {
                mapIO.paintHeight(z1, z2, float.MaxValue, "Topology", 1);
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Clear topology layer"))
            {
                mapIO.clearLayer("Topology");
            }
        }
        #endregion
    }
}
