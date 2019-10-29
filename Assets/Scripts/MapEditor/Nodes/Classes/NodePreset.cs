using UnityEngine;
using System.Reflection;
using UnityEditor;
using XNode;

[CreateAssetMenu(fileName = "Node Preset", menuName = "NodePreset")]
public class NodePreset : NodeGraph
{
    
}
class NodeMonitor : AssetPostprocessor
{
    /// <summary>
    /// Monitors changes to any assets in the project.
    /// </summary>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        MapIO.RefreshAssetList();
    }

}
public static class NodeAsset
{
    public const string nodeAssetName = "NodePreset";
    /// <summary>
    /// Runs the selected NodeGraph.
    /// </summary>
    /// <param name="graph">The NodeGraph to run.</param>
    public static void Parse(NodeGraph graph)
    {
        foreach (var node in graph.nodes)
        {
            if (node.name == "Start")
            {
                if (node.GetOutputPort("NextTask").GetConnections().Count == 0) // Check for start node being in graph but not linked.
                {
                    return;
                }
                Node nodeIteration = node.GetOutputPort("NextTask").Connection.node;
                if (nodeIteration != null)
                {
                    do
                    {
                        MethodInfo runNode = nodeIteration.GetType().GetMethod("RunNode");
                        runNode.Invoke(nodeIteration, null);
                        if (nodeIteration.GetOutputPort("NextTask").IsConnected)
                        {
                            nodeIteration = nodeIteration.GetOutputPort("NextTask").Connection.node;
                        }
                        else
                        {
                            nodeIteration = null;
                        }
                    }
                    while (nodeIteration != null);
                }
            }
        }
    }
}