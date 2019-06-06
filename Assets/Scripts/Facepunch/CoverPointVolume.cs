using UnityEngine;

namespace Rust.Ai
{
    [ExecuteAlways]
    public class CoverPointVolume : MonoBehaviour
    {
        protected void Awake()
        {
            DestroyImmediate(this);
        }
    }
}
