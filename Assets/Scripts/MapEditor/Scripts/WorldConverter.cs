using System.Threading.Tasks;
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
        public TerrainInfo land;
        public TerrainInfo water;
        public TerrainMap<int> topology;
        public PrefabData[] prefabData;
        public PathData[] pathData;
    }

    public struct TerrainInfo
    {
        public float[,] heights;
    }
    
    public static MapInfo EmptyMap(int size)
    {
        MapIO.ProgressBar("Creating New Map", "Setting TerrainMaps", 0.5f);

        MapInfo terrains = new MapInfo();

        int splatRes = Mathf.Clamp(Mathf.NextPowerOfTwo((int)(size * 0.50f)), 16, 2048);

        List<PathData> paths = new List<PathData>();
        List<PrefabData> prefabs = new List<PrefabData>();

        terrains.pathData = paths.ToArray();
        terrains.prefabData = prefabs.ToArray();

        terrains.resolution = Mathf.NextPowerOfTwo((int)(size * 0.50f)) + 1;
        terrains.size = new Vector3(size, 1000, size);

        terrains.land.heights = new float[terrains.resolution, terrains.resolution];
        terrains.water.heights = new float[terrains.resolution, terrains.resolution];

        MapIO.ProgressBar("Creating New Map", "Converting to Terrain", 0.75f);

        terrains.splatMap = new float[splatRes, splatRes, 8];
        terrains.biomeMap = new float[splatRes, splatRes, 4];
        terrains.alphaMap = new float[splatRes, splatRes, 2];
        terrains.topology = new TerrainMap<int>(new byte[(int)Mathf.Pow(splatRes, 2) * 4 * 1], 1);

        return terrains;
    }
    /// <summary>
    /// Converts the MapInfo and TerrainMaps into a Unity map format.
    /// </summary>
    /// <param name="normaliseMulti">Controls if the splatmaps have their values normalised to equal 1.0f or not. Should be set to true unless creating a new map.</param>
    public static MapInfo ConvertMaps(MapInfo terrains, TerrainMap<byte> splatMap, TerrainMap<byte> biomeMap, TerrainMap<byte> alphaMap)
    {
        terrains.splatMap = new float[splatMap.res, splatMap.res, 8];
        terrains.biomeMap = new float[biomeMap.res, biomeMap.res, 4];
        terrains.alphaMap = new float[alphaMap.res, alphaMap.res, 2];

        Parallel.For(0, terrains.splatMap.GetLength(0), i =>
        {
            for (int j = 0; j < terrains.splatMap.GetLength(1); j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    terrains.splatMap[i, j, k] = BitUtility.Byte2Float(splatMap[k, i, j]);
                }
            }
        });
        terrains.splatMap = ArrayMaths.MultiToSingleNormalised(terrains.splatMap, 8);
        Parallel.For(0, terrains.biomeMap.GetLength(0), i => 
        {
            for (int j = 0; j < terrains.biomeMap.GetLength(1); j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    terrains.biomeMap[i, j, k] = BitUtility.Byte2Float(biomeMap[k, i, j]);
                }
            }
        });
        terrains.biomeMap = ArrayMaths.MultiToSingleNormalised(terrains.biomeMap, 4);
        Parallel.For(0, terrains.alphaMap.GetLength(0), i =>
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
        });
        terrains.alphaMap = ArrayMaths.MultiToSingleNormalised(terrains.alphaMap, 2);
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

        terrains.land.heights = ArrayMaths.ShortMapToFloatArray(terrainMap);
        terrains.water.heights = ArrayMaths.ShortMapToFloatArray(waterMap);

        terrains = ConvertMaps(terrains, splatMap, biomeMap, alphaMap);
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