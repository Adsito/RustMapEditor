using UnityEngine;

[ExecuteAlways]
public class RepairBench : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
