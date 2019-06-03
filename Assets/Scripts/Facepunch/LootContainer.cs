using UnityEngine;

[ExecuteAlways]
public class LootContainer : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
