using UnityEngine;

[ExecuteAlways]
public class BaseLadder : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
