using UnityEngine;

[ExecuteAlways]
public class BuildingBlock : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
