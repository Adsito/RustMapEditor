using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnmoveablePrefab : MonoBehaviour {

    Vector3 position;
    Quaternion rotation;
    Vector3 scale;
    float lastTime = 0;
    float timer = 1.5f; //500 ms

    // Use this for initialization
    void Start ()
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
        scale = transform.localScale;
    }
	
	void Update () {
        if(Time.time > lastTime + timer)
        {
            if (position.x != transform.localPosition.x || position.z != transform.localPosition.z)
            {
                transform.localPosition = position;
            }

            if (rotation.x != transform.localRotation.x || rotation.z != transform.localRotation.z)
            {
                transform.localRotation = rotation;
            }

            if (scale.x != transform.localScale.x || scale.z != transform.localScale.z)
            {
                transform.localScale = scale;
            }
            lastTime = Time.time;
        }
    }
}
