using UnityEngine;

public static class EditorVars
{
    public static class ToolTips
    {
        public static GUIContent prefabCategory = new GUIContent("Category:", "The Category group assigned to the prefab.");
        public static GUIContent prefabID = new GUIContent("ID:", "The Prefab ID assigned to the prefab.");
        public static GUIContent prefabName = new GUIContent("Name:", "The Prefab name.");

        public static GUIContent snapToGround = new GUIContent("Snap To Ground", "Snap the selected prefab to the terrain height.");

        public static GUIContent toggleBlend = new GUIContent("Blend", "Blends out the active texture to create a smooth transition the surrounding textures.");
        public static GUIContent rangeLow = new GUIContent("From:", "The lowest value to paint the active texture.");
        public static GUIContent rangeHigh = new GUIContent("To:", "The highest value to paint the active texture.");
        public static GUIContent blendLow = new GUIContent("Blend Low:", "The lowest value to blend out to.");
        public static GUIContent blendHigh = new GUIContent("Blend High:", "The highest value to blend out to.");

        public static GUIContent fromZ = new GUIContent("From Z", "The starting point to paint the active texture.");
        public static GUIContent toZ = new GUIContent("To Z", "The ending point to paint the active texture.");
        public static GUIContent fromX = new GUIContent("From X", "The starting point to paint the active texture.");
        public static GUIContent toX = new GUIContent("To X", "The ending point to paint the active texture.");

        public static GUIContent paintRivers = new GUIContent("Paint Rivers", "Paints the active texture wherever the water is above 500.");
        public static GUIContent eraseRivers = new GUIContent("Erase Rivers", "Paints the inactive texture wherever the water is above 500.");
        public static GUIContent aboveTerrain = new GUIContent("Above Terrain", "Paint only where there is water above sea level and above the terrain.");

        public static GUIContent groundTextureSelect = new GUIContent("Texture:", "The Ground texture the tools will paint with.");
        public static GUIContent biomeTextureSelect = new GUIContent("Texture:", "The Biome texture the tools will paint with.");
        public static GUIContent topologyLayerSelect = new GUIContent("Topology Layer:", "The Topology layer to display.");

        public static GUIContent paintSlopes = new GUIContent("Paint Slopes", "Paints the active texture within the slope range.");
        public static GUIContent eraseSlopes = new GUIContent("Erase Slopes", "Paints the inactive texture within the slope range.");

        public static GUIContent paintHeights = new GUIContent("Paint Heights", "Paints the active texture within the height range.");
        public static GUIContent eraseHeights = new GUIContent("Erase Heights", "Paints the inactive texture within the height range.");

        public static GUIContent rotate90 = new GUIContent("Rotate 90°", "Rotate the layer 90°.");
        public static GUIContent rotate270 = new GUIContent("Rotate 270°", "Rotate the layer 270°.");
    }
}
