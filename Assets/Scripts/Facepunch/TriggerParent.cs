using UnityEngine;

[ExecuteAlways]
public class TriggerParent : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
