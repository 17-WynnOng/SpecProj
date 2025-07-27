using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveText : MonoBehaviour
{
    private Transform cameraTransform;
    void Start()
    {
        // Use the main camera as the target to face
        cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Make this object face the camera
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}
