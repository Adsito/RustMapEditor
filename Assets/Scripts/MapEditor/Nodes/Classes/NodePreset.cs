using UnityEngine;
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
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        MapIO.RefreshAssetList();
    }
}