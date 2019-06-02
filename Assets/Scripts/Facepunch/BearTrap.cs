using UnityEngine;

[ExecuteAlways]
public class BearTrap : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
