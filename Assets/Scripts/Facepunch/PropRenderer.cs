using UnityEngine;

namespace Rust.Global.Rust
{
    [ExecuteAlways]
    public class PropRenderer : MonoBehaviour
    {
        protected void Awake()
        {
            DestroyImmediate(this);
        }
    }
}