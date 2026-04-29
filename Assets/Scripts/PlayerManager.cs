using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public CameraManager Camera;

    public GameObject model;

    private InputAction moveAction;

    private InputAction sprintAction;

    private InputAction attackLightAction;

    private InputAction attackHeavyAction;

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

    public float attackLightTime = 0.75f;

    public float attackLightHoldTime = 0.4f;

    public float attackLightActiveStart = 7f / 24f;

    public float attackLightActiveEnd = 14f / 24f;

    public float attackLightMoveSpeed = 2f;

    public float attackLightMoveStart = 7f / 24f;

    public float attackLightMoveEnd = 12f / 24f;

    public float attackHeavyTime = 1.5f;

    public float attackHeavyHoldTime = 0.9f;

    public float attackHeavyActiveStart = 16f / 24f;

    public float attackHeavyActiveEnd = 24f / 24f;

    public float attackHeavyMoveSpeed = 2f;

    public float attackHeavyMoveStart = 14f / 24f;

    public float attackHeavyMoveEnd = 19f / 24f;

    private AttackIndex attackIndex;

    private float attackTimer = 0f;
    public bool IsAttackActive
    {
        get
        {
            if(State == PlayerState.AttackingLight)
            {
                return attackTimer >= attackLightActiveStart && attackTimer <= attackLightActiveEnd;
            }
            if(State == PlayerState.AttackingHeavy)
            {
                return attackTimer >= attackHeavyActiveStart && attackTimer <= attackHeavyActiveEnd;
            }
            return false;
        }
    }

    public float AttackTime
    {
        get
        {
            if (State == PlayerState.AttackingLight)
            {
                return attackLightTime + attackLightHoldTime;
            }
            if (State == PlayerState.AttackingHeavy)
            {
                return attackHeavyTime + attackHeavyHoldTime;
            }
            return 0f;
        }
    }

    public bool IsAttackHolding
    {
        get
        {
            if (State == PlayerState.AttackingLight)
            {
                return attackTimer >= attackLightTime && attackTimer <= AttackTime;
            }
            if (State == PlayerState.AttackingHeavy)
            {
                return attackTimer >= attackHeavyTime && attackTimer <= AttackTime;
            }
            return false;
        }
    }

    public bool IsAttackMoving
    {
        get
        {
            if (State == PlayerState.AttackingLight)
            {
                return attackTimer >= attackLightMoveStart && attackTimer <= attackLightMoveEnd;
            }
            if (State == PlayerState.AttackingHeavy)
            {
                return attackTimer >= attackHeavyMoveStart && attackTimer <= attackHeavyMoveEnd;
            }
            return false;
        }
    }

    public float AttackMoveSpeed
    {
        get
        {
            if (State == PlayerState.AttackingLight)
            {
                return attackLightMoveSpeed;
            }
            if (State == PlayerState.AttackingHeavy)
            {
                return attackHeavyMoveSpeed;
            }
            return 0f;
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
        attackLightAction = InputSystem.actions.FindAction("AttackLight");
        attackHeavyAction = InputSystem.actions.FindAction("AttackHeavy");
    }

    // Update is called once per frame
    void Update()
    {
        HandleAttacking();
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
        if(State == PlayerState.Ready || IsAttackHolding)
        {
            if (sprintAction.IsPressed())
            {
                sprintTime += Time.deltaTime;
            }
            else
            {
                if (sprintTime > 0 && sprintTime < sprintHold)
                {
                    StartRoll();
                }
                sprintTime = 0;
            }
        }
        else
        {
            sprintTime = 0;
        }
    }

    void HandleMovement()
    {
        if (IsAttackHolding) return;

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (moveValue.magnitude > 0 && CanMove())
        {
            if(IsAttackHolding)
            {
                StopAttacking();
            }

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

            UpdateTargetFacing();

            float speed = 0f;
            switch(mode)
            {
                case MovementMode.Run: { speed = runSpeed; break; }
                case MovementMode.Walk: { speed = walkSpeed; break; }
                case MovementMode.Sprint: { speed = sprintSpeed; break; }
                default: { speed = walkSpeed; break; }
            }

            Vector3 finalMove = new Vector3(targetFacing.x, 0, targetFacing.y) * speed;

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

    void UpdateTargetFacing()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (moveValue.magnitude > 0)
        {
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
        }
    }

    void StartRoll()
    {
        if(IsAttackHolding)
        {
            StopAttacking();
        }

        State = PlayerState.Rolling;
        LookTowardsFacing(true, true);
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

    void LookTowardsFacing(bool immediate = false, bool getFacing = false)
    {
        if (getFacing)
        {
            UpdateTargetFacing();
        }

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
        return State == PlayerState.Ready || IsAttackHolding;
    }

    void HandleAttacking()
    {
        CheckForAttack();

        if (IsAttacking())
        {
            ContinueAttacking();
        }
    }

    void CheckForAttack()
    {
        if (CanMove())
        {
            if (attackLightAction.IsPressed())
            {
                StartAttacking(PlayerState.AttackingLight);
                return;
            }
            else if (attackHeavyAction.IsPressed())
            {
                StartAttacking(PlayerState.AttackingHeavy);
                return;
            }
        }
    }

    void StartAttacking(PlayerState attackType)
    {
        if(attackType != PlayerState.AttackingLight && attackType != PlayerState.AttackingHeavy)
        {
            return;
        }

        if (IsAttackHolding)
        {
            attackTimer = 0;
        }

        State = attackType;
        LookTowardsFacing(true, true);

        attackIndex = GetNextAttackIndex(attackIndex, attackType);
        modelAnimator.Play(Animator.StringToHash(attackIndex.GetAnimation()), 0, 0);
    }

    AttackIndex GetNextAttackIndex(AttackIndex currentIndex, PlayerState attackType)
    {
        if(attackType == PlayerState.AttackingLight)
        {
            switch (attackIndex)
            {
                case AttackIndex.None: return AttackIndex.AttackLight1;
                case AttackIndex.AttackLight1: return AttackIndex.AttackLight2;
                case AttackIndex.AttackLight2: return AttackIndex.AttackLight1;
                case AttackIndex.AttackHeavy: return AttackIndex.AttackLight2;
            }
        }

        if (attackType == PlayerState.AttackingHeavy)
        {
            switch (attackIndex)
            {
                case AttackIndex.None: return AttackIndex.AttackHeavy;
                case AttackIndex.AttackLight1: return AttackIndex.AttackHeavy;
                case AttackIndex.AttackLight2: return AttackIndex.AttackHeavy;
                case AttackIndex.AttackHeavy: return AttackIndex.AttackHeavy;
            }
        }

        return AttackIndex.None;
    }

    void ContinueAttacking()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= AttackTime)
        {
            StopAttacking();
        }
        else if(IsAttackHolding)
        {
            CheckForAttack();
        }
        else
        {
            if (IsAttackMoving)
            {
                Vector3 finalMove = new Vector3(targetFacing.x, 0, targetFacing.y) * AttackMoveSpeed;
                finalMove.y += gravityValue;
                GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);
            }
        }
    }

    void StopAttacking(PlayerState endState = PlayerState.Ready)
    {
        attackTimer = 0;
        State = endState;
        attackIndex = AttackIndex.None;
        modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Idle"));
    }

    public bool IsAttacking()
    {
        return State == PlayerState.AttackingLight || State == PlayerState.AttackingHeavy;
    }
}

public enum PlayerState
{
    Ready,
    AttackingLight,
    AttackingHeavy,
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

public enum AttackIndex
{
    None,
    AttackLight1,
    AttackLight2,
    AttackHeavy
}

public static class AttackIndexExtensions
{
    public static string GetAnimation(this AttackIndex index)
    {
        switch (index)
        {
            case AttackIndex.None: return "RobogirlArmature|Idle";
            case AttackIndex.AttackLight1: return "RobogirlArmature|AttackLight";
            case AttackIndex.AttackLight2: return "RobogirlArmature|AttackLight2";
            case AttackIndex.AttackHeavy: return "RobogirlArmature|AttackHeavy";
            default: return "RobogirlArmature|Idle";
        }
    }
}