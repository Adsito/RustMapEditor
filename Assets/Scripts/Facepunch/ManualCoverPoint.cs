using UnityEngine;

namespace Rust.Ai
{
    [ExecuteAlways]
    public class ManualCoverPoint : MonoBehaviour
    {
        protected void Awake()
        {
            DestroyImmediate(this);
        }
    }
}
