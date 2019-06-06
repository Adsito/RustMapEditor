using UnityEngine;

namespace Rust.Ai
{
    [ExecuteAlways]
    public class AiLocationSpawner : MonoBehaviour
    {
        protected void Awake()
        {
            DestroyImmediate(this);
        }
    }
}