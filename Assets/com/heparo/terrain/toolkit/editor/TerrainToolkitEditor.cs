/*---------------------------------------------------------------------------*/
/* INFOS / REFERENCES */
/*---------------------------------------------------------------------------*/
/* This script is part of the TerrainToolkit2017 asset from the Unity Asset Store 
 * Please refer to : https://unity3d.com/asset-store
 *
 * Author : Hervé PARONNAUD 
 * Contact / Support : contact@heparo.com
 *
 * Asset Store Terms of Service and EULA
 * Please refer to : http://unity3d.com/company/legal/as_terms
 *
 *
 * 
 * WARNING ! WARNING ! WARNING !
 *  
 * The present version of the Unity Terrain Toolkit is based on :
 * 
 * The Unity Terrain Toolkit V1.0.2 from the Unity Summer of Code 2009
 * All code by Sándor Moldán, except where noted.
 * Contains an implementation of Perlin noise by Daniel Greenheck.
 * Contains an implementation of the Diamond-Square algorithm by Jim George.
 * 
 * The present version of the Terrain Toolkit is an adaptation and enhancement 
 * of the original tool for the newer versions of Unity.
 *
 * Beyond the necessary code adaptation, a cosmetic refactoring was performed 
 * (GUI mainly but also core code). This being said, the whole toolkit works 
 * as the original toolkit was designed
 *
 * The original package (Unity 4 compatible) is bundled in the 'original' folder
 *
 * The original toolkit was FREE, this version too !
 *
 * Enjoy and keep it a popular tool !
 *
 *//*------------------------------------------------------------------------*/
 
/*---------------------------------------------------------------------------*/
/* REQUIREMENTS */
/*---------------------------------------------------------------------------*/

/*---------------------------------------------------------------------------*/
/* IMPORTS */
/*---------------------------------------------------------------------------*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*---------------------------------------------------------------------------*/
/* NAMESPACE */
/*---------------------------------------------------------------------------*/

namespace com.heparo.terrain.toolkit.editor {
	
/*---------------------------------------------------------------------------*/
/* START CLASS */
/*---------------------------------------------------------------------------*/

[CustomEditor(typeof(TerrainToolkit))]

public class TerrainToolkitEditor : Editor {
	
/*---------------------------------------------------------------------------*/
/* MEMBERS */
/*---------------------------------------------------------------------------*/

		// ------------------------------------------------------------------------
		// STRINGS CONSTANTS
		
		public const string EMPTY = "";
		
		// ------------------------------------------------------------------------
		// GUI RESOURCES CONSTANTS
		
		public const string TOOLKIT_SKIN = "TerrainToolkitEditorSkin";
		
		public const string ICO_BANNER = "bannerTerrainToolkit";
		
		public const string ICO_PREMADE = "IconGenModel";
		public const string ICO_TOOLKIT = "IconGenToolkit";
		
		public const string ICO_CREATE = "IconGenCreate";
		public const string ICO_ERODE = "IconGenErode";
		public const string ICO_TEXTURE = "IconGenTexture";
		public const string ICO_OPTIONS = "IconGenOptions";
		
		public const string ICO_MOUNTAINS = "IconMountains";
		public const string ICO_HILLS = "IconHills";
		public const string ICO_PLATEAUS = "IconPlateaus";
		
		public const string ICO_MOORE = "IconMoore";
		public const string ICO_VONNEUMANN = "IconVonNeumann";
		
		public const string DEFAULT_TEXTURE = "defaultTexture";
				
		public const string TERRAIN_MODEL = "Model_{0}_{1}";
				
		// ------------------------------------------------------------------------
		
		// Default skin for this editor
		private GUISkin p_skin = null;
		
		// Icons
		private Texture2D p_banner;
		private Texture2D p_modelIcon;
		private Texture2D p_toolkitIcon;
		private Texture2D p_createIcon;
		private Texture2D p_erodeIcon;
		private Texture2D p_textureIcon;
		private Texture2D p_optionsIcon;
		private Texture2D p_mountainsIcon;
		private Texture2D p_hillsIcon;
		private Texture2D p_plateausIcon;
		private Texture2D p_mooreIcon;
		private Texture2D p_vonNeumannIcon;
		private Texture2D p_defaultTexture;
		
		// Terrain models visuals
		private Texture2D p_model_00;
		private Texture2D p_model_00_cliff;
		private Texture2D p_model_00_tx1;
		private Texture2D p_model_00_tx2;
		
		// Terrain models visuals
		private Texture2D p_model_01;
		private Texture2D p_model_01_cliff;
		private Texture2D p_model_01_tx1;
		
		// Terrain models visuals
		private Texture2D p_model_02;
		private Texture2D p_model_02_cliff;
		private Texture2D p_model_02_tx1;
		private Texture2D p_model_02_tx2;
		private Texture2D p_model_02_tx3;
		private Texture2D p_model_02_tx4;
		
		// Terrain models visuals
		private Texture2D p_model_03;
		private Texture2D p_model_03_cliff;
		private Texture2D p_model_03_tx1;
		private Texture2D p_model_03_tx2;
		
		// Terrain models visuals
		private Texture2D p_model_04;
		private Texture2D p_model_04_cliff;
		private Texture2D p_model_04_tx1;
		private Texture2D p_model_04_tx2;
		
		// Terrain models visuals
		private Texture2D p_model_05;
		private Texture2D p_model_05_cliff;
		private Texture2D p_model_05_tx1;
		
		// Drag control value
		private string p_dragControl = EMPTY;
		
		// The working mode
		private int p_workingMode = 0;
		
		// The selected terrain model
		private int p_selectedModel = -1;
		
		// A reserved value for tiling
		private float p_amount = 0.4f;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
	// Default constructor
	
/*---------------------------------------------------------------------------*/
/* OnEnable function */
/*---------------------------------------------------------------------------*/
	
	public void OnEnable(){
		
		// Load resources if needed
		if( p_skin==null ){
			
			// Load skin
			p_skin = Resources.Load(TOOLKIT_SKIN) as GUISkin;
			
			// Load icons resources
			p_banner = Resources.Load(ICO_BANNER) as Texture2D;
			p_modelIcon = Resources.Load(ICO_PREMADE) as Texture2D;
			p_toolkitIcon = Resources.Load(ICO_TOOLKIT) as Texture2D;
			p_createIcon = Resources.Load(ICO_CREATE) as Texture2D;
			p_erodeIcon = Resources.Load(ICO_ERODE) as Texture2D;
			p_textureIcon = Resources.Load(ICO_TEXTURE) as Texture2D;
			p_optionsIcon = Resources.Load(ICO_OPTIONS) as Texture2D;
			p_mountainsIcon = Resources.Load(ICO_MOUNTAINS) as Texture2D;
			p_hillsIcon = Resources.Load(ICO_HILLS) as Texture2D;
			p_plateausIcon = Resources.Load(ICO_PLATEAUS) as Texture2D;
			p_mooreIcon = Resources.Load(ICO_MOORE) as Texture2D;
			p_vonNeumannIcon = Resources.Load(ICO_VONNEUMANN) as Texture2D;
			p_defaultTexture = Resources.Load(DEFAULT_TEXTURE) as Texture2D;
			
			// Load terrain models visuals			
			p_model_00 = Resources.Load( string.Format(TERRAIN_MODEL,"00","Visual") ) as Texture2D;  
			p_model_00_cliff = Resources.Load( string.Format(TERRAIN_MODEL,"00","Cliff") ) as Texture2D;
			p_model_00_tx1 = Resources.Load( string.Format(TERRAIN_MODEL,"00","Tx1") ) as Texture2D;
			p_model_00_tx2 = Resources.Load( string.Format(TERRAIN_MODEL,"00","Tx2") ) as Texture2D;
			
			// Load terrain models visuals	
			p_model_01 = Resources.Load( string.Format(TERRAIN_MODEL,"01","Visual") ) as Texture2D; 
			p_model_01_cliff = Resources.Load( string.Format(TERRAIN_MODEL,"01","Cliff") ) as Texture2D;
			p_model_01_tx1 = Resources.Load( string.Format(TERRAIN_MODEL,"01","Tx1") ) as Texture2D;
			
			// Load terrain models visuals	
			p_model_02 = Resources.Load( string.Format(TERRAIN_MODEL,"02","Visual") ) as Texture2D; 
			p_model_02_cliff = Resources.Load( string.Format(TERRAIN_MODEL,"02","Cliff") ) as Texture2D;
			p_model_02_tx1 = Resources.Load( string.Format(TERRAIN_MODEL,"02","Tx1") ) as Texture2D;
			p_model_02_tx2 = Resources.Load( string.Format(TERRAIN_MODEL,"02","Tx2") ) as Texture2D;
			p_model_02_tx3 = Resources.Load( string.Format(TERRAIN_MODEL,"02","Tx3") ) as Texture2D;
			p_model_02_tx4 = Resources.Load( string.Format(TERRAIN_MODEL,"02","Tx4") ) as Texture2D;
			
			// Load terrain models visuals	
			p_model_03 = Resources.Load( string.Format(TERRAIN_MODEL,"03","Visual") ) as Texture2D; 
			p_model_03_cliff = Resources.Load( string.Format(TERRAIN_MODEL,"03","Cliff") ) as Texture2D;
			p_model_03_tx1 = Resources.Load( string.Format(TERRAIN_MODEL,"03","Tx1") ) as Texture2D;
			p_model_03_tx2 = Resources.Load( string.Format(TERRAIN_MODEL,"03","Tx2") ) as Texture2D;
			
			// Load terrain models visuals	
			p_model_04 = Resources.Load( string.Format(TERRAIN_MODEL,"04","Visual") ) as Texture2D; 
			p_model_04_cliff = Resources.Load( string.Format(TERRAIN_MODEL,"04","Cliff") ) as Texture2D;
			p_model_04_tx1 = Resources.Load( string.Format(TERRAIN_MODEL,"04","Tx1") ) as Texture2D;
			p_model_04_tx2 = Resources.Load( string.Format(TERRAIN_MODEL,"04","Tx2") ) as Texture2D;
			
			// Load terrain models visuals	
			p_model_05 = Resources.Load( string.Format(TERRAIN_MODEL,"05","Visual") ) as Texture2D; 
			p_model_05_cliff = Resources.Load( string.Format(TERRAIN_MODEL,"05","Cliff") ) as Texture2D;
			p_model_05_tx1 = Resources.Load( string.Format(TERRAIN_MODEL,"05","Tx1") ) as Texture2D;
			
		}
		
	}
	
/*---------------------------------------------------------------------------*/
/* OnInspectorGUI function */
/*---------------------------------------------------------------------------*/
	
	public override void OnInspectorGUI(){
		
		// Get the targeted TerrainToolkit
		TerrainToolkit toolkit = target as TerrainToolkit;
		
		// If the toolkit is not attached to a GameObject return
		if( !toolkit.gameObject )	return;			
		
		// No Terrain neither ?
		if( toolkit.GetComponent<Terrain>()==null ){
			
			// Show warning message
			EditorGUILayout.Separator();
			GUI.skin = p_skin;
			GUILayout.Label(
				"[ERROR] The GameObject the current TerrainToolkit is attached to does not have a Terrain component\n\n" +
				"[ERROR] Please attach a Terrain component",
				"errorText"
			);
			GUI.skin = null;
			EditorGUILayout.Separator();
			
			// Return
			return;
			
		}	

		// -------------------------------------------------------------------------------------------
		// BRUTAL... BUT NECESSARY TO AVOID PROBLEMS !!!!
		
		if( toolkit.GetComponent<Terrain>().terrainData.heightmapResolution>2049 ){
			
			// Show warning message
			EditorGUILayout.Separator();
			GUI.skin = p_skin;
			GUILayout.Label(
				"[ERROR] The [Heightmap Resolution] of the current Terrain component is greater than [2049] (2048+1)\n\n" +
				"[ERROR] Please lower the [Heightmap resolution] value to [2049] (Unity limitation)",
				"errorText"
			);
			GUI.skin = null;
			EditorGUILayout.Separator();
			
			// Return
			return;
			
		}	

		// -------------------------------------------------------------------------------------------
		
		
		// If not yet initialized, add presets to toolkit
		if( !toolkit.presetsInitialised )	toolkit.addPresets();
		
		// If not yet initialized, create tookit list for height blend points
		if( toolkit.heightBlendPoints==null )	toolkit.heightBlendPoints = new List<float>();
		
		// Variable(s)
		Terrain ter;
		TerrainData terData;
		int i;
		// int n;
		float mouseX;
		
		
		// -------------------------------------------------------------------------------------------
		// GUI START
		
		EditorGUILayout.BeginVertical();
		
			// -------------------------------------------------------------------------------------------
			// BANNER
			
			EditorGUILayout.Separator();			
			GUI.skin = p_skin;
			GUILayout.Box(p_banner,"ToolkitBanner");
			GUI.skin = null;			
			EditorGUILayout.Separator();
			
			// -------------------------------------------------------------------------------------------
			// WORKING MODE TOOLBAR
		
			GUIContent[] operationsOptions = new GUIContent[2];			
			operationsOptions[0] = new GUIContent("Toolkit", p_toolkitIcon);
			operationsOptions[1] = new GUIContent("Terrain models", p_modelIcon);
			p_workingMode = GUILayout.Toolbar(p_workingMode,operationsOptions);
			EditorGUILayout.Separator();
			
			// Switch working mode		
			switch( p_workingMode ){		
			
				// ---------------------------------------------------------------------------------------
				// LEGACY TOOLKIT MODE
			
				case 0 :
				
					// -------------------------------------------------------------------------------------------
					// MAIN ACTIONS TOOLBAR
				
					GUIContent[] toolbarOptions = new GUIContent[4];
					toolbarOptions[0] = new GUIContent("Create", p_createIcon);
					toolbarOptions[1] = new GUIContent("Erode", p_erodeIcon);
					toolbarOptions[2] = new GUIContent("Texture", p_textureIcon);
					toolbarOptions[3] = new GUIContent("Options", p_optionsIcon);
					toolkit.toolModeInt = GUILayout.Toolbar(toolkit.toolModeInt,toolbarOptions);
					EditorGUILayout.Separator();
				
					// Switch action		
					switch( toolkit.toolModeInt ){			
					
						// ---------------------------------------------------------------------------------------
						// CREATE
					
						case 0 :
					
							string[] generatorOptions = new string[5];
							generatorOptions[0] = "Voronoi";
							generatorOptions[1] = "Fractal";
							generatorOptions[2] = "Perlin";
							generatorOptions[3] = "Smooth";
							generatorOptions[4] = "Normalise";
							toolkit.generatorTypeInt = GUILayout.Toolbar(toolkit.generatorTypeInt,generatorOptions);								
							EditorGUILayout.Separator();
						
							// Switch generator type
							switch( toolkit.generatorTypeInt ){
							
								// ---------------------------------------------------------------------------------------
								// VORONOI
							
								case 0 :
							
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;		

									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Preset");
										string[] voronoiPresetNames = new string[toolkit.voronoiPresets.Count+1];
										int[] voronoiPresetInts = new int[toolkit.voronoiPresets.Count+1];
										voronoiPresetNames[0] = "None";
										TerrainToolkit.voronoiPresetData voronoiPreset;
										for(i=1;i<=toolkit.voronoiPresets.Count;i++){
											voronoiPreset = toolkit.voronoiPresets[i-1] as TerrainToolkit.voronoiPresetData;
											voronoiPresetNames[i] = voronoiPreset.presetName;
											voronoiPresetInts[i] = i;
										}
										toolkit.voronoiPresetId = EditorGUILayout.IntPopup(toolkit.voronoiPresetId, voronoiPresetNames, voronoiPresetInts);
									EditorGUILayout.EndHorizontal();
								
									if( GUI.changed && toolkit.voronoiPresetId>0 ){
										voronoiPreset = (TerrainToolkit.voronoiPresetData) toolkit.voronoiPresets[toolkit.voronoiPresetId-1];
										toolkit.setVoronoiPreset(voronoiPreset);
									}		
								
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
								
									// Feature type
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Feature type");
										GUIContent[] featureStates = new GUIContent[3];
										featureStates[0] = new GUIContent("Mountains", p_mountainsIcon);
										featureStates[1] = new GUIContent("Hills", p_hillsIcon);
										featureStates[2] = new GUIContent("Plateaus", p_plateausIcon);
										toolkit.voronoiTypeInt = GUILayout.Toolbar(toolkit.voronoiTypeInt, featureStates);
									EditorGUILayout.EndHorizontal();
								
									// Cells
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Cells");
										toolkit.voronoiCells = (int) EditorGUILayout.Slider(toolkit.voronoiCells, 2, 100);
									EditorGUILayout.EndHorizontal();
								
									// Features
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Features");
										toolkit.voronoiFeatures = EditorGUILayout.Slider(toolkit.voronoiFeatures, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
								
									// Scale
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Scale");
										toolkit.voronoiScale = EditorGUILayout.Slider(toolkit.voronoiScale, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
								
									// Blend
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Blend");
										toolkit.voronoiBlend = EditorGUILayout.Slider(toolkit.voronoiBlend, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
								
									if( GUI.changed )	toolkit.voronoiPresetId = 0;
									
									EditorGUILayout.Separator();
								
									// Generate !!
									if( GUILayout.Button("Generate Voronoi Features") ){	
									
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData,"Generate Voronoi Features");

										// Generate...
										TerrainToolkit.GeneratorProgressDelegate generatorProgressDelegate = new TerrainToolkit.GeneratorProgressDelegate(updateGeneratorProgress);
										toolkit.generateTerrain(generatorProgressDelegate);
										EditorUtility.ClearProgressBar();
										GUIUtility.ExitGUI();
										
									}

									// Break
									break;
									
								// ---------------------------------------------------------------------------------------
								// FRACTAL / DIAMOND SQUARE
								
								case 1 :
								
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;		

									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Preset");
										string[] fractalPresetNames = new string[toolkit.fractalPresets.Count+1];
										int[] fractalPresetInts = new int[toolkit.fractalPresets.Count+1];
										fractalPresetNames[0] = "None";
										TerrainToolkit.fractalPresetData fractalPreset;
										for(i=1;i<=toolkit.fractalPresets.Count;i++){
											fractalPreset = (TerrainToolkit.fractalPresetData) toolkit.fractalPresets[i-1];
											fractalPresetNames[i] = fractalPreset.presetName;
											fractalPresetInts[i] = i;
										}
										toolkit.fractalPresetId = EditorGUILayout.IntPopup(toolkit.fractalPresetId, fractalPresetNames, fractalPresetInts);
									EditorGUILayout.EndHorizontal();
									
									if( GUI.changed && toolkit.fractalPresetId>0 ){
										fractalPreset = (TerrainToolkit.fractalPresetData) toolkit.fractalPresets[toolkit.fractalPresetId-1];
										toolkit.setFractalPreset(fractalPreset);
									}
										
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
							
									// Delta
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Delta");
										toolkit.diamondSquareDelta = EditorGUILayout.Slider(toolkit.diamondSquareDelta, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Blend
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Blend");
										toolkit.diamondSquareBlend = EditorGUILayout.Slider(toolkit.diamondSquareBlend, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
							
									if( GUI.changed )	toolkit.fractalPresetId = 0;
							
									// Generate !!
									if( GUILayout.Button("Generate Fractal Terrain") ){
										
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData,"Generate Fractal Terrain");
										
										// Generate...
										TerrainToolkit.GeneratorProgressDelegate generatorProgressDelegate = new TerrainToolkit.GeneratorProgressDelegate(updateGeneratorProgress);
										toolkit.generateTerrain(generatorProgressDelegate);
										EditorUtility.ClearProgressBar();
										GUIUtility.ExitGUI();
										
									}
								
									// Break
									break;
							
								// ---------------------------------------------------------------------------------------
								// PERLIN
									
								case 2 :
								
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;		

									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Preset");
										string[] perlinPresetNames = new string[toolkit.perlinPresets.Count+1];
										int[] perlinPresetInts = new int[toolkit.perlinPresets.Count+1];
										perlinPresetNames[0] = "None";
										TerrainToolkit.perlinPresetData perlinPreset;
										for(i=1;i<=toolkit.perlinPresets.Count;i++){
											perlinPreset = (TerrainToolkit.perlinPresetData) toolkit.perlinPresets[i-1];
											perlinPresetNames[i] = perlinPreset.presetName;
											perlinPresetInts[i] = i;
										}
										toolkit.perlinPresetId = EditorGUILayout.IntPopup(toolkit.perlinPresetId, perlinPresetNames, perlinPresetInts);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
									
									if( GUI.changed && toolkit.perlinPresetId>0 ){
										perlinPreset = (TerrainToolkit.perlinPresetData) toolkit.perlinPresets[toolkit.perlinPresetId-1];
										toolkit.setPerlinPreset(perlinPreset);
									}
									
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
													
									// Frequency
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Frequency");
										toolkit.perlinFrequency = EditorGUILayout.IntSlider(toolkit.perlinFrequency, 1, 16);
									EditorGUILayout.EndHorizontal();
									
									// Amplitude
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Amplitude");
										toolkit.perlinAmplitude = EditorGUILayout.Slider(toolkit.perlinAmplitude, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Octaves
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Octaves");
									toolkit.perlinOctaves = EditorGUILayout.IntSlider(toolkit.perlinOctaves, 1, 12);
									EditorGUILayout.EndHorizontal();
									
									// Blend
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Blend");
									toolkit.perlinBlend = EditorGUILayout.Slider(toolkit.perlinBlend, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
								
									if( GUI.changed )	toolkit.perlinPresetId = 0;

									// Generate !!
									if( GUILayout.Button("Generate Perlin Terrain") ){
									
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData,"Generate Perlin Terrain");
										
										// Generate...
										TerrainToolkit.GeneratorProgressDelegate generatorProgressDelegate = new TerrainToolkit.GeneratorProgressDelegate(updateGeneratorProgress);
										toolkit.generateTerrain(generatorProgressDelegate);
										EditorUtility.ClearProgressBar();
										GUIUtility.ExitGUI();
										
									}
								
									// Break
									break;
								
								// ---------------------------------------------------------------------------------------
								// SMOOTH
								
								case 3 :
								
									// Iterations...
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Iterations");
										toolkit.smoothIterations = (int) EditorGUILayout.Slider(toolkit.smoothIterations, 1, 5);
									EditorGUILayout.EndHorizontal();
									
									// Blend
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Blend");
										toolkit.smoothBlend = EditorGUILayout.Slider(toolkit.smoothBlend, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
								
									// Generate !
									if( GUILayout.Button("Smooth Terrain") ){
									
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData,"Smooth Terrain");
										
										// Generate...
										TerrainToolkit.GeneratorProgressDelegate generatorProgressDelegate = new TerrainToolkit.GeneratorProgressDelegate(updateGeneratorProgress);
										toolkit.generateTerrain(generatorProgressDelegate);
										EditorUtility.ClearProgressBar();
										GUIUtility.ExitGUI();
									
									}
									
									// Break
									break;
								
								// ---------------------------------------------------------------------------------------
								// NORMALISE
									
								case 4:
								
									// Minimum height
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Minimum height");
										toolkit.normaliseMin = EditorGUILayout.Slider(toolkit.normaliseMin, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Maximum height
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Maximum height");
										toolkit.normaliseMax = EditorGUILayout.Slider(toolkit.normaliseMax, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Blend
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Blend");
										toolkit.normaliseBlend = EditorGUILayout.Slider(toolkit.normaliseBlend, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
								
									// Generate
									if( GUILayout.Button("Normalise Terrain") ){
										
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData, "Normalise Terrain");
										
										// Generate...
										TerrainToolkit.GeneratorProgressDelegate generatorProgressDelegate = new TerrainToolkit.GeneratorProgressDelegate(updateGeneratorProgress);
										toolkit.generateTerrain(generatorProgressDelegate);
										EditorUtility.ClearProgressBar();
										GUIUtility.ExitGUI();
									}
									
									// Break
									break;
								
							}
							
						// Break
						break;
						
						// ---------------------------------------------------------------------------------------
						// EROSION
							
						case 1 :
						
							string[] erosionOptions = new string[4];
							erosionOptions[0] = "Thermal";
							erosionOptions[1] = "Hydraulic";
							erosionOptions[2] = "Tidal";
							erosionOptions[3] = "Wind";
							toolkit.erosionTypeInt = GUILayout.Toolbar(toolkit.erosionTypeInt, erosionOptions);				
							EditorGUILayout.Separator();
							
							GUI.skin = p_skin;
							GUILayout.Box("Filters");
							GUI.skin = null;
							
							// Switch erosion type
							switch( toolkit.erosionTypeInt ){
							
								// ---------------------------------------------------------------------------------------
								// THERMAL
								
								case 0 :
								
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
							
									// Thermal...
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Preset");
										string[] thermalErosionPresetNames = new string[toolkit.thermalErosionPresets.Count+1];
										int[] thermalErosionPresetInts = new int[toolkit.thermalErosionPresets.Count+1];
										thermalErosionPresetNames[0] = "None";
										TerrainToolkit.thermalErosionPresetData thermalErosionPreset;
										for(i=1;i<=toolkit.thermalErosionPresets.Count;i++){
											thermalErosionPreset = (TerrainToolkit.thermalErosionPresetData) toolkit.thermalErosionPresets[i-1];
											thermalErosionPresetNames[i] = thermalErosionPreset.presetName;
											thermalErosionPresetInts[i] = i;
										}
										toolkit.thermalErosionPresetId = EditorGUILayout.IntPopup(toolkit.thermalErosionPresetId, thermalErosionPresetNames, thermalErosionPresetInts);
									EditorGUILayout.EndHorizontal();
								
									if( GUI.changed && toolkit.thermalErosionPresetId>0 ){
										thermalErosionPreset = (TerrainToolkit.thermalErosionPresetData) toolkit.thermalErosionPresets[toolkit.thermalErosionPresetId-1];
										toolkit.setThermalErosionPreset(thermalErosionPreset);
									}
							
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
									
									// Iterations
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Iterations");
										toolkit.thermalIterations = (int) EditorGUILayout.Slider(toolkit.thermalIterations, 1, 250);
									EditorGUILayout.EndHorizontal();
									
									// Minimum slope
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Minimum slope");
									toolkit.thermalMinSlope = EditorGUILayout.Slider(toolkit.thermalMinSlope, 0.01f, 89.99f);
									EditorGUILayout.EndHorizontal();
									
									// Falloff
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Falloff");
									toolkit.thermalFalloff = EditorGUILayout.Slider(toolkit.thermalFalloff, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
							
									if( GUI.changed )	toolkit.thermalErosionPresetId = 0;

									// Erode 
									if( GUILayout.Button("Apply thermal erosion") ){
										
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData, "Apply thermal erosion");
										
										// Start time...
										DateTime startTime = DateTime.Now;
										TerrainToolkit.ErosionProgressDelegate erosionProgressDelegate = new TerrainToolkit.ErosionProgressDelegate(updateErosionProgress);
										toolkit.erodeAllTerrain(erosionProgressDelegate);
										EditorUtility.ClearProgressBar();
										// TimeSpan processTime = DateTime.Now-startTime;
										// Debug.Log("Process completed in : " + processTime.ToString() );
										GUIUtility.ExitGUI();
									
									}
									
									// Break
									break;
									
								// ---------------------------------------------------------------------------------------
								// HYDRAULIC
										
								case 1 :
								
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Type");
										string[] toggleStates = new string[3];
										toggleStates[0] = "Fast";
										toggleStates[1] = "Full";
										toggleStates[2] = "Velocity";
										toolkit.hydraulicTypeInt = GUILayout.Toolbar(toolkit.hydraulicTypeInt, toggleStates);
									EditorGUILayout.EndHorizontal();
							
									// Switch hyfraulic erosion type
									switch( toolkit.hydraulicTypeInt ){
										
										// ---------------------------------------------------------------------------------------
										// FAST
										
										case 0 :
										
											if( GUI.changed )	EditorUtility.SetDirty(toolkit);
											GUI.changed = false;
										
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Preset");
												string[] fastHydraulicErosionPresetNames = new string[toolkit.fastHydraulicErosionPresets.Count+1];
												int[] fastHydraulicErosionPresetInts = new int[toolkit.fastHydraulicErosionPresets.Count+1];
												fastHydraulicErosionPresetNames[0] = "None";
												TerrainToolkit.fastHydraulicErosionPresetData fastHydraulicErosionPreset;
												for(i=1;i<=toolkit.fastHydraulicErosionPresets.Count;i++){
													fastHydraulicErosionPreset = (TerrainToolkit.fastHydraulicErosionPresetData) toolkit.fastHydraulicErosionPresets[i-1];
													fastHydraulicErosionPresetNames[i] = fastHydraulicErosionPreset.presetName;
													fastHydraulicErosionPresetInts[i] = i;
												}
												toolkit.fastHydraulicErosionPresetId = EditorGUILayout.IntPopup(toolkit.fastHydraulicErosionPresetId, fastHydraulicErosionPresetNames, fastHydraulicErosionPresetInts);
											EditorGUILayout.EndHorizontal();
											
											if( GUI.changed && toolkit.fastHydraulicErosionPresetId>0 ){
												fastHydraulicErosionPreset = (TerrainToolkit.fastHydraulicErosionPresetData) toolkit.fastHydraulicErosionPresets[toolkit.fastHydraulicErosionPresetId-1];
												toolkit.setFastHydraulicErosionPreset(fastHydraulicErosionPreset);
											}
											
											if( GUI.changed )	EditorUtility.SetDirty(toolkit);
											GUI.changed = false;
								
											// Iterations
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Iterations");
												toolkit.hydraulicIterations = (int) EditorGUILayout.Slider(toolkit.hydraulicIterations, 1, 250);
											EditorGUILayout.EndHorizontal();
											
											// Maximum slope
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Maximum slope");
												toolkit.hydraulicMaxSlope = EditorGUILayout.Slider(toolkit.hydraulicMaxSlope, 0.0f, 89.99f);
											EditorGUILayout.EndHorizontal();
											
											// Falloff
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Falloff");
												toolkit.hydraulicFalloff = EditorGUILayout.Slider(toolkit.hydraulicFalloff, 0.0f, 1.0f);
											EditorGUILayout.EndHorizontal();
											
											if( GUI.changed )	toolkit.fastHydraulicErosionPresetId = 0;
								
											// Break
											break;
											
										// ---------------------------------------------------------------------------------------
										// FULL
											
										case 1 :
										
											if( GUI.changed )	EditorUtility.SetDirty(toolkit);
											GUI.changed = false;
											
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Preset");
												string[] fullHydraulicErosionPresetNames = new string[toolkit.fullHydraulicErosionPresets.Count+1];
												int[] fullHydraulicErosionPresetInts = new int[toolkit.fullHydraulicErosionPresets.Count+1];
												fullHydraulicErosionPresetNames[0] = "None";
												TerrainToolkit.fullHydraulicErosionPresetData fullHydraulicErosionPreset;
												for(i=1;i<=toolkit.fullHydraulicErosionPresets.Count;i++){
													fullHydraulicErosionPreset = (TerrainToolkit.fullHydraulicErosionPresetData) toolkit.fullHydraulicErosionPresets[i-1];
													fullHydraulicErosionPresetNames[i] = fullHydraulicErosionPreset.presetName;
													fullHydraulicErosionPresetInts[i] = i;
												}
												toolkit.fullHydraulicErosionPresetId = EditorGUILayout.IntPopup(toolkit.fullHydraulicErosionPresetId, fullHydraulicErosionPresetNames, fullHydraulicErosionPresetInts);
											EditorGUILayout.EndHorizontal();
											
											if( GUI.changed && toolkit.fullHydraulicErosionPresetId>0 ){
												fullHydraulicErosionPreset = (TerrainToolkit.fullHydraulicErosionPresetData) toolkit.fullHydraulicErosionPresets[toolkit.fullHydraulicErosionPresetId-1];
												toolkit.setFullHydraulicErosionPreset(fullHydraulicErosionPreset);
											}
											
											if( GUI.changed )	EditorUtility.SetDirty(toolkit);
											GUI.changed = false;
											
											// Iterations
											EditorGUILayout.BeginHorizontal();
											EditorGUILayout.PrefixLabel("Iterations");
											toolkit.hydraulicIterations = (int) EditorGUILayout.Slider(toolkit.hydraulicIterations, 1, 250);
											EditorGUILayout.EndHorizontal();
											
											// Rainfall
											EditorGUILayout.BeginHorizontal();
											EditorGUILayout.PrefixLabel("Rainfall");
											toolkit.hydraulicRainfall = EditorGUILayout.Slider(toolkit.hydraulicRainfall, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Evaporation
											EditorGUILayout.BeginHorizontal();
											EditorGUILayout.PrefixLabel("Evaporation");
											toolkit.hydraulicEvaporation = EditorGUILayout.Slider(toolkit.hydraulicEvaporation, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Solubility
											EditorGUILayout.BeginHorizontal();
											EditorGUILayout.PrefixLabel("Solubility");
											toolkit.hydraulicSedimentSolubility = EditorGUILayout.Slider(toolkit.hydraulicSedimentSolubility, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Saturation
											EditorGUILayout.BeginHorizontal();
											EditorGUILayout.PrefixLabel("Saturation");
											toolkit.hydraulicSedimentSaturation = EditorGUILayout.Slider(toolkit.hydraulicSedimentSaturation, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											if( GUI.changed )	toolkit.fullHydraulicErosionPresetId = 0;
											
											// Break
											break;
											
										// ---------------------------------------------------------------------------------------
										// VELOCITY
											
										case 2 :
										
											if( GUI.changed )	EditorUtility.SetDirty(toolkit);
											GUI.changed = false;
											
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Preset");
												string[] velocityHydraulicErosionPresetNames = new string[toolkit.velocityHydraulicErosionPresets.Count+1];
												int[] velocityHydraulicErosionPresetInts = new int[toolkit.velocityHydraulicErosionPresets.Count+1];
												velocityHydraulicErosionPresetNames[0] = "None";
												TerrainToolkit.velocityHydraulicErosionPresetData velocityHydraulicErosionPreset;
												for(i=1;i<=toolkit.velocityHydraulicErosionPresets.Count;i++){
													velocityHydraulicErosionPreset = (TerrainToolkit.velocityHydraulicErosionPresetData) toolkit.velocityHydraulicErosionPresets[i-1];
													velocityHydraulicErosionPresetNames[i] = velocityHydraulicErosionPreset.presetName;
													velocityHydraulicErosionPresetInts[i] = i;
												}
												toolkit.velocityHydraulicErosionPresetId = EditorGUILayout.IntPopup(toolkit.velocityHydraulicErosionPresetId, velocityHydraulicErosionPresetNames, velocityHydraulicErosionPresetInts);
											EditorGUILayout.EndHorizontal();
											
											if( GUI.changed && toolkit.velocityHydraulicErosionPresetId>0 ){
												velocityHydraulicErosionPreset = (TerrainToolkit.velocityHydraulicErosionPresetData) toolkit.velocityHydraulicErosionPresets[toolkit.velocityHydraulicErosionPresetId-1];
												toolkit.setVelocityHydraulicErosionPreset(velocityHydraulicErosionPreset);
											}
											
											if( GUI.changed )	EditorUtility.SetDirty(toolkit);
											GUI.changed = false;					
											
											// Iterations
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Iterations");
												toolkit.hydraulicIterations = (int) EditorGUILayout.Slider(toolkit.hydraulicIterations, 1, 250);
											EditorGUILayout.EndHorizontal();
											
											// Rainfall
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Rainfall");
												toolkit.hydraulicVelocityRainfall = EditorGUILayout.Slider(toolkit.hydraulicVelocityRainfall, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Evaporation
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Evaporation");
												toolkit.hydraulicVelocityEvaporation = EditorGUILayout.Slider(toolkit.hydraulicVelocityEvaporation, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Solubility
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Solubility");
												toolkit.hydraulicVelocitySedimentSolubility = EditorGUILayout.Slider(toolkit.hydraulicVelocitySedimentSolubility, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Saturation
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Saturation");
												toolkit.hydraulicVelocitySedimentSaturation = EditorGUILayout.Slider(toolkit.hydraulicVelocitySedimentSaturation, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Velocity
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Velocity");
												toolkit.hydraulicVelocity = EditorGUILayout.Slider(toolkit.hydraulicVelocity, 0, 10);
											EditorGUILayout.EndHorizontal();
											
											// Momentum
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Momentum");
												toolkit.hydraulicMomentum = EditorGUILayout.Slider(toolkit.hydraulicMomentum, 0, 10);
											EditorGUILayout.EndHorizontal();
											
											// Entropy
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Entropy");
												toolkit.hydraulicEntropy = EditorGUILayout.Slider(toolkit.hydraulicEntropy, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											// Downcutting
											EditorGUILayout.BeginHorizontal();
												EditorGUILayout.PrefixLabel("Downcutting");
												toolkit.hydraulicDowncutting = EditorGUILayout.Slider(toolkit.hydraulicDowncutting, 0, 1);
											EditorGUILayout.EndHorizontal();
											
											if( GUI.changed )	toolkit.velocityHydraulicErosionPresetId = 0;
											
											// Break
											break;
											
									}
							
									// Erode
									if( GUILayout.Button("Apply hydraulic erosion") ){
										
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData, "Apply hydraulic erosion");
										
										// Start time...
										DateTime startTime = DateTime.Now;
										TerrainToolkit.ErosionProgressDelegate erosionProgressDelegate = new TerrainToolkit.ErosionProgressDelegate(updateErosionProgress);
										toolkit.erodeAllTerrain(erosionProgressDelegate);
										EditorUtility.ClearProgressBar();
										// TimeSpan processTime = DateTime.Now-startTime;
										// Debug.Log("Process completed in : " + processTime.ToString() );
										GUIUtility.ExitGUI();
										
									}
									
									// Break
									break;
									
								// ---------------------------------------------------------------------------------------
								// TIDAL
									
								case 2 :
								
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
							
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Preset");
										string[] tidalErosionPresetNames = new string[toolkit.tidalErosionPresets.Count+1];
										int[] tidalErosionPresetInts = new int[toolkit.tidalErosionPresets.Count+1];
										tidalErosionPresetNames[0] = "None";
										TerrainToolkit.tidalErosionPresetData tidalErosionPreset;
										for(i=1;i<=toolkit.tidalErosionPresets.Count;i++){
											tidalErosionPreset = (TerrainToolkit.tidalErosionPresetData) toolkit.tidalErosionPresets[i-1];
											tidalErosionPresetNames[i] = tidalErosionPreset.presetName;
											tidalErosionPresetInts[i] = i;
										}
										toolkit.tidalErosionPresetId = EditorGUILayout.IntPopup(toolkit.tidalErosionPresetId, tidalErosionPresetNames, tidalErosionPresetInts);
									EditorGUILayout.EndHorizontal();
									
									if( GUI.changed && toolkit.tidalErosionPresetId>0 ){
										tidalErosionPreset = (TerrainToolkit.tidalErosionPresetData) toolkit.tidalErosionPresets[toolkit.tidalErosionPresetId-1];
										toolkit.setTidalErosionPreset(tidalErosionPreset);
									}
									
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
									
									// Iterations
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Iterations");
									toolkit.tidalIterations = (int) EditorGUILayout.Slider(toolkit.tidalIterations, 1, 250);
									EditorGUILayout.EndHorizontal();
									
									// Sea level
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Sea level");
									toolkit.tidalSeaLevel = EditorGUILayout.FloatField(toolkit.tidalSeaLevel);
									EditorGUILayout.EndHorizontal();
									
									// Tidal range
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Tidal range");
									toolkit.tidalRangeAmount = EditorGUILayout.FloatField(toolkit.tidalRangeAmount);
									EditorGUILayout.EndHorizontal();
									
									// Cliff limit
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Cliff limit");
									toolkit.tidalCliffLimit = EditorGUILayout.Slider(toolkit.tidalCliffLimit, 0.0f, 90.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
									
									if( GUI.changed )	toolkit.tidalErosionPresetId = 0;
									
									// Erode
									if( GUILayout.Button("Apply tidal erosion") ){
										
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData, "Apply tidal erosion");
										
										// Start time...
										DateTime startTime = DateTime.Now;
										TerrainToolkit.ErosionProgressDelegate erosionProgressDelegate = new TerrainToolkit.ErosionProgressDelegate(updateErosionProgress);
										toolkit.erodeAllTerrain(erosionProgressDelegate);
										EditorUtility.ClearProgressBar();
										// TimeSpan processTime = DateTime.Now-startTime;
										// Debug.Log("Process completed in : " + processTime.ToString() );
										GUIUtility.ExitGUI();
									}
									
									
									// Break
									break;
									
								// ---------------------------------------------------------------------------------------
								// WIND
									
								case 3 :
								
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
							
									EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PrefixLabel("Preset");
										string[] windErosionPresetNames = new string[toolkit.windErosionPresets.Count+1];
										int[] windErosionPresetInts = new int[toolkit.windErosionPresets.Count+1];
										windErosionPresetNames[0] = "None";
										TerrainToolkit.windErosionPresetData windErosionPreset;
										for(i=1;i<=toolkit.windErosionPresets.Count;i++){
											windErosionPreset = (TerrainToolkit.windErosionPresetData) toolkit.windErosionPresets[i-1];
											windErosionPresetNames[i] = windErosionPreset.presetName;
											windErosionPresetInts[i] = i;
										}
										toolkit.windErosionPresetId = EditorGUILayout.IntPopup(toolkit.windErosionPresetId, windErosionPresetNames, windErosionPresetInts);
									EditorGUILayout.EndHorizontal();
									
									if( GUI.changed && toolkit.windErosionPresetId>0 ){
										windErosionPreset = (TerrainToolkit.windErosionPresetData) toolkit.windErosionPresets[toolkit.windErosionPresetId-1];
										toolkit.setWindErosionPreset(windErosionPreset);
									}
									
									if( GUI.changed )	EditorUtility.SetDirty(toolkit);
									GUI.changed = false;
									
									// Iterations
									EditorGUILayout.BeginHorizontal();
									EditorGUILayout.PrefixLabel("Iterations");
									toolkit.windIterations = (int) EditorGUILayout.Slider(toolkit.windIterations, 1, 250);
									EditorGUILayout.EndHorizontal();
									
									// Wind direction
									EditorGUILayout.BeginHorizontal();
									toolkit.windDirection = EditorGUILayout.Slider("Wind direction", toolkit.windDirection, 0.0f, 360.0f);
									EditorGUILayout.EndHorizontal();
									
									// Wind force
									EditorGUILayout.BeginHorizontal();
									toolkit.windForce = EditorGUILayout.Slider("Wind force", toolkit.windForce, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Lift
									EditorGUILayout.BeginHorizontal();
									toolkit.windLift = EditorGUILayout.Slider("Lift", toolkit.windLift, 0.0f, 0.01f);
									EditorGUILayout.EndHorizontal();
									
									// Gravity
									EditorGUILayout.BeginHorizontal();
									toolkit.windGravity = EditorGUILayout.Slider("Gravity", toolkit.windGravity, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Capacity
									EditorGUILayout.BeginHorizontal();
									toolkit.windCapacity = EditorGUILayout.Slider("Capacity", toolkit.windCapacity, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Entropy
									EditorGUILayout.BeginHorizontal();
									toolkit.windEntropy = EditorGUILayout.Slider("Entropy", toolkit.windEntropy, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									// Smoothing
									EditorGUILayout.BeginHorizontal();
									toolkit.windSmoothing = EditorGUILayout.Slider("Smoothing", toolkit.windSmoothing, 0.0f, 1.0f);
									EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Separator();
									
									if( GUI.changed )	toolkit.windErosionPresetId = 0;

									// Erode
									if( GUILayout.Button("Apply wind erosion") ){
										
										// Undo...
										ter = toolkit.GetComponent<Terrain>();
										if( ter==null )	return;
										terData = ter.terrainData;
										Undo.RegisterCompleteObjectUndo(terData, "Apply wind erosion");
										
										// Start time...
										DateTime startTime = DateTime.Now;
										TerrainToolkit.ErosionProgressDelegate erosionProgressDelegate = new TerrainToolkit.ErosionProgressDelegate(updateErosionProgress);
										toolkit.erodeAllTerrain(erosionProgressDelegate);
										EditorUtility.ClearProgressBar();
										// TimeSpan processTime = DateTime.Now-startTime;
										// Debug.Log("Process completed in : " + processTime.ToString() );
										GUIUtility.ExitGUI();
									}
									
									// Break
									break;
									
							}
						
							// Brush tools				
							if( toolkit.erosionTypeInt==0 || toolkit.erosionTypeInt==2 || (toolkit.erosionTypeInt==1 && toolkit.hydraulicTypeInt==0) ){
								drawBrushToolsGUI();
							}
							
							// Break
							break;
						
						// ---------------------------------------------------------------------------------------
						// TEXTURING TOOLS
						
						case 2 :
						
							// Just in case....
							ter = toolkit.GetComponent<Terrain>();
							if( ter==null )	return;
							terData = ter.terrainData;
							
							// Get splat prototypes
							toolkit.terrainLayers = terData.terrainLayers;
						
							// Use dedicated skin
							GUI.skin = p_skin;
						
							GUILayout.Box("Texture Slope");
						
							Rect gradientRect = EditorGUILayout.BeginHorizontal();
								float gradientWidth = gradientRect.width-55;
								gradientRect.width = 15;
								gradientRect.height = 19;
							
								// Slope stop 1...
								
								if(p_dragControl=="slopeStop1" && Event.current.type==EventType.MouseDrag ){
									mouseX = Event.current.mousePosition.x-7;
									if( mouseX<20 )	mouseX = 20;
									else if( mouseX>19+gradientWidth*(toolkit.slopeBlendMaxAngle/90) )	mouseX = 19+gradientWidth*(toolkit.slopeBlendMaxAngle/90);
									gradientRect.x = mouseX;
									toolkit.slopeBlendMinAngle = ((mouseX-20) / (gradientWidth+1)) * 90;
								}
								else{
									gradientRect.x = 20+gradientWidth * (toolkit.slopeBlendMinAngle / 90);
								}
							
								if( Event.current.type==EventType.MouseDown && gradientRect.Contains(Event.current.mousePosition) ){
									p_dragControl = "slopeStop1";
								}
							
								if( p_dragControl=="slopeStop1" && Event.current.type==EventType.MouseUp ){
									p_dragControl = "";
								}
							
								// Draw slope Stop1 !!
								GUI.Box(gradientRect,"","slopeStop1");
							
								// Slope stop 2...
								
								if( p_dragControl=="slopeStop2" && Event.current.type==EventType.MouseDrag ){
									mouseX = Event.current.mousePosition.x-7;
									if( mouseX<21+gradientWidth*(toolkit.slopeBlendMinAngle/90) )	mouseX = 21+gradientWidth*(toolkit.slopeBlendMinAngle/90);
									else if( mouseX>21+gradientWidth )	mouseX = 21+gradientWidth;
									gradientRect.x = mouseX;
									toolkit.slopeBlendMaxAngle = ((mouseX-20) / (gradientWidth+1)) * 90;
								}
								else{
									gradientRect.x = 20+gradientWidth * (toolkit.slopeBlendMaxAngle / 90);
								}
								
								if( Event.current.type==EventType.MouseDown && gradientRect.Contains(Event.current.mousePosition) ){
									p_dragControl = "slopeStop2";
								}
								
								if( p_dragControl=="slopeStop2" && Event.current.type==EventType.MouseUp ){
									p_dragControl = "";
								}
								
								// Draw slope Stop2 !!
								GUI.Box(gradientRect, "", "slopeStop2");
								
								// Draw gradients
							
								gradientRect.y += 19;
								gradientRect.width = gradientWidth * (toolkit.slopeBlendMinAngle / 90);
								gradientRect.x = 27;
								GUI.Box(gradientRect, "", "black");
								
								gradientRect.width = gradientWidth * ((toolkit.slopeBlendMaxAngle / 90)-(toolkit.slopeBlendMinAngle / 90));
								gradientRect.x = 27+gradientWidth * (toolkit.slopeBlendMinAngle / 90);
								GUI.Box(gradientRect, "", "blackToWhite");
								
								gradientRect.width = gradientWidth-gradientWidth * (toolkit.slopeBlendMaxAngle / 90);
								gradientRect.x = 27+gradientWidth * (toolkit.slopeBlendMaxAngle / 90);
								GUI.Box(gradientRect, "", "white");
								
							EditorGUILayout.EndHorizontal();
							
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
						
							// Cliff start
							EditorGUILayout.BeginHorizontal();
								EditorGUILayout.PrefixLabel("Cliff start");
								toolkit.slopeBlendMinAngle = EditorGUILayout.FloatField(toolkit.slopeBlendMinAngle);
							EditorGUILayout.EndHorizontal();
							
							// Cliff end
							EditorGUILayout.BeginHorizontal();
								EditorGUILayout.PrefixLabel("Cliff end");
								toolkit.slopeBlendMaxAngle = EditorGUILayout.FloatField(toolkit.slopeBlendMaxAngle);
							EditorGUILayout.EndHorizontal();
							
							// -----------------------------------------------------------------------------------------
							
							EditorGUILayout.Separator();
							
							GUILayout.Box("Texture Height");
										
							gradientRect = EditorGUILayout.BeginHorizontal();
								gradientWidth = gradientRect.width-55;
								gradientRect.width = 15;
								gradientRect.height = 19;
								Rect gradientRect2 = gradientRect;
								gradientRect2.y += 19;
								
								string[] gradientStyles = new string[9];
								gradientStyles[0] = "red";
								gradientStyles[1] = "redToYellow";
								gradientStyles[2] = "yellow";
								gradientStyles[3] = "yellowToGreen";
								gradientStyles[4] = "green";
								gradientStyles[5] = "greenToCyan";
								gradientStyles[6] = "cyan";
								gradientStyles[7] = "cyanToBlue";
								gradientStyles[8] = "blue";
								
								List<float> heightBlendPoints = toolkit.heightBlendPoints;
								int numPoints = heightBlendPoints.Count;
								float firstLimit = 1;
								
								if( numPoints>0 ){
									firstLimit = (float) heightBlendPoints[0];
								}
								else{
									gradientRect.x = 20;
									GUI.Box(gradientRect, "", "greyStop");
									gradientRect.x = 20+gradientWidth;
									GUI.Box(gradientRect, "", "greyStop");
								}
								
								gradientRect2.width = gradientWidth * firstLimit;
								gradientRect2.x = 27;
								
								if( toolkit.terrainLayers.Length<2 ){
									GUI.Box(gradientRect2, "", "grey");
								}
								else{
									GUI.Box(gradientRect2, "", "red");
								}
								
								for(i=0;i<numPoints;i++){
									
									// Height stop...
									float lowerLimit = 0;
									float upperLimit = 1;
									
									if( i>0 )	lowerLimit = (float) heightBlendPoints[i-1];
									if( i<numPoints-1 )	upperLimit = (float) heightBlendPoints[i+1];
									
									if( p_dragControl=="heightStop"+i && Event.current.type==EventType.MouseDrag ){
										mouseX = Event.current.mousePosition.x-7;
										if( mouseX<20+gradientWidth * lowerLimit )	mouseX = 20+gradientWidth * lowerLimit;
										else if( mouseX>19+gradientWidth * upperLimit )	mouseX = 19+gradientWidth * upperLimit;
										gradientRect.x = mouseX;
										heightBlendPoints[i] = (mouseX-20) / (gradientWidth+1);
									}
									else{
										gradientRect.x = 20+gradientWidth * (float) heightBlendPoints[i];
									}
									
									if( Event.current.type==EventType.MouseDown && gradientRect.Contains(Event.current.mousePosition) ){
										p_dragControl = "heightStop"+i;
									}
									
									if( p_dragControl=="heightStop"+i && Event.current.type==EventType.MouseUp ){
										p_dragControl = "";
									}
									
									int stopNum = (int) Mathf.Ceil((float) i / 2)+1;
									
									if( i % 2==0 ){
										GUI.Box(gradientRect, ""+stopNum, "blackStop");
									}
									else{
										GUI.Box(gradientRect, ""+stopNum, "whiteStop");
									}
									
									gradientRect2.width = gradientWidth * (upperLimit-(float) heightBlendPoints[i]);
									gradientRect2.x = 27+gradientWidth * (float) heightBlendPoints[i];
									GUI.Box(gradientRect2, "", gradientStyles[i+1]);
									
								}
						
							EditorGUILayout.EndHorizontal();
						
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
						
							string startOrEnd = "end";
							
							for (i = 0; i<numPoints;i++){
								EditorGUILayout.BeginHorizontal();
									int floatFieldNum = (int) Mathf.Ceil((float) i / 2)+1;
									EditorGUILayout.PrefixLabel("Texture "+floatFieldNum+" "+startOrEnd);
									heightBlendPoints[i] = EditorGUILayout.FloatField((float) heightBlendPoints[i]);
								EditorGUILayout.EndHorizontal();
								if( startOrEnd=="end" ){
									startOrEnd = "start";
								} else {
									startOrEnd = "end";
								}
							}
							
							// -----------------------------------------------------------------------------------------
							
							EditorGUILayout.Separator();
							
							GUILayout.Box("Textures");
							
							toolkit.heightBlendPoints = heightBlendPoints;
						
							int nTextures = 0;
							
							if( GUI.changed )	EditorUtility.SetDirty(toolkit);
							GUI.changed = false;
						
							GUI.skin = null;
							
							foreach (TerrainLayer terrainLayer in toolkit.terrainLayers ){
							
								EditorGUILayout.BeginHorizontal();
								
									if( nTextures==0 ){
										terrainLayer.diffuseTexture = EditorGUILayout.ObjectField("Cliff texture", terrainLayer.diffuseTexture, typeof(Texture2D),false) as Texture2D;
									}
									else{
										terrainLayer.diffuseTexture = EditorGUILayout.ObjectField("Texture "+nTextures, terrainLayer.diffuseTexture, typeof(Texture2D),false) as Texture2D;
									}						
									
									GUI.skin = p_skin;
									if(GUILayout.Button("", "deleteButton") ){
										GUI.changed = true;
										toolkit.deleteTerrainLayer(null, nTextures);
										EditorUtility.SetDirty(toolkit);
									}
									GUI.skin = null;
									
								EditorGUILayout.EndHorizontal();
							
								EditorGUILayout.Separator();
								
								nTextures++;
								if( nTextures>5 )	break;
								
							}
							
							if( GUI.changed ){
								terData.terrainLayers = toolkit.terrainLayers;
							}
							
							if( nTextures==0 ){
								GUI.skin = p_skin;
								GUILayout.Label(
									"[ERROR] No textures have been assigned !\n" +
									"[ERROR] Please assign a texture",
									"errorText"
								);
								GUI.skin = null;
							}
							
							if( nTextures<6 ){
								EditorGUILayout.Separator();
								if( GUILayout.Button("Add texture") ){
									toolkit.addTerrainLayer(p_defaultTexture, nTextures);
									terData.terrainLayers = toolkit.terrainLayers;
									EditorUtility.SetDirty(toolkit);
								}
								EditorGUILayout.Separator();
							}
							
							if( nTextures<2 ){
								GUI.skin = p_skin;
								GUILayout.Label(
									"[ERROR] You must assign at least 2 textures",
									"errorText"
								);
								GUI.skin = null;
							}				
							else if( GUILayout.Button("Apply procedural texture\n[NO UNDO]") ){
								
								// WARNING !!!!!!!!!!
								// Undo not supported!					
								TerrainToolkit.TextureProgressDelegate textureProgressDelegate = new TerrainToolkit.TextureProgressDelegate(updateTextureProgress);
								toolkit.textureTerrain(textureProgressDelegate);
								EditorUtility.ClearProgressBar();
								GUIUtility.ExitGUI();
								
							}
							
							EditorGUILayout.Separator();
							
							// If the user has added or removed textures in the Terrain component, correct the number of blend points...
							if( Event.current.type==EventType.Repaint ){
								
								if( numPoints % 2 != 0 )	toolkit.deleteAllBlendPoints();
								int correctNumPoints = (nTextures-2) * 2;
								if( nTextures<3 )	correctNumPoints = 0;
								if( numPoints<correctNumPoints )	toolkit.addBlendPoints();
								else if( numPoints>correctNumPoints )	toolkit.deleteBlendPoints();

							}
							
							// Break
							break;
							
						// ---------------------------------------------------------------------------------------
						// SETTINGS
					
						case 3 :
						
							// Advanced settings				
							drawAdvancedSettingsGUI();
						
							// Break
							break;
						
					}
					
					// Break
					break;
					
				// ---------------------------------------------------------------------------------------
				// TERRAIN MODELS MODE
			
				case 1 :
				
					// Prepare models contents
					GUIContent[] p_selectedModelsOptions = new GUIContent[6];
					p_selectedModelsOptions[0] = new GUIContent("<b>DESERT MESA</b>\nVoronoi + Fractal + Perlin\nTidal + Wind\n3 Texture(s)", p_model_00);
					p_selectedModelsOptions[1] = new GUIContent("<b>DESERT SANDS</b>\nVoronoi + Smooth + Normalise\nWind\n2 Texture(s)", p_model_01);
					p_selectedModelsOptions[2] = new GUIContent("<b>SEA SHORE</b>\n2xVoronoi + Perlin\nThermal + Full hydraulic + Tidal + Wind\n5 Texture(s)", p_model_02);
					p_selectedModelsOptions[3] = new GUIContent("<b>SNOWY MOUNTS</b>\nVoronoi + Fractal + Perlin\nFull hydraulic + Wind\n3 Texture(s)", p_model_03);
					p_selectedModelsOptions[4] = new GUIContent("<b>PAMPA</b>\nFractal + Perlin + Smooth\nFull hydraulic + Wind\n3 Texture(s)", p_model_04);
					p_selectedModelsOptions[5] = new GUIContent("<b>PATH OF THE FLESH</b>\nPerlin\nTidal\n2 Texture(s)", p_model_05);
					GUI.skin = p_skin;
					p_selectedModel = GUILayout.SelectionGrid(p_selectedModel, p_selectedModelsOptions, 1,"ToolkitModel");
					GUI.skin = null;

					string progressBarTitle = "Generating model [{0}]";
					string progressBarLabel = "Applying [{0}]. Please wait...";
					
					// Check...
					ter = toolkit.GetComponent<Terrain>();
					if( ter==null )	return;
					terData = ter.terrainData;
					
					// NO UNDO !
					
					// Variable(s)
					// float[] slopeStops;
					// float[] heightStops;
					// Texture2D []{
									// p_model_00_cliff,
									// p_model_00_tx1,
									// p_model_00_tx2 
								// }
							// );
							
					// terData.size = new Vector3(1000,600,1000);		
							
					// Switch selected model		
					switch( p_selectedModel ){
						
						// ---------------------------------------------------------------------------------------
						// MODEL 00
					
						case 0 :
						
							// Reset selected model
							p_selectedModel = -1;							
							
							// Reset terrain data
							toolkit.resetTerrain();
							terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							progressBarTitle = string.Format(progressBarTitle,"DESERT MESA");
							EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							toolkit.VoronoiGenerator(
								TerrainToolkit.FeatureType.Plateaus,
								18,
								0.75f,
								0.25f,
								0.25f
							);
							
							// Set UI values
							toolkit.voronoiTypeInt = 2;
							toolkit.voronoiCells = 18;
							toolkit.voronoiFeatures = 0.75f;
							toolkit.voronoiScale = 0.25f;
							toolkit.voronoiBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							toolkit.FractalGenerator(
								0.5f,
								0.2f
							);
							
							// Set UI values
							toolkit.diamondSquareDelta = 0.5f;
							toolkit.diamondSquareBlend = 0.2f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							toolkit.PerlinGenerator(
								8,
								0.5f,
								9,
								0.05f
							);
							
							// Set UI values
							toolkit.perlinFrequency = 8;
							toolkit.perlinAmplitude = 0.5f;
							toolkit.perlinOctaves = 9;
							toolkit.perlinBlend = 0.05f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							// toolkit.SmoothTerrain(2,0.25f);
							
							// Set UI values
							// toolkit.smoothIterations = 2;
							// toolkit.smoothBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							// toolkit.NormaliseTerrain(0.0f,1.0f,1.0f);
							
							// Set UI values
							// toolkit.normaliseMin = 0.0f;
							// toolkit.normaliseMax = 1.0f;
							// toolkit.normaliseBlend = 1.0f
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							// toolkit.FastThermalErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.thermalIterations = 25;
							// toolkit.thermalMinSlope = 5.0f;
							// toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							// toolkit.FullHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicRainfall = 0.5f;
							// toolkit.hydraulicEvaporation = 0.5f;
							// toolkit.hydraulicSedimentSolubility = 0.5f;
							// toolkit.hydraulicSedimentSaturation = 0.5f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							toolkit.TidalErosion(
								50,
								terData.size.y / 4,
								15.0f,
								25.0f
							);
							
							// Set UI values
							toolkit.tidalIterations = 50;
							toolkit.tidalSeaLevel = terData.size.y / 4;
							toolkit.tidalRangeAmount = 15.0f;
							toolkit.tidalCliffLimit = 25.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							toolkit.WindErosion(
								5,
								45.0f,
								0.5f,
								0.01f,
								0.5f,
								0.5f,
								0.1f,
								1.0f
							);
							
							// Set UI values
							toolkit.windIterations = 5;
							toolkit.windDirection = 45.0f;
							toolkit.windForce = 0.5f;
							toolkit.windLift = 0.01f;
							toolkit.windGravity = 0.5f;
							toolkit.windCapacity = 0.5f; 
							toolkit.windEntropy = 0.1f;
							toolkit.windSmoothing = 1.0f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							toolkit.TextureTerrain(
								new float[]{
										15.0f,
										45.0f
								},
								new float[]{
									0.2f,
									0.3f
								},
								new Texture2D []{
									p_model_00_cliff,
									p_model_00_tx1,
									p_model_00_tx2 
								}
							);
							
							
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							EditorUtility.ClearProgressBar();
							
							// Break
							break;
							
						// ---------------------------------------------------------------------------------------
						// MODEL 01
					
						case 1 :
						
							// Reset selected model
							p_selectedModel = -1;							
							
							// Reset terrain data
							toolkit.resetTerrain();
							terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							progressBarTitle = string.Format(progressBarTitle,"DESERT SANDS");
							EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							toolkit.VoronoiGenerator(
								TerrainToolkit.FeatureType.Plateaus,
								9,
								1.0f,
								0.0f,
								0.25f
							);
							
							// Set UI values
							toolkit.voronoiTypeInt = 1;
							toolkit.voronoiCells = 9;
							toolkit.voronoiFeatures = 1.0f;
							toolkit.voronoiScale = 0.0f;
							toolkit.voronoiBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							// toolkit.FractalGenerator(
								// 0.5f,
								// 0.2f
							// );
							
							// Set UI values
							// toolkit.diamondSquareDelta = 0.5f;
							// toolkit.diamondSquareBlend = 0.2f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							// toolkit.PerlinGenerator(
								// 8,
								// 0.5f,
								// 9,
								// 0.05f
							// );
							
							// Set UI values
							// toolkit.perlinFrequency = 8;
							// toolkit.perlinAmplitude = 0.5f;
							// toolkit.perlinOctaves = 9;
							// toolkit.perlinBlend = 0.05f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							toolkit.SmoothTerrain(25,1.0f);
							
							// Set UI values
							toolkit.smoothIterations = 5;
							toolkit.smoothBlend = 1.0f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							toolkit.NormaliseTerrain(0.0f,0.05f,1.0f);
							
							// Set UI values
							toolkit.normaliseMin = 0.0f;
							toolkit.normaliseMax = 0.075f;
							toolkit.normaliseBlend = 1.0f;
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							// toolkit.FastThermalErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.thermalIterations = 25;
							// toolkit.thermalMinSlope = 5.0f;
							// toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							// toolkit.FullHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicRainfall = 0.5f;
							// toolkit.hydraulicEvaporation = 0.5f;
							// toolkit.hydraulicSedimentSolubility = 0.5f;
							// toolkit.hydraulicSedimentSaturation = 0.5f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							// toolkit.TidalErosion(
								// 50,
								// terData.size.y / 4,
								// 15.0f,
								// 25.0f
							// );
							
							// Set UI values
							// toolkit.tidalIterations = 50;
							// toolkit.tidalSeaLevel = terData.size.y / 4;
							// toolkit.tidalRangeAmount = 15.0f;
							// toolkit.tidalCliffLimit = 25.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							toolkit.WindErosion(
								12,
								0.0f,
								0.5f,
								0.01f,
								0.5f,
								0.01f,
								0.1f,
								0.25f
							);
							
							// Set UI values
							toolkit.windIterations = 12;
							toolkit.windDirection = 0.0f;
							toolkit.windForce = 0.5f;
							toolkit.windLift = 0.01f;
							toolkit.windGravity = 0.5f;
							toolkit.windCapacity = 0.01f; 
							toolkit.windEntropy = 0.1f;
							toolkit.windSmoothing = 0.25f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							toolkit.TextureTerrain(
								new float[]{
										10.0f,
										20.0f
								},
								new float[]{
									// 0.2f,
									// 0.3f
								},
								new Texture2D []{
									p_model_01_cliff,
									p_model_01_tx1 
								}
							);
							
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							EditorUtility.ClearProgressBar();
							
							// Break
							break;
							
					// ---------------------------------------------------------------------------------------
					// MODEL 02
					
						case 02 :
						
							// Reset selected model
							p_selectedModel = -1;							
							
							// Reset terrain data
							toolkit.resetTerrain();
							terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							progressBarTitle = string.Format(progressBarTitle,"SEA SHORE");
							EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							toolkit.VoronoiGenerator(
								TerrainToolkit.FeatureType.Hills,
								10,
								1.0f,
								0.75f,
								0.25f
							);
							toolkit.VoronoiGenerator(
								TerrainToolkit.FeatureType.Hills,
								10,
								1.0f,
								0.75f,
								0.25f
							);
							
							// Set UI values
							toolkit.voronoiTypeInt = 1;
							toolkit.voronoiCells = 10;
							toolkit.voronoiFeatures = 1.0f;
							toolkit.voronoiScale = 0.75f;
							toolkit.voronoiBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							// toolkit.FractalGenerator(
								// 0.5f,
								// 0.2f
							// );
							
							// Set UI values
							// toolkit.diamondSquareDelta = 0.5f;
							// toolkit.diamondSquareBlend = 0.2f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							toolkit.PerlinGenerator(
								11,
								1.0f,
								7,
								0.05f
							);
							
							// Set UI values
							toolkit.perlinFrequency = 11;
							toolkit.perlinAmplitude = 1.0f;
							toolkit.perlinOctaves = 7;
							toolkit.perlinBlend = 0.05f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							// toolkit.SmoothTerrain(2,0.25f);
							
							// Set UI values
							// toolkit.smoothIterations = 2;
							// toolkit.smoothBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							// toolkit.NormaliseTerrain(0.0f,1.0f,1.0f);
							
							// Set UI values
							// toolkit.normaliseMin = 0.0f;
							// toolkit.normaliseMax = 1.0f;
							// toolkit.normaliseBlend = 1.0f
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							toolkit.FastThermalErosion(5,15.0f,0.5f);
							
							// Set UI values
							toolkit.thermalIterations = 5;
							toolkit.thermalMinSlope = 15.0f;
							toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							toolkit.FullHydraulicErosion(
								12,
								0.01f,
								0.5f,
								0.01f,
								0.1f
							);
							
							// Set UI values
							toolkit.hydraulicIterations = 12;
							toolkit.hydraulicRainfall = 0.01f;
							toolkit.hydraulicEvaporation = 0.5f;
							toolkit.hydraulicSedimentSolubility = 0.01f;
							toolkit.hydraulicSedimentSaturation = 0.1f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							toolkit.TidalErosion(
								25,
								terData.size.y / 4,
								90.0f,
								45.0f
							);
							
							// Set UI values
							toolkit.tidalIterations = 25;
							toolkit.tidalSeaLevel = terData.size.y / 4;
							toolkit.tidalRangeAmount = 90.0f;
							toolkit.tidalCliffLimit = 45.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							toolkit.WindErosion(
								5,
								180.0f,
								0.5f,
								0.01f,
								0.5f,
								0.5f,
								0.1f,
								1.0f
							);
							
							// Set UI values
							toolkit.windIterations = 5;
							toolkit.windDirection = 180.0f;
							toolkit.windForce = 0.5f;
							toolkit.windLift = 0.01f;
							toolkit.windGravity = 0.5f;
							toolkit.windCapacity = 0.5f; 
							toolkit.windEntropy = 0.1f;
							toolkit.windSmoothing = 1.0f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							toolkit.TextureTerrain(
								new float[]{
										30.0f,
										45.0f
								},
								new float[]{
									0.025f,
									0.05f,
									0.06f,
									0.07f,
									0.125f,
									0.15f
								},
								new Texture2D []{
									p_model_02_cliff,
									p_model_02_tx1,
									p_model_02_tx2,
									p_model_02_tx3,
									p_model_02_tx4
								}
							);							
							
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							EditorUtility.ClearProgressBar();
							
							// Break
							break;
							
					// ---------------------------------------------------------------------------------------
					// MODEL 03
					
						case 03 :
						
							// Reset selected model
							p_selectedModel = -1;							
							
							// Reset terrain data
							toolkit.resetTerrain();
							terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							progressBarTitle = string.Format(progressBarTitle,"SNOWY MOUNTS");
							EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							toolkit.VoronoiGenerator(
								TerrainToolkit.FeatureType.Hills,
								8,
								1.0f,
								0.0f,
								1.0f
							);
							
							// Set UI values
							toolkit.voronoiTypeInt = 1;
							toolkit.voronoiCells = 8;
							toolkit.voronoiFeatures = 1.0f;
							toolkit.voronoiScale = 0.0f;
							toolkit.voronoiBlend = 1.0f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							toolkit.FractalGenerator(
								0.4f,
								0.5f
							);
							
							// Set UI values
							toolkit.diamondSquareDelta = 0.4f;
							toolkit.diamondSquareBlend = 0.5f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							toolkit.PerlinGenerator(
								2,
								0.5f,
								9,
								0.5f
							);
							
							// Set UI values
							toolkit.perlinFrequency = 2;
							toolkit.perlinAmplitude = 0.5f;
							toolkit.perlinOctaves = 9;
							toolkit.perlinBlend = 0.5f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							// toolkit.SmoothTerrain(2,0.25f);
							
							// Set UI values
							// toolkit.smoothIterations = 2;
							// toolkit.smoothBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							// toolkit.NormaliseTerrain(0.0f,1.0f,1.0f);
							
							// Set UI values
							// toolkit.normaliseMin = 0.0f;
							// toolkit.normaliseMax = 1.0f;
							// toolkit.normaliseBlend = 1.0f
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							// toolkit.FastThermalErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.thermalIterations = 25;
							// toolkit.thermalMinSlope = 5.0f;
							// toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							toolkit.FullHydraulicErosion(
								25,
								0.02f,
								0.5f,
								0.01f,
								0.1f
							);
							
							// Set UI values
							toolkit.hydraulicIterations = 25;
							toolkit.hydraulicRainfall = 0.02f;
							toolkit.hydraulicEvaporation = 0.5f;
							toolkit.hydraulicSedimentSolubility = 0.01f;
							toolkit.hydraulicSedimentSaturation = 0.1f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							// toolkit.TidalErosion(
								// 50,
								// terData.size.y / 4,
								// 15.0f,
								// 25.0f
							// );
							
							// Set UI values
							// toolkit.tidalIterations = 50;
							// toolkit.tidalSeaLevel = terData.size.y / 4;
							// toolkit.tidalRangeAmount = 15.0f;
							// toolkit.tidalCliffLimit = 25.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							toolkit.WindErosion(
								12,
								270.0f,
								0.5f,
								0.01f,
								0.5f,
								0.01f,
								0.1f,
								0.25f
							);
							
							// Set UI values
							toolkit.windIterations = 12;
							toolkit.windDirection = 270.0f;
							toolkit.windForce = 0.5f;
							toolkit.windLift = 0.01f;
							toolkit.windGravity = 0.5f;
							toolkit.windCapacity = 0.01f; 
							toolkit.windEntropy = 0.1f;
							toolkit.windSmoothing = 0.25f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							toolkit.TextureTerrain(
								new float[]{
										25.0f,
										40.0f
								},
								new float[]{
									0.1f,
									0.25f
								},
								new Texture2D []{
									p_model_03_cliff,
									p_model_03_tx1,
									p_model_03_tx2 
								}
							);
							
							
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							EditorUtility.ClearProgressBar();
							
							// Break
							break;
							
					// ---------------------------------------------------------------------------------------
					// MODEL 04
					
						case 04 :
						
							// Reset selected model
							p_selectedModel = -1;							
							
							// Reset terrain data
							toolkit.resetTerrain();
							terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							progressBarTitle = string.Format(progressBarTitle,"PAMPA");
							EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							// toolkit.VoronoiGenerator(
								// TerrainToolkit.FeatureType.Plateaus,
								// 18,
								// 0.75f,
								// 0.25f,
								// 0.25f
							// );
							
							// Set UI values
							// toolkit.voronoiTypeInt = 2;
							// toolkit.voronoiCells = 18;
							// toolkit.voronoiFeatures = 0.75f;
							// toolkit.voronoiScale = 0.25f;
							// toolkit.voronoiBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							toolkit.FractalGenerator(
								0.2f,
								0.5f
							);
							
							// Set UI values
							toolkit.diamondSquareDelta = 0.2f;
							toolkit.diamondSquareBlend = 0.5f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							toolkit.PerlinGenerator(
								2,
								0.5f,
								9,
								0.25f
							);
							
							// Set UI values
							toolkit.perlinFrequency = 2;
							toolkit.perlinAmplitude = 0.5f;
							toolkit.perlinOctaves = 9;
							toolkit.perlinBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							toolkit.SmoothTerrain(2,0.25f);
							
							// Set UI values
							toolkit.smoothIterations = 2;
							toolkit.smoothBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							// toolkit.NormaliseTerrain(0.0f,1.0f,1.0f);
							
							// Set UI values
							// toolkit.normaliseMin = 0.0f;
							// toolkit.normaliseMax = 1.0f;
							// toolkit.normaliseBlend = 1.0f
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							// toolkit.FastThermalErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.thermalIterations = 25;
							// toolkit.thermalMinSlope = 5.0f;
							// toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							toolkit.FullHydraulicErosion(
								12,
								0.02f,
								0.5f,
								0.06f,
								0.15f
							);
							
							// Set UI values
							toolkit.hydraulicIterations = 12;
							toolkit.hydraulicRainfall = 0.02f;
							toolkit.hydraulicEvaporation = 0.5f;
							toolkit.hydraulicSedimentSolubility = 0.06f;
							toolkit.hydraulicSedimentSaturation = 0.15f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							// toolkit.TidalErosion(
								// 50,
								// terData.size.y / 4,
								// 15.0f,
								// 25.0f
							// );
							
							// Set UI values
							// toolkit.tidalIterations = 50;
							// toolkit.tidalSeaLevel = terData.size.y / 4;
							// toolkit.tidalRangeAmount = 15.0f;
							// toolkit.tidalCliffLimit = 25.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							toolkit.WindErosion(
								5,
								90.0f,
								0.5f,
								0.01f,
								0.5f,
								0.01f,
								0.1f,
								0.25f
							);
							
							// Set UI values
							toolkit.windIterations = 5;
							toolkit.windDirection = 90.0f;
							toolkit.windForce = 0.5f;
							toolkit.windLift = 0.01f;
							toolkit.windGravity = 0.5f;
							toolkit.windCapacity = 0.01f; 
							toolkit.windEntropy = 0.1f;
							toolkit.windSmoothing = 0.25f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							toolkit.TextureTerrain(
								new float[]{
										10.0f,
										30.0f
								},
								new float[]{
									0.1f,
									0.3f
								},
								new Texture2D []{
									p_model_04_cliff,
									p_model_04_tx1,
									p_model_04_tx2 
								}
							);
														
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							EditorUtility.ClearProgressBar();
							
							// Break
							break;
							
					// ---------------------------------------------------------------------------------------
					// MODEL XXX
					
						case 05 :
						
							// Reset selected model
							p_selectedModel = -1;							
							
							// Reset terrain data
							toolkit.resetTerrain();
							terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							progressBarTitle = string.Format(progressBarTitle,"PATH OF THE FLESH");
							EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							// toolkit.VoronoiGenerator(
								// TerrainToolkit.FeatureType.Plateaus,
								// 18,
								// 0.75f,
								// 0.25f,
								// 0.25f
							// );
							
							// Set UI values
							// toolkit.voronoiTypeInt = 2;
							// toolkit.voronoiCells = 18;
							// toolkit.voronoiFeatures = 0.75f;
							// toolkit.voronoiScale = 0.25f;
							// toolkit.voronoiBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							// toolkit.FractalGenerator(
								// 0.5f,
								// 0.2f
							// );
							
							// Set UI values
							// toolkit.diamondSquareDelta = 0.5f;
							// toolkit.diamondSquareBlend = 0.2f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							toolkit.PerlinGenerator(
								11,
								1.0f,
								7,
								0.25f
							);
							
							// Set UI values
							toolkit.perlinFrequency = 11;
							toolkit.perlinAmplitude = 1.0f;
							toolkit.perlinOctaves = 7;
							toolkit.perlinBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							// toolkit.SmoothTerrain(2,0.25f);
							
							// Set UI values
							// toolkit.smoothIterations = 2;
							// toolkit.smoothBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							// toolkit.NormaliseTerrain(0.0f,1.0f,1.0f);
							
							// Set UI values
							// toolkit.normaliseMin = 0.0f;
							// toolkit.normaliseMax = 1.0f;
							// toolkit.normaliseBlend = 1.0f
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							// toolkit.FastThermalErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.thermalIterations = 25;
							// toolkit.thermalMinSlope = 5.0f;
							// toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							// toolkit.FullHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicRainfall = 0.5f;
							// toolkit.hydraulicEvaporation = 0.5f;
							// toolkit.hydraulicSedimentSolubility = 0.5f;
							// toolkit.hydraulicSedimentSaturation = 0.5f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							toolkit.TidalErosion(
								100,
								terData.size.y / 4,
								100.0f,
								55.0f
							);
							
							// Set UI values
							toolkit.tidalIterations = 100;
							toolkit.tidalSeaLevel = terData.size.y / 4;
							toolkit.tidalRangeAmount = 100.0f;
							toolkit.tidalCliffLimit = 55.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							// toolkit.WindErosion(
								// 5,
								// 45.0f,
								// 0.5f,
								// 0.01f,
								// 0.5f,
								// 0.5f,
								// 0.1f,
								// 1.0f
							// );
							
							// Set UI values
							// toolkit.windIterations = 5;
							// toolkit.windDirection = 45.0f;
							// toolkit.windForce = 0.5f;
							// toolkit.windLift = 0.01f;
							// toolkit.windGravity = 0.5f;
							// toolkit.windCapacity = 0.5f; 
							// toolkit.windEntropy = 0.1f;
							// toolkit.windSmoothing = 1.0f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							toolkit.TextureTerrain(
								new float[]{
										25.0f,
										50.0f
								},
								new float[]{
									// 0.2f,
									// 0.3f
								},
								new Texture2D []{
									p_model_05_cliff,
									p_model_05_tx1 
								}
							);							
							
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							EditorUtility.ClearProgressBar();
							
							// Break
							break;
					
					// ---------------------------------------------------------------------------------------
					// MODEL XXX
					
						// case XXX :
						
							// Reset selected model
							// p_selectedModel = -1;							
							
							// Reset terrain data
							// toolkit.resetTerrain();
							// terData.terrainLayers = toolkit.terrainLayers;
							
							// Initialize UI
							// progressBarTitle = string.Format(progressBarTitle,"DESERT MESA");
							// EditorUtility.DisplayProgressBar(progressBarTitle, "", 0.0f);
							
							// ---------------------------------------------------------------
							// VORONOI
							// FeatureType among Mountains / Hills / Plateaus (TerrainToolkit.FeatureType.Hills)
							// Cells = from 2 to 100
							// Features from 0.0f to 1.0f
							// Scale from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f							
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VORONOI"), 0.05f);
							// toolkit.VoronoiGenerator(
								// TerrainToolkit.FeatureType.Plateaus,
								// 18,
								// 0.75f,
								// 0.25f,
								// 0.25f
							// );
							
							// Set UI values
							// toolkit.voronoiTypeInt = 2;
							// toolkit.voronoiCells = 18;
							// toolkit.voronoiFeatures = 0.75f;
							// toolkit.voronoiScale = 0.25f;
							// toolkit.voronoiBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// FRACTAL
							// Delta from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FRACTAL"), 0.1f);
							// toolkit.FractalGenerator(
								// 0.5f,
								// 0.2f
							// );
							
							// Set UI values
							// toolkit.diamondSquareDelta = 0.5f;
							// toolkit.diamondSquareBlend = 0.2f;
							
							// ---------------------------------------------------------------
							// PERLIN
							// Frequency from 1 to 16
							// Amplitude from 0.0f to 1.0f
							// Octaves from 1 to 12
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"PERLIN"), 0.15f);
							// toolkit.PerlinGenerator(
								// 8,
								// 0.5f,
								// 9,
								// 0.05f
							// );
							
							// Set UI values
							// toolkit.perlinFrequency = 8;
							// toolkit.perlinAmplitude = 0.5f;
							// toolkit.perlinOctaves = 9;
							// toolkit.perlinBlend = 0.05f;
							
							// ---------------------------------------------------------------
							// SMOOTH
							// Iterationsfrom 1 to 5
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"SMOOTH"), 0.20f);
							// toolkit.SmoothTerrain(2,0.25f);
							
							// Set UI values
							// toolkit.smoothIterations = 2;
							// toolkit.smoothBlend = 0.25f;
							
							// ---------------------------------------------------------------
							// NORMALISE
							// NormaliseMin from 0.0f to 1.0f
							// NormaliseMax from 0.0f to 1.0f
							// Blend from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"NORMALISE"), 0.25f);
							// toolkit.NormaliseTerrain(0.0f,1.0f,1.0f);
							
							// Set UI values
							// toolkit.normaliseMin = 0.0f;
							// toolkit.normaliseMax = 1.0f;
							// toolkit.normaliseBlend = 1.0f
							
							// ---------------------------------------------------------------
							// FASTTHERMALEROSION
							// Iterations from 1 to 250
							// minSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"THERMAL EROSION"), 0.30f);
							// toolkit.FastThermalErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.thermalIterations = 25;
							// toolkit.thermalMinSlope = 5.0f;
							// toolkit.thermalFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FASTHYDRAULICEROSION
							// Iterations from 1 to 250
							// maxSlope from 1 to 89.99
							// Falloff from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FAST HYDRAULIC EROSION"), 0.35f);
							// toolkit.FastHydraulicErosion(25,5.0f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicMaxSlope = 5.0f;
							// toolkit.hydraulicFalloff = 0.5f;
							
							// ---------------------------------------------------------------
							// FULLHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"FULL HYDRAULIC EROSION"), 0.40f);
							// toolkit.FullHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicRainfall = 0.5f;
							// toolkit.hydraulicEvaporation = 0.5f;
							// toolkit.hydraulicSedimentSolubility = 0.5f;
							// toolkit.hydraulicSedimentSaturation = 0.5f;
							
							// ---------------------------------------------------------------
							// VELOCITYHYDRAULICEROSION
							// Iterations from 1 to 250
							// Rainfall from 0.0f to 1.0f	
							// Evaporation from 0.0f to 1.0f	
							// Solubility from 0.0f to 1.0f	
							// Saturation from 0.0f to 1.0f	
							// Velocity from 1 to 10
							// Momentum from 1 to 10
							// Entropy from 0.0f to 1.0f	
							// Downcutting from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"VELOCITY HYDRAULIC EROSION"), 0.45f);
							// toolkit.VelocityHydraulicErosion(25,0.5f,0.5f,0.5f,0.5f, 5, 5,0.5f,0.5f);
							
							// Set UI values
							// toolkit.hydraulicIterations = 25;
							// toolkit.hydraulicVelocityRainfall = 0.5f; 
							// toolkit.hydraulicVelocityEvaporation = 0.5f;
							// toolkit.hydraulicVelocitySedimentSolubility = 0.5f; 
							// toolkit.hydraulicVelocitySedimentSaturation = 0.5f;
							// toolkit.hydraulicVelocity = 5;
							// toolkit.hydraulicMomentum = 5;
							// toolkit.hydraulicEntropy = 0.5f;
							// toolkit.hydraulicDowncutting = 0.5f;
							
							// ---------------------------------------------------------------
							// TIDALEROSION
							// Iterations from 1 to 250
							// seaLevel  
							// tidalRange	
							// cliffLimit from 0.0f to 90.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TIDAL EROSION"), 0.50f);
							// toolkit.TidalErosion(
								// 50,
								// terData.size.y / 4,
								// 15.0f,
								// 25.0f
							// );
							
							// Set UI values
							// toolkit.tidalIterations = 50;
							// toolkit.tidalSeaLevel = terData.size.y / 4;
							// toolkit.tidalRangeAmount = 15.0f;
							// toolkit.tidalCliffLimit = 25.0f;
							
							// ---------------------------------------------------------------
							// WINDEROSION
							// Iterations from 1 to 250
							// Wind direction from 0.0f to 360.0f	
							// Wind force from 0.0f to 1.0f	
							// Lift from 0.0f to 0.01f	
							// Gravity from 0.0f to 1.0f	
							// Capacity from 0.0f to 1.0f	
							// Entropy from 0.0f to 1.0f	
							// Smoothing from 0.0f to 1.0f	
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"WIND EROSION"), 0.55f);
							// toolkit.WindErosion(
								// 5,
								// 45.0f,
								// 0.5f,
								// 0.01f,
								// 0.5f,
								// 0.5f,
								// 0.1f,
								// 1.0f
							// );
							
							// Set UI values
							// toolkit.windIterations = 5;
							// toolkit.windDirection = 45.0f;
							// toolkit.windForce = 0.5f;
							// toolkit.windLift = 0.01f;
							// toolkit.windGravity = 0.5f;
							// toolkit.windCapacity = 0.5f; 
							// toolkit.windEntropy = 0.1f;
							// toolkit.windSmoothing = 1.0f;
							
							// ---------------------------------------------------------------
							// TEXTURE
							
							// Process
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"TEXTURES"), 0.60f);
							// toolkit.TextureTerrain(
								// new float[]{
										// 15.0f,
										// 45.0f
								// },
								// new float[]{
									// 0.2f,
									// 0.3f
								// },
								// new Texture2D []{
									// p_model_00_cliff,
									// p_model_00_tx1,
									// p_model_00_tx2 
								// }
							// );							
							
							// UI is set automatically
														
							// ---------------------------------------------------------------
							
							// Finish
							// EditorUtility.DisplayProgressBar(progressBarTitle,string.Format(progressBarLabel,"--"), 1.0f);
							// EditorUtility.ClearProgressBar();
							
							// Break
							// break;

					}
					
				// GUILayout.EndVertical();
					// Break
					break;
				
			}
			
			
		
		EditorGUILayout.EndVertical();	
		
		EditorGUILayout.Separator();
		
		if( GUI.changed )	EditorUtility.SetDirty(toolkit);
				
	}
	
/*---------------------------------------------------------------------------*/
/* OnSceneGUI function */
/*---------------------------------------------------------------------------*/
	
	public void OnSceneGUI( ){
		
		TerrainToolkit toolkit = target as TerrainToolkit;
		
		if( Event.current.type==EventType.MouseDown )	toolkit.isBrushPainting = true;
		
		if( Event.current.type==EventType.MouseUp )	toolkit.isBrushPainting = false;
		
		if( Event.current.shift ){
			
			if (!toolkit.isBrushPainting ){
				// Undo...
				Terrain ter = toolkit.GetComponent<Terrain>();
				if( ter==null )	return;
				TerrainData terData = ter.terrainData;
				Undo.RegisterCompleteObjectUndo(terData,"Terrain Erosion Brush");
			}
			toolkit.isBrushPainting = true;
			
		}
		else{
			
			toolkit.isBrushPainting = false;
			
		}
		
		toolkit.isBrushHidden = false;
		
		if( toolkit.isBrushOn ){
			
			Vector2 mouse = Event.current.mousePosition;
			mouse.y = Camera.current.pixelHeight-mouse.y+20;
			Ray ray = Camera.current.ScreenPointToRay(mouse);
			RaycastHit hit;
			
			// Paint...
			if( Physics.Raycast(ray, out hit, Mathf.Infinity) ){
				if( hit.transform.GetComponent("TerrainToolkit") ){
					toolkit.brushPosition = hit.point;
					if( toolkit.isBrushPainting )	toolkit.paint();
				}
			}
			else{
				toolkit.isBrushHidden = true;
			}
			
		}
		
	}
	
/*---------------------------------------------------------------------------*/
/* Draw brush tools */
/*---------------------------------------------------------------------------*/
	
	public void drawBrushToolsGUI( ){
		
		TerrainToolkit toolkit = target as TerrainToolkit;
		
		EditorGUILayout.Separator();
				
		GUI.skin = p_skin;
		GUILayout.Box(
			"Brushes\n\n" +
			"       HINTS :\n" +
			"       1. Hold down the SHIFT key to use the brush\n" +
			"       2. Use the brush PRESET for best results\n"
		);
		GUI.skin = null;
						
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Brush");
			string[] brushStates = new string[2];
			brushStates[0] = "Off";
			brushStates[1] = "On";
			int brushInt = 0;
			if( toolkit.isBrushOn )	brushInt = 1;
			brushInt = GUILayout.Toolbar(brushInt, brushStates);
			bool brushBool = false;
			if( brushInt==1 )	brushBool = true;
			toolkit.isBrushOn = brushBool;
		EditorGUILayout.EndHorizontal();
		
		// Brush size
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Brush size");
			toolkit.brushSize = EditorGUILayout.Slider(toolkit.brushSize, 1, 100);
		EditorGUILayout.EndHorizontal();
		
		// Opacity
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Opacity");
			toolkit.brushOpacity = EditorGUILayout.Slider(toolkit.brushOpacity, 0, 1);
		EditorGUILayout.EndHorizontal();

		// Softness
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Softness");
			toolkit.brushSoftness = EditorGUILayout.Slider(toolkit.brushSoftness, 0, 1);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator();
		
	}
	
/*---------------------------------------------------------------------------*/
/* Draw advanced settings */
/*---------------------------------------------------------------------------*/
	
	public void drawAdvancedSettingsGUI( ){
		
		TerrainToolkit toolkit = target as TerrainToolkit;
		
		EditorGUILayout.BeginVertical();
		
			GUI.skin = p_skin;
			GUILayout.Box("Cell neighbourhood");
			GUI.skin = null;
		
			EditorGUILayout.Separator();
			
			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Cell neighbourhood");
				GUIContent[] neighbourhoodOptions = new GUIContent[2];
				neighbourhoodOptions[0] = new GUIContent("Moore", p_mooreIcon);
				neighbourhoodOptions[1] = new GUIContent("Von Neumann", p_vonNeumannIcon);
				toolkit.neighbourhoodInt = GUILayout.Toolbar(toolkit.neighbourhoodInt, neighbourhoodOptions);
			EditorGUILayout.EndHorizontal();
			
			// --------------------------------------------------------------------
			
			EditorGUILayout.Separator();
			
			GUI.skin = p_skin;
			GUILayout.Box("Difference maps");
			GUI.skin = null;
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Use difference maps in brush mode");
				string[] diffMapStates = new string[2];
				diffMapStates[0] = "Off";
				diffMapStates[1] = "On";
				int diffMapInt = 0;
				if( toolkit.useDifferenceMaps )	diffMapInt = 1;
				diffMapInt = GUILayout.Toolbar(diffMapInt, diffMapStates);
				bool diffMapBool = false;
				if( diffMapInt==1 )	diffMapBool = true;
				toolkit.useDifferenceMaps = diffMapBool;
			EditorGUILayout.EndHorizontal();
			
			// --------------------------------------------------------------------
			
			EditorGUILayout.Separator();
			
			GUI.skin = p_skin;
			GUILayout.Box("Presets");
			GUI.skin = null;
			
			EditorGUILayout.Separator();
			
			if( GUILayout.Button("Reload Presets") ){
				toolkit.presetsInitialised = false;
			}
			
			EditorGUILayout.Separator();
			
			// --------------------------------------------------------------------
						
			GUI.skin = p_skin;
			GUILayout.Box("Terrain");
			GUI.skin = null;
			
			EditorGUILayout.Separator();
			
			if( GUILayout.Button("Reset terrain\n[NO UNDO]") ){
				
				// Reset terrain
				toolkit.resetTerrain();
				
			}
			
			EditorGUILayout.Separator();
			
			if( GUILayout.Button("Invert terrain\n[NO UNDO][RE-APPLY TEXTURES]") ){
				
				// Invert terrain
				toolkit.invertTerrain();
				
			}
			
			EditorGUILayout.Separator();
			
			// Set the amount of terrain reserved for tiling !
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Seam size (% of terrain)");
				p_amount = EditorGUILayout.Slider(p_amount, 0.01f, 0.5f);
			EditorGUILayout.EndHorizontal();
			
			if( GUILayout.Button("Seamless terrain\n[RE-APPLY TEXTURES]") ){
				
				// Variable(s)
				Terrain ter;
				TerrainData terData;
				
				// Just in case....
				ter = toolkit.GetComponent<Terrain>();
				if( ter==null )	return;
				
				terData = ter.terrainData;
				
				Undo.RegisterCompleteObjectUndo(terData,"Turn terrain seamless");
								
				// Transforma terrain !
				toolkit.seamlessTerrain(p_amount);
				
			}			
			
			EditorGUILayout.Separator();
			
		EditorGUILayout.EndVertical();
		
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void updateErosionProgress(string titleString, string displayString, int iteration, int nIterations, float percentComplete ){
		EditorUtility.DisplayProgressBar(titleString, displayString+" Iteration "+iteration+" of "+nIterations+". Please wait.", percentComplete);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void updateTextureProgress(string titleString, string displayString, float percentComplete ){
		EditorUtility.DisplayProgressBar(titleString, displayString, percentComplete);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void updateGeneratorProgress(string titleString, string displayString, float percentComplete ){
		EditorUtility.DisplayProgressBar(titleString, displayString, percentComplete);
	}
	


/*---------------------------------------------------------------------------*/

}

/*---------------------------------------------------------------------------*/
/* END CLASS */
/*---------------------------------------------------------------------------*/

}

/*---------------------------------------------------------------------------*/
/* END NAMESPACE */
/*---------------------------------------------------------------------------*/