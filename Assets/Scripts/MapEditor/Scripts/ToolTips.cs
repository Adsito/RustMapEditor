﻿using UnityEngine;

namespace RustMapEditor.Variables
{
    public static class ToolTips
    {
        public static GUIContent editorInfoLabel = new GUIContent("Editor Info", "Info about the current editor, when reporting bugs make sure to include a copy of these values.");
        public static GUIContent systemOS = new GUIContent("OS: " + SystemInfo.operatingSystem);
        public static GUIContent systemRAM = new GUIContent("RAM: " + SystemInfo.systemMemorySize / 1000 + "GB");
        public static GUIContent unityVersion = new GUIContent("Unity Version: " + Application.unityVersion);
        public static GUIContent editorVersion = new GUIContent("Editor Version: v2.3-prerelease");

        public static GUIContent runPreset = new GUIContent("Run Preset", "Run this preset with all it's current nodes.");
        public static GUIContent deletePreset = new GUIContent("Delete Preset", "Delete this preset from Unity.");
        public static GUIContent renamePreset = new GUIContent("Rename Preset", "Rename this preset.");
        public static GUIContent presetWiki = new GUIContent("Wiki", "Opens the wiki at the NodeSystem page.");

        public static GUIContent prefabHierachyName = new GUIContent("Name", "The name of the prefab.");
        public static GUIContent prefabHierachyType = new GUIContent("Type", "If the prefab is custom built or native to Rust.");
        public static GUIContent prefabHierachyCategory = new GUIContent("Category", "The prefab category if from Rust.");
        public static GUIContent prefabHierachyRustID = new GUIContent("ID", "The prefab ID in the Rust game manifest.");

        public static GUIContent pathHierachyName = new GUIContent("Name", "The name of the path.");
        public static GUIContent pathHierachyInnerPadding = new GUIContent("Inner Padding", "The inner padding of the path.");
        public static GUIContent pathHierachyOuterPadding = new GUIContent("Outer Padding", "The outer padding of the path.");
        public static GUIContent pathHierachyInnerFade = new GUIContent("Inner Fade", "The inner fade of the path");
        public static GUIContent pathHierachyOuterFade = new GUIContent("Outer Fade", "The outer fade of the path");

        public static GUIContent prefabCategory = new GUIContent("Category:", "The Category group assigned to the prefab.");
        public static GUIContent prefabID = new GUIContent("ID:", "The Prefab ID assigned to the prefab.");
        public static GUIContent prefabName = new GUIContent("Name:", "The Prefab name.");

        public static GUIContent assetBundleLabel = new GUIContent("Asset Bundle");
        public static GUIContent loadBundle = new GUIContent("Load", "Loads the Rust asset bundle into memory.");
        public static GUIContent unloadBundle = new GUIContent("Unload", "Unloads the loaded bundle.");

        public static GUIContent presetsLabel = new GUIContent("Node Presets", "List of all the node presets in the project.");
        public static GUIContent openPreset = new GUIContent("Open", "Opens the Node preset.");
        public static GUIContent refreshPresets = new GUIContent("Refresh presets list.", "Refreshes the list of all the Node Presets in the project.");
        public static GUIContent noPresets = new GUIContent("No presets in list.", "Try creating a some presets first.");

        public static GUIContent editorLinksLabel = new GUIContent("Links", "Links to discord, wiki and the project GitHub.");
        public static GUIContent reportBug = new GUIContent("Report Bug", "Opens up the editor bug report in GitHub.");
        public static GUIContent requestFeature = new GUIContent("Request Feature", "Opens up the editor feature request in GitHub.");
        public static GUIContent roadMap = new GUIContent("RoadMap", "Opens up the editor roadmap in GitHub.");
        public static GUIContent wiki = new GUIContent("Wiki", "Opens up the editor wiki in GitHub.");
        public static GUIContent discord = new GUIContent("Discord", "Discord invitation link.");

        public static GUIContent toolsLabel = new GUIContent("Tools");
        public static GUIContent editorSettingsLabel = new GUIContent("Settings");
        public static GUIContent saveSettings = new GUIContent("Save", "Sets and saves the current settings.");
        public static GUIContent discardSettings = new GUIContent("Discard", "Discards the changes to the settings.");
        public static GUIContent defaultSettings = new GUIContent("Default", "Sets the settings back to the default.");
        public static GUIContent rustDirectory = new GUIContent("Rust Directory", @"The base install directory of Rust. Normally located at steamapps\common\Rust");
        public static GUIContent browseRustDirectory = new GUIContent("Browse", "Browse and select the base directory of Rust.");
        public static GUIContent rustDirectoryPath = new GUIContent(MapEditorSettings.rustDirectory, "The install directory of Rust on the local PC.");
        public static GUIContent renderDistanceLabel = new GUIContent("Render Distance");
        public static GUIContent prefabRenderDistance = new GUIContent("Prefabs", "Changes the distance prefabs can be seen from.");
        public static GUIContent pathRenderDistance = new GUIContent("Paths", "Changes the paths can be seen from.");
        public static GUIContent objectQuality = new GUIContent("Object Quality", "Controls the LODs shown and the distances they render from. Provides 1:1 clarity with ingame.");

        public static GUIContent mapInfoLabel = new GUIContent("Map Info", "General info about the currently loaded map.");
        public static GUIContent loadMap = new GUIContent("Load", "Opens a file viewer to find and open a Rust .map file.");
        public static GUIContent saveMap = new GUIContent("Save", "Opens a file viewer to find and save a Rust .map file.");
        public static GUIContent newMap = new GUIContent("New", "Creates a new map with the selected size.");
        public static GUIContent mapSize = new GUIContent("Size", "The size to create any new maps. Must be between (1000-6000)");

        public static GUIContent exportMapPrefabs = new GUIContent("Export Map Prefabs", "Exports all map prefabs to a .JSON file.");
        public static GUIContent exportMapLootCrates = new GUIContent("Export LootCrates", "Exports all lootcrates that don't yet respawn in Rust to a JSON for use with the LootCrateRespawn plugin.");
        public static GUIContent deleteOnExport = new GUIContent("Delete on Export.", "Deletes prefabs/lootcrates after exporting.");
        public static GUIContent groupRustEditPrefabs = new GUIContent("Group RustEdit Custom Prefabs", "Groups all custom prefabs saved in the map file.");
        public static GUIContent breakRustEditPrefabs = new GUIContent("Break RustEdit Custom Prefabs", "Breaks down all custom prefabs saved in the map file.");
        public static GUIContent hidePrefabsInRustEdit = new GUIContent("Hide Prefabs in RustEdit", "Changes all the prefab categories to a semi-colon. Hides all of the prefabs from appearing in RustEdit.");

        public static GUIContent deleteMapPrefabs = new GUIContent("Delete All Map Prefabs", "Removes all the prefabs from the map.");
        public static GUIContent deleteMapPaths = new GUIContent("Delete All Map Paths", "Removes all the paths from the map.");

        public static GUIContent snapToGround = new GUIContent("Snap To Ground", "Snap the selected prefab to the terrain height.");

        public static GUIContent toggleBlend = new GUIContent("Blend", "Blends out the active texture to create a smooth transition the surrounding textures.");
        public static GUIContent rangeLow = new GUIContent("From:", "The lowest value to paint the active texture.");
        public static GUIContent rangeHigh = new GUIContent("To:", "The highest value to paint the active texture.");
        public static GUIContent blendLow = new GUIContent("Blend Low:", "The lowest value to blend out to.");
        public static GUIContent blendHigh = new GUIContent("Blend High:", "The highest value to blend out to.");

        public static GUIContent areaToolsLabel = new GUIContent("Area Tools");
        public static GUIContent fromZ = new GUIContent("From Z", "The starting point of the area.");
        public static GUIContent toZ = new GUIContent("To Z", "The ending point of the area.");
        public static GUIContent fromX = new GUIContent("From X", "The starting point of the area.");
        public static GUIContent toX = new GUIContent("To X", "The ending point of the area.");
        public static GUIContent paintArea = new GUIContent("Paint Area", "Paints the selected area with the active texture.");
        public static GUIContent eraseArea = new GUIContent("Erase Area", "Paints the selected area with the inactive texture.");

        public static GUIContent riverToolsLabel = new GUIContent("River Tools");
        public static GUIContent paintRivers = new GUIContent("Paint Rivers", "Paints the active texture wherever the water is above 500.");
        public static GUIContent eraseRivers = new GUIContent("Erase Rivers", "Paints the inactive texture wherever the water is above 500.");
        public static GUIContent aboveTerrain = new GUIContent("Above Terrain", "Paint only where there is water above sea level and above the terrain.");

        public static GUIContent slopeToolsLabel = new GUIContent("Slope Tools");
        public static GUIContent paintSlopes = new GUIContent("Paint Slopes", "Paints the active texture within the slope range.");
        public static GUIContent paintSlopesBlend = new GUIContent("Paint Slopes Blend", "Paints the active texture within the slope range, whilst blending out to the blend range.");
        public static GUIContent eraseSlopes = new GUIContent("Erase Slopes", "Paints the inactive texture within the slope range.");

        public static GUIContent heightsLabel = new GUIContent("Heights");
        public static GUIContent heightToolsLabel = new GUIContent("Height Tools");
        public static GUIContent paintHeights = new GUIContent("Paint Heights", "Paints the active texture within the height range.");
        public static GUIContent paintHeightsBlend = new GUIContent("Paint Heights Blend", "Paints the active texture within the height range, whilst blending out to the blend range.");
        public static GUIContent eraseHeights = new GUIContent("Erase Heights", "Paints the inactive texture within the height range.");

        public static GUIContent miscLabel = new GUIContent("Misc");

        public static GUIContent rotateMapLabel = new GUIContent("Rotate Map");
        public static GUIContent rotateSelection = new GUIContent("Rotation Selection:", "The items to rotate.");
        public static GUIContent rotate90 = new GUIContent("Rotate 90°", "Rotate the layer 90°.");
        public static GUIContent rotate270 = new GUIContent("Rotate 270°", "Rotate the layer 270°.");
        public static GUIContent rotateAll90 = new GUIContent("Rotate All 90°", "Rotate all Topology layers 90°");
        public static GUIContent rotateAll270 = new GUIContent("Rotate All 270°", "Rotate all Topology layers 270°");

        public static GUIContent terraceLabel = new GUIContent("Terrace");
        public static GUIContent featureSize = new GUIContent("Feature Size", "The higher the value the more terrace levels generated.");
        public static GUIContent cornerWeight = new GUIContent("Corner Weight", "The strength of the corners of the terrace.");
        public static GUIContent terraceMap = new GUIContent("Terrace Map", "Terraces the terrain.");

        public static GUIContent smoothLabel = new GUIContent("Smooth");
        public static GUIContent smoothStrength = new GUIContent("Strength", "The strength of the smoothing operation.");
        public static GUIContent blurDirection = new GUIContent("Blur Direction", "The direction the terrain should blur towards. Negative is down, positive is up.");
        public static GUIContent smoothMap = new GUIContent("Smooth Map", "Smoothes the terrain the selected amount of times.");

        public static GUIContent normaliseLabel = new GUIContent("Normalise");
        public static GUIContent normaliseLow = new GUIContent("Low", "The lowest point on the map after being normalised.");
        public static GUIContent normaliseHigh = new GUIContent("High", "The highest point on the map after being normalised.");
        public static GUIContent normaliseMap = new GUIContent("Normalise", "Scales the terrain between these heights.");
        public static GUIContent autoUpdateNormalise = new GUIContent("Auto Update", "Automatically normalises the changes to the terrain on value change.");

        public static GUIContent setHeightLabel = new GUIContent("Height Set");
        public static GUIContent heightToSet = new GUIContent("Height", "The height to set.");
        public static GUIContent setLandHeight = new GUIContent("Set Land Height", "Sets the terrain height to the height selected.");
        public static GUIContent setWaterHeight = new GUIContent("Set Water Height", "Sets the water height to the height selected.");

        public static GUIContent clampHeightLabel = new GUIContent("Clamp Height");
        public static GUIContent minHeight = new GUIContent("Minimum Height", "The minimum height to set the terrain to.");
        public static GUIContent maxHeight = new GUIContent("Maximum Height", "The maximum height to set the terrain to.");
        public static GUIContent setMinHeight = new GUIContent("Set Minimum Height", "Lowers any of the terrain below the minimum height to the minimum height.");
        public static GUIContent setMaxHeight = new GUIContent("Set Maximum Height", "Raises any of the terrain above the maximum height to the maximum height.");

        public static GUIContent offsetLabel = new GUIContent("Offset");
        public static GUIContent offsetHeight = new GUIContent("Height", "The height to offset.");
        public static GUIContent clampOffset = new GUIContent("Clamp Offset", "Prevents the flattening effect if you raise or lower the terrain too far.");
        public static GUIContent offsetLand = new GUIContent("Offset Land", "Adds the offset height to the terrain. Negative values lower the height.");
        public static GUIContent offsetWater = new GUIContent("Offset Water", "Adds the offset height to the water. Negative values lower the height.");

        public static GUIContent invertLabel = new GUIContent("Invert");
        public static GUIContent invertLand = new GUIContent("Invert Land", "Inverts the terrain heights. The heighest point becomes the lowest point.");
        public static GUIContent invertWater = new GUIContent("Invert Water", "Inverts the water heights. The heighest point becomes the lowest point.");

        public static GUIContent conditionalPaintLabel = new GUIContent("Conditional Paint");
        public static GUIContent conditionsLabel = new GUIContent("Conditions");
        public static GUIContent conditionalTextureWeight = new GUIContent("Weight", "The minimum texture weight when checking the texture.");
        public static GUIContent checkTexture = new GUIContent("Check Texture", "If toggled the texture selected will be checked.");
        public static GUIContent checkTopologyLayer = new GUIContent("Check Topology", "If toggled the topology selected will be checked.");
        public static GUIContent checkSlopes = new GUIContent("Check Slopes", "If toggled the Slopes will be checked within the selected range.");
        public static GUIContent checkHeights = new GUIContent("Check Heights", "If toggled the Height will be checked within the selected range.");
        public static GUIContent paintConditional = new GUIContent("Paint Conditional", "Paints the selected texture if it matches all of the conditions set.");

        public static GUIContent textureToPaintLabel = new GUIContent("Texture To Paint");
        public static GUIContent textureSelectLabel = new GUIContent("Texture Select");
        public static GUIContent textureSelect = new GUIContent("Texture:", "The texture to paint with.");
        public static GUIContent layerSelectLabel = new GUIContent("Layer Select");
        public static GUIContent layerSelect = new GUIContent("Layer:", "The layer to display.");
        public static GUIContent topologyLayerSelect = new GUIContent("Topology Layer:", "The Topology layer the tools will use.");

        public static GUIContent layerToolsLabel = new GUIContent("Layer Tools");
        public static GUIContent paintLayer = new GUIContent("Paint Layer", "Paints the active texture on the entire terrain.");
        public static GUIContent clearLayer = new GUIContent("Clear Layer", "Paints the inactive texture on the entire terrain.");
        public static GUIContent invertLayer = new GUIContent("Invert Layer", "Inverts the active and inactive textures over the entire terrain.");
        public static GUIContent invertAll = new GUIContent("Invert All", "Invert all Topology layers.");
        public static GUIContent clearAll = new GUIContent("Clear All", "Clear all Topology layers.");
        public static GUIContent paintAll = new GUIContent("Paint All", "Paint all Topology layers.");
    }
}