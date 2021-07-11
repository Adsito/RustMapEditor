using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PathDataHolder : MonoBehaviour {
    
    public WorldSerialization.PathData pathData;
    public float resolutionFactor = 0;


    public void AddNodeToStart()
    {
        GameObject pathNodeObj = Resources.Load<GameObject>("Paths/PathNode");
        if (transform.childCount <= 1)
            Instantiate(pathNodeObj, transform);
        else
        {
            Transform firstNode = transform.GetChild(0);
            Transform secondNode = transform.GetChild(1);

            Vector3 pos = (firstNode.position - secondNode.position) + firstNode.position;

            GameObject newNode = Instantiate(pathNodeObj, pos, Quaternion.identity, transform);
            newNode.transform.SetSiblingIndex(0);
        }
    }

    public void AddNodeToEnd()
    {
        GameObject pathNodeObj = Resources.Load<GameObject>("Paths/PathNode");
        if (transform.childCount <= 1)
            Instantiate(pathNodeObj, transform);
        else
        {
            Transform lastNode = transform.GetChild(transform.childCount - 1);
            Transform secondLastNode = transform.GetChild(transform.childCount - 2);

            Vector3 pos = (lastNode.position - secondLastNode.position) + lastNode.position;
            Instantiate(pathNodeObj, pos, Quaternion.identity, transform);
        }
    }

    public void IncreaseNodesRes()
    {
        GameObject pathNodeObj = Resources.Load<GameObject>("Paths/PathNode");
        int amount = (int)(resolutionFactor * transform.childCount);
        if (amount == 0)
            return;
        int step = transform.childCount / amount;
        
        List<GameObject> newNodes = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i += step)
        {
            if (i + 1 >= transform.childCount)
                continue;

            Transform current = transform.GetChild(i);
            Transform next = transform.GetChild(i+1);

            Vector3 pos = (current.position + next.position) / 2;
            GameObject newNode = Instantiate(pathNodeObj, pos, Quaternion.identity);
            newNodes.Add(newNode);
        }
        int count = 0;
        for (int i = 0; i < newNodes.Count; i ++)
        {
            newNodes[i].transform.parent = transform;
            newNodes[i].transform.SetSiblingIndex((i*step)+count+1);
            count++;
        }
    }

    public void DecreaseNodesRes()
    {
        int amount = (int)(resolutionFactor * transform.childCount);
        if (amount == 0)
            return;
        int step = transform.childCount / amount;
        var nodes = new List<Transform>();
        for (int i = 0; i < transform.childCount; i += step)
            nodes.Add(transform.GetChild(i));
        for (int i = 0; i < nodes.Count; i ++)
            DestroyImmediate(nodes[i].gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 lastNode = Vector3.zero;

        for(int i = 0; i < transform.childCount; i++)
        {
            Transform g = transform.GetChild(i);
            Vector3 pos = g.position;
            if (lastNode == Vector3.zero)
            {
                lastNode = pos;
                continue;
            }
            else
                Gizmos.DrawLine(lastNode, pos);
            lastNode = pos;
        }
    }
}