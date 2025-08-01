using UnityEngine;

public class RotateModel : MonoBehaviour
{

    public Transform model;
    public float rotationSpeed = 50f;

    void Update()
    {
        model.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
