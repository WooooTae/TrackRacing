using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    public float motorForce = 1500f;
    public float maxSteerAngle = 30f;

    private float steerInput;
    private float motorInput;

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");

        UpdateWheelVisual(frontLeftCollider, frontLeftTransform);
        UpdateWheelVisual(frontRightCollider, frontRightTransform);
        UpdateWheelVisual(rearLeftCollider, rearLeftTransform);
        UpdateWheelVisual(rearRightCollider, rearRightTransform);
    }

    void FixedUpdate()
    {
        frontLeftCollider.steerAngle = maxSteerAngle * steerInput;
        frontRightCollider.steerAngle = maxSteerAngle * steerInput;

        frontLeftCollider.motorTorque = motorForce * motorInput;
        frontRightCollider.motorTorque = motorForce * motorInput;
    }

    private void UpdateWheelVisual(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}
