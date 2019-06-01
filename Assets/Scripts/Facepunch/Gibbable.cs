using UnityEngine;

[ExecuteAlways]
public class Gibbable : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
