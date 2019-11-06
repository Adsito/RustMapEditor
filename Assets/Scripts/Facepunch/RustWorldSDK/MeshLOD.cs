using System;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshLOD : MonoBehaviour
{
	[Serializable]
	public class State
	{
		public float distance;

		public Mesh mesh;
	}

	public State[] States;

	private MeshRenderer meshRenderer;

	private MeshFilter meshFilter;

	private ShadowCastingMode meshShadowMode;

	private int curlod;

	private bool force;

    public void SetLODS()
    {
        
    }
}
