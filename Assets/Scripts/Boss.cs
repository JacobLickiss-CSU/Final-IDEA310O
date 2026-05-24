using UnityEngine;
using System;

public class Boss : Enemy
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

    private System.Random random = new System.Random();

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
            if (gameObject.activeInHierarchy) agent.destination = homePosition;
            if (gameObject.activeInHierarchy) agent.isStopped = false;
            CurrentHealth = MaxHealth;
            CurrentPoise = MaxPoise;
            IsAware = false;
            if (gameObject.activeInHierarchy) modelAnimator.Play(Animator.StringToHash(InactiveAnimationName));
            staggerTimer = 0f;
            weaponTouching = false;
            attackTimer = 0f;
            attackNeutralized = false;
            GetComponent<CharacterController>().enabled = true;
            currentAttack = -1;
            currentAttackTime = -1f;
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

        // TODO boss attack behaviors
        HandleAttacking();
        //ChasePlayer();
        //ContinueAttacking();
    }

    new void HandleAwareness()
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

    new void GetHit(PlayerManager attacker)
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

    new void Die()
    {
        base.Die();

        InterruptAttacking();
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

    new void ContinueAttacking()
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

                    if(currentAttackTime >= NoneAttackTime)
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
    }

    public void FinishAttacking()
    {
        currentAttackTime = -1f;
        StopShake();
    }

    void InterruptAttacking()
    {
        FinishAttacking();
        StopJumping();
        StopLanding();
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
        if(attackShakeLevel > 0f || force)
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
