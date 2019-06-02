using UnityEngine;

[ExecuteAlways]
public class BuildingBlockDecay : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}