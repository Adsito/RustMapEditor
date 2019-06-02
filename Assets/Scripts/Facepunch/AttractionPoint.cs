using UnityEngine;

[ExecuteAlways]
public class AttractionPoint : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
