using UnityEngine;

[ExecuteAlways]
public class DeployableDecay : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
