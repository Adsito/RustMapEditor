using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using static WorldSerialization;

public static class PathManager
{
    public static GameObject DefaultPath { get; private set; }
    public static GameObject DefaultNode { get; private set; }
    public static Transform PathParent { get; private set; }

    public static PathDataHolder[] CurrentMapPaths { get => PathParent.GetComponentsInChildren<PathDataHolder>(); }

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
        if (DefaultPath != null && DefaultNode != null && PathParent != null)
            EditorApplication.update -= OnProjectLoad;
    }

    public static void SpawnPath(PathData pathData)
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

    public static void SpawnPaths(PathData[] paths, int progressID)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SpawnPaths(paths, progressID));
    }

    public static void DeletePaths(PathDataHolder[] paths, int progressID = 0)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.DeletePaths(paths, progressID));
    }

    private static class Coroutines
    {
        public static IEnumerator SpawnPaths(PathData[] paths, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < paths.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.1f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / paths.Length, "Spawning Paths: " + i + " / " + paths.Length);
                    sw.Restart();
                }
                SpawnPath(paths[i]);
            }
            Progress.Report(progressID, 0.99f, "Spawned " + paths.Length + " paths.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }

        public static IEnumerator DeletePaths(PathDataHolder[] paths, int progressID = 0)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (progressID == 0)
                progressID = Progress.Start("Delete Paths", null, Progress.Options.Sticky);

            for (int i = 0; i < paths.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.1f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / paths.Length, "Deleting Paths: " + i + " / " + paths.Length);
                    sw.Restart();
                }
                GameObject.DestroyImmediate(paths[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Deleted " + paths.Length + " paths.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }
    }
}