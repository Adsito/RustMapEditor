using UnityEngine;

namespace Rust.Ai
{
    [ExecuteAlways]
    public class ScientistSpawner : MonoBehaviour
    {
        protected void Awake()
        {
            DestroyImmediate(this);
        }
    }
}