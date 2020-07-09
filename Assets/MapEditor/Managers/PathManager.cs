using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static WorldSerialization;

public static class PathManager
{
    public static GameObject DefaultPath { get; private set; }
    public static GameObject DefaultNode { get; private set; }
    public static Transform PathParent { get; private set; }

    public enum PathType
    {
        River = 0,
        Road = 1,
        Powerline = 2,
    }

    [InitializeOnLoadMethod]
    public static void Init()
    {
        EditorApplication.update += OnProjectLoad;
    }

    static void OnProjectLoad()
    {
        DefaultPath = Resources.Load<GameObject>("Paths/Path");
        DefaultNode = Resources.Load<GameObject>("Paths/PathNode");
        PathParent = GameObject.FindGameObjectWithTag("Paths").transform;
        EditorApplication.update -= OnProjectLoad;
    }

    public static void Spawn(PathData pathData)
    {
        Vector3 averageLocation = Vector3.zero;
        for (int j = 0; j < pathData.nodes.Length; j++)
            averageLocation += pathData.nodes[j];

        averageLocation /= pathData.nodes.Length;
        GameObject newObject = GameObject.Instantiate(DefaultPath, averageLocation + PathParent.position, Quaternion.identity, PathParent);
        newObject.name = pathData.name;

        List<GameObject> pathNodes = new List<GameObject>();
        for (int j = 0; j < pathData.nodes.Length; j++)
        {
            GameObject newNode = GameObject.Instantiate(DefaultNode, newObject.transform);
            newNode.transform.position = pathData.nodes[j] + PathParent.position;
            pathNodes.Add(newNode);
        }
        newObject.GetComponent<PathDataHolder>().pathData = pathData;
    }
}