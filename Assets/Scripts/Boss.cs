using UnityEngine;

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

            if (transform.position.y < FloorHeight.position.y)
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
}
