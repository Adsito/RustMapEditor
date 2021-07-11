using System.Threading.Tasks;
using static TerrainManager;

public static class TopologyData
{
    private static byte[] Data;

    public static TerrainMap<int> GetTerrainMap()
    {
        return new TerrainMap<int>(Data, 1);
    }

    /// <summary>Returns the Splatmap of the selected Topology Layer.</summary>
    /// <param name="layer">The Topology layer to return.</param>
    public static float[,,] GetTopologyLayer(int layer)
    {
        TerrainMap<int> topology = GetTerrainMap();
        float[,,] splatMap = new float[topology.res, topology.res, 2];
        Parallel.For(0, topology.res, i =>
        {
            for (int j = 0; j < topology.res; j++)
            {
                if ((topology[i, j] & layer) != 0)
                    splatMap[i, j, 0] = 1f;
                else
                    splatMap[i, j, 1] = 1f;
            }
        });
        return splatMap;
    }

    /// <summary>Converts all the Topology Layer arrays back into a single byte array.</summary>
    public static void SaveTopologyLayers()
    {
        TerrainMap<int> topologyMap = GetTerrainMap();
        Parallel.For(0, TerrainTopology.COUNT, i =>
        {
            Parallel.For(0, topologyMap.res, j =>
            {
                for (int k = 0; k < topologyMap.res; k++)
                {
                    if (Topology[i][j, k, 0] > 0)
                        topologyMap[j, k] = topologyMap[j, k] | TerrainTopology.IndexToType(i);

                    if (Topology[i][j, k, 1] > 0)
                        topologyMap[j, k] = topologyMap[j, k] & ~TerrainTopology.IndexToType(i);
                }
            });
        });
        Data = topologyMap.ToByteArray();
    }

    public static void Set(TerrainMap<int> topology) => Data = topology.ToByteArray();
}