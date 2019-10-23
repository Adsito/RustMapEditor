using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public static class LandData
{
    /// <summary>
    /// The Ground textures of the map.
    /// </summary>
    public static float[,,] groundArray;
    /// <summary>
    /// The Biome textures of the map.
    /// </summary>
    public static float[,,] biomeArray;
    /// <summary>
    /// The Alpha textures of the map.
    /// </summary>
    public static float[,,] alphaArray;
    /// <summary>
    /// The Topology layers, and textures of the map.
    /// </summary>
    public static float[][,,] topologyArray = new float[TerrainTopology.COUNT][,,];

    private static TerrainLayer[] groundTextures = null;
    private static TerrainLayer[] biomeTextures = null;
    private static TerrainLayer[] miscTextures = null;

    public static string landLayer = "";
    public static int landIndex = 0;
    public static TerrainTopology.Enum topologyLayer, oldTopologyLayer;

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        TerrainCallbacks.textureChanged += TextureChanged;
        TerrainCallbacks.heightmapChanged += HeightmapChanged;
    }
    private static void HeightmapChanged(Terrain terrain, RectInt heightRegion, bool synched)
    {
        
    }
    private static void TextureChanged(Terrain terrain, string textureName, RectInt texelRegion, bool synched)
    {

    }
    /// <summary>
    /// Change the active land layer.
    /// </summary>
    /// <param name="layer">The LandLayer to change to. (Ground, Biome, Alpha & Topology)</param>
    public static void ChangeLayer(string layer)
    {
        landLayer = layer;
        ChangeLandLayer();
    }
    public static void ChangeLandLayer()
    {
        SaveLayer(TerrainTopology.TypeToIndex((int)oldTopologyLayer));
        Undo.ClearAll();
        switch (landLayer.ToLower())
        {
            case "ground":
                SetLayer("ground");
                break;
            case "biome":
                SetLayer("biome");
                break;
            case "alpha":
                SetLayer("alpha");
                break;
            case "topology":
                SetLayer("topology", TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
        }
    }
    /// <summary>
    /// Sets the array data of LandLayer.
    /// </summary>
    /// <param name="floatArray">The alphamap array of all the textures.</param>
    /// <param name="layer">The landlayer to save the floatArray to.</param>
    /// <param name="topology">The topology layer if the landlayer is topology.</param>
    public static void SetData(float[,,] floatArray, string layer, int topology = 0)
    {
        switch (layer.ToLower())
        {
            case "ground":
                groundArray = floatArray;
                break;
            case "biome":
                biomeArray = floatArray;
                break;
            case "alpha":
                alphaArray = floatArray;
                break;
            case "topology":
                topologyArray[topology] = floatArray; 
                break;
        }
        landLayer = layer;
    }
    public static void GetTextures()
    {
        groundTextures = GetGroundTextures();
        biomeTextures = GetBiomeTextures();
        miscTextures = GetAlphaTextures();
        AssetDatabase.SaveAssets();
    }
    /// <summary>
    /// Sets the terrain alphamaps to the LandLayer.
    /// </summary>
    /// <param name="layer">The LandLayer to set.</param>
    /// <param name="topology">The Topology layer to set.</param>
    public static void SetLayer(string layer, int topology = 0)
    {
        if (groundTextures == null || biomeTextures == null || miscTextures == null)
        {
            GetTextures();
        }
        switch (layer.ToLower())
        {
            case "ground":
                landLayer = "ground";
                MapIO.terrain.terrainData.terrainLayers = groundTextures;
                MapIO.terrain.terrainData.SetAlphamaps(0, 0, groundArray);
                break;
            case "biome":
                landLayer = "biome";
                MapIO.terrain.terrainData.terrainLayers = biomeTextures;
                MapIO.terrain.terrainData.SetAlphamaps(0, 0, biomeArray);
                break;
            case "alpha":
                landLayer = "alpha";
                MapIO.terrain.terrainData.terrainLayers = miscTextures;
                MapIO.terrain.terrainData.SetAlphamaps(0, 0, alphaArray);
                break;
            case "topology":
                landLayer = "topology";
                MapIO.terrain.terrainData.terrainLayers = miscTextures;
                MapIO.terrain.terrainData.SetAlphamaps(0, 0, topologyArray[topology]);
                break;
        }
    }
    /// <summary>
    /// Saves any changes made to the Alphamaps, like the paint brush.
    /// </summary>
    /// <param name="topologyLayer">The Topology layer, if active.</param>
    public static void SaveLayer(int topologyLayer = 0)
    {
        switch (landLayer)
        {
            case "ground":
                groundArray = MapIO.terrain.terrainData.GetAlphamaps(0, 0, MapIO.terrain.terrainData.alphamapWidth, MapIO.terrain.terrainData.alphamapHeight);
                break;
            case "biome":
                biomeArray = MapIO.terrain.terrainData.GetAlphamaps(0, 0, MapIO.terrain.terrainData.alphamapWidth, MapIO.terrain.terrainData.alphamapHeight);
                break;
            case "alpha":
                alphaArray = MapIO.terrain.terrainData.GetAlphamaps(0, 0, MapIO.terrain.terrainData.alphamapWidth, MapIO.terrain.terrainData.alphamapHeight);
                break;
            case "topology":
                topologyArray[topologyLayer] = MapIO.terrain.terrainData.GetAlphamaps(0, 0, MapIO.terrain.terrainData.alphamapWidth, MapIO.terrain.terrainData.alphamapHeight);
                break;
        }
    }
    public static TerrainLayer[] GetAlphaTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[2];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Misc/Active.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/active");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Misc/InActive.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Misc/inactive");
        return textures;
    }
    public static TerrainLayer[] GetBiomeTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[4];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Tundra.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/tundra");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Temperate.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/temperate");
        textures[2] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arid.terrainlayer");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arid");
        textures[3] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Biome/Arctic.terrainlayer");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Biome/arctic");
        return textures;
    }
    public static TerrainLayer[] GetGroundTextures()
    {
        TerrainLayer[] textures = new TerrainLayer[8];
        textures[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Dirt.terrainlayer");
        textures[0].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/dirt");
        textures[1] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Snow.terrainlayer");
        textures[1].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/snow");
        textures[2] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Sand.terrainlayer");
        textures[2].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/sand");
        textures[3] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Rock.terrainlayer");
        textures[3].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/rock");
        textures[4] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Grass.terrainlayer");
        textures[4].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/grass");
        textures[5] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Forest.terrainlayer");
        textures[5].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/forest");
        textures[6] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Stones.terrainlayer");
        textures[6].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/stones");
        textures[7] = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Resources/Textures/Ground/Gravel.terrainlayer");
        textures[7].diffuseTexture = Resources.Load<Texture2D>("Textures/Ground/gravel");
        return textures;
    }
}