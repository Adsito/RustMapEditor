using UnityEngine;

[ExecuteAlways]
public class CargoShip : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
