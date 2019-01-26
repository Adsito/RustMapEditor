using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayerOptions))]
public class LayerOptionEditor : Editor
{
    MapIO mapIO;
	float z1 = 0f;
	float z2 = 500f;
	int thicc = 2;
	float z3 = 0f;
	float z4 = 500f;
	float s = .99f;
	float s1 = .99f;
	float s2 = .995f;
	float t = .5f;
	int p = 100;
	int min = 100;
	int max = 200;
	int w = 8;
	
	int u7 = 510;
	int u8 = 540;
	
	int x5 = 0;
	int y5 = 0;
	float o1 = .66f;
	int scale = 50;
	float contrast = 1.5f;
	bool tog = true;
	bool tog1 = true;
	
	
    public override void OnInspectorGUI()
    {
        LayerOptions script = (LayerOptions)target;
        if (mapIO == null)
            mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();

        GUILayout.Label("Land Options", EditorStyles.boldLabel);

        //GUILayout.Label("Show Bounds", EditorStyles.boldLabel);
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
		
		if (mapIO.landLayer.Equals("Ground"))
		{
			GUILayout.Label("Auto Terrain Paint Tool:", EditorStyles.boldLabel);
			mapIO.terrainLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Terrain to paint:", mapIO.terrainLayer);
			
			GUILayout.Label("Custom height range");
			z3 = EditorGUILayout.FloatField("bottom", z3);
			z4 = EditorGUILayout.FloatField("top", z4);
			
			if (GUILayout.Button("Paint range"))
            {
                mapIO.paintSplatHeight(z3,z4);
            }
			
			GUILayout.Label("Slope Painter");
			GUILayout.Label("Slope threshhold:");
			s1 = GUILayout.HorizontalSlider(s1, 0.99f, 1f);
			s2 = GUILayout.HorizontalSlider(s2, 0.99f, 1f);
			if (GUILayout.Button("Paint slopes"))
			{
				mapIO.paintTerrainSlope(s1, s2);
				//Debug.LogError(s);
			}
			
			if (GUILayout.Button("Paint Perlin Noise"))
			{
				mapIO.paintPerlin(scale, contrast, tog, tog1);
				
			}
			scale = EditorGUILayout.IntField("scale", scale);
			contrast = EditorGUILayout.FloatField("contrast", contrast);
			tog = EditorGUILayout.Toggle("invert", tog);
			
			tog1 = EditorGUILayout.Toggle("paint on biome", tog1);
			mapIO.targetBiomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Target Biome:", mapIO.targetBiomeLayer);
			
			if (GUILayout.Button("Paint crazing"))
			{
				mapIO.paintCrazing(p, min, max);
				//Debug.LogError(s);
			}
			p = EditorGUILayout.IntField("Patches", p);
			min = EditorGUILayout.IntField("Minimum size", min);
			max = EditorGUILayout.IntField("Maximum size", max);
			
			if (GUILayout.Button("Outline terrain"))
			{
				mapIO.paintTerrainOutline(w, o1);
				
			}
			
			w = EditorGUILayout.IntField("Outline Width", w);
			GUILayout.Label("Transparency");
			o1 = GUILayout.HorizontalSlider(o1, .5f, .75f);
			
			if (GUILayout.Button("Pixel Debug"))
			{
				mapIO.debugSplatPixel(x5,y5);	
			}
			x5 = EditorGUILayout.IntField("x", x5);
			y5 = EditorGUILayout.IntField("y", y5);
			if (GUILayout.Button("Rotate CW"))
            {
                mapIO.rotateGroundmap(true);
            }
            if (GUILayout.Button("Rotate CCW"))
            {
                mapIO.rotateGroundmap(false);
            }
        }
		
		if (mapIO.landLayer.Equals("Biome"))
		{
			if (GUILayout.Button("Clear Biomes"))
			{
				mapIO.clearBiome();
			}
			
			mapIO.paintBiomeLayer = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Terrain to paint:", mapIO.paintBiomeLayer);
			u7 = EditorGUILayout.IntField("upper", u7);
			u8 = EditorGUILayout.IntField("lower", u8);
			if (GUILayout.Button("Paint Biome Height"))
			{
				mapIO.paintBiomeHeight(u7, u8);
			}
			
			if (GUILayout.Button("INVERT Biomes, fuck!"))
			{
				mapIO.invertBiome();
			}
            if (GUILayout.Button("Rotate CW"))
            {
                mapIO.rotateBiomemap(true);
            }
            if (GUILayout.Button("Rotate CCW"))
            {
                mapIO.rotateBiomemap(false);
            }
        }
		
		if (mapIO.landLayer.Equals("Alpha"))
		{
			if (GUILayout.Button("Clear Alpha"))
			{
				mapIO.clearAlpha();
			}
			if (GUILayout.Button("Rotate CW"))
            {
                mapIO.rotateAlphamap(true);
            }
            if (GUILayout.Button("Rotate CCW"))
            {
                mapIO.rotateAlphamap(false);
            }
        }
		
        if (mapIO.landLayer.Equals("Topology"))
        {
            GUILayout.Label("Topology Option", EditorStyles.boldLabel);
            mapIO.oldTopologyLayer = mapIO.topologyLayer;
            mapIO.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Select Topology Layer:", mapIO.topologyLayer);
            if (mapIO.topologyLayer != mapIO.oldTopologyLayer)
            {
                //script.saveTopologyLayer();
                mapIO.changeLandLayer();
                Repaint();
            }
            
			if (GUILayout.Button("Invert topology layer"))
            {
                mapIO.invertTopologyLayer();
            }
			
			if (GUILayout.Button("Paint land"))
			{
				mapIO.paintHeight(500, 1000);
			}
			
			if (GUILayout.Button("Paint beaches"))
			{
				mapIO.paintHeight(500, 502);
			}
			
			if (GUILayout.Button("Paint beachfronts"))
			{
				mapIO.paintHeight(502, 504);
			}
			
			if (GUILayout.Button("Paint oceans"))
			{
				mapIO.paintHeight(0, 500);
			}
			
			if (GUILayout.Button("Paint custom height range"))
			{
				mapIO.paintHeight(z1, z2);
			}
			
			if (GUILayout.Button("Erase custom height range"))
			{
				mapIO.eraseHeight(z1, z2);
			}
			
			GUILayout.Label("Custom height range");
			z1 = EditorGUILayout.FloatField("bottom", z1);
			z2 = EditorGUILayout.FloatField("top", z2);
			
			if (GUILayout.Button("Clear topology layer"))
            {
                mapIO.clearTopologyLayer();
            }
			
			if (GUILayout.Button("Copy topology layer"))
			{
				mapIO.copyTopologyLayer();
			}
			
			if (GUILayout.Button("Erase overlapping topology"))
			{
				mapIO.notTopologyLayer();
			}
			
			if (GUILayout.Button("Outline topology layer"))
			{
				mapIO.paintTopologyOutline(thicc);
			}
			thicc = EditorGUILayout.IntField("Thickness:", thicc);
			if (thicc > 6)
				thicc = 5;
			//this can be increased but it just starts to take forever and the results start to look shitty
			
			mapIO.targetTopologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Source Topology Layer:", mapIO.targetTopologyLayer);
			
			GUILayout.Label("Ground to topology painter:");
			mapIO.targetTerrainLayer = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Terrain to paint:", mapIO.targetTerrainLayer);
			
			GUILayout.Label("Threshhold:");
			t = GUILayout.HorizontalSlider(t, 0.0f, 1);
			
			if (GUILayout.Button("Paint terrain"))
			{
				mapIO.terrainToTopology(t);
			}
			
			GUILayout.Label("Slope Painter");
			GUILayout.Label("Slope threshhold:");
			s = GUILayout.HorizontalSlider(s, 0.99f, 1f);
			if (GUILayout.Button("Paint slopes"))
			{
				mapIO.paintSlope(s);
				//Debug.LogError(s);
			}
            GUILayout.Label("You need to rotate each topology layer individually atm :(");
            if (GUILayout.Button("Rotate CW"))
            {
                mapIO.rotateTopologymap(true);
            }
            if (GUILayout.Button("Rotate CCW"))
            {
                mapIO.rotateTopologymap(false);
            }

        }
    }
}
