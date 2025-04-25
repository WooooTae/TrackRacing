using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarInput : MonoBehaviour
{
    public bool isPressed;

    public float danpenPress = 0;
    public float ssensitivity = 2f;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    void SetInput()
    {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
    }
}
