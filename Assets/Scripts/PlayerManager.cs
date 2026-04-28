using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public CameraManager Camera;

    public GameObject model;

    private InputAction moveAction;

    private InputAction sprintAction;

    public Animator modelAnimator;

    public float gravityValue = -9.81f;

    public float walkSpeed = 2f;

    public float runSpeed = 5f;

    public float sprintSpeed = 9f;

    public float turnSpeed = 1f;

    public float walkThreshhold = .5f;

    public float sprintHold = .75f;

    public PlayerState State { get; private set; } = PlayerState.Ready;

    private float sprintTime = 0f;
    public bool IsSprinting {
        get {
            return sprintTime >= sprintHold;
        }
    }

    public float rollSpeed = 7f;

    public float rollTime = 1.0f;

    public float rollIStart = .25f;

    public float rollIEnd = .85f;

    private float rollTimer = 0f;
    public bool IsRollInvincible {
        get {
            return rollTimer >= rollIStart && rollTimer <= rollIEnd;
        }
    }

    private Vector2 targetFacing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetFacing = new Vector2(model.transform.forward.x, model.transform.forward.z);

        SetupInputs();
    }

    void SetupInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    // Update is called once per frame
    void Update()
    {
        CheckSprinting();
        HandleMovement();
        LookTowardsFacing();

        if(State == PlayerState.Rolling)
        {
            ContinueRoll();
        }
    }

    void CheckSprinting()
    {
        if (sprintAction.IsPressed())
        {
            sprintTime += Time.deltaTime;
        }
        else
        {
            if(sprintTime > 0 && sprintTime < sprintHold)
            {
                StartRoll();
            }
            sprintTime = 0;
        }
    }

    void HandleMovement()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (moveValue.magnitude > 0 && CanMove())
        {
            MovementMode mode = MovementMode.Run;
            if(moveValue.magnitude < walkThreshhold)
            {
                mode = MovementMode.Walk;
            }
            if(IsSprinting)
            {
                mode = MovementMode.Sprint;
            }

            // TODO move somewhere more organized
            switch (mode)
            {
                case MovementMode.Run: { modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Run")); break; }
                case MovementMode.Walk: { modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Walk")); break; }
                case MovementMode.Sprint: { modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Sprint")); break; }
                default: { modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Run")); break; }
            }

            Vector3 cameraForward = Camera.GetForward();
            Vector3 cameraRight = Camera.GetRight();

            Vector2 moveForward = new Vector2(cameraForward.x, cameraForward.z);
            moveForward.Normalize();

            Vector2 moveRight = new Vector2(cameraRight.x, cameraRight.z);
            moveRight.Normalize();

            Vector2 forwardMovement = moveValue.y * moveForward;
            Vector2 sideMovement = moveValue.x * moveRight;

            Vector2 totalMovement = forwardMovement + sideMovement;
            totalMovement.Normalize();

            targetFacing = totalMovement;

            float speed = 0f;
            switch(mode)
            {
                case MovementMode.Run: { speed = runSpeed; break; }
                case MovementMode.Walk: { speed = walkSpeed; break; }
                case MovementMode.Sprint: { speed = sprintSpeed; break; }
                default: { speed = walkSpeed; break; }
            }

            Vector3 finalMove = new Vector3(totalMovement.x, 0, totalMovement.y) * speed;

            // TODO fall even when not moving
            finalMove.y += gravityValue;

            GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);
        }
        else if(State == PlayerState.Ready)
        {
            // TODO move somewhere more organized
            modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Idle"));
        }
    }

    void StartRoll()
    {
        State = PlayerState.Rolling;
        LookTowardsFacing(true);
        modelAnimator.speed = 1.5f;
        modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Roll"));
    }

    void ContinueRoll()
    {
        rollTimer += Time.deltaTime;

        if(rollTimer >= rollTime)
        {
            StopRoll();
        }
        else
        {
            Vector3 finalMove = new Vector3(targetFacing.x, 0, targetFacing.y) * rollSpeed;
            finalMove.y += gravityValue;
            GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);
        }
    }

    void StopRoll(PlayerState endState = PlayerState.Ready)
    {
        rollTimer = 0;
        State = endState;
        modelAnimator.speed = 1.0f;
    }

    void LookTowardsFacing(bool immediate = false)
    {
        Vector3 lookDirection = new Vector3(targetFacing.x, 0, targetFacing.y);
        if(immediate)
        {
            model.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(lookDirection), turnSpeed * Time.deltaTime);
        }
            
    }

    public bool CanLook()
    {
        // TODO
        return Cursor.lockState == CursorLockMode.Locked && !Cursor.visible;
        //return true;
    }

    public bool CanMove()
    {
        return State == PlayerState.Ready;
    }
}

public enum PlayerState
{
    Ready,
    Attacking,
    Hit,
    Rolling,
    Falling,
    Dead,
    Frozen
}

public enum MovementMode
{
    Walk,
    Run,
    Sprint,
    Blocking
}
