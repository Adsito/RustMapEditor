using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapIO))]
public class MapIOEditor : Editor
{
    string loadFile = "";
	private string bundlename = string.Empty;
    string saveFile = "";
    string mapName = "";
	int x=0;
	int y=0;
	int x1=0;
	int y1=0;
	int l=0;
	int w=0;
    string bundleFile = "No bundle file selected";

	int dsR = 37;
	int dsH = 100;
	int dsW = 175;
	
	int layer = 9;
	int period = 75;
	float scaley = 75f;
	int znud = 0;
	
    public override void OnInspectorGUI()
    {
        MapIO script = (MapIO)target;

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
			#if UNITY_EDITOR
			if (GUILayout.Button("Browse"))
			{
				bundlename = UnityEditor.EditorUtility.OpenFilePanel("Select Bundle File", bundlename, "");

				if (script.prefabs != null)
				{
					script.prefabs.Dispose();
					script.prefabs = null;
				}

				script.prefabs = new PrefabLookup(bundlename);
			}
			#endif

			if (GUILayout.Button("Load"))
			{
				if (script.prefabs != null)
				{
					script.prefabs.Dispose();
					script.prefabs = null;
				}

				script.prefabs = new PrefabLookup(bundlename);
			}

        znud = EditorGUILayout.IntField("z=", znud);
        if (GUILayout.Button("Adjust height(adds or subtracts terrains height not the terrain)"))
		{
			script.zNudge(znud);
		}

        GUILayout.Label("Don't move any objects in the editor after loading map or rotating won't work", EditorStyles.boldLabel);

        GUILayout.Label("Land Heightmap Offset (Move Land to correct position)");
        
		
		
		if (GUILayout.Button("Click here to bake heightmap values"))
        {
            script.offsetHeightmap();
        }

		
		
		
		
		GUILayout.Label("Paste Maps / areas to:");
       
		y = EditorGUILayout.IntField("X", y);
		x = EditorGUILayout.IntField("Z", x);
		
		if (GUILayout.Button("Import and Paste Whole Map"))
        {
            loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File to Paste", loadFile, "map");
            var blob = new WorldSerialization();
			blob.Load(loadFile);
			script.Paste(blob, x, y);
        }
		
		if (GUILayout.Button("Import Monument"))
		{
			y = (int)GameObject.Find("MapIO").transform.position.x;
			x = (int)GameObject.Find("MapIO").transform.position.z;
			loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File to Paste", loadFile, "map");
            var blob = new WorldSerialization();
			blob.Load(loadFile);
			script.pasteMonument(blob, x, y, l);
		}
		if (GUILayout.Button("Get CHORDS!"))
        {
			y = (int)GameObject.Find("MapIO").transform.position.x;
			x = (int)GameObject.Find("MapIO").transform.position.z;
        }
		GUILayout.Label("Area dimensions");
		l = EditorGUILayout.IntField("length", l);
		w = EditorGUILayout.IntField("width", w);
		
		
		GUILayout.Label("Import Area from");
		
		y1 = EditorGUILayout.IntField("X", y1);
		x1 = EditorGUILayout.IntField("Z", x1);
		
		
		
		if (GUILayout.Button("Import and Paste Area"))
        {
            loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File to Paste", loadFile, "map");
            var blob = new WorldSerialization();
			blob.Load(loadFile);
			script.Paste(blob, x, y, x1, y1, l, w);
        }
		
        GUILayout.Label("Land Heightmap Scale");
        script.scale = float.Parse(GUILayout.TextField(script.scale + ""));
        script.scale = GUILayout.HorizontalSlider(script.scale, 0.1f, 2);
        if (GUILayout.Button("Scale Map"))
        {
            script.scaleHeightmap();
            script.scale = 1f;
        }

		if (GUILayout.Button("Christmasize Terrain:"))
		{
			script.christmasize();
		}
		
		if (GUILayout.Button("Diamond Square Terrain"))
		{
			script.diamondSquareNoise(dsR, dsH, dsW);
		}
		
		dsR = EditorGUILayout.IntField("Roughness", dsR);
		dsH = EditorGUILayout.IntField("Height", dsH);
		dsW = EditorGUILayout.IntField("Weight", dsW);
		
		
		
		if (GUILayout.Button("Perlin Average Terrain:"))
		{
			script.perlinTerrain(layer, period, scaley);
		}
		
		if (GUILayout.Button("Perlin Less Average Terrain:"))
		{
			script.perlinOctave(layer, period, scaley);
		}
		
		if (GUILayout.Button("Perlin Whatever Terrain:"))
		{
			script.perlinHatred(layer, period, scaley);
		}
		
		
		layer = EditorGUILayout.IntField("layers:", layer);
		period = EditorGUILayout.IntField("period:", period);
		scaley = EditorGUILayout.FloatField("scale:", scaley);
		
		if (GUILayout.Button("Borders"))
		{
			script.bordering();
		}

        GUILayout.Label("Heightmap Options");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate CW"))
        {
            script.rotateHeightmap(true);
            script.rotateObjects(true);
        }
        if (GUILayout.Button("Rotate CCW"))
        {
            script.rotateHeightmap(false);
            script.rotateObjects(false);
        }

        if (GUILayout.Button("Shitflip Heightmap"))
        {
            script.flipHeightmap();
        }
        if (GUILayout.Button("Transpose Heightmap"))
        {
            script.transposeHeightmap();
        }
        EditorGUILayout.EndHorizontal();
    }
}
