using System;
using UnityEngine;

[Serializable]
public static class TopologyData
{
    [SerializeField]
    public static byte[] top;

    public static TerrainMapSDK<int> GetTerrainMap()
    {
        return new TerrainMapSDK<int>(top, 1);
    }
    /// <summary>
    /// Returns the Splatmap of the selected Topology Layer.
    /// </summary>
    /// <param name="layer">The Topology layer to return.</param>
    /// <returns></returns>
    public static float[,,] GetTopologyLayer(int layer)
    {
        TerrainMapSDK<int> topology = GetTerrainMap();
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
    /// <summary>
    /// Converts all the Topology Layer arrays back into a single byte array.
    /// </summary>
    public static void SaveTopologyLayers()
    {
        TerrainMapSDK<int> topologyMap = new TerrainMapSDK<int>(top, 1);
        var splatMap = LandData.topologyArray;
        for (int i = 0; i < TerrainTopologySDK.COUNT; i++)
        {
            for (int j = 0; j < topologyMap.res; j++)
            {
                for (int k = 0; k < topologyMap.res; k++)
                {
                    if (splatMap[i][j, k, 0] > 0)
                    {
                        topologyMap[j, k] = topologyMap[j, k] | TerrainTopologySDK.IndexToType(i);
                    }
                    if (splatMap[i][j, k, 1] > 0)
                    {
                        topologyMap[j, k] = topologyMap[j, k] & ~TerrainTopologySDK.IndexToType(i);
                    }
                }
            }
        }
        top = topologyMap.ToByteArray();
    }
    public static void InitMesh(TerrainMapSDK<int> topology)
    {
        top = topology.ToByteArray();
    }
}