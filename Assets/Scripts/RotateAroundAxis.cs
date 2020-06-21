using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    public Vector3 axis = Vector3.forward;
    public float rotationSpeed = 10.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.rotation = Quaternion.AngleAxis(Time.deltaTime * rotationSpeed, axis) * transform.rotation;
    }
}
