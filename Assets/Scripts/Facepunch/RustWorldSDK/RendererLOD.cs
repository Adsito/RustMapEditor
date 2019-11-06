using System;
using UnityEngine;
using UnityEngine.Rendering;

public class RendererLOD : MonoBehaviour
{
	[Serializable]
	public class State
	{
		public float distance;

		public Renderer renderer;

		[NonSerialized]
		public MeshFilter filter;

		[NonSerialized]
		public ShadowCastingMode shadowMode;

		[NonSerialized]
		public bool isImpostor;
	}
	public State[] States;

	private int curlod;

	private bool force;

    public void SetLODS()
    {
        if (gameObject.GetComponent<LODGroup>() == null)
        {
            var lodGroup = gameObject.AddComponent<LODGroup>();
            var statesLength = States.Length - 1;
            LOD[] lods = new LOD[statesLength];
            for (int i = 0; i < statesLength; i++)
            {
                if (States[i].renderer != null)
                {
                    States[i].renderer.enabled = true;
                }
                lods[i] = new LOD(1.0F / (i + 5f), new Renderer[] { States[i].renderer });
            }
            lodGroup.SetLODs(lods);
            lodGroup.fadeMode = LODFadeMode.SpeedTree;
            lodGroup.RecalculateBounds();
        }
    }
}
