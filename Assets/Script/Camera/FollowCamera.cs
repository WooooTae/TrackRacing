using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}
