using UnityEngine;

[ExecuteAlways]
public class HideIfOwnerFirstPerson : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
