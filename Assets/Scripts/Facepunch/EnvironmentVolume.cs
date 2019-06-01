using UnityEngine;

[ExecuteAlways]
public class EnvironmentVolume : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}