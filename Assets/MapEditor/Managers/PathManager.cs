using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static WorldSerialization;

public static class PathManager
{
    public static GameObject defaultPath { get; private set; }
    public static GameObject defaultNode { get; private set; }
    public static Transform pathParent { get; private set; }

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
        defaultPath = Resources.Load<GameObject>("Paths/Path");
        defaultNode = Resources.Load<GameObject>("Paths/PathNode");
        pathParent = GameObject.FindGameObjectWithTag("Paths").transform;
        EditorApplication.update -= OnProjectLoad;
    }

    public static void Spawn(PathData pathData)
    {
        Vector3 averageLocation = Vector3.zero;
        for (int j = 0; j < pathData.nodes.Length; j++)
        {
            averageLocation += pathData.nodes[j];
        }
        averageLocation /= pathData.nodes.Length;
        GameObject newObject = GameObject.Instantiate(defaultPath, averageLocation + pathParent.position, Quaternion.identity, pathParent);

        List<GameObject> pathNodes = new List<GameObject>();
        for (int j = 0; j < pathData.nodes.Length; j++)
        {
            GameObject newNode = GameObject.Instantiate(defaultNode, newObject.transform);
            newNode.transform.position = pathData.nodes[j] + pathParent.position;
            pathNodes.Add(newNode);
        }
        newObject.GetComponent<PathDataHolder>().pathData = pathData;
    }
}