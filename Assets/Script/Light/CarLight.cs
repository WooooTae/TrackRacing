using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLight : MonoBehaviour
{
    public Rigidbody rb;
    public Light[] brakeLights;
    public float brakeThreshold = -1f; 

    void Update()
    {
        bool isBraking = Input.GetKey(KeyCode.Space); 
        bool isReversing = Input.GetKey(KeyCode.S);

        foreach (Light light in brakeLights)
        {
            light.enabled = isBraking || isReversing;
        }
    }
}
