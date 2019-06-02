using UnityEngine;

[ExecuteAlways]
public class ConstructionGrade : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
