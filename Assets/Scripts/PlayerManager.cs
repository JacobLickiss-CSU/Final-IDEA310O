using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public CameraManager Camera;

    public GameObject model;

    private InputAction moveAction;

    private InputAction sprintAction;

    private InputAction walkAction;

    private InputAction attackLightAction;

    private InputAction attackHeavyAction;

    private InputAction blockAction;

    public Animator modelAnimator;

    public float gravityValue = -9.81f;

    public float walkSpeed = 2f;

    public float runSpeed = 5f;

    public float sprintSpeed = 9f;

    public float turnSpeed = 1f;

    public float walkThreshhold = .5f;

    public float walkTimeThreshhold = .25f;

    public float walkTimer = 0f;

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

    public int attackLightDamage = 15;

    public int attackHeavyDamage = 35;

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

    public long AttackId { get; internal set; }

    public int AttackDamage { get; internal set; }

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

    bool isBlocking = false;
    public bool IsBlocking
    {
        get
        {
            return isBlocking;
        }
    }

    public float StaggerTime = 2f;

    private float staggerTimer = 0f;

    private Vector2 targetFacing;

    private HashSet<Collider> overlappingWeapons = new HashSet<Collider>();

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
        walkAction = InputSystem.actions.FindAction("Walk");
        attackLightAction = InputSystem.actions.FindAction("AttackLight");
        attackHeavyAction = InputSystem.actions.FindAction("AttackHeavy");
        blockAction = InputSystem.actions.FindAction("Block");
    }

    // Update is called once per frame
    void Update()
    {
        HandleAttacking();
        CheckSprinting();
        HandleMovement();
        LookTowardsFacing();
        HandleBlocking();
        HandleStaggering();
        RegainPoise();
        CheckHit();

        if (State == PlayerState.Rolling)
        {
            ContinueRoll();
        }
    }

    void CheckSprinting()
    {
        if(State == PlayerState.Ready || IsAttackHolding)
        {
            if(Camera.IsTargetLocked)
            {
                if (sprintAction.triggered && sprintAction.IsPressed())
                {
                    StartRoll();
                }
            }
            else
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
        }
        else
        {
            sprintTime = 0;
        }
    }

    public void SetFacing(Vector2 facing, bool force = false)
    {
        if(State == PlayerState.Ready || force)
        {
            targetFacing = facing;
            LookTowardsFacing();
        }
    }

    public void FacePosition(Vector3 targetPosition)
    {
        Vector2 newFacing = new Vector2(targetPosition.x - transform.position.x, targetPosition.z - transform.position.z);
        newFacing.Normalize();
        targetFacing = newFacing;
    }

    public Vector2 GetFacing()
    {
        return targetFacing;
    }

    void HandleMovement()
    {
        if (IsAttackHolding) return;

        Vector3 finalMove = new Vector3(0,0,0);

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (moveValue.magnitude > 0 && CanMove())
        {
            if(IsAttackHolding)
            {
                StopAttacking();
            }

            MovementMode mode = MovementMode.Run;
            if(walkAction.IsPressed())
            {
                mode = MovementMode.Walk;
            }
            if(moveValue.magnitude < walkThreshhold)
            {
                if(walkTimer >= walkTimeThreshhold)
                {
                    mode = MovementMode.Walk;
                }
                else
                {
                    walkTimer += Time.deltaTime;
                    return;
                }
            }
            else
            {
                walkTimer = 0f;
            }

            if (IsSprinting)
            {
                mode = MovementMode.Sprint;
            }

            if(IsBlocking)
            {
                switch (mode)
                {
                    case MovementMode.Run: { mode = MovementMode.BlockingRun; break; }
                    case MovementMode.Walk: { mode = MovementMode.BlockingWalk; break; }
                    case MovementMode.Sprint: { mode = MovementMode.BlockingSprint; break; }
                }
            }

            // TODO move somewhere more organized
            if (!modelAnimator.IsInTransition(0))
            {
                switch (mode)
                {
                    case MovementMode.Run: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Run"), 0.2f); break; }
                    case MovementMode.Walk: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Walk"), 0.2f); break; }
                    case MovementMode.Sprint: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Sprint"), 0.2f); break; }
                    case MovementMode.BlockingRun: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Block"), 0.2f); break; } // TODO
                    case MovementMode.BlockingWalk: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Block"), 0.2f); break; } // TODO
                    case MovementMode.BlockingSprint: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Block"), 0.2f); break; } // TODO
                    default: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Run"), 0.2f); break; }
                }
            }

            Vector2 tempFacing = new Vector2(0, 0);
            if(!Camera.IsTargetLocked)
            {
                UpdateTargetFacing();
            }
            else
            {
                tempFacing = GetInputFacing();
            }

                float speed = 0f;
            switch(mode)
            {
                case MovementMode.Run: { speed = runSpeed; break; }
                case MovementMode.Walk: { speed = walkSpeed; break; }
                case MovementMode.Sprint: { speed = sprintSpeed; break; }
                case MovementMode.BlockingRun: { speed = runSpeed; break; }
                case MovementMode.BlockingWalk: { speed = walkSpeed; break; }
                case MovementMode.BlockingSprint: { speed = sprintSpeed; break; }
                default: { speed = walkSpeed; break; }
            }

            finalMove = new Vector3(tempFacing.magnitude > 0 ? tempFacing.x : targetFacing.x, 0, tempFacing.magnitude > 0 ? tempFacing.y : targetFacing.y) * speed;
        }
        else if(State == PlayerState.Ready)
        {
            // TODO move somewhere more organized
            if (!modelAnimator.IsInTransition(0))
            {
                if(IsBlocking)
                {
                    modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Block"), 0.2f);
                }
                else
                {
                    modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Idle"), 0.2f);
                }
            }
        }

        // Add gravity and move
        finalMove.y += gravityValue;
        GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);
    }

    void UpdateTargetFacing()
    {
        Vector2 facing = GetInputFacing();
        if (facing.magnitude > 0)
        {
            targetFacing = facing;
        }
    }

    Vector2 GetInputFacing()
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

            return totalMovement;
        }

        return new Vector2(0, 0);
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
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Roll"), 0.2f);
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
        modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Idle"));
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

        SetAttackId();
        SetAttackDamage(attackType);

        if (IsAttackHolding)
        {
            attackTimer = 0;
        }

        if (Camera.IsTargetLocked)
        {
            FacePosition(Camera.LockTarget.GetComponent<Enemy>().GetFocusPosition());
        }

        State = attackType;
        LookTowardsFacing(true, !Camera.IsTargetLocked);

        attackIndex = GetNextAttackIndex(attackIndex, attackType);
        modelAnimator.CrossFade(Animator.StringToHash(attackIndex.GetAnimation()), 0.2f, 0);
    }

    void SetAttackId()
    {
        // A random attack ID is used to prevent duplicate damage
        System.Random random = new System.Random();
        AttackId = random.Next(); // This returns a plain 32 bit int instead of a long, but should suffice
    }

    void SetAttackDamage(PlayerState attackType)
    {
        if(attackType == PlayerState.AttackingLight)
        {
            AttackDamage = attackLightDamage;
        }

        if (attackType == PlayerState.AttackingHeavy)
        {
            AttackDamage = attackHeavyDamage;
        }
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
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Idle"), 0.2f);
    }

    public bool IsAttacking()
    {
        return State == PlayerState.AttackingLight || State == PlayerState.AttackingHeavy;
    }

    void HandleBlocking()
    {
        if (blockAction.IsPressed() && State == PlayerState.Ready)
        {
            isBlocking = true;
        }
        else
        {
            isBlocking = false;
        }
    }

    void CheckHit()
    {
        // TODO check iframes
        if (State != PlayerState.Dead)
        {
            foreach(Collider collider in overlappingWeapons)
            {
                Enemy enemy = collider.gameObject.GetComponent<Enemy>();

                if (enemy.IsAttackActive)
                {
                    if (IsBlocking)
                    {
                        // TODO check if we're facing the correct direction
                        // TODO play block animation, stop movement temporarily
                        // TODO block break if we're out of stamina
                    }
                    else
                    {
                        GetHit(enemy);
                        enemy.NeutralizeAttack();
                    }
                }
            }

            //CapsuleGeometry playerGeometry = GetComponent<CharacterController>().GetGeometry<CapsuleGeometry>();
            //Collider[] touching = Physics.OverlapCapsule(playerGeometry.center1, playerGeometry.center2, playerGeometry.radius, null, true);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "EnemyWeapon")
        {
            overlappingWeapons.Add(collider);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "EnemyWeapon")
        {
            overlappingWeapons.Remove(collider);
        }
    }

    void GetHit(Enemy attacker)
    {
        TakeDamage(attacker.AttackDamage);

        if (State != PlayerState.Dead)
        {
            if (DataManager.Instance.CurrentPoise <= 0)
            {
                Stagger();
            }
            // TODO play hit animation?
        }
    }

    void TakeDamage(int damage)
    {
        DataManager.Instance.CurrentHealth -= damage;
        DataManager.Instance.CurrentPoise -= damage;

        if (DataManager.Instance.CurrentHealth <= 0)
        {
            DataManager.Instance.CurrentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        State = PlayerState.Dead;
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Die"), 0.2f);
        // TODO game over screen
    }

    void Stagger()
    {
        State = PlayerState.Stagger;
        staggerTimer = 0;
        // TODO instant face source of damage?
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Hit"), 0.2f);
        // TODO move backwards during animation in HandleStaggering
    }

    void HandleStaggering()
    {
        if (State == PlayerState.Stagger)
        {
            staggerTimer += Time.deltaTime;
            if (staggerTimer >= StaggerTime)
            {
                State = PlayerState.Ready;
                DataManager.Instance.CurrentPoise = DataManager.Instance.MaxPoise;
            }
        }
    }

    void RegainPoise()
    {
        DataManager.Instance.CurrentPoise += DataManager.Instance.PoiseRegen * Time.deltaTime;

        if (DataManager.Instance.CurrentPoise > DataManager.Instance.MaxPoise)
        {
            DataManager.Instance.CurrentPoise = DataManager.Instance.MaxPoise;
        }
    }
}

public enum PlayerState
{
    Ready,
    AttackingLight,
    AttackingHeavy,
    Stagger,
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
    BlockingWalk,
    BlockingRun,
    BlockingSprint,
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