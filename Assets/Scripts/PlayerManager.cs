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

    private InputAction healAction;

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

    public float SprintStaminaDrain = 4f;

    public PlayerState State { get; private set; } = PlayerState.Ready;

    private float sprintTime = 0f;
    public bool IsSprinting {
        get {
            return sprintTime >= sprintHold;
        }
    }

    public float rollSpeed = 7f;

    public int RollStaminaCost = 20;

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

    public int AttackLightDamage = 15;

    public int AttackLightStamina = 15;

    public int AttackHeavyDamage = 35;

    public int AttackHeavyStamina = 25;

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

    float blockingAttackTimer = 0f;

    public float BlockingAttackTime = 1f;

    public float BlockAngle = 45f;

    public float StaggerTime = 2f;

    private float staggerTimer = 0f;

    private Vector2 staggerFacing;

    private float staggerSpeed = 1f;

    private float staminaRegenPause = 0f;

    private float staminaRegenPartial = 0f;

    public float StaminaEmptyPenaltyTime = 1f;

    private RestPoint restAttemptPoint = null;

    public float GameOverSpeed = 4f;

    public float GameOverMinSpeed = .1f;

    public float GameOverHold = 4f;

    private float gameOverTimer = 0f;

    private bool isGameOver = false;

    private bool didGameOverRespawn = false;

    private float respawnEffectTimer = 0f;

    public float RespawnEffectTime = 1f;

    private Vector2 targetFacing;

    private HashSet<GameObject> overlappingWeapons = new HashSet<GameObject>();

    public GameObject HitEffect;

    public SoundPlayer FootstepSound;

    public SoundPlayer HealSound;

    public SoundPlayer SwingSound;

    public SoundPlayer HitSound;

    public SoundPlayer RestSound;

    public SoundPlayer DieSound;

    public SoundPlayer RollSound;

    public SoundPlayer BlockSound;

    public SoundPlayer StaggerSound;

    public SoundPlayer TextWarning;

    private bool endingJumped = false;

    public float EndingJumpSpeed = 1f;

    public GameObject EndingTarget;

    public float EndingFadeDelay = 1f;

    public float EndingFadeTime = 3f;

    public float EndingMessageTime = 4f;

    public float EndingCutTime = 10f;

    private float endingFadeTimer = 0f;

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
        healAction = InputSystem.actions.FindAction("Heal");
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
        ContinueBlockingAttack();
        RegenStamina();
        HandleHealing();
        ContinueGameOver();
        ContinueRespawn();
        ContinueEnding();

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
                    if(HasStamina(RollStaminaCost))
                    {
                        StartRoll();
                    }
                }
            }
            else
            {
                if (sprintAction.IsPressed() && HasStamina(1))
                {
                    sprintTime += Time.deltaTime;
                }
                else
                {
                    if (sprintTime > 0 && sprintTime < sprintHold)
                    {
                        if (HasStamina(RollStaminaCost))
                        {
                            StartRoll();
                        }
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
            if(walkAction.IsPressed() && !Camera.IsTargetLocked)
            {
                mode = MovementMode.Walk;
            }
            if(moveValue.magnitude < walkThreshhold && !Camera.IsTargetLocked)
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
                    case MovementMode.Run: {
                            if (moveValue.x < -.5 && Camera.IsTargetLocked)
                            {
                                modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|RunStrafeLeft"), 0.2f);
                            }
                            else if (moveValue.x > .5 && Camera.IsTargetLocked)
                            {
                                modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|RunStrafeRight"), 0.2f);
                            }
                            else
                            {
                                modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Run"), 0.2f);
                            }
                            break;
                        }
                    case MovementMode.Walk: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Walk"), 0.2f); break; }
                    case MovementMode.Sprint: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Sprint"), 0.2f); break; }
                    case MovementMode.BlockingRun: {
                            if (moveValue.x < -.5 && Camera.IsTargetLocked)
                            {
                                modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|RunStrafeLeftBlock"), 0.2f);
                            }
                            else if (moveValue.x > .5 && Camera.IsTargetLocked)
                            {
                                modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|RunStrafeRightBlock"), 0.2f);
                            }
                            else
                            {
                                modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|RunBlock"), 0.2f);
                            }
                            break;
                        }
                    case MovementMode.BlockingWalk: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|WalkBlock"), 0.2f); break; }
                    case MovementMode.BlockingSprint: { modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|SprintBlock"), 0.2f); break; }
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

        ConsumeStamina(RollStaminaCost);

        State = PlayerState.Rolling;
        LookTowardsFacing(true, true);
        modelAnimator.speed = 1.5f;
        PlaySound(RollSound);
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
            if (attackLightAction.IsPressed() && HasStamina(AttackLightStamina))
            {
                StartAttacking(PlayerState.AttackingLight);
                return;
            }
            else if (attackHeavyAction.IsPressed() && HasStamina(AttackHeavyStamina))
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

        switch(attackType)
        {
            case PlayerState.AttackingLight: ConsumeStamina(AttackLightStamina); break;
            case PlayerState.AttackingHeavy: ConsumeStamina(AttackHeavyStamina); break;
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
            AttackDamage = AttackLightDamage;
        }

        if (attackType == PlayerState.AttackingHeavy)
        {
            AttackDamage = AttackHeavyDamage;
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
                case AttackIndex.AttackHeavy2: return AttackIndex.AttackLight1;
            }
        }

        if (attackType == PlayerState.AttackingHeavy)
        {
            switch (attackIndex)
            {
                case AttackIndex.None: return AttackIndex.AttackHeavy;
                case AttackIndex.AttackLight1: return AttackIndex.AttackHeavy2;
                case AttackIndex.AttackLight2: return AttackIndex.AttackHeavy;
                case AttackIndex.AttackHeavy: return AttackIndex.AttackHeavy2;
                case AttackIndex.AttackHeavy2: return AttackIndex.AttackHeavy;
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
        if (blockAction.IsPressed() && State == PlayerState.Ready && DataManager.Instance.CurrentStamina > 0)
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
        if (State != PlayerState.Dead)
        {
            foreach(GameObject enemyObject in overlappingWeapons)
            {
                Enemy enemy = enemyObject.GetComponent<Enemy>();

                if (enemy.IsAttackActive)
                {
                    if(IsRollInvincible)
                    {
                        // TODO is neutralize necessary? Prevents re-attack during same animation,
                        //  but may not have the desired effect.
                        enemy.NeutralizeAttack();
                    }
                    else if (IsBlocking)
                    {
                        if(WithinBlockAngle(enemyObject))
                        {
                            if(ConsumeStamina(enemy.AttackDamage))
                            {
                                TakeDamage(enemy.AttackDamage / 2);
                                if(State != PlayerState.Dead)
                                {
                                    StartBlockingAttack(enemy);
                                }
                            }
                            else
                            {
                                // TODO - special block break animation/effects?
                                GetHit(enemy, true);
                            }
                        }
                        else
                        {
                            GetHit(enemy);
                        }
                    }
                    else
                    {
                        GetHit(enemy);
                    }
                }
            }
        }
    }

    bool WithinBlockAngle(GameObject enemyObject)
    {
        Vector3 enemyDirection = enemyObject.transform.position - transform.position;
        Vector2 enemyFlatDirection = new Vector2(enemyDirection.x, enemyDirection.z);
        enemyFlatDirection.Normalize();
        float angle = Vector3.Angle(targetFacing, enemyFlatDirection);

        return angle <= BlockAngle;
    }

    void StartBlockingAttack(Enemy enemy)
    {
        State = PlayerState.BlockingAttack;
        enemy.NeutralizeAttack();
        blockingAttackTimer = 0f;
        PlaySound(BlockSound);
        modelAnimator.Play(Animator.StringToHash("RobogirlArmature|BlockHit"));
    }

    void ContinueBlockingAttack()
    {
        if(State == PlayerState.BlockingAttack)
        {
            blockingAttackTimer += Time.deltaTime;

            if (blockingAttackTimer >= BlockingAttackTime)
            {
                FinishBlockingAttack();
            }
        }
    }

    void FinishBlockingAttack(PlayerState endState = PlayerState.Ready)
    {
        State = endState;
        blockingAttackTimer = 0f;
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Block"), 0.2f);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "EnemyWeapon")
        {
            overlappingWeapons.Add(collider.gameObject.GetComponent<EnemyWeapon>().Enemy);
        }

        if(collider.tag == "Ending")
        {
            TriggerEnding(collider);
        }

        if(collider.tag == "TextTrigger")
        {
            PlayerInterface.Instance.ShowWarningMessage();
            Destroy(collider.gameObject);
            PlaySound(TextWarning);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "EnemyWeapon")
        {
            overlappingWeapons.Remove(collider.gameObject.GetComponent<EnemyWeapon>().Enemy);
        }
    }

    void GetHit(Enemy attacker, bool forceStagger = false)
    {
        if(State == PlayerState.Resting)
        {
            forceStagger = true;
        }

        TakeDamage(attacker.AttackDamage);
        PlayHitEffect();
        attacker.NeutralizeAttack();

        if (State != PlayerState.Dead)
        {
            if (DataManager.Instance.CurrentPoise <= 0 || forceStagger)
            {
                Stagger(attacker);
            }
        }
    }

    void PlayHitEffect()
    {
        CharacterController controller = GetComponent<CharacterController>();

        Vector3 point1 = controller.center + transform.position + (transform.up * ((-controller.height * 0.5f) + controller.radius));
        Vector3 point2 = controller.center + transform.position + (transform.up * ((controller.height * 0.5f) + controller.radius));

        point1.y -= controller.height;
        point2.y -= controller.height;

        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, controller.radius, new Vector3(0, 1, 0), controller.height, Int32.MaxValue, QueryTriggerInteraction.Collide);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.tag == "EnemyWeapon")
            {
                if (hit.collider.gameObject.GetComponent<EnemyWeapon>().Enemy.GetComponent<Enemy>().IsAttackActive)
                {
                    GameObject effect = Instantiate(HitEffect);
                    if (hit.point.magnitude < .05f)
                    {
                        Vector3 center = hit.collider.gameObject.GetComponent<Collider>().bounds.center;
                        effect.transform.position = controller.ClosestPoint(center);
                    }
                    else
                    {
                        effect.transform.position = hit.point;
                    }
                }
            }
        }

        PlayAttackConnectSound();
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

    bool ConsumeStamina(int amount)
    {
        bool enough = HasStamina(amount);
        DataManager.Instance.CurrentStamina -= amount;
        if(DataManager.Instance.CurrentStamina < 0)
        {
            DataManager.Instance.CurrentStamina = 0;
        }

        if(!enough)
        {
            staminaRegenPause = StaminaEmptyPenaltyTime;
        }

        return enough;
    }

    bool HasStamina(int amount)
    {
        return DataManager.Instance.CurrentStamina >= amount;
    }

    void Die()
    {
        InterruptHealing(PlayerState.Dead);
        InterruptResting(PlayerState.Dead);
        State = PlayerState.Dead;
        PlaySound(DieSound);
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Die"), 0.2f);
    }

    public void EventGameOver()
    {
        isGameOver = true;
        gameOverTimer = GameOverSpeed;
    }

    void ContinueGameOver()
    {
        if(isGameOver)
        {
            gameOverTimer -= Time.deltaTime;

            float gameSpeed = Mathf.Max(gameOverTimer / GameOverSpeed, GameOverMinSpeed);
            Time.timeScale = gameSpeed;

            float gameOverProgress = 1f - Mathf.Max(gameOverTimer / GameOverSpeed, 0);
            PlayerInterface.Instance.SetGameOverProgress(gameOverProgress);

            if(gameOverProgress >= 1 && !didGameOverRespawn)
            {
                Respawn();
                didGameOverRespawn = true;
            }

            if (gameOverTimer <= -(GameOverHold * GameOverMinSpeed))
            {
                FinishGameOver();
            }
        }
    }

    void FinishGameOver()
    {
        gameOverTimer = 0;
        isGameOver = false;
        didGameOverRespawn = false;
        Time.timeScale = 1f;
        respawnEffectTimer = RespawnEffectTime;
    }

    void Respawn()
    {
        DataManager.Instance.ResetWorld();

        RestPoint respawnPoint = GetRespawnPoint();
        respawnPoint.PrepareArea();
        GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
        transform.position = respawnPoint.gameObject.transform.position + new Vector3(1f, 0f, 0f);
        GetComponent<CharacterController>().enabled = true;
        FacePosition(respawnPoint.gameObject.transform.position);
        State = PlayerState.Ready;
    }

    void ContinueRespawn()
    {
        if(respawnEffectTimer > 0)
        {
            respawnEffectTimer -= Time.deltaTime;
            PlayerInterface.Instance.SetGameOverProgress(respawnEffectTimer / RespawnEffectTime, false);

            if(respawnEffectTimer <= 0)
            {
                respawnEffectTimer = 0;
                PlayerInterface.Instance.SetGameOverProgress(0);
            }
        }
    }

    RestPoint GetRespawnPoint()
    {
        return DataManager.Instance.GetRespawnPoint();
    }

    void Stagger(Enemy attacker)
    {
        InterruptHealing(PlayerState.Stagger);
        InterruptResting(PlayerState.Stagger);
        State = PlayerState.Stagger;
        SetStaggerDirection(attacker);
        staggerTimer = 0;
        PlaySound(StaggerSound);
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Hit"), 0.2f);
    }

    void SetStaggerDirection(Enemy attacker)
    {
        FacePosition(attacker.GetFocusPosition());
        staggerFacing = targetFacing;
    }

    void HandleStaggering()
    {
        if (State == PlayerState.Stagger)
        {
            // Move away from the stagger point
            Vector3 finalMove = new Vector3(-targetFacing.x, 0, -targetFacing.y) * staggerSpeed;
            finalMove.y += gravityValue;
            GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);

            // Continue stagger
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

    void RegenStamina()
    {
        if(staminaRegenPause > 0)
        {
            staminaRegenPartial = 0f;
            staminaRegenPause -= Time.deltaTime;
            return;
        }

        // Partial regen for fractional regains between frames
        if(!IsSprinting)
        {
            staminaRegenPartial += DataManager.Instance.StaminaRegen * Time.deltaTime;
        }
        else
        {
            staminaRegenPartial -= SprintStaminaDrain * Time.deltaTime;
        }

        // Take the fractional regain/loss and apply it when possible
        while (staminaRegenPartial > 1)
        {
            DataManager.Instance.CurrentStamina += 1;
            staminaRegenPartial -= 1f;
        }
        while (staminaRegenPartial < -1)
        {
            DataManager.Instance.CurrentStamina -= 1;
            staminaRegenPartial += 1f;

            if(DataManager.Instance.CurrentStamina <= 0)
            {
                DataManager.Instance.CurrentStamina = 0;
                staminaRegenPause = StaminaEmptyPenaltyTime;
            }
        }

        if (DataManager.Instance.CurrentStamina > DataManager.Instance.MaxStamina)
        {
            DataManager.Instance.CurrentStamina = DataManager.Instance.MaxStamina;
        }
    }

    void HandleHealing()
    {
        // Healing uses the animation event system to drive actions, rather than
        //   timers like the other systems currently use.
        // TODO refactor other essential systems to use animation events for more
        //   accurate timing.
        CheckHealing();
    }

    void CheckHealing()
    {
        if (State == PlayerState.Ready)
        {
            if (healAction.triggered && healAction.IsPressed())
            {
                if(DataManager.Instance.CurrentHeals > 0)
                {
                    StartHealing();
                }
            }
        }
    }

    void StartHealing()
    {
        State = PlayerState.Healing;
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Heal"), 0.2f);
    }


    public void EventHeal()
    {
        if (State == PlayerState.Healing)
        {
            PlaySound(HealSound);

            DataManager.Instance.CurrentHeals -= 1;
            DataManager.Instance.CurrentHealth += DataManager.Instance.HealAmount;

            if (DataManager.Instance.CurrentHealth > DataManager.Instance.MaxHealth)
            {
                DataManager.Instance.CurrentHealth = DataManager.Instance.MaxHealth;
            }
        }
    }

    public void EventFinishHealing()
    {
        State = PlayerState.Ready;
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Idle"), 0.2f);
    }

    void InterruptHealing(PlayerState resultState = PlayerState.Ready)
    {
        State = resultState;
    }

    public void StartRest(RestPoint point)
    {
        State = PlayerState.Resting;
        restAttemptPoint = point;
        PlayRestEffects(point);
        PlayRestAnimation(point);
        FacePosition(point.gameObject.transform.position);
    }

    public void EventFinishResting()
    {
        State = PlayerState.Ready;
        restAttemptPoint?.PlayDisable();
    }

    public void EventStartReseting()
    {
        PlaySound(RestSound);
        ResetWorld();
        SetRespawn(restAttemptPoint);
    }

    void InterruptResting(PlayerState resultState = PlayerState.Ready)
    {
        State = resultState;
        restAttemptPoint?.PlayDisable();
    }

    void PlayRestEffects(RestPoint point)
    {
        point.PlayEnable();
    }

    void PlayRestAnimation(RestPoint point)
    {
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Rest"), 0.2f);
    }

    void ResetWorld()
    {
        DataManager.Instance.ResetWorld();
    }

    void SetRespawn(RestPoint point)
    {
        DataManager.Instance.SetRespawn(point);
    }

    void PlaySound(SoundPlayer player)
    {
        if (player != null) Instantiate(player.gameObject);
    }

    public void PlayFootstepSound()
    {
        PlaySound(FootstepSound);
    }

    public void PlayAttackSwingSound()
    {
        PlaySound(SwingSound);
    }

    public void PlayAttackConnectSound()
    {
        PlaySound(HitSound);
    }

    public void TriggerEnding(Collider collider)
    {
        State = PlayerState.Ending;
        modelAnimator.CrossFade(Animator.StringToHash("RobogirlArmature|Leap"), 0.2f);
    }

    public void EventEndingJump()
    {
        endingJumped = true;
    }

    public void ContinueEnding()
    {
        if(endingJumped)
        {
            Vector3 target = EndingTarget.transform.position;
            Vector2 targetHorizontal = new Vector2(target.x, target.z);

            Vector2 currentHorizontal = new Vector2(transform.position.x, transform.position.z);
            Vector2 newHorizontal = Vector2.Lerp(currentHorizontal, targetHorizontal, EndingJumpSpeed * Time.deltaTime);

            GetComponent<CharacterController>().enabled = false;
            transform.position = new Vector3(newHorizontal.x, transform.position.y, newHorizontal.y);
            GetComponent<CharacterController>().enabled = true;

            endingFadeTimer += Time.deltaTime;
            if(endingFadeTimer >= EndingFadeDelay)
            {
                PlayerInterface.Instance.SetEndingFade((endingFadeTimer - EndingFadeDelay) / EndingFadeTime);
            }
            if(endingFadeTimer >= EndingMessageTime)
            {
                PlayerInterface.Instance.ShowEndingMessage((endingFadeTimer - EndingMessageTime));
            }
            if(endingFadeTimer >= EndingCutTime)
            {
                MainMenu();
            }
        }
    }

    // TODO refactor to share with GameMenuManager
    public void MainMenu()
    {
        // Unpause time
        Time.timeScale = 1.0f;

        // Hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DataManager.Instance.LoadMenu();
    }
}

public enum PlayerState
{
    Ready,
    AttackingLight,
    AttackingHeavy,
    Stagger,
    Rolling,
    BlockingAttack,
    Healing,
    Resting,
    Falling,
    Dead,
    Frozen,
    Ending
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
    AttackHeavy,
    AttackHeavy2
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
            case AttackIndex.AttackHeavy2: return "RobogirlArmature|AttackHeavy2";
            default: return "RobogirlArmature|Idle";
        }
    }
}