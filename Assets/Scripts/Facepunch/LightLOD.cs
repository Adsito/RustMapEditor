using System.Collections.Generic;
using UnityEngine;

public class LightLOD : MonoBehaviour
{
	private Light lightComponent;

	protected void Awake()
	{
		lightComponent = GetComponent<Light>();
        lightComponent.enabled = false;
	}

	private void ToggleLight(bool state)
	{
        lightComponent.enabled = state;
    }
}
