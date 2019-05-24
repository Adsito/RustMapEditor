using UnityEngine;

[ExecuteAlways]
public class Impostor : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
