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
    public float brakeForce = 3000f;

    private float steerInput;
    private float motorInput;
    private float currentBrakeForce;
    private bool isBrake;

    //자동 변속 관련
    public float[] gearRatios = { 2.5f, 2.0f, 1.5f, 1.0f, 0.75f };
    private int currentGear = 0;

    public float shiftUpRpm = 4000f;
    public float shiftDownRpm = 1500f;
    public float engineRpm;

    public Rigidbody rb;

    //드리프트 관련
    public TrailRenderer leftSkid;
    public TrailRenderer rightSkid;

    private bool isDrift;
    private bool isDrifted;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
        isBrake = Input.GetKey(KeyCode.Space);
        isDrift = Input.GetKey(KeyCode.LeftShift);

    }

    private void LateUpdate()
    {

    }

    void FixedUpdate()
    {
        UpdateWheelVisual(frontLeftCollider, frontLeftTransform);
        UpdateWheelVisual(frontRightCollider, frontRightTransform);
        UpdateWheelVisual(rearLeftCollider, rearLeftTransform);
        UpdateWheelVisual(rearRightCollider, rearRightTransform);

        HandleSteer();
        HandleMotor();
        ApplyBrake();
        GearShifting();

        if (isDrift != isDrifted)
        {
            if (isDrift)
            {
                SetDriftFriction(0.7f); 
                EnableSkid(true); 
            }
            else
            {
                SetDriftFriction(1.0f); 
                EnableSkid(false); 
            }

            isDrifted = isDrift; 
        }
    }

    private void HandleSteer()
    {
        frontLeftCollider.steerAngle = maxSteerAngle * steerInput;
        frontRightCollider.steerAngle = maxSteerAngle * steerInput;
    }

    private void HandleMotor()
    {
        frontLeftCollider.motorTorque = motorForce * motorInput;
        frontRightCollider.motorTorque = motorForce * motorInput;
        rearLeftCollider.motorTorque = motorForce * motorInput;
        rearRightCollider.motorTorque = motorForce * motorInput;
    }

    private void ApplyBrake()
    {
        currentBrakeForce = isBrake ? brakeForce : 0f;

        frontLeftCollider.brakeTorque = currentBrakeForce;
        frontRightCollider.brakeTorque = currentBrakeForce;
        rearLeftCollider.brakeTorque = currentBrakeForce;
        rearRightCollider.brakeTorque = currentBrakeForce;
    }

    private void GearShifting()
    {
        float averageRpm = (frontLeftCollider.rpm + frontRightCollider.rpm + rearLeftCollider.rpm + rearLeftCollider.rpm) / 4f;
        engineRpm = Mathf.Abs(averageRpm * gearRatios[currentGear]);

        if (engineRpm > shiftUpRpm && currentGear < gearRatios.Length - 1)
        {
            currentGear++;
        }
        else if (engineRpm < shiftDownRpm && currentGear > 0)
        {
            currentGear--;
        }
    }

    private void UpdateWheelVisual(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private bool IsDrifting()
    {
        return isDrift && Mathf.Abs(steerInput) > 0.3f && GetSpeedKmh() > 20f;
    }

    private void SetDriftFriction(float stiffness)
    {
        WheelFrictionCurve sidewaysFriction;

        sidewaysFriction = rearLeftCollider.sidewaysFriction;
        sidewaysFriction.stiffness = stiffness;
        rearLeftCollider.sidewaysFriction = sidewaysFriction;

        sidewaysFriction = rearRightCollider.sidewaysFriction;
        sidewaysFriction.stiffness = stiffness;
        rearRightCollider.sidewaysFriction = sidewaysFriction;

        Debug.Log("Dirfting");
    }

    private void EnableSkid(bool enable)
    {
        if (leftSkid != null)
        {
            leftSkid.emitting = enable;
        }
        if (rightSkid != null)
        {
            rightSkid.emitting = enable;
        }
    }

    public int GetCurrentGear() => currentGear + 1;
    public float GetEngineRPM() => engineRpm;
    public float GetSpeedKmh() => rb.velocity.magnitude * 3.6f;
}
