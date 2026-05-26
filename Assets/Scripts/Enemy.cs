using UnityEngine;
using System;

public class Enemy : MonoBehaviour, IReset
{
    public GameObject TargetFocus = null;

    public Animator modelAnimator;

    public string IdleAnimationName = "|Idle";

    public string WalkAnimationName = "|Walk";

    public string AttackAnimationName = "|Attack";

    public string HitAnimationName = "|Hit";

    public string DieAnimationName = "|Die";

    public int MaxHealth = 50;

    public int CurrentHealth = 50;

    public float MaxPoise = 10f;

    public float CurrentPoise = 10f;

    public float PoiseRegen = 5f;

    public float StaggerTime = 2f;

    protected float staggerTimer = 0f;

    public EnemyState State { get; set; }

    protected bool weaponTouching = false;

    protected long lastHitId;

    public float AwarenessDistance = 10f;

    public float AwarenessOffset = 5f;

    public float ForgetDistance = 50f;

    protected Vector3 homePosition;

    public bool AwareThroughObstacles = false;

    public bool IsAware = false;

    public float EngageDistance = 2f;

    public float PersonalSpace = 1f;

    protected UnityEngine.AI.NavMeshAgent agent;

    public float EngageTurnSpeed = 180f;

    public float gravityValue = -9.81f;

    public float MaxAttackAngle = 10f;

    public int AttackDamage = 15;

    protected bool attackActive = false;
    public bool IsAttackActive
    {
        get
        {
            return State == EnemyState.Attacking && attackActive;
        }
    }

    protected bool attackMoving = false;
    public bool IsAttackMoving
    {
        get
        {
            return State == EnemyState.Attacking && attackMoving;
        }
    }

    public float attackMoveSpeed = 2f;

    protected Vector3 resetPosition;

    protected Quaternion resetRotation;

    public GameObject HitEffect;

    public SoundPlayer FootstepSound;

    public SoundPlayer SwingSound;

    public SoundPlayer HitSound;

    public SoundPlayer DieSound;

    public SoundPlayer StaggerSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        homePosition = transform.position;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        State = EnemyState.Ready;
        resetPosition = transform.position;
        resetRotation = transform.rotation;
        DataManager.Instance.RegisterReset(this);
    }

    protected void PrepareAnimationStates()
    {

    }

    public void DoReset()
    {
        if(gameObject != null)
        {
            State = EnemyState.Ready;
            transform.position = resetPosition;
            transform.rotation = resetRotation;
            if (gameObject.activeInHierarchy) agent.destination = homePosition;
            if (gameObject.activeInHierarchy) agent.isStopped = false;
            CurrentHealth = MaxHealth;
            CurrentPoise = MaxPoise;
            IsAware = false;
            if (gameObject.activeInHierarchy) modelAnimator.Play(Animator.StringToHash(IdleAnimationName));
            staggerTimer = 0f;
            weaponTouching = false;
            attackActive = false;
            attackMoving = false;
            GetComponent<CharacterController>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleStaggering();
        RegainPoise();

        if(weaponTouching && State != EnemyState.Dead)
        {
            if (PlayerManager.Instance.IsAttackActive)
            {
                if (PlayerManager.Instance.AttackId != lastHitId)
                {
                    GetHit(PlayerManager.Instance);

                    lastHitId = PlayerManager.Instance.AttackId;
                }
            }
        }

        HandleAwareness();
        ChasePlayer();
        ContinueAttacking();
    }

    protected virtual void HandleAwareness()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.State != PlayerState.Dead)
        {
            Vector3 playerPos = PlayerManager.Instance.transform.position;

            if(Vector3.Distance(playerPos, homePosition) > ForgetDistance)
            {
                IsAware = false;
            }
            else
            {
                if(CanSeePlayer(!IsAware && !AwareThroughObstacles))
                {
                    IsAware = true;
                }
                else
                {
                    IsAware = false;
                }
            }
        }
        else
        {
            IsAware = false;
        }
    }

    protected bool CanSeePlayer(bool needLOS = true)
    {
        if (PlayerManager.Instance == null) return false;

        Vector3 playerPos = PlayerManager.Instance.transform.position + new Vector3(0, .5f, 0); // Looking at player core, rather than footing

        if (Vector3.Distance(playerPos, transform.position + (transform.forward * AwarenessOffset)) < AwarenessDistance)
        {
            Vector3 start = GetFocusPosition(); // TODO check from custom head position, not focus position
            Vector3 direction = playerPos - GetFocusPosition();

            if(needLOS)
            {
                RaycastHit hit;
                if (Physics.Raycast(start, direction, out hit, direction.magnitude + 1f))
                {
                    if (hit.collider.gameObject.tag == "Player" || hit.collider.gameObject.tag == "PlayerWeapon")
                    {
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    protected void ChasePlayer()
    {
        if(State == EnemyState.Ready)
        {
            if (IsAware)
            {
                Vector3 playerPos = PlayerManager.Instance.transform.position;

                if(Vector3.Distance(transform.position, playerPos) > EngageDistance)
                {
                    agent.destination = playerPos;
                }
                else
                {
                    agent.destination = transform.position;
                    EngagePlayer();
                }
            }
            else
            {
                agent.destination = homePosition;
            }

            if(Vector3.Distance(transform.position, agent.destination) > .1f && !agent.isStopped)
            {
                CrossFadeIfExists(WalkAnimationName, 0.1f);
            }
            else
            {
                if(!agent.isStopped)
                {
                    CrossFadeIfExists(IdleAnimationName, 0.1f);
                }
            }
        }
    }

    protected void EngagePlayer()
    {
        Vector3 playerPos = PlayerManager.Instance.transform.position;

        if (Vector3.Distance(transform.position, playerPos) < PersonalSpace)
        {
            Vector3 retreatDirection = transform.position - playerPos;
            retreatDirection.Normalize();
            retreatDirection *= PersonalSpace;
            agent.destination = transform.position + retreatDirection;
        }

        // Turn to face player (slowly)
        LookTowardsPlayer();

        // Engage attack behaviors
        ConsiderAttack();
    }

    protected void LookTowardsPlayer()
    {
        Quaternion currentFacing = transform.rotation;
        transform.LookAt(PlayerManager.Instance.transform);
        Quaternion targetFacing = transform.rotation;
        transform.rotation = Quaternion.RotateTowards(currentFacing, targetFacing, Time.deltaTime * EngageTurnSpeed);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    protected void ConsiderAttack()
    {
        if(PlayerAngle() <= MaxAttackAngle)
        {
            StartAttack();
        }
    }

    protected float PlayerAngle()
    {
        Vector3 playerDirection = PlayerManager.Instance.transform.position - transform.position;
        Vector2 playerFlatDirection = new Vector2(playerDirection.x, playerDirection.z);
        playerFlatDirection.Normalize();
        Vector2 forwardDirection = new Vector2(transform.forward.x, transform.forward.z);

        return Vector3.Angle(forwardDirection, playerFlatDirection);
    }

    protected void RegainPoise()
    {
        CurrentPoise += PoiseRegen * Time.deltaTime;

        if(CurrentPoise > MaxPoise)
        {
            CurrentPoise = MaxPoise;
        }
    }

    public Vector3 GetFocusPosition()
    {
        if(TargetFocus == null)
        {
            return transform.position;
        }

        return TargetFocus.transform.position;
    }

    public Transform GetFocusTransform()
    {
        if (TargetFocus == null)
        {
            return transform;
        }

        return TargetFocus.transform;
    }

    protected void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "PlayerWeapon")
        {
            weaponTouching = true;
        }
    }

    protected void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "PlayerWeapon")
        {
            weaponTouching = false;
        }
    }

    protected virtual void GetHit(PlayerManager attacker)
    {
        TakeDamage(attacker.AttackDamage);
        PlayHitEffect();

        if(State != EnemyState.Dead)
        {
            if(CurrentPoise <= 0)
            {
                Stagger();
            }
        }
    }

    protected void PlayHitEffect()
    {
        CharacterController controller = GetComponent<CharacterController>();

        Vector3 point1 = controller.center + transform.position + (transform.up * ((-controller.height * 0.5f) + controller.radius));
        Vector3 point2 = controller.center + transform.position + (transform.up * ((controller.height * 0.5f) + controller.radius));

        point1.y -= controller.height;
        point2.y -= controller.height;

        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, controller.radius, new Vector3(0, 1, 0), controller.height, Int32.MaxValue, QueryTriggerInteraction.Collide);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.tag == "PlayerWeapon")
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

        PlaySound(HitSound);
    }

    protected void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentPoise -= damage;

        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        State = EnemyState.Dead;
        GetComponent<CharacterController>().enabled = false;
        PlaySound(DieSound);
        CrossFadeIfExists(DieAnimationName, 0.1f);
    }

    protected void Stagger()
    {
        if(State == EnemyState.Attacking)
        {
            StaggerAttack();
        }

        State = EnemyState.Stagger;
        staggerTimer = 0;
        agent.isStopped = true;
        PlaySound(StaggerSound);
        CrossFadeIfExists(HitAnimationName, 0.1f, true);
    }

    protected void HandleStaggering()
    {
        if (State == EnemyState.Stagger)
        {
            LookTowardsPlayer();

            staggerTimer += Time.deltaTime;
            if (staggerTimer >= StaggerTime)
            {
                State = EnemyState.Ready;
                CurrentPoise = MaxPoise;
                agent.isStopped = false;
            }
        }
    }

    protected void StartAttack()
    {
        State = EnemyState.Attacking;
        attackActive = false;
        attackMoving = false;
        agent.isStopped = true;
        CrossFadeIfExists(AttackAnimationName, 0.1f, true);
    }

    protected virtual void ContinueAttacking()
    {
        if(State == EnemyState.Attacking)
        {
            if (IsAttackMoving)
            {
                Vector3 finalMove = transform.forward * attackMoveSpeed;
                finalMove.y += gravityValue;
                GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);
            }
        }
    }

    protected void StaggerAttack()
    {
        attackActive = false;
        attackMoving = false;
        agent.isStopped = false;
        State = EnemyState.Stagger;
    }

    protected void StopAttacking(EnemyState endState = EnemyState.Ready)
    {
        attackActive = false;
        attackMoving = false;
        State = endState;
        agent.isStopped = false;
        CrossFadeIfExists(IdleAnimationName, 0.1f);
    }

    public void NeutralizeAttack()
    {
        // Keep using the attack animation, but make IsAttackActive false to prevent repeated hits
        attackActive = false;
    }

    protected void CrossFadeIfExists(string animationName, float normalizedTransitionDuration, bool force = false)
    {
        int targetState = Animator.StringToHash(animationName);
        if(modelAnimator.HasState(0, targetState))
        {
            int nextState = -1;
            if(modelAnimator.IsInTransition(0))
            {
                nextState = modelAnimator.GetNextAnimatorStateInfo(0).shortNameHash;
            }

            int currentState = modelAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if((targetState != currentState && targetState != nextState) || force)
            {
                if(currentState == targetState)
                {
                    modelAnimator.Play(targetState, 0, 0f);
                }
                else
                {
                    modelAnimator.CrossFade(targetState, normalizedTransitionDuration);
                }
            }
            //modelAnimator.Play(targetState, 0, normalizedTransitionDuration);
        }
        else
        {
            Debug.Log("Cannot play animation " + animationName);
        }
    }

    protected void PlaySound(SoundPlayer player, bool follow = true)
    {
        if (player == null) return;

        if (follow)
        {
            Instantiate(player.gameObject, transform);
        }
        else
        {
            Instantiate(player.gameObject);
        }
    }

    public void EventFootstep()
    {
        PlaySound(FootstepSound);
    }

    public void EventAttackSwing()
    {
        PlaySound(SwingSound);
    }

    public void EventStartWeapon()
    {
        attackActive = true;
    }

    public void EventEndWeapon()
    {
        attackActive = false;
    }

    public void EventAttackMove()
    {
        attackMoving = true;
    }

    public void EventAttackHalt()
    {
        attackMoving = false;
    }

    public void AttackFinish()
    {
        StopAttacking();
    }
}

public enum EnemyState
{
    Ready,
    Stagger,
    Attacking,
    Dead,
    Waiting,
    Intro
}