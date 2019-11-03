using System.Collections.Generic;
using UnityEngine;
using EditorMaths;
using static WorldSerialization;

public class WorldConverter
{
    public struct MapInfo
    {
        public int resolution;
        public Vector3 size;
        public float[,,] splatMap;
        public float[,,] biomeMap;
        public float[,,] alphaMap;
        public TerrainInfo terrain;
        public TerrainInfo land;
        public TerrainInfo water;
        public TerrainMap<int> topology;
        public WorldSerialization.PrefabData[] prefabData;
        public WorldSerialization.PathData[] pathData;
    }

    public struct TerrainInfo
    {
        public float[,] heights;
    }
    
    public static MapInfo EmptyMap(int size)
    {
        MapInfo terrains = new MapInfo();
        MapIO.ProgressBar("Creating New Map", "Creating Terrain", 0.0f);

        var terrainSize = new Vector3(size, 1000, size);

        int resolution = Mathf.NextPowerOfTwo((int)(size * 0.50f));
        
        var terrainMap  = new TerrainMap<short> (new byte[(int)Mathf.Pow((resolution + 1), 2) * 2 * 1], 1); //2 bytes 1 channel
        var heightMap   = new TerrainMap<short> (new byte[(int)Mathf.Pow((resolution + 1), 2) * 2 * 1], 1); //2 bytes 1 channel
        var waterMap    = new TerrainMap<short> (new byte[(int)Mathf.Pow((resolution + 1), 2) * 2 * 1], 1); //2 bytes 1 channel
        var splatMap    = new TerrainMap<byte>  (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution,16,2048), 2) * 1 * 8], 8); //1 byte 8 channels
        var topologyMap = new TerrainMap<int>   (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution, 16, 2048), 2) * 4 * 1], 1); //4 bytes 1 channel
        var biomeMap    = new TerrainMap<byte>  (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution, 16, 2048), 2) * 1 * 4], 4); //1 bytes 4 channels
        var alphaMap    = new TerrainMap<byte>  (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution, 16, 2048), 2) * 1 * 1], 1); //1 byte 1 channel

        MapIO.ProgressBar("Creating New Map", "Creating TerrainMaps", 0.25f);

        float[,] landHeight = new float[resolution + 1, resolution + 1];
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                landHeight[i, j] = 480f / 1000f;
            }
        }

        float[,] waterHeight = new float[resolution + 1, resolution + 1];
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                waterHeight[i, j] = 500f / 1000f;
            }
        }
        byte[] landHeightBytes = ArrayMaths.FloatArrayToByteArray(landHeight);
        byte[] waterHeightBytes = ArrayMaths.FloatArrayToByteArray(waterHeight);

        MapIO.ProgressBar("Creating New Map", "Setting TerrainMaps", 0.5f);

        terrainMap.FromByteArray(landHeightBytes);
        heightMap.FromByteArray(landHeightBytes);
        waterMap.FromByteArray(waterHeightBytes);

        terrains.topology = topologyMap;

        List<PathData> paths = new List<PathData>();
        List<PrefabData> prefabs = new List<PrefabData>();

        terrains.pathData = paths.ToArray();
        terrains.prefabData = prefabs.ToArray();


        terrains.resolution = heightMap.res;
        terrains.size = terrainSize;

        terrains.terrain.heights = ArrayMaths.ShortMapToFloatArray(terrainMap);
        terrains.land.heights = ArrayMaths.ShortMapToFloatArray(terrainMap);
        terrains.water.heights = ArrayMaths.ShortMapToFloatArray(waterMap);

        MapIO.ProgressBar("Creating New Map", "Converting to Terrain", 0.75f);

        terrains = ConvertMaps(terrains, splatMap, biomeMap, alphaMap, false);
        
        return terrains;
    }
    /// <summary>
    /// Converts the MapInfo and TerrainMaps into a Unity map format.
    /// </summary>
    /// <param name="normaliseMulti">Controls if the splatmaps have their values normalised to equal 1.0f or not. Should be set to true unless creating a new map.</param>
    /// <returns></returns>
    public static MapInfo ConvertMaps(MapInfo terrains, TerrainMap<byte> splatMap, TerrainMap<byte> biomeMap, TerrainMap<byte> alphaMap, bool normaliseMulti)
    {

        terrains.splatMap = new float[splatMap.res, splatMap.res, 8];
        for (int i = 0; i < terrains.splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.splatMap.GetLength(1); j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    terrains.splatMap[i, j, k] = BitUtility.Byte2Float(splatMap[k, i, j]);
                }
            }
        }
        terrains.splatMap = (normaliseMulti) ? ArrayMaths.MultiToSingleNormalised(terrains.splatMap, 8) : terrains.splatMap;

        terrains.biomeMap = new float[biomeMap.res, biomeMap.res, 4];
        for (int i = 0; i < terrains.biomeMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.biomeMap.GetLength(1); j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    terrains.biomeMap[i, j, k] = BitUtility.Byte2Float(biomeMap[k, i, j]);
                }
            }
        }
        terrains.biomeMap = (normaliseMulti) ? ArrayMaths.MultiToSingleNormalised(terrains.biomeMap, 4) : terrains.biomeMap;

        terrains.alphaMap = new float[alphaMap.res, alphaMap.res, 2];
        for (int i = 0; i < terrains.alphaMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.alphaMap.GetLength(1); j++)
            {
                if (alphaMap[0, i, j] > 0)
                {
                    terrains.alphaMap[i, j, 0] = BitUtility.Byte2Float(alphaMap[0, i, j]);
                }
                else
                {
                    terrains.alphaMap[i, j, 1] = 0xFF;
                }
            }
        }
        terrains.alphaMap = (normaliseMulti) ? ArrayMaths.MultiToSingleNormalised(terrains.alphaMap, 2) : terrains.alphaMap;
        return terrains;
    }

    /// <summary>
    /// Converts World to MapInfo.
    /// </summary>
    /// <returns></returns>
    public static MapInfo WorldToTerrain(WorldSerialization world)
    {
        MapInfo terrains = new MapInfo();

        if (world.GetMap("terrain") == null)
        {
            Debug.LogError("Old map file");
        }

        var terrainSize = new Vector3(world.world.size, 1000, world.world.size);
        var terrainMap = new TerrainMap<short>(world.GetMap("terrain").data, 1);
        var heightMap = new TerrainMap<short>(world.GetMap("height").data, 1);
        var waterMap = new TerrainMap<short>(world.GetMap("water").data, 1);
        var splatMap = new TerrainMap<byte>(world.GetMap("splat").data, 8);
        var topologyMap = new TerrainMap<int>(world.GetMap("topology").data, 1);
        var biomeMap = new TerrainMap<byte>(world.GetMap("biome").data, 4);
        var alphaMap = new TerrainMap<byte>(world.GetMap("alpha").data, 1);
        
        terrains.topology = topologyMap;

        terrains.pathData = world.world.paths.ToArray();
        terrains.prefabData = world.world.prefabs.ToArray();

        terrains.resolution = heightMap.res;
        terrains.size = terrainSize;

        terrains.terrain.heights = ArrayMaths.ShortMapToFloatArray(terrainMap);
        terrains.land.heights = ArrayMaths.ShortMapToFloatArray(terrainMap);
        terrains.water.heights = ArrayMaths.ShortMapToFloatArray(waterMap);

        terrains = ConvertMaps(terrains, splatMap, biomeMap, alphaMap, true);
        return terrains;
    }
    /// <summary>
    /// Converts map to WorldSerialization. 
    /// </summary>
    /// <param name="land"></param>
    /// <param name="water"></param>
    /// <returns></returns>
    public static WorldSerialization TerrainToWorld(Terrain land, Terrain water) 
    {
        WorldSerialization world = new WorldSerialization();
        world.world.size = (uint) land.terrainData.size.x;

        byte[] landHeightBytes = ArrayMaths.FloatArrayToByteArray(land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight));

        byte[] waterHeightBytes = ArrayMaths.FloatArrayToByteArray(water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight));
    
        var textureResolution = Mathf.Clamp(Mathf.NextPowerOfTwo((int)(world.world.size * 0.50f)), 16, 2048);

        byte[] splatBytes = new byte[textureResolution * textureResolution * 8];
        var splatMap = new TerrainMap<byte>(splatBytes, 8);
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < textureResolution; j++)
            {
                for(int k = 0; k < textureResolution; k++)
                {
                    splatMap[i, j, k] = BitUtility.Float2Byte(LandData.groundArray[j, k, i]);
                }
            }
        }
        byte[] biomeBytes = new byte[textureResolution * textureResolution * 4];
        var biomeMap = new TerrainMap<byte>(biomeBytes, 4);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < textureResolution; j++)
            {
                for (int k = 0; k < textureResolution; k++)
                {
                    biomeMap[i, j, k] = BitUtility.Float2Byte(LandData.biomeArray[j, k, i]);
                }
            }
        }
        byte[] alphaBytes = new byte[textureResolution * textureResolution * 1];
        var alphaMap = new TerrainMap<byte>(alphaBytes, 1);
        for (int j = 0; j < textureResolution; j++)
        {
            for (int k = 0; k < textureResolution; k++)
            {
                 alphaMap[0, j, k] = BitUtility.Float2Byte(LandData.alphaArray[j, k, 0]);
            }
        }
        TopologyData.SaveTopologyLayers();

        world.AddMap("terrain", landHeightBytes);
        world.AddMap("height", landHeightBytes);
        world.AddMap("splat", splatMap.ToByteArray());
        world.AddMap("biome", biomeMap.ToByteArray());
        world.AddMap("topology", TopologyData.GetTerrainMap().ToByteArray());
        world.AddMap("alpha", alphaMap.ToByteArray());
        world.AddMap("water", waterHeightBytes);

        PrefabDataHolder[] prefabs = GameObject.FindGameObjectWithTag("Prefabs").GetComponentsInChildren<PrefabDataHolder>(false);

        foreach (PrefabDataHolder p in prefabs)
        {
            if (p.prefabData != null)
            {
                p.UpdatePrefabData(); // Updates the prefabdata before saving.
                world.world.prefabs.Insert(0, p.prefabData);
            }
        }

        PathDataHolder[] paths = GameObject.FindObjectsOfType<PathDataHolder>();

        foreach (PathDataHolder p in paths)
        {
            if (p.pathData != null)
            {
                p.pathData.nodes = new VectorData[p.transform.childCount];
                for(int i = 0; i < p.transform.childCount; i++)
                {
                    Transform g = p.transform.GetChild(i);
                    p.pathData.nodes[i] = g.position - (0.5f * land.terrainData.size);
                }
                world.world.paths.Insert(0, p.pathData);
            }
        }
        return world;
    }
}