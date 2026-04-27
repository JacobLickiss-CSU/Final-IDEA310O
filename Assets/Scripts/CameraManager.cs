using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CameraManager : MonoBehaviour
{
    public float LookSensitivity = 0.6f;

    public float MinAngle = -80f;

    public float MaxAngle = 80f;

    public bool VerticalInverted = false;

    public GameObject CameraFocus;

    public GameObject CameraPivot;

    public GameObject CameraTarget;

    private InputAction lookAction;

    private float verticalLookAngle = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupInputs();
    }

    void SetupInputs()
    {
        lookAction = InputSystem.actions.FindAction("Look");
    }

    // Update is called once per frame
    void Update()
    {
        if(CanLook())
        {
            ManageInput();
        }

        MoveToTarget();
    }

    bool CanLook()
    {
        if(PlayerManager.Instance != null && PlayerManager.Instance.CanLook())
        {
            return true;
        }

        return false;
    }

    void ManageInput()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>();

        CameraFocus.transform.Rotate(0, lookValue.x * LookSensitivity, 0);

        verticalLookAngle += lookValue.y * LookSensitivity * (VerticalInverted ? -1 : 1);
        verticalLookAngle = Mathf.Clamp(verticalLookAngle, MinAngle, MaxAngle);
        CameraPivot.transform.localRotation = Quaternion.Euler(verticalLookAngle, 0f, 0f);
    }

    void MoveToTarget()
    {
        Vector3 cameraDirection = CameraTarget.transform.position - CameraPivot.transform.position;

        RaycastHit hit;
        if (Physics.Raycast(CameraPivot.transform.position, cameraDirection, out hit, cameraDirection.magnitude))
        {
            if (hit.collider.gameObject.tag != "Player" && hit.collider.gameObject.tag != "Enemy")
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = CameraTarget.transform.position;
            }
        }
        else
        {
            transform.position = CameraTarget.transform.position;
        }
    }

    public Vector3 GetForward()
    {
        return CameraTarget.transform.forward;
    }

    public Vector3 GetRight()
    {
        return CameraTarget.transform.right;
    }
}
