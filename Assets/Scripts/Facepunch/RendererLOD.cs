using UnityEngine;

[ExecuteAlways]
public class RendererLOD : MonoBehaviour
{
    private Renderer[] renderers;
    private Shader standard;
    protected void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        standard = Shader.Find("Standard");
        foreach (Renderer renderer in renderers)
        {
            if (renderer.enabled)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        if (material.shader != null)
                        {
                            if (material.shader.name.Contains("Blend"))
                            {
                                material.shader = standard;
                            }
                        }
                    }
                }
            }
        }
        DestroyImmediate(this);
    }
}
