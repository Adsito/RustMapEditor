using UnityEngine;

[ExecuteAlways]
public class MeshCull : MonoBehaviour
{
    private Renderer[] renderers;
    private Shader standard;
    private Shader specular;
    protected void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        standard = Shader.Find("Standard");
        specular = Shader.Find("Standard (Specular setup)");
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
                            if (material.shader.name.Contains("Specular"))
                            {
                                material.shader = specular;
                            }
                            else
                            {
                                material.shader = standard;
                            }
                        }
                    }
                }
            }
        }
    }
}
