using UnityEngine;

[ExecuteAlways]
public class SimpleBuildingBlock : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
