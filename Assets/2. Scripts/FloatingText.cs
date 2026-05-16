using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Awake()
    {
        if(Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if(mainCameraTransform == null) return;

        transform.rotation = mainCameraTransform.rotation;
    }
}
