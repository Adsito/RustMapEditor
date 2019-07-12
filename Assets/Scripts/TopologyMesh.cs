using System;
using UnityEngine;

[Serializable]
public static class TopologyMesh
{
    [SerializeField]
    public static byte[] top;

    public static TerrainMap<int> getTerrainMap()
    {
        return new TerrainMap<int>(top, 1);
    }
    public static float[,,] getSplatMap(int layer)
    {
        TerrainMap<int> topology = getTerrainMap();
        float[,,] splatMap = new float[topology.res, topology.res, 2];
        for (int i = 0; i < topology.res; i++)
        {
            for (int j = 0; j < topology.res; j++)
            {
                if ((topology[i, j] & layer) != 0)
                {
                    splatMap[i, j, 0] = float.MaxValue;
                }
                else
                {
                    splatMap[i, j, 1] = float.MaxValue;
                }
            }
        }
        return splatMap;
    }
    public static void SaveTopologyLayers()
    {
        TerrainMap<int> topologyMap = new TerrainMap<int>(top, 1);
        var splatMap = LandData.topologyArray;
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            for (int j = 0; j < topologyMap.res; j++)
            {
                for (int k = 0; k < topologyMap.res; k++)
                {
                    if (splatMap[i][j, k, 0] > 0)
                    {
                        topologyMap[j, k] = topologyMap[j, k] | TerrainTopology.IndexToType(i);
                    }
                    if (splatMap[i][j, k, 1] > 0)
                    {
                        topologyMap[j, k] = topologyMap[j, k] & ~TerrainTopology.IndexToType(i);
                    }
                }
            }
        }
        top = topologyMap.ToByteArray();
    }
    public static void InitMesh(TerrainMap<int> topology)
    {
        top = topology.ToByteArray();
    }
}