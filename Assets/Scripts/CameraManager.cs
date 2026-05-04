using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CameraManager : MonoBehaviour
{
    public LockOn LockEffect;

    public float LookSensitivity = 0.6f;

    public float MinAngle = -80f;

    public float MaxAngle = 80f;

    public bool VerticalInverted = false;

    public GameObject CameraFocus;

    public GameObject CameraPivot;

    public GameObject CameraTarget;

    private InputAction lookAction;

    private InputAction cameraLockAction;

    private float verticalLookAngle = 0;

    public float LockMaxDistance = 15f;

    public GameObject LockTarget = null;

    private bool lockSwitched = false;

    public float LockLookSpeed = 10f;

    public float LockJoystickSensitivity = 0.5f;

    public float LockMouseSensitivity = 8f;

    public bool IsTargetLocked
    {
        get
        {
            CheckUnlock();
            return LockTarget != null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupInputs();
    }

    void SetupInputs()
    {
        lookAction = InputSystem.actions.FindAction("Look");
        cameraLockAction = InputSystem.actions.FindAction("CameraLock");
    }

    // Update is called once per frame
    void Update()
    {
        if(CanLook() && !IsTargetLocked)
        {
            ManageInput();
        }

        MoveToTarget();

        CheckSwitchLock();
        CheckLock();
        UseLock();
        UpdateLockEffect();
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

        AddVerticalAngle(lookValue.y);
    }

    void AddVerticalAngle(float angle)
    {
        verticalLookAngle += angle * LookSensitivity * (VerticalInverted ? -1 : 1);
        verticalLookAngle = Mathf.Clamp(verticalLookAngle, MinAngle, MaxAngle);
        CameraPivot.transform.localRotation = Quaternion.Euler(verticalLookAngle, 0f, 0f);
    }

    void SetVerticalAngle(float angle)
    {
        verticalLookAngle = angle;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle, MinAngle, MaxAngle);
        CameraPivot.transform.localRotation = Quaternion.Euler(verticalLookAngle, 0f, 0f);
    }

    void MoveToTarget()
    {
        Vector3 cameraDirection = CameraTarget.transform.position - CameraPivot.transform.position;

        RaycastHit hit;
        if (Physics.Raycast(CameraPivot.transform.position, cameraDirection, out hit, cameraDirection.magnitude))
        {
            if (hit.collider.gameObject.tag != "Player" && hit.collider.gameObject.tag != "Enemy" && hit.collider.gameObject.tag != "PlayerWeapon" && hit.collider.gameObject.tag != "InvisibleWall")
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

    public void CheckLock()
    {
        if (cameraLockAction.triggered && cameraLockAction.IsPressed())
        {
            if(IsTargetLocked)
            {
                LockTarget = null;
            }
            else
            {
                LockTarget = GetNewLockTarget();
            }
        }
    }

    public void CheckSwitchLock()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        // Value checking just under or far above 1 allow for both mouse and thumbstick use
        if (IsTargetLocked && ((Math.Abs(lookValue.x) > LockJoystickSensitivity && Math.Abs(lookValue.x) < 1f) || (Math.Abs(lookValue.x) > LockMouseSensitivity)))
        {
            if(!lockSwitched)
            {
                LockTarget = GetSwitchLockTarget(lookValue.x <= 0f);
                lockSwitched = true;
            }
        }
        else if(Math.Abs(lookValue.x) != 1f)
        {
            lockSwitched = false;
        }
    }

    public void CheckUnlock()
    {
        if(LockTarget != null)
        {
            if(GetCurrentLockDistance() > LockMaxDistance)
            {
                LockTarget = null;
                return;
            }

            if(LockTarget.GetComponent<Enemy>().State == EnemyState.Dead)
            {
                LockTarget = null;
                return;
            }

            //else
            //{
            //    Vector3 targetPosition = LockTarget.GetComponent<Enemy>().GetFocusPosition();
            //    Vector2 flatTargetPosition = new Vector2(targetPosition.x, targetPosition.z);
            //    Vector2 flatFocusPosition = new Vector2(CameraFocus.transform.position.x, CameraFocus.transform.position.z);
            //    Vector2 flatCameraPosition = new Vector2(transform.position.x, transform.position.z);

            //    if (Vector2.Distance(flatTargetPosition, flatFocusPosition) <
            //        Vector2.Distance(flatCameraPosition, flatFocusPosition)
            //    )
            //    {
            //        LockTarget = null;
            //    }
            //}
        }
    }

    public float GetCurrentLockDistance()
    {
        if(LockTarget == null) return 0f;

        return Vector3.Distance(LockTarget.transform.position, CameraPivot.transform.position);
    }

    public GameObject GetNewLockTarget()
    {
        Collider[] possibleTargets = Physics.OverlapSphere(CameraPivot.transform.position + (GetForward() * (LockMaxDistance / 2f)), LockMaxDistance);
        float bestMatch = -1;
        GameObject match = null;
        foreach (Collider possibleTarget in possibleTargets)
        {
            GameObject targetGameObject = possibleTarget.gameObject;
            if(targetGameObject.tag == "Enemy")
            {
                Vector3 targetPosition = targetGameObject.GetComponent<Enemy>().GetFocusPosition();
                float targetDistance = Vector3.Distance(targetPosition, CameraPivot.transform.position);
                float targetAngle = Vector3.Angle(GetForward(), targetPosition - CameraPivot.transform.position);
                bool targetOnScreen = IsOnScreen(targetPosition);

                float matchScore = (LockMaxDistance - targetDistance) / targetAngle;
                if(matchScore > 0 && matchScore > bestMatch && targetOnScreen)
                {
                    bestMatch = matchScore;
                    match = targetGameObject;
                }
            }
        }

        return match;
    }

    public GameObject GetSwitchLockTarget(bool left)
    {
        Collider[] possibleTargets = Physics.OverlapSphere(CameraPivot.transform.position + (GetForward() * (LockMaxDistance / 2f)), LockMaxDistance);
        float bestMatch = -1;
        GameObject match = LockTarget;
        foreach (Collider possibleTarget in possibleTargets)
        {
            GameObject targetGameObject = possibleTarget.gameObject;
            if (targetGameObject.tag == "Enemy" && targetGameObject.GetComponent<Enemy>().State != EnemyState.Dead)
            {
                if (targetGameObject == LockTarget) continue;

                Vector3 targetPosition = targetGameObject.GetComponent<Enemy>().GetFocusPosition();
                float targetDistance = Vector3.Distance(targetPosition, CameraPivot.transform.position);
                float targetAngle = Vector3.Angle(GetForward(), targetPosition - CameraPivot.transform.position);
                bool targetOnLeft = IsOnLeft(targetPosition);

                float matchScore = (LockMaxDistance - targetDistance) / targetAngle;
                if (matchScore > 0 && matchScore > bestMatch && targetAngle < 90 && targetOnLeft == left)
                {
                    bestMatch = matchScore;
                    match = targetGameObject;
                }
            }
        }

        return match;
    }

    public bool IsOnScreen(Vector3 toCheck)
    {
        Vector3 screenPosition = GetComponent<Camera>().WorldToViewportPoint(toCheck);
        return screenPosition.x >= 0 && screenPosition.x <= 1 && screenPosition.y >= 0 && screenPosition.y <= 1;
    }

    public bool IsOnLeft(Vector3 toCheck)
    {
        Vector3 screenPosition = GetComponent<Camera>().WorldToViewportPoint(toCheck);
        return screenPosition.x <= 0.5f;
    }

    public void UseLock()
    {
        if(IsTargetLocked)
        {
            Quaternion oldFocusFacing = CameraFocus.transform.rotation;
            Transform targetTransform = LockTarget.GetComponent<Enemy>().GetFocusTransform();
            CameraFocus.transform.LookAt(targetTransform);
            CameraFocus.transform.localEulerAngles = new Vector3(0, CameraFocus.transform.localEulerAngles.y, 0);
            Quaternion newFocusFacing = CameraFocus.transform.rotation;
            CameraFocus.transform.rotation = Quaternion.Lerp(oldFocusFacing, newFocusFacing, Time.deltaTime * LockLookSpeed);

            float flatDistance = Vector2.Distance(
                new Vector2(CameraPivot.transform.position.x, CameraPivot.transform.position.z),
                new Vector2(targetTransform.position.x, targetTransform.position.z)
            );
            float verticalDistance = targetTransform.position.y - CameraPivot.transform.position.y;
            float angle = Vector2.Angle(new Vector2(flatDistance, verticalDistance), new Vector2(flatDistance, 0));
            if (verticalDistance > 0) angle *= -1f;
            SetVerticalAngle(Mathf.Lerp(verticalLookAngle, angle, Time.deltaTime * LockLookSpeed));

            Vector2 newFacing = Vector2.Lerp(PlayerManager.Instance.GetFacing(), new Vector2(GetForward().x, GetForward().z), Time.deltaTime * LockLookSpeed * 2);
            PlayerManager.Instance.SetFacing(newFacing);

            //CameraPivot.transform.LookAt(targetTransform);
            //SetVerticalAngle(CameraPivot.transform.localEulerAngles.x);

            //CameraFocus.transform.LookAt(targetTransform);
            //CameraFocus.transform.localEulerAngles = new Vector3(0, CameraFocus.transform.localEulerAngles.y, 0);
        }
    }

    void UpdateLockEffect()
    {
        if (IsTargetLocked)
        {
            LockEffect?.Show();

            Vector3 targetPosition = LockTarget.GetComponent<Enemy>().GetFocusPosition();
            LockEffect?.SetPosition(targetPosition);
        }
        else
        {
            LockEffect?.Hide();
        }
    }
}
