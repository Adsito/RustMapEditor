using UnityEngine;

[ExecuteAlways]
public class WorldModel : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
