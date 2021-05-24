using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 cameraOffset;
    [SerializeField] float rotationSpeed = 1f;
    [SerializeField] float minFov = 15f;
    [SerializeField] float maxFov = 90f;
    [SerializeField] float sensitivity = 10f;

    void LateUpdate()
    {

        if (Input.GetMouseButton(2))
        {
            Quaternion turnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, transform.up);
            cameraOffset = turnAngle * cameraOffset;
            turnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeed, -transform.right);
            cameraOffset = turnAngle * cameraOffset;
        }

        {
            float fov = Camera.main.fieldOfView;
            fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
            fov = Mathf.Clamp(fov, minFov, maxFov);
            Camera.main.fieldOfView = fov;
        }

        transform.position = target.position + cameraOffset;
        transform.LookAt(target);
    }
}
