using UnityEngine;

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
                foreach (Material material in renderer.materials)
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
