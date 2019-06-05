using UnityEngine;

[ExecuteAlways]
public class BearTrapTrigger : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
