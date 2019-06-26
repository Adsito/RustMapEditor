using UnityEngine;

[ExecuteAlways]
public class Workbench : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}