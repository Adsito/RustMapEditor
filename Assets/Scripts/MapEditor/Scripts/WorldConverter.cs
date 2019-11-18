using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using RustMapEditor.Variables;
using static RustMapEditor.Data.LandData;
using static RustMapEditor.Maths.Array;
using static WorldSerialization;

public static class WorldConverter
{
    public struct MapInfo
    {
        public int terrainRes;
        public int splatRes;
        public Vector3 size;
        public float[,,] splatMap;
        public float[,,] biomeMap;
        public bool[,] alphaMap;
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
        MapInfo terrains = new MapInfo();

        int splatRes = Mathf.Clamp(Mathf.NextPowerOfTwo((int)(size * 0.50f)), 16, 2048);

        List<PathData> paths = new List<PathData>();
        List<PrefabData> prefabs = new List<PrefabData>();

        terrains.pathData = paths.ToArray();
        terrains.prefabData = prefabs.ToArray();

        terrains.terrainRes = Mathf.NextPowerOfTwo((int)(size * 0.50f)) + 1;
        terrains.size = new Vector3(size, 1000, size);

        terrains.land.heights = new float[terrains.terrainRes, terrains.terrainRes];
        terrains.water.heights = new float[terrains.terrainRes, terrains.terrainRes];

        terrains.splatRes = splatRes;
        terrains.splatMap = new float[splatRes, splatRes, 8];
        terrains.biomeMap = new float[splatRes, splatRes, 4];
        terrains.alphaMap = new bool[splatRes, splatRes];
        Parallel.For(0, splatRes, i =>
        {
            for (int j = 0; j < splatRes; j++)
            {
                terrains.alphaMap[i, j] = true;
            }
        });
        terrains.topology = new TerrainMap<int>(new byte[(int)Mathf.Pow(splatRes, 2) * 4 * 1], 1);
        return terrains;
    }
    /// <summary>Converts the MapInfo and TerrainMaps into a Unity map format.</summary>
    public static MapInfo ConvertMaps(MapInfo terrains, TerrainMap<byte> splatMap, TerrainMap<byte> biomeMap, TerrainMap<byte> alphaMap)
    {
        terrains.splatMap = new float[splatMap.res, splatMap.res, 8];
        terrains.biomeMap = new float[biomeMap.res, biomeMap.res, 4];
        terrains.alphaMap = new bool[alphaMap.res, alphaMap.res];

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
        terrains.splatMap = NormaliseMulti(terrains.splatMap, 8);
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
        terrains.biomeMap = NormaliseMulti(terrains.biomeMap, 4);
        Parallel.For(0, terrains.alphaMap.GetLength(0), i =>
        {
            for (int j = 0; j < terrains.alphaMap.GetLength(1); j++)
            {
                if (alphaMap[0, i, j] > 0)
                {
                    terrains.alphaMap[i, j] = true;
                }
                else
                {
                    terrains.alphaMap[i, j] = false;
                }
            }
        });
        return terrains;
    }
    /// <summary>Parses World Serialization and converts into MapInfo struct.</summary>
    /// <param name="world">Serialization of the map file to parse.</param>
    public static MapInfo WorldToTerrain(WorldSerialization world)
    {
        MapInfo terrains = new MapInfo();

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

        terrains.terrainRes = heightMap.res;
        terrains.splatRes = Mathf.Clamp(Mathf.NextPowerOfTwo((int)(world.world.size * 0.5f)), 16, 2048);
        terrains.size = terrainSize;

        terrains.land.heights = ShortMapToFloatArray(terrainMap);
        terrains.water.heights = ShortMapToFloatArray(waterMap);

        terrains = ConvertMaps(terrains, splatMap, biomeMap, alphaMap);
        return terrains;
    }
    /// <summary>Converts Unity terrains to WorldSerialization.</summary>
    public static WorldSerialization TerrainToWorld(Terrain land, Terrain water) 
    {
        WorldSerialization world = new WorldSerialization();
        world.world.size = (uint) land.terrainData.size.x;

        byte[] landHeightBytes = FloatArrayToByteArray(land.terrainData.GetHeights(0, 0, land.terrainData.heightmapResolution, land.terrainData.heightmapResolution));

        byte[] waterHeightBytes = FloatArrayToByteArray(water.terrainData.GetHeights(0, 0, water.terrainData.heightmapResolution, water.terrainData.heightmapResolution));
    
        var textureResolution = Mathf.Clamp(Mathf.NextPowerOfTwo((int)(world.world.size * 0.50f)), 16, 2048);

        byte[] splatBytes = new byte[textureResolution * textureResolution * 8];
        var splatMap = new TerrainMap<byte>(splatBytes, 8);
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < textureResolution; j++)
            {
                for(int k = 0; k < textureResolution; k++)
                {
                    splatMap[i, j, k] = BitUtility.Float2Byte(GetSplatMap(LandLayers.Ground)[j, k, i]);
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
                    biomeMap[i, j, k] = BitUtility.Float2Byte(GetSplatMap(LandLayers.Biome)[j, k, i]);
                }
            }
        }
        byte[] alphaBytes = new byte[textureResolution * textureResolution * 1];
        var alphaMap = new TerrainMap<byte>(alphaBytes, 1);
        for (int j = 0; j < textureResolution; j++)
        {
            for (int k = 0; k < textureResolution; k++)
            {
                 alphaMap[0, j, k] = BitUtility.Bool2Byte(GetAlphaMap()[j, k]);
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