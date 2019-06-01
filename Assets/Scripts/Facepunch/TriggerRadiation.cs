using UnityEngine;

[ExecuteAlways]
public class TriggerRadiation : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}