using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    public TextMeshProUGUI SpeedText;

    public void UpdateSpeedUI(float speed)
    {
        if (SpeedText != null)
        {
            SpeedText.text = $"{speed:0} km/h";
        }
    }
}
