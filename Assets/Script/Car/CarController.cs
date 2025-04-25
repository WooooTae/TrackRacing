using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState
{
    Neutral,
    Drive,
    CheckingChange,
    Changing
};

public class CarController : MonoBehaviour
{
    //rpm(엔진 회전수),torque(페달 밟는 힘),horsepower(빠르게 달리는 힘)

    public Rigidbody rb;

    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    public MeshRenderer frontLeftMesh;
    public MeshRenderer frontRightMesh;
    public MeshRenderer rearLeftMesh;
    public MeshRenderer rearRightMesh;

    public TrailRenderer rearLeftTrail;
    public TrailRenderer rearRightTrail;

    public GameObject tireTrail;

    public float motorPower;
    public float brakePower;
    public float slipAngle;
    public float speed;
    public float maxSpeed;
    public float RPM;
    public float redLine;
    public float idleRPM;
    public float[] gearRatios;
    public float differntialRatio;
    public float ChangeGearTime = 0.5f;

    public SpeedUI speedMeter;
    public SettingManager settingManager;

    private bool isbrake = false;
    private bool isDrift = false;

    public int currentGear;
    public int isEngineRunning;

    public AnimationCurve steeringCurve;
    public AnimationCurve hpToRPMCurve; // rpm에 따른 마력 계산

    private float engineInput;
    private float brakeInput;
    private float steeringInput;
    private float currentTorque;
    private float clutch;
    private float wheelRPM;
    private float speedClamped;
    private float downRPM;
    private float upRPM;

    private GearState gearState;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Application.targetFrameRate = 60;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        InitiateTrails();
    }

    void Update()
    {
        speed = rb.velocity.magnitude * 3.6f;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);

        SettingOpen();
        CheckInput();
        CheckTrails();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        ApplyWheelPosition();
    }

    void InitiateTrails()
    {
        if (tireTrail)
        {
            rearLeftTrail = Instantiate(tireTrail, rearLeftCollider.transform.position - Vector3.up * rearLeftCollider.radius, Quaternion.identity, rearLeftCollider.transform).GetComponent<TrailRenderer>();
            rearRightTrail = Instantiate(tireTrail, rearRightCollider.transform.position - Vector3.up * rearRightCollider.radius, Quaternion.identity, rearRightCollider.transform).GetComponent<TrailRenderer>();
        }
    }

    void SettingOpen()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isActive = settingManager.gameObject.activeSelf;
            settingManager.gameObject.SetActive(!isActive);
            Time.timeScale = isActive ? 1f : 0f;
        }
    }

    void CheckInput()
    {
        engineInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        slipAngle = Vector3.Angle(transform.forward, rb.velocity.normalized);

        isDrift = Input.GetKey(KeyCode.LeftShift);

        float movingDirection = Vector3.Dot(transform.forward, rb.velocity);

        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if (Mathf.Abs(engineInput) > 0f)
                {
                    StartCoroutine(StartEngine());
                    gearState = GearState.Drive;
                }
            }
            else
            {
                clutch = isDrift ? 0f : Mathf.MoveTowards(clutch, 1f, Time.deltaTime * 5f);
            }
        }
        else
        {
            clutch = 0;
        }

        isbrake = Input.GetKey(KeyCode.Space);

        brakeInput = isbrake ? 1 : 0;
    }

    void CheckTrails()
    {
        if (isDrift)
        {
            rearLeftTrail.emitting = true;
        }
        else
        {
            rearLeftTrail.emitting = false;
        }

        if (isDrift)
        {
            rearRightTrail.emitting = true;
        }
        else
        {
            rearRightTrail.emitting = false;
        }
    }

    void ApplyBrake()
    {
        frontRightCollider.brakeTorque = brakeInput * brakePower * 0.7f;
        frontLeftCollider.brakeTorque = brakeInput * brakePower * 0.7f;

        rearRightCollider.brakeTorque = brakeInput * brakePower * 0.3f;
        rearLeftCollider.brakeTorque = brakeInput * brakePower * 0.3f;

        if (isbrake)
        {
            clutch = 0;
            rearRightCollider.brakeTorque = brakeInput * brakePower * 1000f;
            rearLeftCollider.brakeTorque = brakeInput * brakePower * 1000f;
        }
    }

    void ApplyMotor()
    {
        currentTorque = CalculateTorque();
        rearRightCollider.motorTorque = currentTorque * engineInput;
        rearLeftCollider.motorTorque = currentTorque * engineInput;
    }

    void ApplySteering()
    {
        float steeringAngle = steeringInput * steeringCurve.Evaluate(speed);
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, rb.velocity + transform.forward, Vector3.up);
        }

        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);
        frontRightCollider.steerAngle = steeringAngle;
        frontLeftCollider.steerAngle = steeringAngle;
    }

    void ApplyWheelPosition()
    {
        UpdateWheel(frontRightCollider, frontRightMesh);
        UpdateWheel(frontLeftCollider, frontLeftMesh);
        UpdateWheel(rearRightCollider, rearRightMesh);
        UpdateWheel(rearLeftCollider, rearLeftMesh);
    }

    void UpdateWheel(WheelCollider col,MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        col.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }

    float CalculateTorque()
    {
        float torque = 0f;

        if (RPM < idleRPM + 200 && engineInput == 0 && currentGear == 0)
        {
            gearState = GearState.Neutral;
        }

        downRPM = redLine * 0.25f * (1f - currentGear / (float)(gearRatios.Length - 1));
        upRPM = redLine * (0.5f + 0.4f * (currentGear / (float)(gearRatios.Length - 1)));

        if (gearState == GearState.Drive && clutch > 0)
        {
            if (RPM > upRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (RPM < downRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }

        if (isEngineRunning > 0)
        {
            if (clutch < 0.1f)
            {
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * engineInput) + Random.Range(-50,50), Time.deltaTime);
            }
            else
            {
                //마력(Hp) = (torque * rpm) / 5252(단위 변환 때문에 나눔) ,마력 = 힘 * 속도
                wheelRPM = Mathf.Abs((rearRightCollider.rpm + rearLeftCollider.rpm) / 2f) * gearRatios[currentGear] * differntialRatio;
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
                torque = (motorPower / RPM) * (hpToRPMCurve.Evaluate(RPM / redLine)) * gearRatios[currentGear] * differntialRatio * clutch * 5252f;
            }
        }
        return torque;
    }

    IEnumerator StartEngine()
    {
        isEngineRunning = 1;
        yield return new WaitForSeconds(0.6f);
        isEngineRunning = 2;
        yield return new WaitForSeconds(0.4f);
    }

    IEnumerator ChangeGear(int addgear)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + addgear >= 0)
        {
            if (addgear > 0)
            {
                yield return new WaitForSeconds(0.7f);
                if (RPM < upRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Drive;
                    yield break;
                }
            }
            else if (addgear < 0)
            {
                yield return new WaitForSeconds(0.7f);
                if (RPM > downRPM || currentGear <= 0)
                {
                    gearState = GearState.Drive;
                    yield break;
                }
            }

            gearState = GearState.Changing;
            yield return new WaitForSeconds(ChangeGearTime);
            currentGear += addgear;
        }
      
        if (gearState != GearState.Neutral)
        {
            gearState = GearState.Drive;
        }
    }

    public float GetSpeedRatio()
    {
        return RPM * engineInput / redLine;
    }
}

