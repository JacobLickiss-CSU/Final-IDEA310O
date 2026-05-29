using UnityEngine;
using System;

public class Boss : Enemy, IReset
{
    public string InactiveAnimationName = "|Inactive";

    public string IntroAnimationName = "|Intro";

    public string AttackForwardAnimationName = "|AttackForwardStill";

    public string AttackAroundAnimationName = "|AttackAround";

    public string AttackAirAnimationName = "|AttackAir";

    public string LeapUpAnimationName = "|LeapUp";

    public string LeapLandAnimationName = "|LeapLand";

    public Transform FloorHeight;

    public BossAttack[] AttackSequence;

    int currentAttack = -1;

    float currentAttackTime = -1f;

    public float NoneAttackTime = 2f;

    public float AttackMaxShake = .3f;

    float attackShakeLevel = 0;

    float attackShakePeakTime = -1f;

    bool attackShakeReverse = false;

    Vector3 attackShakeCenter;

    public Transform JumpTarget;

    public Transform[] LandTargets;

    bool isJumping;

    bool isLanding;

    Transform JumpingTarget;

    Transform LandingTarget;

    public float JumpSpeed = 1f;

    public float FallSpeed = 1f;

    bool isTracking = false;

    bool deathLeaping = false;

    public GameObject ForwardAttack;

    public GameObject AroundAttack;

    public GameObject AirAttack;

    public float AirAttackDistance = 15f;

    private System.Random random = new System.Random();

    public SoundPlayer RockSound;

    public SoundPlayer RockHitSound;

    public SoundPlayer ChainsSound;

    public SoundPlayer ChainsLongSound;

    public SoundPlayer JumpSound;

    new public bool IsAttackActive
    {
        get
        {
            return State == EnemyState.Attacking && attackActive;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        homePosition = transform.position;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        State = EnemyState.Waiting;
        resetPosition = transform.position;
        resetRotation = transform.rotation;
        DataManager.Instance.RegisterReset(this);
    }

    new public void DoReset()
    {
        if (gameObject != null)
        {
            InterruptAttacking();
            State = EnemyState.Waiting;
            transform.position = resetPosition;
            transform.rotation = resetRotation;
            //if (gameObject.activeInHierarchy) agent.destination = homePosition;
            //if (gameObject.activeInHierarchy) agent.isStopped = false;
            CurrentHealth = MaxHealth;
            CurrentPoise = MaxPoise;
            IsAware = false;
            if (gameObject.activeInHierarchy) modelAnimator.Play(Animator.StringToHash(InactiveAnimationName));
            staggerTimer = 0f;
            weaponTouching = false;
            attackActive = false;
            attackMoving = false;
            GetComponent<CharacterController>().enabled = true;
            currentAttack = -1;
            currentAttackTime = -1f;
            attackShakeLevel = 0f;
            deathLeaping = false;
            PlayerInterface.Instance.HideBossHealth();
        }
    }

    // Update is called once per frame
    void Update()
    {
        ContinueIntro();
        HandleStaggering();
        RegainPoise();

        if (weaponTouching && State != EnemyState.Dead)
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

        // Boss attack behaviors
        HandleAttacking();
        HandleDeathLeaping();
    }

    protected override void HandleAwareness()
    {
        if (State != EnemyState.Waiting && State != EnemyState.Intro && State != EnemyState.Dead)
        {
            IsAware = true;
        }
        else
        {
            IsAware = false;
        }
    }

    protected override void GetHit(PlayerManager attacker)
    {
        TakeDamage(attacker.AttackDamage);
        PlayHitEffect();
        
        // Boss immune to stagger
    }

    public void PlayIntro()
    {
        State = EnemyState.Intro;
        CrossFadeIfExists(IntroAnimationName, 0.2f);
    }

    public void ContinueIntro()
    {
        if(State == EnemyState.Intro)
        {
            if (transform.position.y > FloorHeight.position.y)
            {
                GetComponent<CharacterController>().Move(new Vector3(0, -5f, 0) * Time.deltaTime);
            }

            if (transform.position.y <= FloorHeight.position.y)
            {
                GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
                transform.position = new Vector3(transform.position.x, FloorHeight.position.y, transform.position.z);
                GetComponent<CharacterController>().enabled = true;
                IntroFinished();
            }
        }
    }

    public void IntroFinished()
    {
        State = EnemyState.Ready;
        CrossFadeIfExists(IdleAnimationName, 0.2f);
    }

    protected override void Die()
    {
        base.Die();

        InterruptAttacking();
        PlayerInterface.Instance.HideBossHealth();
        MusicManager.Instance.StartEndingMusic();
    }

    void HandleAttacking()
    {
        if(State == EnemyState.Ready)
        {
            if(currentAttackTime < 0)
            {
                StartAttacking();
            }

            ContinueAttacking();
        }
    }

    void StartAttacking()
    {
        currentAttackTime = 0f;
        GetNextAttack();
    }

    protected override void ContinueAttacking()
    {
        currentAttackTime += Time.deltaTime;
        ContinueShake();
        ContinueJumping();
        ContinueLanding();

        switch (GetCurrentAttack())
        {
            case BossAttack.None:
                {
                    CrossFadeIfExists(IdleAnimationName, 0.2f);
                    StartTracking();

                    if (currentAttackTime >= NoneAttackTime)
                    {
                        FinishAttacking();
                    }

                    break;
                }
            case BossAttack.Forward:
                {
                    StopJumping();
                    StopLanding();
                    CrossFadeIfExists(AttackForwardAnimationName, 0.2f);

                    break;
                }
            case BossAttack.Around:
                {
                    StopJumping();
                    StopLanding();
                    CrossFadeIfExists(AttackAroundAnimationName, 0.2f);

                    break;
                }
            case BossAttack.Air:
                {
                    StopJumping();
                    StopLanding();
                    CrossFadeIfExists(AttackAirAnimationName, 0.2f);

                    break;
                }
            case BossAttack.LeapUp:
                {
                    CrossFadeIfExists(LeapUpAnimationName, 0.2f);

                    break;
                }
            case BossAttack.LeapLand:
                {
                    CrossFadeIfExists(LeapLandAnimationName, 0.2f);

                    break;
                }
            default: break;
        }

        if(isTracking)
        {
            LookTowardsPlayer();
        }
    }

    public void FinishAttacking()
    {
        currentAttackTime = -1f;
        StopShake();
        StopTracking();
    }

    void InterruptAttacking()
    {
        FinishAttacking();
        StopJumping();
        StopLanding();
        StopTracking();
    }

    BossAttack GetNextAttack()
    {
        currentAttack++;

        if(currentAttack >= AttackSequence.Length)
        {
            currentAttack = 0;
        }

        return GetCurrentAttack();
    }

    BossAttack GetCurrentAttack()
    {
        if (currentAttack < 0) return BossAttack.None;

        return AttackSequence[currentAttack];
    }

    public void AttackShake(float peakTime)
    {
        attackShakeCenter = transform.position;
        attackShakePeakTime = peakTime;
        ContinueShake(true);
    }

    void ContinueShake(bool force = false)
    {
        if((attackShakeLevel > 0f || force) && Time.timeScale > 0f)
        {
            if(attackShakeReverse)
            {
                attackShakeLevel -= Time.deltaTime / attackShakePeakTime;

                if(attackShakeLevel <= 0f)
                {
                    StopShake();
                    return;
                }
            }
            else
            {
                attackShakeLevel += Time.deltaTime / attackShakePeakTime;

                if (attackShakeLevel >= 1f)
                {
                    attackShakeReverse = true;
                }
            }

            transform.position = attackShakeCenter + new Vector3(UnityEngine.Random.Range(0, AttackMaxShake * attackShakeLevel), UnityEngine.Random.Range(0, AttackMaxShake * attackShakeLevel), UnityEngine.Random.Range(0, AttackMaxShake * attackShakeLevel));
        }
    }

    public void StopShake()
    {
        attackShakeLevel = 0f;
        transform.position = attackShakeCenter;
        attackShakeReverse = false;
    }

    public void Jump()
    {
        JumpingTarget = JumpTarget;
        isJumping = true;
    }

    void ContinueJumping()
    {
        if (isJumping)
        {
            GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
            transform.position = Vector3.Lerp(transform.position, JumpingTarget.position, JumpSpeed * Time.deltaTime);
            attackShakeCenter = transform.position;
            GetComponent<CharacterController>().enabled = true;

            if(Vector3.Distance(transform.position, JumpingTarget.position) < .1f)
            {
                GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
                transform.position = JumpingTarget.position;
                attackShakeCenter = transform.position;
                GetComponent<CharacterController>().enabled = true;
                StopJumping();
            }
        }
    }

    void StopJumping()
    {
        if(isJumping)
        {
            GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
            transform.position = JumpingTarget.position;
            attackShakeCenter = transform.position;
            GetComponent<CharacterController>().enabled = true;
        }

        isJumping = false;
    }

    public void Land()
    {
        LandingTarget = LandTargets[random.Next(LandTargets.Length)];
        isLanding = true;
    }

    void ContinueLanding()
    {
        if (isLanding)
        {
            GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
            transform.position = Vector3.Lerp(transform.position, LandingTarget.position, FallSpeed * Time.deltaTime);
            attackShakeCenter = transform.position;
            GetComponent<CharacterController>().enabled = true;

            if (Vector3.Distance(transform.position, LandingTarget.position) < .1f)
            {
                GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
                transform.position = LandingTarget.position;
                attackShakeCenter = transform.position;
                GetComponent<CharacterController>().enabled = true;
                StopJumping();
            }
        }
    }

    void StopLanding()
    {
        if (isLanding)
        {
            GetComponent<CharacterController>().enabled = false; // See https://discussions.unity.com/t/teleporting-character-issue-with-transform-position-in-unity-2018-3/221631/4
            transform.position = LandingTarget.position;
            attackShakeCenter = transform.position;
            GetComponent<CharacterController>().enabled = true;
        }

        isLanding = false;
    }

    public override void StartTracking()
    {
        isTracking = true;
    }

    public override void StopTracking()
    {
        isTracking = false;
    }

    public void DeathLeap()
    {
        deathLeaping = true;
    }

    void HandleDeathLeaping()
    {
        if(State == EnemyState.Dead && deathLeaping)
        {
            transform.Translate(new Vector3(0, 30f, 0) * Time.deltaTime, Space.World);
        }
    }

    public void ArmAttack()
    {
        attackActive = true;
    }

    public void ExecuteForwardAttack()
    {
        GameObject attack = Instantiate(ForwardAttack);
        attack.transform.position = transform.position;

        SpikeSpawner spawner = attack.GetComponent<SpikeSpawner>();

        spawner.Vector = new Vector3(transform.forward.x, -transform.forward.z, 0f);
    }

    public void ExecuteAroundAttack()
    {
        GameObject attack = Instantiate(AroundAttack);
        attack.transform.position = transform.position;

        SpikeSpawner spawner = attack.GetComponent<SpikeSpawner>();

        spawner.Vector = new Vector3(transform.forward.x, -transform.forward.z, 0f);
    }

    public void ExecuteAirAttack()
    {
        Vector3 targetPosition = PlayerManager.Instance.gameObject.transform.position;

        Vector3[] startPositions = new Vector3[] { 
            targetPosition + new Vector3(AirAttackDistance, 0, AirAttackDistance),
            targetPosition + new Vector3(-AirAttackDistance, 0, AirAttackDistance),
            targetPosition + new Vector3(AirAttackDistance, 0, -AirAttackDistance),
            targetPosition + new Vector3(-AirAttackDistance, 0, -AirAttackDistance),
        };

        for(int i=random.Next(4),icount=0; icount<3; i++, icount++)
        {
            Vector3 startPosition = startPositions[i % 4];
            Vector3 direction = startPosition - targetPosition;

            GameObject attack = Instantiate(AirAttack);
            attack.transform.position = startPosition;

            SpikeSpawner spawner = attack.GetComponent<SpikeSpawner>();

            spawner.Vector = -direction;
        }
    }

    public void EventSoundRock()
    {
        PlaySound(RockSound, true);
    }

    public void EventSoundRockHit()
    {
        PlaySound(RockHitSound, true);
    }

    public void EventSoundChains()
    {
        PlaySound(ChainsSound, true);
    }

    public void EventSoundChainsLong()
    {
        PlaySound(ChainsLongSound, true);
    }

    public void EventSoundJump()
    {
        PlaySound(JumpSound, true);
    }
}

public enum BossAttack
{
    None,
    Forward,
    Around,
    Air,
    LeapUp,
    LeapLand
}
