using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0, 5, -10); 
    public float smoothSpeed = 0.125f; 

    private void LateUpdate()
    {
        Vector3 backDir = -target.forward;
        Vector3 desiredPosition = target.position + backDir * Mathf.Abs(offset.z) + Vector3.up * offset.y;

        RaycastHit hit;
        Vector3 rayDir = (desiredPosition - target.position).normalized;
        float rayDistance = Vector3.Distance(target.position, desiredPosition);

        if (Physics.Raycast(target.position, rayDir, out hit, rayDistance))
        {
            desiredPosition = hit.point - rayDir * 0.5f; 
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
