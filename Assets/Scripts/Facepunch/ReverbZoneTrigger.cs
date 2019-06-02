using UnityEngine;

[ExecuteAlways]
public class ReverbZoneTrigger : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
