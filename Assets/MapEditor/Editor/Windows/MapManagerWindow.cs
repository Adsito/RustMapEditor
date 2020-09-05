using UnityEditor;
using UnityEngine;
using RustMapEditor.UI;
using RustMapEditor.Variables;

public class MapManagerWindow : EditorWindow
{
    #region Values
    string prefabSaveFile = "", mapPrefabSaveFile = "";
    int mainMenuOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0;
    float offset = 0f, heightSet = 500f, heightLow = 450f, heightHigh = 750f;
    bool clampOffset = true;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    Conditions conditions = new Conditions() 
    { 
        GroundConditions = new GroundConditions(TerrainSplat.Enum.Grass), BiomeConditions = new BiomeConditions(TerrainBiome.Enum.Temperate), TopologyConditions = new TopologyConditions(TerrainTopology.Enum.Beach)
    };
    Layers layers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field};
    int texture = 0, smoothPasses = 0;
    bool deletePrefabs = false, autoUpdate = false;
    Vector2 scrollPos = new Vector2(0, 0);
    Selections.Objects rotateSelection;
    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f, blurDirection = 0f, filterStrength = 1f;
    #endregion

    public void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIContent[] mainMenu = new GUIContent[3];
        mainMenu[0] = new GUIContent("File");
        mainMenu[1] = new GUIContent("Prefabs");
        mainMenu[2] = new GUIContent("Advanced");

        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu, EditorStyles.toolbarButton);

        #region Menu
        switch (mainMenuOptions)
        {
            #region File
            case 0:
                Functions.EditorIO();
                Functions.EditorInfo();
                Functions.MapInfo();
                Functions.EditorLinks();
                break;
            #endregion
            #region Prefabs
            case 1:
                Functions.AssetBundle();
                Functions.PrefabTools(ref deletePrefabs, prefabSaveFile, mapPrefabSaveFile);
                break;
            #endregion
            case 2:
            GUIContent[] mapToolsMenu = new GUIContent[3];
            mapToolsMenu[0] = new GUIContent("HeightMap");
            mapToolsMenu[1] = new GUIContent("Textures");
            mapToolsMenu[2] = new GUIContent("Misc");
            mapToolsOptions = GUILayout.Toolbar(mapToolsOptions, mapToolsMenu, EditorStyles.toolbarButton);
            
            switch (mapToolsOptions)
            {
                #region HeightMap
                case 0:
                    GUIContent[] heightMapMenu = new GUIContent[2];
                    heightMapMenu[0] = new GUIContent("Heights");
                    heightMapMenu[1] = new GUIContent("Filters");
                    heightMapOptions = GUILayout.Toolbar(heightMapOptions, heightMapMenu, EditorStyles.toolbarButton);
            
                    switch (heightMapOptions)
                    {
                        case 0:
                            Elements.BoldLabel(ToolTips.heightsLabel);
                            Functions.OffsetMap(ref offset, ref clampOffset);
                            Functions.SetHeight(ref heightSet);
                            Functions.ClampHeight(ref heightLow, ref heightHigh);
                            Elements.BoldLabel(ToolTips.miscLabel);
                            Functions.InvertMap();
                            break;
                        case 1:
                            Functions.NormaliseMap(ref normaliseLow, ref normaliseHigh, ref autoUpdate);
                            Functions.SmoothMap(ref filterStrength, ref blurDirection, ref smoothPasses);
                            Functions.TerraceMap(ref terraceErodeFeatureSize, ref terraceErodeInteriorCornerWeight);
                            break;
                    }
                    break;
                #endregion

                #region Textures
                case 1:
                    Functions.ConditionalPaint(ref conditionalPaintOptions, ref texture, ref conditions, ref layers);
                    break;
                #endregion

                #region Misc
                case 2:
                    Functions.RotateMap(ref rotateSelection);
                    break;
                    #endregion
            }
            break;
        }
        #endregion
        EditorGUILayout.EndScrollView();
    }
}