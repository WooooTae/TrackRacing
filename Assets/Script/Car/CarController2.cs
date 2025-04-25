using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CarController2 : MonoBehaviour
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

    //조향 관련
    private float currentSteerAngle;

    //자동 변속 관련
    public float[] gearRatios = { 3.0f, 2.2f, 1.6f, 1.1f, 0.8f };
    private int currentGear = 0;

    public float shiftUpRpm = 5000f;
    public float shiftDownRpm = 1500f;
    public float engineRpm;
    private float shiftDelay = 1.0f;
    private float lastShiftTime;

    public Rigidbody rb;

    //드리프트 관련
    public TrailRenderer leftSkid;
    public TrailRenderer rightSkid;

    private bool isDrift;
    private bool isDrifted;

    private float baseFriction = 1.0f;
    private float maxDriftFriction = 0.5f;

    // UI관련
    private SpeedUI speedUI;

    //부스터 관련
    public ParticleSystem boostEffect;
    public float boostDuration = 2f;
    private float boostTimer;

    private bool isBoost;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        speedUI = GetComponentInChildren<SpeedUI>();

        isBoost = false;
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
        isBrake = Input.GetKey(KeyCode.Space);
        isDrift = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCoroutine(BoostCoroutine());
        }

        //speedUI?.UpdateSpeedUI(GetSpeedKmh());
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
        Dirft();
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

    private IEnumerator BoostCoroutine()
    {
        if (isBoost) yield break;
        Debug.Log("Boost");

        isBoost = true;

        float originalForce = motorForce;

        if (boostEffect != null)
        {
            boostEffect.Play();
        }

        float boostEndTime = Time.time + boostDuration;
        while (Time.time < boostEndTime)
        {
            float speedIncrease = 700f; // 원하는 힘 수치 조정
            rb.AddForce(transform.forward * speedIncrease * Time.deltaTime, ForceMode.Acceleration);

            //float speedIncrease = 20f * Time.deltaTime;  

            //// 150km/h를 초과하지 않도록 제한
            //if (rb.velocity.magnitude < 41.6f) 
            //{
            //     rb.velocity += transform.forward * speedIncrease;
            //}

            yield return null;
        }

        motorForce = originalForce;
        isBoost = false;

        if (boostEffect != null)
        {
            boostEffect.Stop();
        }
    }

    private void GearShifting()
    {
        if (isBoost) return;

        float averageRpm = (frontLeftCollider.rpm + frontRightCollider.rpm + rearLeftCollider.rpm + rearRightCollider.rpm) / 4f;
        engineRpm = Mathf.Abs(averageRpm * gearRatios[currentGear]);

        if (Time.time - lastShiftTime < shiftDelay)
            return;

        if (engineRpm > shiftUpRpm && currentGear < gearRatios.Length - 1)
        {
            currentGear++;
            lastShiftTime = Time.time;
        }
        else if (engineRpm < shiftDownRpm && currentGear > 0)
        {
            currentGear--;
            lastShiftTime = Time.time;
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

    private void Dirft()
    {
        if (isDrift != isDrifted)
        {
            EnableSkid(isDrift);
            isDrifted = isDrift;
        }

        float speed = GetSpeedKmh();
        float steerAmount = Mathf.Abs(steerInput);

        if (isDrift && steerAmount > 0.2f && speed > 25f)
        {
            float driftFactor = Mathf.Clamp01(speed / 120f);

            float driftRearFriction = Mathf.Lerp(baseFriction, maxDriftFriction, driftFactor);
            float driftFrontFriction = Mathf.Lerp(baseFriction, 0.8f, driftFactor); // 전륜도 살짝 줄임

            SetWheelFriction(rearLeftCollider, driftRearFriction);
            SetWheelFriction(rearRightCollider, driftRearFriction);
            SetWheelFriction(frontLeftCollider, driftFrontFriction);
            SetWheelFriction(frontRightCollider, driftFrontFriction);

            // 측면 드리프트 방향력 추가
            Vector3 driftDirection = Quaternion.AngleAxis(steerInput * 25f, Vector3.up) * rb.velocity.normalized;
            rb.AddForce(driftDirection * 150f * Time.deltaTime, ForceMode.Acceleration);

            // 회전력 추가 (토크)
            float driftTorque = steerInput * speed * 0.8f;
            rb.AddTorque(transform.up * driftTorque);
        }
        else
        {
            SetWheelFriction(rearLeftCollider, baseFriction);
            SetWheelFriction(rearRightCollider, baseFriction);
            SetWheelFriction(frontLeftCollider, baseFriction);
            SetWheelFriction(frontRightCollider, baseFriction);
        }
    }

    private void SetWheelFriction(WheelCollider wheel, float stiffness)
    {
        WheelFrictionCurve friction = wheel.sidewaysFriction;
        friction.stiffness = stiffness;
        wheel.sidewaysFriction = friction;
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
