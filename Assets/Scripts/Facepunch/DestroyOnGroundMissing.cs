using UnityEngine;

[ExecuteAlways]
public class DestroyOnGroundMissing : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
