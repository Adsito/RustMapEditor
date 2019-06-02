using UnityEngine;

[ExecuteAlways]
public class TriggerLadder : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
