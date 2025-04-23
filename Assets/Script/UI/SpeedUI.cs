using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    public Rigidbody target;
    public float maxSpeed = 0.0f;

    private float speed;

    public float minAngle;
    public float maxAngle;

    [Header("UI")]
    public TextMeshProUGUI SpeedText;
    public RectTransform arrow;

    private void Update()
    {
        speed = target.velocity.magnitude * 3.6f;

        if (SpeedText != null)
        {
            SpeedText.text = $"{speed:0}km/h";
        }

        if (arrow != null)
        {
            arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minAngle, maxAngle, speed / maxSpeed));
        }
    }
}
