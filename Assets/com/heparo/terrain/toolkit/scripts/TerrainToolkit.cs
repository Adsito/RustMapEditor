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
 * The Unity Terrain Toolkit
 * Unity Summer of Code 2009
 * Terrain Toolkit for Unity (Version 1.0.2)
 * All code by Sándor Moldán, except where noted.
 * Contains an implementation of Perlin noise by Daniel Greenheck.
 * Contains an implementation of the Diamond-Square algorithm by Jim George.
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
using System.IO;
using System.Collections;
using System.Collections.Generic;

/*---------------------------------------------------------------------------*/
/* NAMESPACE */
/*---------------------------------------------------------------------------*/

namespace com.heparo.terrain.toolkit {

/*---------------------------------------------------------------------------*/
/* START CLASS */
/*---------------------------------------------------------------------------*/

[ExecuteInEditMode()]
[AddComponentMenu("Terrain/Terrain Toolkit")]

public class TerrainToolkit : MonoBehaviour {
	
/*---------------------------------------------------------------------------*/
/* MEMBERS */
/*---------------------------------------------------------------------------*/

		// ------------------------------------------------------------------------
		// STRINGS CONSTANTS
		
		public const string EMPTY = "";
		public const string TERRAIN_LAYER_EXTENSION = ".terrainlayer";
		public const string TERRAIN_LAYER_PREFIX = "Layer_";
		public const string TOOLKIT_LAYERS_FOLDER = "TerrainToolkitLayers";
		public const string TERRAIN_LAYERS_FOLDER = "_Layers";

		// --------------------------------------------------------------------------

		public enum ToolMode {Create = 0, Erode = 1, Texture = 2}
		public enum ErosionMode {Filter = 0, Brush = 1}
		public enum ErosionType {Thermal = 0, Hydraulic = 1, Tidal = 2, Wind = 3, Glacial = 4}
		public enum HydraulicType {Fast = 0, Full = 1, Velocity = 2}
		public enum Neighbourhood {Moore = 0, VonNeumann = 1}
		public enum GeneratorType {Voronoi = 0, DiamondSquare = 1, Perlin = 2, Smooth = 3, Normalise = 4}
		public enum VoronoiType {Linear = 0, Sine = 1, Tangent = 2}
		public enum FeatureType {Mountains = 0, Hills = 1, Plateaus = 2}
		
		// --------------------------------------------------------------------------	
		
		public struct Peak {
			public Vector2 peakPoint;
			public float peakHeight;
		}

		// --------------------------------------------------------------------------	
		
		private ErosionMode p_erosionMode = ErosionMode.Filter;
		private ErosionType p_erosionType = ErosionType.Thermal;
		private GeneratorType p_generatorType = GeneratorType.Voronoi;
		private Neighbourhood p_neighbourhood = Neighbourhood.Moore;
		private HydraulicType p_hydraulicType = HydraulicType.Fast;
		private VoronoiType p_voronoiType = VoronoiType.Linear;

		// --------------------------------------------------------------------------
	
		// Delegates...
		public delegate void ErosionProgressDelegate(string titleString, string displayString, int iteration, int nIterations, float percentComplete);
		public delegate void TextureProgressDelegate(string titleString, string displayString, float percentComplete);
		public delegate void GeneratorProgressDelegate(string titleString, string displayString, float percentComplete);
	
		// --------------------------------------------------------------------------
	
		// Global..
		public int toolModeInt = 0;
		public int erosionTypeInt = 0;
		public int generatorTypeInt = 0;
		public bool isBrushOn = false;
		public bool isBrushHidden = false;
		public bool isBrushPainting = false;
		public Vector3 brushPosition;
		public float brushSize = 50.0f;
		public float brushOpacity = 1.0f;
		public float brushSoftness = 0.5f;
		
		// Settings...
		public int neighbourhoodInt = 0;
		public bool useDifferenceMaps = true;
		
		// Thermal...
		public int thermalIterations = 25;
		public float thermalMinSlope = 1.0f;
		public float thermalFalloff = 0.5f;
		
		// Hydraulic...
		public int hydraulicTypeInt = 0;
		public int hydraulicIterations = 25;
		
		// Fast...
		public float hydraulicMaxSlope = 60.0f;
		public float hydraulicFalloff = 0.5f;
		
		// Full...
		public float hydraulicRainfall = 0.01f;
		public float hydraulicEvaporation = 0.5f;
		public float hydraulicSedimentSolubility = 0.01f;
		public float hydraulicSedimentSaturation = 0.1f;
		
		// Velocity...
		public float hydraulicVelocityRainfall = 0.01f;
		public float hydraulicVelocityEvaporation = 0.5f;
		public float hydraulicVelocitySedimentSolubility = 0.01f;
		public float hydraulicVelocitySedimentSaturation = 0.1f;
		public float hydraulicVelocity = 20.0f;
		public float hydraulicMomentum = 1.0f;
		public float hydraulicEntropy = 0.0f;
		public float hydraulicDowncutting = 0.1f;
		
		// Tidal...
		public int tidalIterations = 25;
		public float tidalSeaLevel = 50.0f;
		public float tidalRangeAmount = 5.0f;
		public float tidalCliffLimit = 60.0f;
		
		// Wind...
		public int windIterations = 25;
		public float windDirection = 0.0f;
		public float windForce = 0.5f;
		public float windLift = 0.01f;
		public float windGravity = 0.5f;
		public float windCapacity = 0.01f;
		public float windEntropy = 0.1f;
		public float windSmoothing = 0.25f;
		
		// Texturing...
		public TerrainLayer[] terrainLayers;
		public float slopeBlendMinAngle = 60.0f;
		public float slopeBlendMaxAngle = 75.0f;
		public List<float> heightBlendPoints;
		public string[] gradientStyles;
		
		// Generators...
		public int voronoiTypeInt = 0;		
		public int voronoiCells = 16;
		public float voronoiFeatures = 1.0f;
		public float voronoiScale = 1.0f;
		public float voronoiBlend = 1.0f;
		public float diamondSquareDelta = 0.5f;
		public float diamondSquareBlend = 1.0f;
		public int perlinFrequency = 4;
		public float perlinAmplitude = 1.0f;
		public int perlinOctaves = 8;
		public float perlinBlend = 1.0f;
		public float smoothBlend = 1.0f;
		public int smoothIterations;
		public float normaliseMin = 0.0f;
		public float normaliseMax = 1.0f;
		public float normaliseBlend = 1.0f;
		
		// Presets...
		public ArrayList voronoiPresets = new ArrayList();
		public ArrayList fractalPresets = new ArrayList();
		public ArrayList perlinPresets = new ArrayList();
		public ArrayList thermalErosionPresets = new ArrayList();
		public ArrayList fastHydraulicErosionPresets = new ArrayList();
		public ArrayList fullHydraulicErosionPresets = new ArrayList();
		public ArrayList velocityHydraulicErosionPresets = new ArrayList();
		public ArrayList tidalErosionPresets = new ArrayList();
		public ArrayList windErosionPresets = new ArrayList();
		
		// --------------------------------------------------------------------------
		
		[System.NonSerialized]
		public bool presetsInitialised = false;
		[System.NonSerialized]
		public int voronoiPresetId = 0;
		[System.NonSerialized]
		public int fractalPresetId = 0;
		[System.NonSerialized]
		public int perlinPresetId = 0;
		[System.NonSerialized]
		public int thermalErosionPresetId = 0;
		[System.NonSerialized]
		public int fastHydraulicErosionPresetId = 0;
		[System.NonSerialized]
		public int fullHydraulicErosionPresetId = 0;
		[System.NonSerialized]
		public int velocityHydraulicErosionPresetId = 0;
		[System.NonSerialized]
		public int tidalErosionPresetId = 0;
		[System.NonSerialized]
		public int windErosionPresetId = 0;	
		
		// --------------------------------------------------------------------------
		
		string layersPath = EMPTY;
		string assetPath = EMPTY ; 
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
	// Default constructor
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void addPresets(){
		
		presetsInitialised = true;
		voronoiPresets = new ArrayList();
		fractalPresets = new ArrayList();
		perlinPresets = new ArrayList();
		thermalErosionPresets = new ArrayList();
		fastHydraulicErosionPresets = new ArrayList();
		fullHydraulicErosionPresets = new ArrayList();
		velocityHydraulicErosionPresets = new ArrayList();
		tidalErosionPresets = new ArrayList();
		windErosionPresets = new ArrayList();
		voronoiPresets.Add(new voronoiPresetData("Scattered Peaks", VoronoiType.Linear, 16, 8, 0.5f, 1.0f));
		voronoiPresets.Add(new voronoiPresetData("Rolling Hills", VoronoiType.Sine, 8, 8, 0.0f, 1.0f));
		voronoiPresets.Add(new voronoiPresetData("Jagged Mountains", VoronoiType.Linear, 32, 32, 0.5f, 1.0f));
		fractalPresets.Add(new fractalPresetData("Rolling Plains", 0.4f, 1.0f));
		fractalPresets.Add(new fractalPresetData("Rough Mountains", 0.5f, 1.0f));
		fractalPresets.Add(new fractalPresetData("Add Noise", 0.75f, 0.05f));
		perlinPresets.Add(new perlinPresetData("Rough Plains", 2, 0.5f, 9, 1.0f));
		perlinPresets.Add(new perlinPresetData("Rolling Hills", 5, 0.75f, 3, 1.0f));
		perlinPresets.Add(new perlinPresetData("Rocky Mountains", 4, 1.0f, 8, 1.0f));
		perlinPresets.Add(new perlinPresetData("Hellish Landscape", 11, 1.0f, 7, 1.0f));
		perlinPresets.Add(new perlinPresetData("Add Noise", 10, 1.0f, 8, 0.2f));
		thermalErosionPresets.Add(new thermalErosionPresetData("Gradual, Weak Erosion", 25, 7.5f, 0.5f));
		thermalErosionPresets.Add(new thermalErosionPresetData("Fast, Harsh Erosion", 25, 2.5f, 0.1f));
		thermalErosionPresets.Add(new thermalErosionPresetData("Thermal Erosion Brush", 25, 0.1f, 0.0f));
		fastHydraulicErosionPresets.Add(new fastHydraulicErosionPresetData("Rainswept Earth", 25, 70.0f, 1.0f));
		fastHydraulicErosionPresets.Add(new fastHydraulicErosionPresetData("Terraced Slopes", 25, 30.0f, 0.4f));
		fastHydraulicErosionPresets.Add(new fastHydraulicErosionPresetData("Hydraulic Erosion Brush", 25, 85.0f, 1.0f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Low Rainfall, Hard Rock", 25, 0.01f, 0.5f, 0.01f, 0.1f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Low Rainfall, Soft Earth", 25, 0.01f, 0.5f, 0.06f, 0.15f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Heavy Rainfall, Hard Rock", 25, 0.02f, 0.5f, 0.01f, 0.1f));
		fullHydraulicErosionPresets.Add(new fullHydraulicErosionPresetData("Heavy Rainfall, Soft Earth", 25, 0.02f, 0.5f, 0.06f, 0.15f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Low Rainfall, Hard Rock", 25, 0.01f, 0.5f, 0.01f, 0.1f, 1.0f, 1.0f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Low Rainfall, Soft Earth", 25, 0.01f, 0.5f, 0.06f, 0.15f, 1.2f, 2.8f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Heavy Rainfall, Hard Rock", 25, 0.02f, 0.5f, 0.01f, 0.1f, 1.1f, 2.2f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Heavy Rainfall, Soft Earth", 25, 0.02f, 0.5f, 0.06f, 0.15f, 1.2f, 2.4f, 0.05f, 0.12f));
		velocityHydraulicErosionPresets.Add(new velocityHydraulicErosionPresetData("Carved Stone", 25, 0.01f, 0.5f, 0.01f, 0.1f, 2.0f, 1.25f, 0.05f, 0.35f));
		tidalErosionPresets.Add(new tidalErosionPresetData("Low Tidal Range, Calm Waves", 25, 5.0f, 65.0f));
		tidalErosionPresets.Add(new tidalErosionPresetData("Low Tidal Range, Strong Waves", 25, 5.0f, 35.0f));
		tidalErosionPresets.Add(new tidalErosionPresetData("High Tidal Range, Calm Water", 25, 15.0f, 55.0f));
		tidalErosionPresets.Add(new tidalErosionPresetData("High Tidal Range, Strong Waves", 25, 15.0f, 25.0f));
		windErosionPresets.Add(new windErosionPresetData("Default (Northerly)", 25, 180.0f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Southerly)", 25, 0.0f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Easterly)", 25, 270.0f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		windErosionPresets.Add(new windErosionPresetData("Default (Westerly)", 25, 90.0f, 0.5f, 0.01f, 0.5f, 0.01f, 0.1f, 0.25f));
		
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setVoronoiPreset(voronoiPresetData preset){
		generatorTypeInt = 0;
		p_generatorType = GeneratorType.Voronoi;
		voronoiTypeInt = (int) preset.p_voronoiType;
		p_voronoiType = preset.p_voronoiType;
		voronoiCells = preset.voronoiCells;
		voronoiFeatures = preset.voronoiFeatures;
		voronoiScale = preset.voronoiScale;
		voronoiBlend = preset.voronoiBlend;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setFractalPreset(fractalPresetData preset){
		generatorTypeInt = 1;
		p_generatorType = GeneratorType.DiamondSquare;
		diamondSquareDelta = preset.diamondSquareDelta;
		diamondSquareBlend = preset.diamondSquareBlend;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setPerlinPreset(perlinPresetData preset){
		generatorTypeInt = 2;
		p_generatorType = GeneratorType.Perlin;
		perlinFrequency = preset.perlinFrequency;
		perlinAmplitude = preset.perlinAmplitude;
		perlinOctaves = preset.perlinOctaves;
		perlinBlend = preset.perlinBlend;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setThermalErosionPreset(thermalErosionPresetData preset){
		erosionTypeInt = 0;
		p_erosionType = ErosionType.Thermal;
		thermalIterations = preset.thermalIterations;
		thermalMinSlope = preset.thermalMinSlope;
		thermalFalloff = preset.thermalFalloff;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setFastHydraulicErosionPreset(fastHydraulicErosionPresetData preset){
		erosionTypeInt = 1;
		p_erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 0;
		p_hydraulicType = HydraulicType.Fast;
		hydraulicIterations = preset.hydraulicIterations;
		hydraulicMaxSlope = preset.hydraulicMaxSlope;
		hydraulicFalloff = preset.hydraulicFalloff;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setFullHydraulicErosionPreset(fullHydraulicErosionPresetData preset){
		erosionTypeInt = 1;
		p_erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 1;
		p_hydraulicType = HydraulicType.Full;
		hydraulicIterations = preset.hydraulicIterations;
		hydraulicRainfall = preset.hydraulicRainfall;
		hydraulicEvaporation = preset.hydraulicEvaporation;
		hydraulicSedimentSolubility = preset.hydraulicSedimentSolubility;
		hydraulicSedimentSaturation = preset.hydraulicSedimentSaturation;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setVelocityHydraulicErosionPreset(velocityHydraulicErosionPresetData preset){
		erosionTypeInt = 1;
		p_erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 2;
		p_hydraulicType = HydraulicType.Velocity;
		hydraulicIterations = preset.hydraulicIterations;
		hydraulicVelocityRainfall = preset.hydraulicVelocityRainfall;
		hydraulicVelocityEvaporation = preset.hydraulicVelocityEvaporation;
		hydraulicVelocitySedimentSolubility = preset.hydraulicVelocitySedimentSolubility;
		hydraulicVelocitySedimentSaturation = preset.hydraulicVelocitySedimentSaturation;
		hydraulicVelocity = preset.hydraulicVelocity;
		hydraulicMomentum = preset.hydraulicMomentum;
		hydraulicEntropy = preset.hydraulicEntropy;
		hydraulicDowncutting = preset.hydraulicDowncutting;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setTidalErosionPreset(tidalErosionPresetData preset){
		erosionTypeInt = 2;
		p_erosionType = ErosionType.Tidal;
		tidalIterations = preset.tidalIterations;
		tidalRangeAmount = preset.tidalRangeAmount;
		tidalCliffLimit = preset.tidalCliffLimit;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void setWindErosionPreset(windErosionPresetData preset){
		erosionTypeInt = 3;
		p_erosionType = ErosionType.Wind;
		windIterations = preset.windIterations;
		windDirection = preset.windDirection;
		windForce = preset.windForce;
		windLift = preset.windLift;
		windGravity = preset.windGravity;
		windCapacity = preset.windCapacity;
		windEntropy = preset.windEntropy;
		windSmoothing = preset.windSmoothing;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void Update(){
		if( isBrushOn ){
			if( toolModeInt!=1 || erosionTypeInt>2 || ( erosionTypeInt==1 && hydraulicTypeInt>0 ) ){
				isBrushOn = false;
			}
		}
		
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void OnDrawGizmos(){

		Terrain ter = (Terrain) GetComponent(typeof(Terrain));

		if( ter==null )	return;
		
		// Brush gizmos...
		
		if( isBrushOn && !isBrushHidden ){
			
			if( isBrushPainting ){
				Gizmos.color = Color.red;
			}
			else{
				Gizmos.color = Color.white;
			}
			
			float crossHairSize = brushSize / 4.0f;
			Gizmos.DrawLine((brushPosition + new Vector3(-crossHairSize, 0, 0)), (brushPosition + new Vector3(crossHairSize, 0, 0)));
			Gizmos.DrawLine(brushPosition + new Vector3(0, -crossHairSize, 0), brushPosition + new Vector3(0, crossHairSize, 0));
			Gizmos.DrawLine(brushPosition + new Vector3(0, 0, -crossHairSize), brushPosition + new Vector3(0, 0, crossHairSize));
			Gizmos.DrawWireCube(brushPosition, new Vector3(brushSize, 0, brushSize));
			Gizmos.DrawWireSphere(brushPosition, brushSize / 2);
		}
		TerrainData terData = ter.terrainData;
		Vector3 terSize = terData.size;
		// Tidal gizmos...
		if( toolModeInt==1 && erosionTypeInt==2 ){
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(new Vector3(transform.position.x + terSize.x / 2, tidalSeaLevel, transform.position.z + terSize.z / 2), new Vector3(terSize.x, 0.0f, terSize.z));
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(new Vector3(transform.position.x + terSize.x / 2, tidalSeaLevel, transform.position.z + terSize.z / 2), new Vector3(terSize.x, tidalRangeAmount * 2, terSize.z));
		}
		// Wind gizmos...
		if( toolModeInt==1 && erosionTypeInt==3 ){
			Gizmos.color = Color.blue;
			Quaternion windQuaternion = Quaternion.Euler(0.0f, windDirection, 0);
			Vector3 windVector = windQuaternion * Vector3.forward;
			Vector3 startPoint = new Vector3(transform.position.x + terSize.x / 2, transform.position.y + terSize.y, transform.position.z + terSize.z / 2);
			Vector3 endPoint = startPoint + windVector * (terSize.x / 4);
			Vector3 midPoint = startPoint + windVector * (terSize.x / 6);
			Gizmos.DrawLine(startPoint, endPoint);
			Gizmos.DrawLine(endPoint, midPoint + new Vector3(0, terSize.x / 16, 0));
			Gizmos.DrawLine(endPoint, midPoint - new Vector3(0, terSize.x / 16, 0));
		}
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void paint(){
		convertIntVarsToEnums();
		erodeTerrainWithBrush();
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void erodeTerrainWithBrush(){
		p_erosionMode = ErosionMode.Brush;
		// Error checking...
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		
		if( ter==null )	return;
		
		int Px = 0;
		int Py = 0;
		
		try {
			
			TerrainData terData = ter.terrainData;
			int Tw = terData.heightmapWidth;
			int Th = terData.heightmapHeight;
			Vector3 Ts = terData.size;
			int Sx = (int) Mathf.Floor((Tw / Ts.x) * brushSize);
			int Sy = (int) Mathf.Floor((Th / Ts.z) * brushSize);
			Vector3 localBrushPosition = transform.InverseTransformPoint(brushPosition);
			Px = (int) Mathf.Round((localBrushPosition.x / Ts.x) * Tw - (Sx / 2));
			Py = (int) Mathf.Round((localBrushPosition.z / Ts.z) * Th - (Sy / 2));
			if( Px<0 ){
				Sx = Sx + Px;
				Px = 0;
			}
			if( Py<0 ){
				Sy = Sy + Py;
				Py = 0;
			}
			if( Px + Sx>Tw){
				Sx = Tw - Px;
			}
			if( Py + Sy>Th ){
				Sy = Th - Py;
			}
			float[,] heightMap = terData.GetHeights(Px, Py, Sx, Sy);
			Sx = heightMap.GetLength(1);
			Sy = heightMap.GetLength(0);
			// Pass the height array to the erosion script...
			float[,] erodedheightMap = (float[,]) heightMap.Clone();
			ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
			erodedheightMap = fastErosion(erodedheightMap, new Vector2(Sx, Sy), 1, erosionProgressDelegate);
			// Apply it to the terrain object
			float sampleRadius = Sx / 2.0f;
			for( int Ty = 0; Ty<Sx; Ty++ ){
				for( int Tx = 0; Tx<Sy; Tx++ ){
					float oldHeightAtPoint = heightMap[Tx, Ty];
					float newHeightAtPoint = erodedheightMap[Tx, Ty];
					float distancefromOrigin = Vector2.Distance(new Vector2(Tx, Ty), new Vector2(sampleRadius, sampleRadius));
					float weightAtPoint = 1.0f - ((distancefromOrigin - (sampleRadius - (sampleRadius * brushSoftness))) / (sampleRadius * brushSoftness));
					if( weightAtPoint<0.0f ){
						weightAtPoint = 0.0f;
					} else if( weightAtPoint>1.0f ){
						weightAtPoint = 1.0f;
					}
					weightAtPoint *= brushOpacity;
					float blendedHeightAtPoint = (newHeightAtPoint * weightAtPoint) + (oldHeightAtPoint * (1.0f - weightAtPoint));
					heightMap[Tx, Ty] = blendedHeightAtPoint;
				}
			}
			terData.SetHeights(Px, Py, heightMap);
		}
		catch(System.Exception e){
		 	Debug.LogError("A brush error occurred : "+e);
		}
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void erodeAllTerrain(ErosionProgressDelegate erosionProgressDelegate){
		p_erosionMode = ErosionMode.Filter;
		// Check enum vars...
		convertIntVarsToEnums();
		// Error checking...
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		if( ter==null )	return;
		try {
			TerrainData terData = ter.terrainData;
			int Tw = terData.heightmapWidth;
			int Th = terData.heightmapHeight;
			float[,] heightMap = terData.GetHeights(0, 0, Tw, Th);
			// Set the number of iterations and pass the height array to the appropriate erosion script...
			int iterations;
			switch( p_erosionType){
				case ErosionType.Thermal:
					iterations = thermalIterations;
					heightMap = fastErosion(heightMap, new Vector2(Tw, Th), iterations, erosionProgressDelegate);
					break;
				case ErosionType.Hydraulic:
					iterations = hydraulicIterations;
					switch( p_hydraulicType){
						case HydraulicType.Fast:
							heightMap = fastErosion(heightMap, new Vector2(Tw, Th), iterations, erosionProgressDelegate);
							break;
						case HydraulicType.Full:
							heightMap = fullHydraulicErosion(heightMap, new Vector2(Tw, Th), iterations, erosionProgressDelegate);
							break;
						case HydraulicType.Velocity:
							heightMap = velocityHydraulicErosion(heightMap, new Vector2(Tw, Th), iterations, erosionProgressDelegate);
							break;
					}
					break;
				case ErosionType.Tidal:
					Vector3 terSize = terData.size;
					if( tidalSeaLevel>=transform.position.y && tidalSeaLevel<=transform.position.y + terSize.y ){
						iterations = tidalIterations;
						heightMap = fastErosion(heightMap, new Vector2(Tw, Th), iterations, erosionProgressDelegate);
					}
					else{
						Debug.LogError("Sea level does not intersect terrain object. Erosion operation failed.");
					}
					break;
				case ErosionType.Wind:
					iterations = windIterations;
					heightMap = windErosion(heightMap, new Vector2(Tw, Th), iterations, erosionProgressDelegate);
					break;
				default:
					return;
			}
			// Apply it to the terrain object
			terData.SetHeights(0, 0, heightMap);
		}
		catch(System.Exception e){
			Debug.LogError("An error occurred : "+e);
		}
		
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] fastErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate){
		int Tw = (int) arraySize.y;
		int Th = (int) arraySize.x;
		float[,] heightDiffMap = new float[Tw, Th];
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		TerrainData terData = ter.terrainData;
		Vector3 terSize = terData.size;
		float minSlopeBlendMin = 0.0f;
		float minSlopeBlendMax = 0.0f;
		float maxSlopeBlendMin = 0.0f;
		float maxSlopeBlendMax = 0.0f;
		float seaLevel = 0.0f;
		float lowerTidalLimit = 0.0f;
		float upperTidalLimit = 0.0f;
		float tidalRange = 0.0f;
		float cliffLimit = 0.0f;
		switch( p_erosionType){
			case ErosionType.Thermal:
				minSlopeBlendMin = ((terSize.x / Tw) * Mathf.Tan(thermalMinSlope * Mathf.Deg2Rad)) / terSize.y;
				if( minSlopeBlendMin>1.0f ){
					minSlopeBlendMin = 1.0f;
				}
				if( thermalFalloff==1.0f ){
					thermalFalloff = 0.999f;
				}
				float thermalMaxSlope = thermalMinSlope + ((90 - thermalMinSlope) * thermalFalloff);
				minSlopeBlendMax = ((terSize.x / Tw) * Mathf.Tan(thermalMaxSlope * Mathf.Deg2Rad)) / terSize.y;
				if( minSlopeBlendMax>1.0f ){
					 minSlopeBlendMax = 1.0f;
				}
				break;
			case ErosionType.Hydraulic:
				maxSlopeBlendMax = ((terSize.x / Tw) * Mathf.Tan(hydraulicMaxSlope * Mathf.Deg2Rad)) / terSize.y;
				if( hydraulicFalloff==0.0f ){
					hydraulicFalloff = 0.001f;
				}
				float hydraulicMinSlope = hydraulicMaxSlope * (1 - hydraulicFalloff);
				maxSlopeBlendMin = ((terSize.x / Tw) * Mathf.Tan(hydraulicMinSlope * Mathf.Deg2Rad)) / terSize.y;
				break;
			case ErosionType.Tidal:
				seaLevel = (tidalSeaLevel - transform.position.y) / (transform.position.y + terSize.y);
				lowerTidalLimit = (tidalSeaLevel - transform.position.y - tidalRangeAmount) / (transform.position.y + terSize.y);
				upperTidalLimit = (tidalSeaLevel - transform.position.y + tidalRangeAmount) / (transform.position.y + terSize.y);
				tidalRange = upperTidalLimit - seaLevel;
				cliffLimit = ((terSize.x / Tw) * Mathf.Tan(tidalCliffLimit * Mathf.Deg2Rad)) / terSize.y;
				break;
			default:
				return heightMap;
		}
		int xNeighbours;
		int yNeighbours;
		int xShift;
		int yShift;
		int xIndex;
		int yIndex;
		int Tx;
		int Ty;
		// Start iterations...
		for( int iter = 0; iter<iterations; iter++ ){
			for( Ty = 0; Ty<Th; Ty++ ){
				// y...
				if( Ty==0 ){
					yNeighbours = 2;
					yShift = 0;
					yIndex = 0;
				}
				else if( Ty==Th-1 ){
					yNeighbours = 2;
					yShift = -1;
					yIndex = 1;
				}
				else {
					yNeighbours = 3;
					yShift = -1;
					yIndex = 1;
				}
				for( Tx = 0; Tx<Tw; Tx++ ){
					// x...
					if( Tx==0 ){
						xNeighbours = 2;
						xShift = 0;
						xIndex = 0;
					}
					else if( Tx==Tw-1 ){
						xNeighbours = 2;
						xShift = -1;
						xIndex = 1;
					}
					else {
						xNeighbours = 3;
						xShift = -1;
						xIndex = 1;
					}
					// Calculate slope...
					float tMin = 1.0f;
					float tMax = 0.0f;
					float tCumulative = 0.0f;
					int Ny;
					int Nx;
					float t;
					float heightAtIndex = heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift]; // Get height at index
					float hCumulative = heightAtIndex;
					int nNeighbours = 0;
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( Nx!=xIndex || Ny!=yIndex ){
								if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex)) ){
									float heightAtPoint = heightMap[Tx + Nx + xShift, Ty + Ny + yShift]; // Get height at point
									// Tidal...
									hCumulative += heightAtPoint;
									// Others...
									t = heightAtIndex - heightAtPoint;
									if( t>0 ){
										tCumulative += t;
										if( t<tMin )	tMin = t;
										if( t>tMax )	tMax = t;
									}
									nNeighbours++;
								}
							}
						}
					}
					float tAverage = tCumulative / nNeighbours;
					// float tAverage = tMax;
					// Erosion type...
					bool doErode = false;
					switch( p_erosionType ){
						case ErosionType.Thermal:
							if( tAverage>=minSlopeBlendMin ){
								doErode = true;
							}
							break;
						case ErosionType.Hydraulic:
							if( tAverage>0 && tAverage<=maxSlopeBlendMax ){
								doErode = true;
							}
							break;
						case ErosionType.Tidal:
							if( tAverage>0 && tAverage<=cliffLimit && heightAtIndex<upperTidalLimit && heightAtIndex>lowerTidalLimit ){
								doErode = true;
							}
							break;
						default:
							return heightMap;
					}
					if( doErode ){
						float blendAmount;
						if( p_erosionType==ErosionType.Tidal ){
							// Tidal...
							float hAverage = hCumulative / (nNeighbours + 1);
							float dTidalSeaLevel = Mathf.Abs(seaLevel - heightAtIndex);
							blendAmount = dTidalSeaLevel / tidalRange;
							float blendHeight = heightAtIndex * blendAmount + hAverage * (1 - blendAmount);
							float blendTidalSeaLevel = Mathf.Pow(dTidalSeaLevel, 3);
							heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = seaLevel * blendTidalSeaLevel + blendHeight * (1 - blendTidalSeaLevel);
						}
						else {
							// Thermal or Hydraulic...
							float blendRange;
							if( p_erosionType==ErosionType.Thermal ){
								if( tAverage>minSlopeBlendMax ){
									blendAmount = 1;
								}
								else {
									blendRange = minSlopeBlendMax - minSlopeBlendMin;
									blendAmount = (tAverage - minSlopeBlendMin) / blendRange; // minSlopeBlendMin = 0; minSlopeBlendMax = 1
								}
							}
							else {
								if( tAverage<maxSlopeBlendMin ){
									blendAmount = 1;
								}
								else {
									blendRange = maxSlopeBlendMax - maxSlopeBlendMin;
									blendAmount = 1 - ((tAverage - maxSlopeBlendMin) / blendRange); // maxSlopeBlendMin = 1; maxSlopeBlendMax = 0
								}
							}
							
							// From SDudzic on version 20180515170000 in user reviews
							// In fastErosion method this line seems to be bugged: 
							// float m = tMin / 2 * blendAmount; 
							// Thermal erosion is producing artifacts, when I have changed this to: 
							float m = tAverage / 2 * blendAmount; 
							// It started generating smooth terrain, no sharp edges. I also cannot find any reason to use tMin. If any surrounding point is higher erosion will never happen.

							float pointValue = heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift];
							if( p_erosionMode==ErosionMode.Filter || (p_erosionMode==ErosionMode.Brush && useDifferenceMaps) ){
								// Pass to difference map...
								float heightDiffAtIndexSoFar = heightDiffMap[Tx + xIndex + xShift, Ty + yIndex + yShift];
								float heightAtIndexDiff = heightDiffAtIndexSoFar - m;
								heightDiffMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = heightAtIndexDiff;
							}
							else {
								float pointValueAfter = pointValue - m;
								if( pointValueAfter<0 )	pointValueAfter = 0;
								heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = pointValueAfter;
							}
							for( Ny = 0; Ny<yNeighbours; Ny++ ){
								for( Nx = 0; Nx<xNeighbours; Nx++ ){
									if( Nx!=xIndex || Ny!=yIndex ){
										if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
											float neighbourValue = heightMap[Tx + Nx + xShift, Ty + Ny + yShift];
											t = pointValue - neighbourValue;
											// Only move material downhill...
											if( t>0 ){
												float mProportional = m * (t / tCumulative);
												if( p_erosionMode==ErosionMode.Filter || (p_erosionMode==ErosionMode.Brush && useDifferenceMaps) ){
													// Pass to difference map...
													float heightDiffAtNeighbourSoFar = heightDiffMap[Tx + Nx + xShift, Ty + Ny + yShift];
													float heightAtNeighbourDiff = heightDiffAtNeighbourSoFar + mProportional;
													heightDiffMap[Tx + Nx + xShift, Ty + Ny + yShift] = heightAtNeighbourDiff;
												}
												else {
													neighbourValue += mProportional;
													if( neighbourValue<0 )	neighbourValue = 0;
													heightMap[Tx + Nx + xShift, Ty + Ny + yShift] = neighbourValue;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if( (p_erosionMode==ErosionMode.Filter || (p_erosionMode==ErosionMode.Brush && useDifferenceMaps)) && p_erosionType!=ErosionType.Tidal ){
				// Apply height difference map to height map...
				float heightAtCell;
				for( Ty = 0; Ty<Th; Ty++ ){
					for( Tx = 0; Tx<Tw; Tx++ ){
						heightAtCell = heightMap[Tx, Ty] + heightDiffMap[Tx, Ty];
						if( heightAtCell>1.0f){
							heightAtCell = 1.0f;
						}
						else if( heightAtCell<0.0f){
							heightAtCell = 0.0f;
						}
						heightMap[Tx, Ty] = heightAtCell;
						heightDiffMap[Tx, Ty] = 0.0f;
					}
				}
			}
			if( p_erosionMode==ErosionMode.Filter ){
				// Update progress...
				string titleString = "";
				string displayString = "";
				switch( p_erosionType){
					case ErosionType.Thermal:
						titleString = "Applying Thermal Erosion";
						displayString = "Applying thermal erosion.";
						break;
					case ErosionType.Hydraulic:
						titleString = "Applying Hydraulic Erosion";
						displayString = "Applying hydraulic erosion.";
						break;
					case ErosionType.Tidal:
						titleString = "Applying Tidal Erosion";
						displayString = "Applying tidal erosion.";
						break;
					default:
						return heightMap;
				}
				float percentComplete = (float) iter / (float) iterations;
				erosionProgressDelegate(titleString, displayString, iter, iterations, percentComplete);
			}
		}
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] velocityHydraulicErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate){
		int Tx = (int) arraySize.x;
		int Ty = (int) arraySize.y;
		float[,] precipitationMap = new float[Tx, Ty];
		float[,] slopeMap = new float[Tx, Ty];
		float[,] waterMap = new float[Tx, Ty];
		float[,] waterDiffMap = new float[Tx, Ty];
		float[,] velocityMap = new float[Tx, Ty];
		float[,] velocityDiffMap = new float[Tx, Ty];
		float[,] sedimentMap = new float[Tx, Ty];
		float[,] sedimentDiffMap = new float[Tx, Ty];
		int Mx;
		int My;
		int xNeighbours;
		int yNeighbours;
		int xShift;
		int yShift;
		int xIndex;
		int yIndex;
		float maxSediment;
		// Zero maps...
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				waterMap[Mx, My] = 0.0f;
				waterDiffMap[Mx, My] = 0.0f;
				velocityMap[Mx, My] = 0.0f;
				velocityDiffMap[Mx, My] = 0.0f;
				sedimentMap[Mx, My] = 0.0f;
				sedimentDiffMap[Mx, My] = 0.0f;
			}
		}
		// Cache precipitation map...
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				float precipitationAmount = heightMap[Mx, My];
				precipitationMap[Mx, My] = precipitationAmount;
			}
		}
		// Cache slope map and initialise velocity map...
		float tCumulative;
		int Nx;
		int Ny;
		float t;
		float heightAtIndex;
		float heightAtPoint;
		int nNeighbours;
		for( My = 0; My<Ty; My++ ){
			// y...
			if( My==0 ){
				yNeighbours = 2;
				yShift = 0;
				yIndex = 0;
			} 
			else if( My==Ty - 1){
				yNeighbours = 2;
				yShift = -1;
				yIndex = 1;
			}
			else {
				yNeighbours = 3;
				yShift = -1;
				yIndex = 1;
			}
			for( Mx = 0; Mx<Tx; Mx++ ){
				// x...
				if( Mx==0){
					xNeighbours = 2;
					xShift = 0;
					xIndex = 0;
				}
				else if( Mx==Tx - 1){
					xNeighbours = 2;
					xShift = -1;
					xIndex = 1;
				}
				else {
					xNeighbours = 3;
					xShift = -1;
					xIndex = 1;
				}
				// Calculate slope and create velocity map...
				tCumulative = 0.0f;
				heightAtIndex = heightMap[Mx + xIndex + xShift, My + yIndex + yShift]; // Get height at index
				nNeighbours = 0;
				for( Ny = 0; Ny<yNeighbours; Ny++ ){
					for( Nx = 0; Nx<xNeighbours; Nx++ ){
						if( Nx!=xIndex || Ny!=yIndex){
							if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
								heightAtPoint = heightMap[Mx + Nx + xShift, My + Ny + yShift]; // Get height at point 
								t = Mathf.Abs(heightAtIndex - heightAtPoint);
								tCumulative += t;
								nNeighbours++;
							}
						}
					}
				}
				float tAverage = tCumulative / nNeighbours;
				slopeMap[Mx + xIndex + xShift, My + yIndex + yShift] = tAverage;
				// velocityMap[Mx + xIndex + xShift, My + yIndex + yShift] = Mathf.Min(tAverage * hydraulicVelocity, 1.0f);
			}
		}
		// Start iterations...
		for( int iter = 0; iter<iterations; iter++ ){
			// Add water proportional to precipitation...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					float waterAmount = waterMap[Mx, My] + precipitationMap[Mx, My] * hydraulicVelocityRainfall;
					if( waterAmount>1.0f)	waterAmount = 1.0f;
					waterMap[Mx, My] = waterAmount;
				}
			}
			// Dissolve material as sediment...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					float sedimentAmount = sedimentMap[Mx, My];
					maxSediment = waterMap[Mx, My] * hydraulicVelocitySedimentSaturation;
					if( sedimentAmount<maxSediment ){
						float erodedSediment = waterMap[Mx, My] * velocityMap[Mx, My] * hydraulicVelocitySedimentSolubility;
						if( sedimentAmount + erodedSediment>maxSediment)	erodedSediment = maxSediment - sedimentAmount;
						heightAtIndex = heightMap[Mx, My];
						if( erodedSediment>heightAtIndex)	erodedSediment = heightAtIndex;
						sedimentMap[Mx, My] = sedimentAmount + erodedSediment;
						heightMap[Mx, My] = heightAtIndex - erodedSediment;
					}
				}
			}
			// Calculate velocity and move water...
			for( My = 0; My<Ty; My++ ){
				// y...
				if( My==0 ){
					yNeighbours = 2;
					yShift = 0;
					yIndex = 0;
				}
				else if( My==Ty - 1){
					yNeighbours = 2;
					yShift = -1;
					yIndex = 1;
				}
				else {
					yNeighbours = 3;
					yShift = -1;
					yIndex = 1;
				}
				for( Mx = 0; Mx<Tx; Mx++ ){
					// x...
					if( Mx==0 ){
						xNeighbours = 2;
						xShift = 0;
						xIndex = 0;
					}
					else if( Mx==Tx - 1 ){
						xNeighbours = 2;
						xShift = -1;
						xIndex = 1;
					}
					else {
						xNeighbours = 3;
						xShift = -1;
						xIndex = 1;
					}
					// Calculate slope...
					tCumulative = 0.0f;
					heightAtIndex = heightMap[Mx, My]; // Get height at index
					float hCumulative = heightAtIndex;
					float waterAtIndex = waterMap[Mx, My]; // Get water at index
					nNeighbours = 0;
					float waterAtPoint;
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( Nx!=xIndex || Ny!=yIndex){
								if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
									heightAtPoint = heightMap[Mx + Nx + xShift, My + Ny + yShift]; // Get height at point
									waterAtPoint = waterMap[Mx + Nx + xShift, My + Ny + yShift]; // Get water at point
									t = (heightAtIndex + waterAtIndex) - (heightAtPoint + waterAtPoint);
									// Only calculate downhill cells...
									if( t>0){
										tCumulative += t;
										hCumulative += heightAtIndex + waterAtIndex;
										nNeighbours++;
									}
								}
							}
						}
					}
					float velocityAtIndex = velocityMap[Mx, My];
					float slopeAtIndex = slopeMap[Mx, My];
					float sedimentAtIndex = sedimentMap[Mx, My];
					float totalVelocityAtIndex = velocityAtIndex + (hydraulicVelocity * slopeAtIndex);
					// Calculate water to be transported away from the index...
					float hAverage = hCumulative / (nNeighbours + 1);
					float dWater = (heightAtIndex + waterAtIndex) - hAverage;
					float transportedWater = Mathf.Min(waterAtIndex, dWater * (1.0f + velocityAtIndex));
					float waterDiffAtIndexSoFar = waterDiffMap[Mx, My];
					// Pass to difference map...
					float waterAtIndexDiff = waterDiffAtIndexSoFar - transportedWater;
					waterDiffMap[Mx, My] = waterAtIndexDiff;
					float transferredVelocity = totalVelocityAtIndex * (transportedWater / waterAtIndex);
					float transferredSediment = sedimentAtIndex * (transportedWater / waterAtIndex);
					// Neighbours...
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( Nx!=xIndex || Ny!=yIndex){
								if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
									heightAtPoint = heightMap[Mx + Nx + xShift, My + Ny + yShift]; // Get height at point
									waterAtPoint = waterMap[Mx + Nx + xShift, My + Ny + yShift]; // Get water at point
									t = (heightAtIndex + waterAtIndex) - (heightAtPoint + waterAtPoint);
									// Only affect downhill cells...
									if( t>0.0f){
										// Move water...
										float transportedWaterSoFar = waterDiffMap[Mx + Nx + xShift, My + Ny + yShift];
										float transportedWaterProportional = transportedWater * (t / tCumulative);
										float transportedWaterAfter = transportedWaterSoFar + transportedWaterProportional;
										// Pass to difference map...
										waterDiffMap[Mx + Nx + xShift, My + Ny + yShift] = transportedWaterAfter;
										// Transfer velocity...
										float transferredVelocitySoFar = velocityDiffMap[Mx + Nx + xShift, My + Ny + yShift];
										float transferredVelocityProportional = transferredVelocity * hydraulicMomentum * (t / tCumulative);
										float transferredVelocityAfter = transferredVelocitySoFar + transferredVelocityProportional;
										// Pass to difference map...
										velocityDiffMap[Mx + Nx + xShift, My + Ny + yShift] = transferredVelocityAfter;
										// Transfer sediment...
										float transferredSedimentSoFar = sedimentDiffMap[Mx + Nx + xShift, My + Ny + yShift];
										float transferredSedimentProportional = transferredSediment * hydraulicMomentum * (t / tCumulative);
										float transferredSedimentAfter = transferredSedimentSoFar + transferredSedimentProportional;
										// Pass to difference map...
										sedimentDiffMap[Mx + Nx + xShift, My + Ny + yShift] = transferredSedimentAfter;
									}
								}
							}
						}
					}
					// Lose velocity at index...
					float velocityDiffSoFar = velocityDiffMap[Mx, My];
					velocityDiffMap[Mx, My] = velocityDiffSoFar - transferredVelocity;
				}
			}
			// Apply velocity difference map to velocity map...
			float velocityAtCell;
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					velocityAtCell = velocityMap[Mx, My] + velocityDiffMap[Mx, My];
					// Calculate entropy...
					velocityAtCell *= 1.0f - hydraulicEntropy;
					if( velocityAtCell>1.0f){
						velocityAtCell = 1.0f;
					}
					else if( velocityAtCell<0.0f){
						velocityAtCell = 0.0f;
					}
					velocityMap[Mx, My] = velocityAtCell;
					velocityDiffMap[Mx, My] = 0.0f;
				}
			}
			// Apply water difference map to water map...
			float waterAtCell;
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					waterAtCell = waterMap[Mx, My] + waterDiffMap[Mx, My];
					// Calculate evaporation...
					float evaporatedWater = waterAtCell * hydraulicVelocityEvaporation;
					waterAtCell -= evaporatedWater;
					if( waterAtCell>1.0f){
						waterAtCell = 1.0f;
					}
					else if( waterAtCell<0.0f){
						waterAtCell = 0.0f;
					}
					waterMap[Mx, My] = waterAtCell;
					waterDiffMap[Mx, My] = 0.0f;
				}
			}
			// Apply sediment difference map to sediment map...
			float sedimentAtCell;
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					sedimentAtCell = sedimentMap[Mx, My] + sedimentDiffMap[Mx, My];
					if( sedimentAtCell>1.0f){
						sedimentAtCell = 1.0f;
					}
					else if( sedimentAtCell<0.0f){
						sedimentAtCell = 0.0f;
					}
					sedimentMap[Mx, My] = sedimentAtCell;
					sedimentDiffMap[Mx, My] = 0.0f;
				}
			}
			// Deposit sediment...
			float heightAtCell;
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					maxSediment = waterMap[Mx, My] * hydraulicVelocitySedimentSaturation;
					sedimentAtCell = sedimentMap[Mx, My];
					if( sedimentAtCell>maxSediment){
						float depositedSediment = sedimentAtCell - maxSediment;
						sedimentMap[Mx, My] = maxSediment;
						heightAtCell = heightMap[Mx, My];
						heightMap[Mx, My] = heightAtCell + depositedSediment;
					}
				}
			}
			// Downcutting...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					velocityAtCell = waterMap[Mx, My];
					heightAtCell = heightMap[Mx, My];
					float heightModifier = 1 - (Mathf.Abs(0.5f - heightAtCell) * 2);
					float cutMaterial = hydraulicDowncutting * velocityAtCell * heightModifier;
					heightAtCell -= cutMaterial;
					heightMap[Mx, My] = heightAtCell;
				}
			}
			// Update progress...
			float percentComplete = (float) iter / (float) iterations;
			erosionProgressDelegate("Applying Hydraulic Erosion", "Applying hydraulic erosion.", iter, iterations, percentComplete);
		}
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] fullHydraulicErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate){
		int Tx = (int) arraySize.x;
		int Ty = (int) arraySize.y;
		float[,] waterMap = new float[Tx, Ty];
		float[,] waterDiffMap = new float[Tx, Ty];
		float[,] sedimentMap = new float[Tx, Ty];
		float[,] sedimentDiffMap = new float[Tx, Ty];
		int Mx;
		int My;
		int xNeighbours;
		int yNeighbours;
		int xShift;
		int yShift;
		int xIndex;
		int yIndex;
		int Nx;
		int Ny;
		float t;
		float heightAtIndex;
		float heightAtPoint;
		int nNeighbours;
		float maxSediment;
		float sedimentAtCell;
		float heightAtCell;
		// Zero maps...
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				waterMap[Mx, My] = 0.0f;
				waterDiffMap[Mx, My] = 0.0f;
				sedimentMap[Mx, My] = 0.0f;
				sedimentDiffMap[Mx, My] = 0.0f;
			}
		}
		// Start iterations...
		for( int iter = 0; iter<iterations; iter++ ){
			// Add water proportional to precipitation...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					float waterAmount = waterMap[Mx, My] + hydraulicRainfall;
					if( waterAmount>1.0f)	waterAmount = 1.0f;
					waterMap[Mx, My] = waterAmount;
				}
			}
			// Dissolve material as sediment...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					float sedimentAmount = sedimentMap[Mx, My];
					maxSediment = waterMap[Mx, My] * hydraulicSedimentSaturation;
					if( sedimentAmount<maxSediment){
						float erodedSediment = waterMap[Mx, My] * hydraulicSedimentSolubility;
						if( sedimentAmount + erodedSediment>maxSediment)	erodedSediment = maxSediment - sedimentAmount;
						heightAtIndex = heightMap[Mx, My];
						if( erodedSediment>heightAtIndex)	erodedSediment = heightAtIndex;
						sedimentMap[Mx, My] = sedimentAmount + erodedSediment;
						heightMap[Mx, My] = heightAtIndex - erodedSediment;
					}
				}
			}
			// Find slope and move water...
			for( My = 0; My<Ty; My++ ){
				// y...
				if( My==0){
					yNeighbours = 2;
					yShift = 0;
					yIndex = 0;
				}
				else if( My==Ty - 1){
					yNeighbours = 2;
					yShift = -1;
					yIndex = 1;
				}
				else {
					yNeighbours = 3;
					yShift = -1;
					yIndex = 1;
				}
				for( Mx = 0; Mx<Tx; Mx++ ){
					// x...
					if( Mx==0){
						xNeighbours = 2;
						xShift = 0;
						xIndex = 0;
					}
					else if( Mx==Tx - 1){
						xNeighbours = 2;
						xShift = -1;
						xIndex = 1;
					}
					else {
						xNeighbours = 3;
						xShift = -1;
						xIndex = 1;
					}
					// Calculate slope...
					float tCumulative = 0.0f;
					float tMax = 0.0f;
					heightAtIndex = heightMap[Mx + xIndex + xShift, My + yIndex + yShift]; // Get height at index
					float waterAtIndex = waterMap[Mx + xIndex + xShift, My + yIndex + yShift]; // Get water at index
					float waterAtPoint;
					float hCumulative = heightAtIndex;
					nNeighbours = 0;
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( Nx!=xIndex || Ny!=yIndex ){
								if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
									heightAtPoint = heightMap[Mx + Nx + xShift, My + Ny + yShift]; // Get height at point
									waterAtPoint = waterMap[Mx + Nx + xShift, My + Ny + yShift]; // Get water at point
									t = (heightAtIndex + waterAtIndex) - (heightAtPoint + waterAtPoint);
									if( t>0 ){
										tCumulative += t;
										hCumulative += heightAtPoint + waterAtPoint;
										nNeighbours++;
										if( t>tMax)	t = tMax;
									}
								}
							}
						}
					}
					// Calculate water to be transported away from the index...
					float hAverage = hCumulative / (nNeighbours + 1);
					float dWater = (heightAtIndex + waterAtIndex) - hAverage;
					float transportedWater = Mathf.Min(waterAtIndex, dWater);
					float waterDiffAtIndexSoFar = waterDiffMap[Mx + xIndex + xShift, My + yIndex + yShift];
					float waterAtIndexDiff = waterDiffAtIndexSoFar - transportedWater;
					// Pass to difference map...
					waterDiffMap[Mx + xIndex + xShift, My + yIndex + yShift] = waterAtIndexDiff;
					// Calculate sediment to be transported away from the index...
					float sedimentAtIndex = sedimentMap[Mx + xIndex + xShift, My + yIndex + yShift];
					float transportedSediment = sedimentAtIndex * (transportedWater / waterAtIndex);
					float sedimentDiffAtIndexSoFar = sedimentDiffMap[Mx + xIndex + xShift, My + yIndex + yShift];
					float sedimentAtIndexDiff = sedimentDiffAtIndexSoFar - transportedSediment;
					// Pass to difference map...
					sedimentDiffMap[Mx + xIndex + xShift, My + yIndex + yShift] = sedimentAtIndexDiff;
					// Neighbours...
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( Nx!=xIndex || Ny!=yIndex){
								if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
									heightAtPoint = heightMap[Mx + Nx + xShift, My + Ny + yShift]; // Get height at point
									waterAtPoint = waterMap[Mx + Nx + xShift, My + Ny + yShift]; // Get water at point
									t = (heightAtIndex + waterAtIndex) - (heightAtPoint + waterAtPoint);
									// Only affect downhill cells...
									if( t>0 ){
										// Move water...
										float transportedWaterSoFar = waterDiffMap[Mx + Nx + xShift, My + Ny + yShift];
										float transportedWaterProportional = transportedWater * (t / tCumulative);
										float transportedWaterAfter = transportedWaterSoFar + transportedWaterProportional;
										// Pass to difference map...
										waterDiffMap[Mx + Nx + xShift, My + Ny + yShift] = transportedWaterAfter;
										// Move sediment...
										float transportedSedimentSoFar = sedimentDiffMap[Mx + Nx + xShift, My + Ny + yShift];
										float transportedSedimentProportional = transportedSediment * (t / tCumulative);
										float transportedSedimentAfter = transportedSedimentSoFar + transportedSedimentProportional;
										// Pass to difference map...
										sedimentDiffMap[Mx + Nx + xShift, My + Ny + yShift] = transportedSedimentAfter;
									}
								}
							}
						}
					}
				}
			}
			// Apply water difference map to water map...
			float waterAtCell;
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					waterAtCell = waterMap[Mx, My] + waterDiffMap[Mx, My];
					// Calculate evaporation...
					float evaporatedWater = waterAtCell * hydraulicEvaporation;
					waterAtCell -= evaporatedWater;
					if( waterAtCell<0.0f)	waterAtCell = 0.0f;
					waterMap[Mx, My] = waterAtCell;
					waterDiffMap[Mx, My] = 0.0f;
				}
			}
			// Apply sediment difference map to sediment map...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					sedimentAtCell = sedimentMap[Mx, My] + sedimentDiffMap[Mx, My];
					if( sedimentAtCell>1.0f ){
						sedimentAtCell = 1.0f;
					} 
					else if( sedimentAtCell<0.0f){
						sedimentAtCell = 0.0f;
					}
					sedimentMap[Mx, My] = sedimentAtCell;
					sedimentDiffMap[Mx, My] = 0.0f;
				}
			}
			// Deposit sediment...
			for( My = 0; My<Ty; My++ ){
				for( Mx = 0; Mx<Tx; Mx++ ){
					maxSediment = waterMap[Mx, My] * hydraulicSedimentSaturation;
					sedimentAtCell = sedimentMap[Mx, My];
					if( sedimentAtCell>maxSediment){
						float depositedSediment = sedimentAtCell - maxSediment;
						sedimentMap[Mx, My] = maxSediment;
						heightAtCell = heightMap[Mx, My];
						heightMap[Mx, My] = heightAtCell + depositedSediment;
					}
				}
			}
			// Update progress...
			float percentComplete = (float) iter / (float) iterations;
			erosionProgressDelegate("Applying Hydraulic Erosion", "Applying hydraulic erosion.", iter, iterations, percentComplete);
		}
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] windErosion(float[,] heightMap, Vector2 arraySize, int iterations, ErosionProgressDelegate erosionProgressDelegate){
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		TerrainData terData = ter.terrainData;
		Quaternion windQuaternion = Quaternion.Euler(0, (windDirection + 180.0f), 0);
		Vector3 windVector = windQuaternion * Vector3.forward;
		int Tw = (int) arraySize.x;
		int Th = (int) arraySize.y;
		float[,] stressMap = new float[Tw, Th];
		float[,] flowMap = new float[Tw, Th];
		float[,] velocityMap = new float[Tw, Th];
		float[,] velocityDiffMap = new float[Tw, Th];
		float[,] suspensionMap = new float[Tw, Th];
		float[,] suspensionDiffMap = new float[Tw, Th];
		float[,] heightDiffMap = new float[Tw, Th];
		int xNeighbours;
		int yNeighbours;
		int xShift;
		int yShift;
		int xIndex;
		int yIndex;
		int Mx;
		int My;
		float materialAtIndex;
		float velocityAtCell;
		// Zero maps...
		for( My = 0; My<Th; My++ ){
			for( Mx = 0; Mx<Tw; Mx++ ){
				stressMap[Mx, My] = 0.0f;
				flowMap[Mx, My] = 0.0f;
				velocityMap[Mx, My] = 0.0f;
				velocityDiffMap[Mx, My] = 0.0f;
				suspensionMap[Mx, My] = 0.0f;
				suspensionDiffMap[Mx, My] = 0.0f;
				heightDiffMap[Mx, My] = 0.0f;
			}
		}
		// Start iterations...
		for( int iter = 0; iter<iterations; iter++ ){
			// Drop material...
			for( My = 0; My<Th; My++ ){
				for( Mx = 0; Mx<Tw; Mx++ ){
					velocityAtCell = velocityMap[Mx, My];
					float heightAtCell = heightMap[Mx, My];
					float materialAtCell = suspensionMap[Mx, My];
					float droppedMaterial = materialAtCell * windGravity; //  * 1.0f - (velocityAtCell)
					suspensionMap[Mx, My] = materialAtCell - droppedMaterial;
					heightMap[Mx, My] = heightAtCell + droppedMaterial;
				}
			}
			// Calculate stress and flow...
			for( My = 0; My<Th; My++ ){
				for( Mx = 0; Mx<Tw; Mx++ ){
					float heightAtIndex = heightMap[Mx, My]; // ALTITUDE
					Vector3 pNormal = terData.GetInterpolatedNormal((float) Mx / Tw, (float) My / Th); // NORMAL
					float stress = (Vector3.Angle(pNormal, windVector) - 90) / 90;
					if( stress<0.0f)	stress = 0.0f;
					stressMap[Mx, My] = stress * heightAtIndex;
					float flow = 1.0f - Mathf.Abs(Vector3.Angle(pNormal, windVector) - 90) / 90;
					flowMap[Mx, My] = flow * heightAtIndex; // ^2
					float inFlow = flow * heightAtIndex * windForce; // ^2
					float velocityAtPoint = velocityMap[Mx, My];
					float newVelocityAtPoint = velocityAtPoint + inFlow; // * UnityEngine.Random.value;
					velocityMap[Mx, My] = newVelocityAtPoint;
					// Lift material...
					materialAtIndex = suspensionMap[Mx, My];
					float liftedMaterial = windLift * newVelocityAtPoint; // * UnityEngine.Random.value;
					if( materialAtIndex + liftedMaterial>windCapacity)	liftedMaterial = windCapacity - materialAtIndex;
					suspensionMap[Mx, My] = materialAtIndex + liftedMaterial;
					heightMap[Mx, My] = heightAtIndex - liftedMaterial;
				}
			}
			// Calculate flow propagation...
			for( My = 0; My<Th; My++ ){
				// y...
				if( My==0){
					yNeighbours = 2;
					yShift = 0;
					yIndex = 0;
				}
				else if( My==Th - 1){
					yNeighbours = 2;
					yShift = -1;
					yIndex = 1;
				}
				else {
					yNeighbours = 3;
					yShift = -1;
					yIndex = 1;
				}
				for( Mx = 0; Mx<Tw; Mx++ ){
					// x...
					if( Mx==0 ){
						xNeighbours = 2;
						xShift = 0;
						xIndex = 0;
					}
					else if( Mx==Tw - 1){
						xNeighbours = 2;
						xShift = -1;
						xIndex = 1;
					}
					else {
						xNeighbours = 3;
						xShift = -1;
						xIndex = 1;
					}
					int Ny;
					int Nx;
					float flowAtIndex = flowMap[Mx, My];
					float stressAtIndex = stressMap[Mx, My];
					materialAtIndex = suspensionMap[Mx, My];
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( Nx!=xIndex || Ny!=yIndex){
								if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
									Vector3 vectorToNeighbour = new Vector3(Nx + xShift, 0, -1 * (Ny + yShift));
									float pWeighting = (90 - Vector3.Angle(vectorToNeighbour, windVector)) / 90;
									if( pWeighting<0.0f)	pWeighting = 0.0f;
									// Propagate velocity...
									float propagatedVelocitySoFar = velocityDiffMap[Mx + Nx + xShift, My + Ny + yShift];
									float propagatedVelocity = pWeighting * (flowAtIndex - stressAtIndex) * 0.1f;
									if( propagatedVelocity<0.0f)	propagatedVelocity = 0.0f;
									float propagatedVelocityAfter = propagatedVelocitySoFar + propagatedVelocity;
									// Pass to difference map...
									velocityDiffMap[Mx + Nx + xShift, My + Ny + yShift] = propagatedVelocityAfter;
									// Lose velocity...
									float lostVelocitySoFar = velocityDiffMap[Mx, My];
									float lostVelocityAfter = lostVelocitySoFar - propagatedVelocity;
									// Pass to difference map...
									velocityDiffMap[Mx, My] = lostVelocityAfter;
									// Propagate material...
									float propagatedMaterialSoFar = suspensionDiffMap[Mx + Nx + xShift, My + Ny + yShift];
									float propagatedMaterial = materialAtIndex * propagatedVelocity;
									float propagatedMaterialAfter = propagatedMaterialSoFar + propagatedMaterial;
									// Pass to difference map...
									suspensionDiffMap[Mx + Nx + xShift, My + Ny + yShift] = propagatedMaterialAfter;
									// Lose material...
									float lostMaterialSoFar = suspensionDiffMap[Mx, My];
									float lostMaterialAfter = lostMaterialSoFar - propagatedMaterial;
									// Pass to difference map...
									suspensionDiffMap[Mx, My] = lostMaterialAfter;
								}
							}
						}
					}
				}
			}
			// Apply suspension difference map to suspension map...
			float suspensionAtCell;
			for( My = 0; My<Th; My++ ){
				for( Mx = 0; Mx<Tw; Mx++ ){
					suspensionAtCell = suspensionMap[Mx, My] + suspensionDiffMap[Mx, My];
					if( suspensionAtCell>1.0f){
						suspensionAtCell = 1.0f;
					}
					else if( suspensionAtCell<0.0f){
						suspensionAtCell = 0.0f;
					}
					suspensionMap[Mx, My] = suspensionAtCell;
					suspensionDiffMap[Mx, My] = 0.0f;
				}
			}
			// Apply velocity difference map to velocity map...
			for( My = 0; My<Th; My++ ){
				for( Mx = 0; Mx<Tw; Mx++ ){
					velocityAtCell = velocityMap[Mx, My] + velocityDiffMap[Mx, My];
					// Calculate entropy...
					velocityAtCell *= 1.0f - windEntropy;
					if( velocityAtCell>1.0f){
						velocityAtCell = 1.0f;
					}
					else if( velocityAtCell<0.0f){
						velocityAtCell = 0.0f;
					}
					velocityMap[Mx, My] = velocityAtCell;
					velocityDiffMap[Mx, My] = 0.0f;
				}
			}
			// Smooth...
			smoothIterations = 1;
			smoothBlend = 0.25f;
			float[,] smoothedHeightMap = (float[,]) heightMap.Clone();
			GeneratorProgressDelegate generatorProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
			smoothedHeightMap = smooth(smoothedHeightMap, arraySize, generatorProgressDelegate);
			// Combine...
			for( My = 0; My<Th; My++ ){
				for( Mx = 0; Mx<Tw; Mx++ ){
					float oldHeightAtPoint = heightMap[Mx, My];
					float newHeightAtPoint = smoothedHeightMap[Mx, My];
					float blendAmount = stressMap[Mx, My] * windSmoothing;
					float blendedHeightAtPoint = (newHeightAtPoint * blendAmount) + (oldHeightAtPoint * (1.0f - blendAmount));
					heightMap[Mx, My] = blendedHeightAtPoint;
				}
			}
			// Update progress...
			float percentComplete = (float) iter / (float) iterations;
			erosionProgressDelegate("Applying Wind Erosion", "Applying wind erosion.", iter, iterations, percentComplete);
		}
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/

	public void textureTerrain(TextureProgressDelegate textureProgressDelegate){
		
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		if( ter==null)	return;
		
		TerrainData terData = ter.terrainData;
		terrainLayers = terData.terrainLayers;
		int nTextures = terrainLayers.Length;
		
		if( nTextures<2 ){
			Debug.LogError("Error: You must assign at least 2 textures.");
			return;
		}
		
		// -----------------------------------------------------------------------
		// CLIFF SLOPES REFERENCE FOR ONE HEIGHTMAP PIXEL
		
		textureProgressDelegate("Procedural Terrain Texture", "Generating height and slope maps. Please wait.", 0.1f);
		
		int Tw = terData.heightmapWidth - 1;
		int Th = terData.heightmapHeight - 1;
		
		float[,] heightMapData = new float[Tw, Th];
		float[,] slopeMapData = new float[Tw, Th];
		float[,,] splatMapData = terData.GetAlphamaps(0, 0, Tw, Tw);
		
		terData.alphamapResolution = Tw;
		Vector3 terSize = terData.size;
		
		// At which angle cliff texture stars appearing ? >>> slopeBlendMinAngle
		// At which angle cliff texture is totally visible ? >>> slopeBlendMaxAngle
		// Convert this angle to HEIGHTMAP VALUE
		// TAN(angle) = oposite side / (adjacent side reduced to one unit !!) then normalize result (HEIGHT value !)
		// float slopeBlendMinimum = ( (terSize.x / Tw) * Mathf.Tan( slopeBlendMinAngle * Mathf.Deg2Rad ) ) / terSize.y;
		// float slopeBlendMaximum = ( (terSize.x / Tw) * Mathf.Tan( slopeBlendMaxAngle * Mathf.Deg2Rad ) ) / terSize.y;
		
		float angleToSlope =  terSize.x / ( Tw * terSize.y );
		float slopeBlendMinimum = angleToSlope * Mathf.Tan( slopeBlendMinAngle * Mathf.Deg2Rad );
		float slopeBlendMaximum = angleToSlope * Mathf.Tan( slopeBlendMaxAngle * Mathf.Deg2Rad );
		float slopeBlendAmplitude = slopeBlendMaximum - slopeBlendMinimum;
		
		try {
			
			// -----------------------------------------------------------------------
			// SLOPES PER HEIGHTMAP PIXEL NEIGHBOUR
			
			textureProgressDelegate("Procedural Terrain Texture", "Generating height and slope maps. Please wait.", 0.25f);
			
			int xNeighbours;
			int yNeighbours;
			int xShift;
			int yShift;
			int xIndex;
			int yIndex;
			float[,] heightMap = terData.GetHeights(0, 0, Tw, Th);
			float h;
			float tCumulative;
			float nNeighbours;
			int Ny;
			int Nx;
			int Ty;
			int Tx;
			
			for( Ty = 0; Ty<Th; Ty++ ){
				
				// y...
				if( Ty==0 ){
					yNeighbours = 2;
					yShift = 0;
					yIndex = 0;
				}
				else if( Ty==Th-1 ){
					yNeighbours = 2;
					yShift = -1;
					yIndex = 1;
				}
				else{
					yNeighbours = 3;
					yShift = -1;
					yIndex = 1;
				}
				
				for( Tx = 0; Tx<Tw; Tx++ ){
					
					// x...
					if( Tx==0 ){
						xNeighbours = 2;
						xShift = 0;
						xIndex = 0;
					}
					else if( Tx==Tw-1 ){
						xNeighbours = 2;
						xShift = -1;
						xIndex = 1;
					}
					else{
						xNeighbours = 3;
						xShift = -1;
						xIndex = 1;
					}
					
					// Get height... And apply to height map...
					h = heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift];
					heightMapData[Tx, Ty] = h;
					
					// Calculate average height using neighbours only (ignoring index)...
					tCumulative = 0.0f;
					nNeighbours = xNeighbours * yNeighbours - 1;
					for(Ny=0;Ny<yNeighbours;Ny++){
						for(Nx=0;Nx<xNeighbours;Nx++){
							if( Nx!=xIndex || Ny!=yIndex ){
								tCumulative += Mathf.Abs(h - heightMap[Tx + Nx + xShift, Ty + Ny + yShift]);
							}
						}
					}
					
					// Compute and assign average HEIGHT value to the slope map...
					slopeMapData[Tx, Ty] = tCumulative / nNeighbours;
					
				}
				
			}
			
			// -----------------------------------------------------------------------

			textureProgressDelegate("Procedural Terrain Texture", "Generating height and slope maps. Please wait.", 0.6f);
			
			float sBlended = 0;
			int Px;
			int Py;
			
			float hBlendInMinimum;
			float hBlendInMaximum;
			float hBlendOutMinimum;
			float hBlendOutMaximum;
			float hValue;
			float hBlended;
			int i;
						
			for( Py = 0; Py<Th; Py++ ){
				for( Px = 0; Px<Tw; Px++ ){
					
					// -----------------------------------------------------------------------
					// Blend slope... FOR CLIFF
					
					sBlended = slopeMapData[Px, Py];
					
					if( sBlended<slopeBlendMinimum ){
						sBlended = 0;
					}
					else if( sBlended<=slopeBlendMaximum){
						sBlended = Mathf.Clamp01( (sBlended - slopeBlendMinimum) / slopeBlendAmplitude );
					}
					else if( sBlended>slopeBlendMaximum){
						sBlended = 1;
					}
					
					slopeMapData[Px, Py] = sBlended;
					splatMapData[Px, Py, 0] = sBlended;
					tCumulative = sBlended;

					// -----------------------------------------------------------------------
					// Blend slope... FOR TEXTURES
					
					for( i = 1; i<nTextures; i++ ){
				
						hBlendInMinimum = 0;
						hBlendInMaximum = 0;
						hBlendOutMinimum = 1;
						hBlendOutMaximum = 1;
							
						if( i>1 ){													
							hBlendInMinimum = (float) heightBlendPoints[i * 2 - 4];
							hBlendInMaximum = (float) heightBlendPoints[i * 2 - 3];
						}

						if( i<nTextures - 1){
							hBlendOutMinimum = (float) heightBlendPoints[i * 2 - 2];
							hBlendOutMaximum = (float) heightBlendPoints[i * 2 - 1];
						}
								
						hValue = heightMapData[Px, Py];
						hBlended = 0;
						
						if( hValue>=hBlendInMaximum && hValue<=hBlendOutMinimum){
							// Full...
							hBlended = 1;
						}
						else if( hValue>=hBlendInMinimum && hValue<hBlendInMaximum){
							// Blend in...
							hBlended = Mathf.Clamp01( (hValue - hBlendInMinimum) / (hBlendInMaximum - hBlendInMinimum) );
						}
						else if( hValue>hBlendOutMinimum && hValue<=hBlendOutMaximum){
							// Blend out...
							hBlended = Mathf.Clamp01( 1 - ( (hValue - hBlendOutMinimum) / (hBlendOutMaximum - hBlendOutMinimum) ) );
						}
						
						// Assign value
						splatMapData[Px, Py, i] = Mathf.Clamp01(hBlended - slopeMapData[Px, Py] );
						
						// Cumulate
						tCumulative += splatMapData[Px, Py, i];
								
					}
					
					// Final test for global splatMapData impacting CLIFF BLENDING !
					if( tCumulative<1.0f )	splatMapData[Px, Py, 0] = Mathf.Clamp01( splatMapData[Px, Py, 0] + 1.0f - tCumulative );
										
				}

			}
						
			// -----------------------------------------------------------------------
			// Generate splat maps...
			
			textureProgressDelegate("Procedural Terrain Texture", "Generating splat map. Please wait.", 0.9f);
			
			// Assign generated splatmap data to terrain data
			terData.SetAlphamaps(0, 0, splatMapData);
			
			// Clean up...
			heightMapData = null;
			slopeMapData = null;
			splatMapData = null;
			
		}
		catch (System.Exception e){
			
			// Clean up...
			heightMapData = null;
			slopeMapData = null;
			splatMapData = null;
			Debug.LogError("An error occurred : "+e);
			
		}
		
	}	
	
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void BuildPaths(){
		
		layersPath = Application.dataPath + Path.DirectorySeparatorChar + TOOLKIT_LAYERS_FOLDER + Path.DirectorySeparatorChar + gameObject.name + TERRAIN_LAYERS_FOLDER ;
		assetPath = "Assets" + Path.DirectorySeparatorChar + TOOLKIT_LAYERS_FOLDER + Path.DirectorySeparatorChar + gameObject.name + TERRAIN_LAYERS_FOLDER + Path.DirectorySeparatorChar ;
		System.IO.Directory.CreateDirectory(layersPath);
		
	}
	
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void addTerrainLayer(Texture2D tex, int index){
		
		// try{
			
			if( terrainLayers==null ){ terrainLayers = new TerrainLayer[0];	}
		
		// Build paths
		BuildPaths();
		
		// New layers repository
		TerrainLayer[] newTerrainLayers = new TerrainLayer[index + 1];
		
		// For each layer from 0 to index
		for( int i = 0; i<=index; i++ ){
			
			// Reached new layer ?
			if( i==index ){
				
				// Create a nex layer
				newTerrainLayers[i] = new TerrainLayer();
				
				// Generate asset
				AssetDatabase.CreateAsset(newTerrainLayers[i], assetPath + TERRAIN_LAYER_PREFIX + i + TERRAIN_LAYER_EXTENSION );
			
				// Assign values
				newTerrainLayers[i].diffuseTexture = tex;
				newTerrainLayers[i].tileSize = new Vector2(15, 15);
				
			} 
			// Previous layer
			else {
		
				// Simply point to asset
				newTerrainLayers[i] = terrainLayers[i];
		
			}
			
		}
		
		// Refresh repository
		terrainLayers = newTerrainLayers;
		
		// Blend points
		if( index+1>2 )	addBlendPoints();
		
		// }
		// catch(System.Exception e){
			// Debug.LogError("addTerrainLayer error : "+e);
		// }
			
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void deleteTerrainLayer(Texture2D tex, int index){
		
		// try{
			
		if( terrainLayers==null ){ terrainLayers = new TerrainLayer[0];	}
		
		// Build paths
		BuildPaths();
		
		// Get current layer repository length
		int c = terrainLayers.Length;
		
		// If empty... return
		if( c==0 )  return;
		
		// New repository to populate
		TerrainLayer[] newTerrainLayers = new TerrainLayer[c - 1];
		
		// Index to use
		int n = 0;
		
		// Delete the asset corresponding to removed index
		AssetDatabase.DeleteAsset( assetPath + TERRAIN_LAYER_PREFIX + index + TERRAIN_LAYER_EXTENSION );
		terrainLayers[index] = null; 
		
		// For each layer in the old repository
		for(int i=0;i<c;i++){
			
			// Not removed index ?
			if( i!=index ){
				
				// Simply point to asset
				newTerrainLayers[n] = terrainLayers[i];
				
				// Difference entree old and new index ?
				if( i!=n ){
					
					// Rename asset to new index
					AssetDatabase.RenameAsset(
						assetPath + TERRAIN_LAYER_PREFIX + i + TERRAIN_LAYER_EXTENSION,
						TERRAIN_LAYER_PREFIX + n + TERRAIN_LAYER_EXTENSION
					);
					
				}
				
				// Next index
				n++;			
				
			}
			
		}
		
		// Refresh repository
		terrainLayers = newTerrainLayers;
		
		// Blend points
		if( c-1>1)	deleteBlendPoints();
		
		// }
		// catch(System.Exception e){
			// Debug.LogError("addTerrainLayer error : "+e);
		// }
		
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void deleteAllTerrainLayers(){
		
		// try{
			
		if( terrainLayers==null ){ terrainLayers = new TerrainLayer[0];	}
		
		// For each layer in the repository
		for(int i=0;i<terrainLayers.Length;i++){
		
			// Delete the asset corresponding to removed index
			AssetDatabase.DeleteAsset( assetPath + TERRAIN_LAYER_PREFIX + i + TERRAIN_LAYER_EXTENSION );
			terrainLayers[i] = null; 
			
		}
		
		// Reset repository
		terrainLayers = new TerrainLayer[0];
		
		// }
		// catch(System.Exception e){
			// Debug.LogError("addTerrainLayer error : "+e);
		// }
		
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void addBlendPoints(){
		float lastBlendPoint = 0.0f;
		if( heightBlendPoints.Count>0){
			lastBlendPoint = (float) heightBlendPoints[heightBlendPoints.Count - 1];
		}
		float newBlendPoint = lastBlendPoint + (1.0f - lastBlendPoint) * 0.33f;
		heightBlendPoints.Add(newBlendPoint);
		newBlendPoint = lastBlendPoint + (1.0f - lastBlendPoint) * 0.66f;
		heightBlendPoints.Add(newBlendPoint);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void deleteBlendPoints(){
		if( heightBlendPoints.Count>0)	heightBlendPoints.RemoveAt( heightBlendPoints.Count-1 );
		if( heightBlendPoints.Count>0)	heightBlendPoints.RemoveAt( heightBlendPoints.Count-1 );
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void deleteAllBlendPoints(){
		heightBlendPoints.Clear();
		heightBlendPoints = new List<float>();
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void generateTerrain(GeneratorProgressDelegate generatorProgressDelegate){
		// Check enum vars...
		convertIntVarsToEnums();
		// Error checking...
		//Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		Terrain ter =  gameObject.GetComponent<Terrain>();
		if( ter==null )	return;
		TerrainData terData = ter.terrainData;
		int Tw = terData.heightmapWidth;
		int Th = terData.heightmapHeight;
		float[,] heightMap = terData.GetHeights(0, 0, Tw, Th);
		float[,] generatedHeightMap = (float[,]) heightMap.Clone();
		// Set the number of iterations and pass the height array to the appropriate generator script...
		switch( p_generatorType){
			case GeneratorType.Voronoi:
				generatedHeightMap = generateVoronoi(generatedHeightMap, new Vector2(Tw, Th), generatorProgressDelegate);
				break;
			case GeneratorType.DiamondSquare:
				generatedHeightMap = generateDiamondSquare(generatedHeightMap, new Vector2(Tw, Th), generatorProgressDelegate);
				break;
			case GeneratorType.Perlin:
				generatedHeightMap = generatePerlin(generatedHeightMap, new Vector2(Tw, Th), generatorProgressDelegate);
				break;
			case GeneratorType.Smooth:
				generatedHeightMap = smooth(generatedHeightMap, new Vector2(Tw, Th), generatorProgressDelegate);
				break;
			case GeneratorType.Normalise:
				generatedHeightMap = normalise(generatedHeightMap, new Vector2(Tw, Th), generatorProgressDelegate);
				break;
			default:
				return;
		}
		// Apply it to the terrain object...
		for( int Ty = 0; Ty<Th; Ty++ ){
			for( int Tx = 0; Tx<Tw; Tx++ ){
				float oldHeightAtPoint = heightMap[Tx, Ty];
				float newHeightAtPoint = generatedHeightMap[Tx, Ty];
				float blendedHeightAtPoint = 0.0f;
				switch( p_generatorType){
					case GeneratorType.Voronoi:
						blendedHeightAtPoint = (newHeightAtPoint * voronoiBlend) + (oldHeightAtPoint * (1.0f - voronoiBlend));
						break;
					case GeneratorType.DiamondSquare:
						blendedHeightAtPoint = (newHeightAtPoint * diamondSquareBlend) + (oldHeightAtPoint * (1.0f - diamondSquareBlend));
						break;
					case GeneratorType.Perlin:
						blendedHeightAtPoint = (newHeightAtPoint * perlinBlend) + (oldHeightAtPoint * (1.0f - perlinBlend));
						break;
					case GeneratorType.Smooth:
						blendedHeightAtPoint = (newHeightAtPoint * smoothBlend) + (oldHeightAtPoint * (1.0f - smoothBlend));
						break;
					case GeneratorType.Normalise:
						blendedHeightAtPoint = (newHeightAtPoint * normaliseBlend) + (oldHeightAtPoint * (1.0f - normaliseBlend));
						break;
				}
				heightMap[Tx, Ty] = blendedHeightAtPoint;
			}
		}
		 terData.SetHeights(0, 0, heightMap);
		// terData.SetHeightsDelayLOD(0, 0, heightMap);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] generateVoronoi(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate){
		int Tx = (int) arraySize.x;
		int Ty = (int) arraySize.y;
		// Create Voronoi set...
		ArrayList voronoiSet = new ArrayList();
		int i;
		for( i = 0; i<voronoiCells; i++ ){
			Peak newPeak = new Peak();
			int xCoord = (int) Mathf.Floor(UnityEngine.Random.value * Tx);
			int yCoord = (int) Mathf.Floor(UnityEngine.Random.value * Ty);
			float pointHeight = UnityEngine.Random.value;
			if( UnityEngine.Random.value>voronoiFeatures)	pointHeight = 0.0f;
			newPeak.peakPoint = new Vector2(xCoord, yCoord);
			newPeak.peakHeight = pointHeight;
			voronoiSet.Add(newPeak);
		}
		int Mx;
		int My;
		float highestScore = 0.0f;
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				ArrayList peakDistances = new ArrayList();
				for( i = 0; i<voronoiCells; i++ ){
					Peak peakI = (Peak) voronoiSet[i];
					Vector2 peakPoint = peakI.peakPoint;
					float distanceToPeak = Vector2.Distance(peakPoint, new Vector2(Mx, My));
					PeakDistance newPeakDistance = new PeakDistance();
					newPeakDistance.id = i;
					newPeakDistance.dist = distanceToPeak;
					peakDistances.Add(newPeakDistance);
				}
				peakDistances.Sort();
				PeakDistance peakDistOne = (PeakDistance) peakDistances[0];
				PeakDistance peakDistTwo = (PeakDistance) peakDistances[1];
				int p1 = peakDistOne.id;
				float d1 = peakDistOne.dist;
				float d2 = peakDistTwo.dist;
				float scale = Mathf.Abs(d1 - d2) / ((Tx + Ty) / Mathf.Sqrt(voronoiCells));
				Peak peakOne = (Peak) voronoiSet[p1];
				float h1 = (float) peakOne.peakHeight;
				float hScore = h1 - Mathf.Abs(d1 / d2) * h1;
				float asRadians;
				switch( p_voronoiType){
					case VoronoiType.Linear:
						// Nothing...
						break;
					case VoronoiType.Sine:
						asRadians = hScore * Mathf.PI - Mathf.PI / 2;
						hScore = 0.5f + Mathf.Sin(asRadians) / 2;
						break;
					case VoronoiType.Tangent:
						asRadians = hScore * Mathf.PI / 2;
						hScore = 0.5f + Mathf.Tan(asRadians) / 2;
						break;
				}
				hScore = (hScore * scale * voronoiScale) + (hScore * (1.0f - voronoiScale));
				if( hScore<0.0f){
					hScore = 0.0f;
				}
				else if( hScore>1.0f){
					hScore = 1.0f;
				}
				heightMap[Mx, My] = hScore;
				if( hScore>highestScore)	highestScore = hScore;
			}
			// Show progress...
			float completePoints = My * Ty;
			float totalPoints = Tx * Ty;
			float percentComplete = completePoints / totalPoints;
			generatorProgressDelegate("Voronoi Generator", "Generating height map. Please wait.", percentComplete);
		}
		// Normalise...
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				float normalisedHeight = heightMap[Mx, My] * (1.0f / highestScore);
				heightMap[Mx, My] = normalisedHeight;
			}
		}
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] generateDiamondSquare(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate){
		int Tw = (int) arraySize.x;
		int Th = (int) arraySize.y;
		float heightRange = 1.0f;
		int step = Tw - 1;
		heightMap[0, 0] = 0.5f;
		heightMap[Tw - 1, 0] = 0.5f;
		heightMap[0, Th - 1] = 0.5f;
		heightMap[Tw - 1, Th - 1] = 0.5f;
		generatorProgressDelegate("Fractal Generator", "Generating height map. Please wait.", 0.0f);
		while( step>1 ){
			// diamond
			for( int Tx = 0; Tx<Tw - 1; Tx += step){
				for( int Ty = 0; Ty<Th - 1; Ty += step){
					int sx = Tx + (step >> 1);
					int sy = Ty + (step >> 1);
					Vector2[] points = new Vector2[4];
					points[0] = new Vector2(Tx, Ty);
					points[1] = new Vector2(Tx + step, Ty);
					points[2] = new Vector2(Tx, Ty + step);
					points[3] = new Vector2(Tx + step, Ty + step);
					dsCalculateHeight(heightMap, arraySize, sx, sy, points, heightRange);
				}
			}
			// square
			for( int Tx = 0; Tx<Tw - 1; Tx += step){
				for( int Ty = 0; Ty<Th - 1; Ty += step){
					int halfstep = step >> 1;
					int x1 = Tx + halfstep;
					int y1 = Ty;
					int x2 = Tx;
					int y2 = Ty + halfstep;
					Vector2[] points1 = new Vector2[4];
					points1[0] = new Vector2(x1 - halfstep, y1);
					points1[1] = new Vector2(x1, y1 - halfstep);
					points1[2] = new Vector2(x1 + halfstep, y1);
					points1[3] = new Vector2(x1, y1 + halfstep);
					Vector2[] points2 = new Vector2[4];
					points2[0] = new Vector2(x2 - halfstep, y2);
					points2[1] = new Vector2(x2, y2 - halfstep);
					points2[2] = new Vector2(x2 + halfstep, y2);
					points2[3] = new Vector2(x2, y2 + halfstep);
					dsCalculateHeight(heightMap, arraySize, x1, y1, points1, heightRange);
					dsCalculateHeight(heightMap, arraySize, x2, y2, points2, heightRange);
				}
			}
			heightRange *= diamondSquareDelta;
			step >>= 1;
		}
		generatorProgressDelegate("Fractal Generator", "Generating height map. Please wait.", 1.0f);
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void dsCalculateHeight(float[,] heightMap, Vector2 arraySize, int Tx, int Ty, Vector2[] points, float heightRange){
		int Tw = (int) arraySize.x;
		int Th = (int) arraySize.y;
		float h = 0.0f;
		for( int i = 0; i<4; i++){
			if( points[i].x<0){
				points[i].x += (Tw - 1);
			}
			else if( points[i].x>Tw){
				points[i].x -= (Tw - 1);
			}
			else if( points[i].y<0){
				points[i].y += Th - 1;
			}
			else if( points[i].y>Th){
				points[i].y -= Th - 1;
			}
			h += (float) (heightMap[(int) points[i].x, (int) points[i].y] / 4);
		}
		h += (UnityEngine.Random.value * heightRange - heightRange / 2);
		if( h<0.0f){
			h = 0.0f;
		}
		else if( h>1.0f){
			h = 1.0f;
		}
		heightMap[Tx, Ty] = h;
		if( Tx==0){
			heightMap[Tw - 1, Ty] = h;
		}
		else if( Tx==Tw - 1){
			heightMap[0, Ty] = h;
		}
		else if( Ty==0){
			heightMap[Tx, Th - 1] = h;
		}
		else if( Ty==Th - 1){
			heightMap[Tx, 0] = h;
		}
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] generatePerlin(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate){
		int Tw = (int) arraySize.x;
		int Th = (int) arraySize.y;
		// Zero...
		for( int My = 0; My<Th; My++ ){
			for( int Mx = 0; Mx<Tw; Mx++ ){
				heightMap[Mx, My] = 0.0f;
			}
		}
		PerlinNoise2D[] noiseFunctions = new PerlinNoise2D[perlinOctaves];
		int freq = perlinFrequency;
		float amp = 1.0f;
		int i;
		for( i = 0; i<perlinOctaves; i++ ){
			noiseFunctions[i] = new PerlinNoise2D(freq, amp);
			freq *= 2;
			amp /= 2;
		}
		for( i = 0; i<perlinOctaves; i++ ){
			double xStep = (float) Tw / (float) noiseFunctions[i].Frequency;
			double yStep = (float) Th / (float) noiseFunctions[i].Frequency;
			for( int Px = 0; Px<Tw; Px++ ){
				for( int Py = 0; Py<Th; Py++ ){
					int Pa = (int) (Px / xStep);
					int Pb = Pa + 1;
					int Pc = (int) (Py / yStep);
					int Pd = Pc + 1;
					double interpValue = noiseFunctions[i].getInterpolatedPoint(Pa, Pb, Pc, Pd, (Px / xStep) - Pa, (Py / yStep) - Pc);
					heightMap[Px, Py] += (float) (interpValue * noiseFunctions[i].Amplitude);
				}
			}
			// Show progress...
			float percentComplete = (i + 1) / perlinOctaves;
			generatorProgressDelegate("Perlin Generator", "Generating height map. Please wait.", percentComplete);
		}
		GeneratorProgressDelegate normaliseProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
		float oldNormaliseMin = normaliseMin;
		float oldNormaliseMax = normaliseMax;
		float oldNormaliseBlend = normaliseBlend;
		normaliseMin = 0.0f;
		normaliseMax = 1.0f;
		normaliseBlend = 1.0f;
		heightMap = normalise(heightMap, arraySize, normaliseProgressDelegate);
		normaliseMin = oldNormaliseMin;
		normaliseMax = oldNormaliseMax;
		normaliseBlend = oldNormaliseBlend;
		for( int Px = 0; Px<Tw; Px++ ){
			for( int Py = 0; Py<Th; Py++ ){
				heightMap[Px, Py] = heightMap[Px, Py] * perlinAmplitude;
			}
		}
		for( i = 0; i<perlinOctaves; i++ ){
			noiseFunctions[i] = null;
		}
		noiseFunctions = null;
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] smooth(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate){
		int Tw = (int) arraySize.x;
		int Th = (int) arraySize.y;
		int xNeighbours;
		int yNeighbours;
		int xShift;
		int yShift;
		int xIndex;
		int yIndex;
		int Tx;
		int Ty;
		// Start iterations...
		for( int iter = 0; iter<smoothIterations; iter++ ){
			for( Ty = 0; Ty<Th; Ty++ ){
				// y...
				if( Ty==0){
					yNeighbours = 2;
					yShift = 0;
					yIndex = 0;
				}
				else if( Ty==Th - 1){
					yNeighbours = 2;
					yShift = -1;
					yIndex = 1;
				}
				else {
					yNeighbours = 3;
					yShift = -1;
					yIndex = 1;
				}
				for( Tx = 0; Tx<Tw; Tx++ ){
					// x...
					if( Tx==0){
						xNeighbours = 2;
						xShift = 0;
						xIndex = 0;
					}
					else if( Tx==Tw - 1){
						xNeighbours = 2;
						xShift = -1;
						xIndex = 1;
					}
					else {
						xNeighbours = 3;
						xShift = -1;
						xIndex = 1;
					}
					int Ny;
					int Nx;
					float hCumulative = 0.0f;
					int nNeighbours = 0;
					for( Ny = 0; Ny<yNeighbours; Ny++ ){
						for( Nx = 0; Nx<xNeighbours; Nx++ ){
							if( p_neighbourhood==Neighbourhood.Moore || (p_neighbourhood==Neighbourhood.VonNeumann && (Nx==xIndex || Ny==yIndex))) {
								float heightAtPoint = heightMap[Tx + Nx + xShift, Ty + Ny + yShift]; // Get height at point
								hCumulative += heightAtPoint;
								nNeighbours++;
							}
						}
					}
					float hAverage = hCumulative / nNeighbours;
					heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = hAverage;
				}
			}
			// Show progress...
			float percentComplete = (iter + 1) / smoothIterations;
			generatorProgressDelegate("Smoothing Filter", "Smoothing height map. Please wait.", percentComplete);
		}
		return heightMap;
	}
		
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public float[,] normalise(float[,] heightMap, Vector2 arraySize, GeneratorProgressDelegate generatorProgressDelegate){
		int Tx = (int) arraySize.x;
		int Ty = (int) arraySize.y;
		int Mx;
		int My;
		float highestPoint = 0.0f;
		float lowestPoint = 1.0f;
		generatorProgressDelegate("Normalise Filter", "Normalising height map. Please wait.", 0.0f);
		// Find highest and lowest points...
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				float heightAtPoint = heightMap[Mx, My];
				if( heightAtPoint<lowestPoint){
					lowestPoint = heightAtPoint;
				}
				else if( heightAtPoint>highestPoint){
					highestPoint = heightAtPoint;
				}
			}
		}
		generatorProgressDelegate("Normalise Filter", "Normalising height map. Please wait.", 0.5f);
		// Normalise...
		float heightRange = highestPoint - lowestPoint;
		float normalisedHeightRange = normaliseMax - normaliseMin;
		for( My = 0; My<Ty; My++ ){
			for( Mx = 0; Mx<Tx; Mx++ ){
				float normalisedHeight = ((heightMap[Mx, My] - lowestPoint) / heightRange) * normalisedHeightRange;
				heightMap[Mx, My] = normaliseMin + normalisedHeight;
			}
		}
		generatorProgressDelegate("Normalise Filter", "Normalising height map. Please wait.", 1.0f);
		return heightMap;
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void FastThermalErosion(int iterations, float minSlope, float blendAmount){
		erosionTypeInt = 0;
		p_erosionType = ErosionType.Thermal;
		thermalIterations = iterations;
		thermalMinSlope = minSlope;
		thermalFalloff = blendAmount;
		p_neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
		erodeAllTerrain(erosionProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void FastHydraulicErosion(int iterations, float maxSlope, float blendAmount){
		erosionTypeInt = 1;
		p_erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 0;
		p_hydraulicType = HydraulicType.Fast;
		hydraulicIterations = iterations;
		hydraulicMaxSlope = maxSlope;
		hydraulicFalloff = blendAmount;
		p_neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
		erodeAllTerrain(erosionProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void FullHydraulicErosion(int iterations, float rainfall, float evaporation, float solubility, float saturation){
		erosionTypeInt = 1;
		p_erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 1;
		p_hydraulicType = HydraulicType.Full;
		hydraulicIterations = iterations;
		hydraulicRainfall = rainfall;
		hydraulicEvaporation = evaporation;
		hydraulicSedimentSolubility = solubility;
		hydraulicSedimentSaturation = saturation;
		p_neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
		erodeAllTerrain(erosionProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void VelocityHydraulicErosion(int iterations, float rainfall, float evaporation, float solubility, float saturation, float velocity, float momentum, float entropy, float downcutting){
		erosionTypeInt = 1;
		p_erosionType = ErosionType.Hydraulic;
		hydraulicTypeInt = 2;
		p_hydraulicType = HydraulicType.Velocity;
		hydraulicIterations = iterations;
		hydraulicVelocityRainfall = rainfall;
		hydraulicVelocityEvaporation = evaporation;
		hydraulicVelocitySedimentSolubility = solubility;
		hydraulicVelocitySedimentSaturation = saturation;
		hydraulicVelocity = velocity;
		hydraulicMomentum = momentum;
		hydraulicEntropy = entropy;
		hydraulicDowncutting = downcutting;
		p_neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
		erodeAllTerrain(erosionProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void TidalErosion(int iterations, float seaLevel, float tidalRange, float cliffLimit){
		erosionTypeInt = 2;
		p_erosionType = ErosionType.Tidal;
		tidalIterations = iterations;
		tidalSeaLevel = seaLevel;
		tidalRangeAmount = tidalRange;
		tidalCliffLimit = cliffLimit;
		p_neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
		erodeAllTerrain(erosionProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void WindErosion(int iterations, float direction, float force, float lift, float gravity, float capacity, float entropy, float smoothing){
		erosionTypeInt = 3;
		p_erosionType = ErosionType.Wind;
		windIterations = iterations;
		windDirection = direction;
		windForce = force;
		windLift = lift;
		windGravity = gravity;
		windCapacity = capacity;
		windEntropy = entropy;
		windSmoothing = smoothing;
		p_neighbourhood = Neighbourhood.Moore;
		ErosionProgressDelegate erosionProgressDelegate = new ErosionProgressDelegate(dummyErosionProgress);
		erodeAllTerrain(erosionProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void TextureTerrain(float[] slopeStops, float[] heightStops, Texture2D[] textures){
		if( slopeStops.Length!=2 ){
			Debug.LogError("Error: slopeStops must have 2 values");
			return;
		}
		if( heightStops.Length>8 ){
			Debug.LogError("Error: heightStops must have no more than 8 values");
			return;
		}
		if( heightStops.Length % 2!=0 ){
			Debug.LogError("Error: heightStops must have an even number of values");
			return;
		}
		int numTextures = textures.Length;
		int numTexturesByStops = (heightStops.Length / 2) + 2;
		if( numTextures!=numTexturesByStops){
			Debug.LogError("Error: heightStops contains an incorrect number of values");
			return;
		}
		foreach (float stop in slopeStops){
			if( stop<0 || stop>90 ){
				Debug.LogError("Error: The value of all slopeStops must be in the range 0.0 to 90.0");
				return;
			}
		}
		foreach (float stop in heightStops){
			if( stop<0 || stop>1 ){
				Debug.LogError("Error: The value of all heightStops must be in the range 0.0 to 1.0");
				return;
			}
		}
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		TerrainData terData = ter.terrainData;
		terrainLayers = terData.terrainLayers;
		deleteAllTerrainLayers();
		int n = 0;
		foreach (Texture2D tex in textures){
			addTerrainLayer(tex, n);
			n++;
		}
		slopeBlendMinAngle = slopeStops[0];
		slopeBlendMaxAngle = slopeStops[1];
		n = 0;
		foreach (float stop in heightStops){
			heightBlendPoints[n] = stop;
			n++;
		}
		terData.terrainLayers = terrainLayers;
		TextureProgressDelegate textureProgressDelegate = new TextureProgressDelegate(dummyTextureProgress);
		textureTerrain(textureProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void VoronoiGenerator(FeatureType featureType, int cells, float features, float scale, float blend){
		generatorTypeInt = 0;
		p_generatorType = GeneratorType.Voronoi;
		switch( featureType ){
			case FeatureType.Mountains:
				voronoiTypeInt = 0;
				p_voronoiType = VoronoiType.Linear;
				break;
			case FeatureType.Hills:
				voronoiTypeInt = 1;
				p_voronoiType = VoronoiType.Sine;
				break;
			case FeatureType.Plateaus:
				voronoiTypeInt = 2;
				p_voronoiType = VoronoiType.Tangent;
				break;
		}
		voronoiCells = cells;
		voronoiFeatures = features;
		voronoiScale = scale;
		voronoiBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
		generateTerrain(generatorProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void FractalGenerator(float fractalDelta, float blend){
		generatorTypeInt = 1;
		p_generatorType = GeneratorType.DiamondSquare;
		diamondSquareDelta = fractalDelta;
		diamondSquareBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
		generateTerrain(generatorProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void PerlinGenerator(int frequency, float amplitude, int octaves, float blend){
		generatorTypeInt = 2;
		p_generatorType = GeneratorType.Perlin;
		perlinFrequency = frequency;
		perlinAmplitude = amplitude;
		perlinOctaves = octaves;
		perlinBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
		generateTerrain(generatorProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void SmoothTerrain(int iterations, float blend){
		generatorTypeInt = 3;
		p_generatorType = GeneratorType.Smooth;
		smoothIterations = iterations;
		smoothBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
		generateTerrain(generatorProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void NormaliseTerrain(float minHeight, float maxHeight, float blend){
		generatorTypeInt = 4;
		p_generatorType = GeneratorType.Normalise;
		normaliseMin = minHeight;
		normaliseMax = maxHeight;
		normaliseBlend = blend;
		GeneratorProgressDelegate generatorProgressDelegate = new GeneratorProgressDelegate(dummyGeneratorProgress);
		generateTerrain(generatorProgressDelegate);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void NormalizeTerrain(float minHeight, float maxHeight, float blend){
		NormaliseTerrain(minHeight, maxHeight, blend);
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void convertIntVarsToEnums(){
		switch( erosionTypeInt ){
			case 0:
				p_erosionType = ErosionType.Thermal;
				break;
			case 1:
				p_erosionType = ErosionType.Hydraulic;
				break;
			case 2:
				p_erosionType = ErosionType.Tidal;
				break;
			case 3:
				p_erosionType = ErosionType.Wind;
				break;
			case 4:
				p_erosionType = ErosionType.Glacial;
				break;
		}
		switch( hydraulicTypeInt ){
			case 0:
				p_hydraulicType = HydraulicType.Fast;
				break;
			case 1:
				p_hydraulicType = HydraulicType.Full;
				break;
			case 2:
				p_hydraulicType = HydraulicType.Velocity;
				break;
		}
		switch( generatorTypeInt ){
			case 0:
				p_generatorType = GeneratorType.Voronoi;
				break;
			case 1:
				p_generatorType = GeneratorType.DiamondSquare;
				break;
			case 2:
				p_generatorType = GeneratorType.Perlin;
				break;
			case 3:
				p_generatorType = GeneratorType.Smooth;
				break;
			case 4:
				p_generatorType = GeneratorType.Normalise;
				break;
		}
		switch( voronoiTypeInt ){
			case 0:
				p_voronoiType = VoronoiType.Linear;
				break;
			case 1:
				p_voronoiType = VoronoiType.Sine;
				break;
			case 2:
				p_voronoiType = VoronoiType.Tangent;
				break;
		}
		switch( neighbourhoodInt ){
			case 0:
				p_neighbourhood = Neighbourhood.Moore;
				break;
			case 1:
				p_neighbourhood = Neighbourhood.VonNeumann;
				break;
		}
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void dummyErosionProgress(string titleString, string displayString, int iteration, int nIterations, float percentComplete){
		//
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void dummyTextureProgress(string titleString, string displayString, float percentComplete){
		//
	}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	public void dummyGeneratorProgress(string titleString, string displayString, float percentComplete){
		//
	}

/*---------------------------------------------------------------------------*/
/* Reset terrain */
/*---------------------------------------------------------------------------*/
	
	public void resetTerrain(){
		
		// Check...
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		if( ter==null )	return;
		
		try {
			
			TerrainData terData = ter.terrainData;
			
			// Delete texture informations
			deleteAllTerrainLayers();
			deleteAllBlendPoints();
			
			// Reset splats !
			terData.terrainLayers = terrainLayers;
			
			int Tw = terData.heightmapWidth;
			int Th = terData.heightmapHeight;
			
			float[,] heightMap = terData.GetHeights(0, 0, Tw, Th);
			
			int Mw;
			int Mh;
		
			// Reset to ZERO the heights ([0.0f,1.0f] value)
			for( Mw = 0; Mw<Tw; Mw++ ){
				for( Mh = 0; Mh<Th; Mh++ ){
					heightMap[Mw, Mh] = 0.0f;
				}
			}
			
			// Apply back to terrain data
			terData.SetHeights(0, 0, heightMap);
						
		}
		catch(System.Exception e){
			Debug.LogError("An error occurred : "+e);
		}
			
	}
	
/*---------------------------------------------------------------------------*/
/* Invert height of the terrain */
/*---------------------------------------------------------------------------*/
	
	public void invertTerrain(){
		
		// Check...
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		if( ter==null )	return;
		
		try {
			
			TerrainData terData = ter.terrainData;
			
			int Tw = terData.heightmapWidth;
			int Th = terData.heightmapHeight;
			float[,] heightMap = terData.GetHeights(0, 0, Tw, Th);
			
			int Mw;
			int Mh;
		
			// Invert height ([0.0f,1.0f] value)
			for( Mw = 0; Mw<Tw; Mw++ ){
				for( Mh = 0; Mh<Th; Mh++ ){
					heightMap[Mw, Mh] = 1.0f - heightMap[Mw, Mh];
				}
			}			
			
			// Apply back to terrain data
			terData.SetHeights(0, 0, heightMap);
			
		}
		catch(System.Exception e){
			Debug.LogError("An error occurred : "+e);
		}
			
	}
	
/*---------------------------------------------------------------------------*/
/* Modify terrain in order to be tileable */
/*---------------------------------------------------------------------------*/
	
	public void seamlessTerrain(float amount){
		
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));		
		if( ter==null )	return;
		
		try {
			
			TerrainData terData = ter.terrainData;
			
			int Tw = terData.heightmapWidth;
			int Th = terData.heightmapHeight;
			
			int hTwLimit = (int) (Tw * amount);
			int hThLimit = (int) (Th * amount);			
			
			float[,] heightMap = terData.GetHeights(0, 0, Tw, Th);
			
			int Mw;
			int Mh;
			
			float avrgH, avrgLerp;
			
			for( Mw = 0; Mw<Tw; Mw++ ){
				for( Mh = 0; Mh<Th; Mh++ ){
					
					if( Mw<hTwLimit || Mh<hThLimit ){
						
						// The closer we get to the border of the terrain the stronger
						// the average value must be and this for the 4 impacted points
						
						// Compute the average Lerp (one for each limit)
						avrgLerp = ( 
							Mathf.InverseLerp(0.0f,hTwLimit,Mw) + 
							Mathf.InverseLerp(0.0f,hThLimit,Mh) 
						) * 0.5f;
						
						// Compute average height
						avrgH = ( 
							heightMap[Mw, Mh] + 
							heightMap[Tw-Mw-1, Mh] + 
							heightMap[Mw, Th-Mh-1] + 
							heightMap[Tw-Mw-1, Th-Mh-1] 
						) / 4;
						
						// For each of the 4 points compute new height value
						heightMap[Mw, Mh] = Mathf.Lerp(avrgH,heightMap[Mw, Mh],avrgLerp);
						heightMap[Tw-Mw-1, Mh] = Mathf.Lerp(avrgH,heightMap[Tw-Mw-1, Mh],avrgLerp);
						heightMap[Mw, Th-Mh-1] = Mathf.Lerp(avrgH,heightMap[Mw, Th-Mh-1],avrgLerp);
						heightMap[Tw-Mw-1, Th-Mh-1] = Mathf.Lerp(avrgH,heightMap[Tw-Mw-1, Th-Mh-1],avrgLerp);
					
					}
					
				}
			}
			
			
			// Apply it to the terrain object
			terData.SetHeights(0, 0, heightMap);
			
		}
		catch(System.Exception e){
			Debug.LogError("An error occurred : "+e);
		}
			
	}
	
/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class PeakDistance : IComparable {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/
		
			public int id;
			public float dist;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
	// Default constructor

/*---------------------------------------------------------------------------*/
/* IComparable interface method */
/*---------------------------------------------------------------------------*/
	
		public int CompareTo(object obj){
			
			PeakDistance Compare = (PeakDistance) obj;
			
			int result = this.dist.CompareTo(Compare.dist);
			
			if( result==0 )	result = this.dist.CompareTo(Compare.dist);

			return(result);
			
		}

/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class voronoiPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public VoronoiType p_voronoiType;
			public int voronoiCells;
			public float voronoiFeatures;
			public float voronoiScale;
			public float voronoiBlend;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public voronoiPresetData(string pn, VoronoiType vt, int c, float vf, float vs, float vb){
			presetName = pn;
			p_voronoiType = vt;
			voronoiCells = c;
			voronoiFeatures = vf;
			voronoiScale = vs;
			voronoiBlend = vb;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class fractalPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public float diamondSquareDelta;
			public float diamondSquareBlend;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public fractalPresetData(string pn, float dsd, float dsb){
			presetName = pn;
			diamondSquareDelta = dsd;
			diamondSquareBlend = dsb;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class perlinPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public int perlinFrequency;
			public float perlinAmplitude;
			public int perlinOctaves;
			public float perlinBlend;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public perlinPresetData(string pn, int pf, float pa, int po, float pb){
			presetName = pn;
			perlinFrequency = pf;
			perlinAmplitude = pa;
			perlinOctaves = po;
			perlinBlend = pb;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class thermalErosionPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public int thermalIterations;
			public float thermalMinSlope;
			public float thermalFalloff;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
			
		public thermalErosionPresetData(string pn, int ti, float tms, float tba){
			presetName = pn;
			thermalIterations = ti;
			thermalMinSlope = tms;
			thermalFalloff = tba;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class fastHydraulicErosionPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public int hydraulicIterations;
			public float hydraulicMaxSlope;
			public float hydraulicFalloff;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public fastHydraulicErosionPresetData(string pn, int hi, float hms, float hba){
			presetName = pn;
			hydraulicIterations = hi;
			hydraulicMaxSlope = hms;
			hydraulicFalloff = hba;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class fullHydraulicErosionPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/
			
		public string presetName;
		public int hydraulicIterations;
		public float hydraulicRainfall;
		public float hydraulicEvaporation;
		public float hydraulicSedimentSolubility;
		public float hydraulicSedimentSaturation;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public fullHydraulicErosionPresetData(string pn, int hi, float hr, float he, float hso, float hsa){
			presetName = pn;
			hydraulicIterations = hi;
			hydraulicRainfall = hr;
			hydraulicEvaporation = he;
			hydraulicSedimentSolubility = hso;
			hydraulicSedimentSaturation = hsa;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class velocityHydraulicErosionPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public int hydraulicIterations;
			public float hydraulicVelocityRainfall;
			public float hydraulicVelocityEvaporation;
			public float hydraulicVelocitySedimentSolubility;
			public float hydraulicVelocitySedimentSaturation;
			public float hydraulicVelocity;
			public float hydraulicMomentum;
			public float hydraulicEntropy;
			public float hydraulicDowncutting;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public velocityHydraulicErosionPresetData(string pn, int hi, float hvr, float hve, float hso, float hsa, float hv, float hm, float he, float hd){
			presetName = pn;
			hydraulicIterations = hi;
			hydraulicVelocityRainfall = hvr;
			hydraulicVelocityEvaporation = hve;
			hydraulicVelocitySedimentSolubility = hso;
			hydraulicVelocitySedimentSaturation = hsa;
			hydraulicVelocity = hv;
			hydraulicMomentum = hm;
			hydraulicEntropy = he;
			hydraulicDowncutting = hd;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class tidalErosionPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public int tidalIterations;
			public float tidalRangeAmount;
			public float tidalCliffLimit;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public tidalErosionPresetData(string pn, int ti, float tra, float tcl){
			presetName = pn;
			tidalIterations = ti;
			tidalRangeAmount = tra;
			tidalCliffLimit = tcl;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class windErosionPresetData {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			public string presetName;
			public int windIterations;
			public float windDirection;
			public float windForce;
			public float windLift;
			public float windGravity;
			public float windCapacity;
			public float windEntropy;
			public float windSmoothing;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public windErosionPresetData(string pn, int wi, float wd, float wf, float wl, float wg, float wc, float we, float ws){
			presetName = pn;
			windIterations = wi;
			windDirection = wd;
			windForce = wf;
			windLift = wl;
			windGravity = wg;
			windCapacity = wc;
			windEntropy = we;
			windSmoothing = ws;
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
	// XXXX
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */

/* ************************************************************************* */
/*---------------------------------------------------------------------------*/
/* START INNER CLASS */
/*---------------------------------------------------------------------------*/

	public class PerlinNoise2D {
		
/*---------------------------------------------------------------------------*/
/* VARIABLES */
/*---------------------------------------------------------------------------*/

			private double[,] p_noiseValues;
			private float p_amplitude = 1.0f;
			private int p_frequency = 1;
		
/*---------------------------------------------------------------------------*/
/* Constructor */
/*---------------------------------------------------------------------------*/
	
		public PerlinNoise2D(int freq, float _amp){
			System.Random rand = new System.Random(System.Environment.TickCount);
			p_noiseValues = new double[freq, freq];
			p_amplitude = _amp;
			p_frequency = freq;
			for( int i = 0; i<freq; i++ ){
				for( int j = 0; j<freq; j++ ){
					p_noiseValues[i, j] = rand.NextDouble();
				}
			}
		}

/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
		public double getInterpolatedPoint(int _xa, int _xb, int _ya, int _yb, double Px, double Py){
			double i1 = interpolate(p_noiseValues[_xa % Frequency, _ya % p_frequency], p_noiseValues[_xb % Frequency, _ya % p_frequency], Px);
			double i2 = interpolate(p_noiseValues[_xa % Frequency, _yb % p_frequency], p_noiseValues[_xb % Frequency, _yb % p_frequency], Px);
			return interpolate(i1, i2, Py);
		}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
		public double interpolate(double Pa, double Pb, double Px){
			double ft = Px * Mathf.PI;
			double f = (1 - Mathf.Cos((float) ft)) * 0.5;
			return Pa * (1 - f) + Pb * f;
		}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
		public float Amplitude {
			get {
				return p_amplitude;
			}
		}
	
/*---------------------------------------------------------------------------*/
/* XXX */
/*---------------------------------------------------------------------------*/
	
		public int Frequency {
			get {
				return p_frequency;
			}
		}
	
/*---------------------------------------------------------------------------*/

	}

/*---------------------------------------------------------------------------*/
/* END INNER CLASS */
/*---------------------------------------------------------------------------*/
/* ************************************************************************* */
	
/*---------------------------------------------------------------------------*/

}

/*---------------------------------------------------------------------------*/
/* END CLASS */
/*---------------------------------------------------------------------------*/

}

/*---------------------------------------------------------------------------*/
/* END NAMESPACE */
/*---------------------------------------------------------------------------*/