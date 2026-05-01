using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject TargetFocus = null;

    public int MaxHealth = 50;

    public int CurrentHealth = 50;

    public float MaxPoise = 25f;

    public float CurrentPoise = 25f;

    public float PoiseRegen = 5f;

    public float StaggerTime = 2f;

    private float staggerTimer = 0f;

    public EnemyState State { get; set; }

    private bool weaponTouching = false;

    private long lastHitId;

    public float AwarenessDistance = 10f;

    public float AwarenessOffset = 5f;

    public float ForgetDistance = 50f;

    private Vector3 homePosition;

    public bool AwareThroughObstacles = false;

    public bool IsAware = false;

    public float EngageDistance = 2f;

    public float PersonalSpace = 1f;

    UnityEngine.AI.NavMeshAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        homePosition = transform.position;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        State = EnemyState.Ready;
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
    }

    void HandleAwareness()
    {
        if (PlayerManager.Instance != null)
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

    bool CanSeePlayer(bool needLOS = true)
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

    void ChasePlayer()
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
        }
    }

    void EngagePlayer()
    {
        Vector3 playerPos = PlayerManager.Instance.transform.position;

        if (Vector3.Distance(transform.position, playerPos) < PersonalSpace)
        {
            Vector3 retreatDirection = transform.position - playerPos;
            retreatDirection.Normalize();
            retreatDirection *= PersonalSpace;
            agent.destination = transform.position + retreatDirection;
        }

        // TODO turn to face player to prevent backstabbing (slowly)
        // TODO engage attack behaviors
    }

    void RegainPoise()
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

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "PlayerWeapon")
        {
            weaponTouching = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "PlayerWeapon")
        {
            weaponTouching = false;
        }
    }

    void GetHit(PlayerManager attacker)
    {
        TakeDamage(attacker.AttackDamage);

        if(State != EnemyState.Dead)
        {
            if (CurrentPoise <= 0)
            {
                Stagger();
            }
            // TODO play hit animation
        }
    }

    void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentPoise -= damage;

        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        State = EnemyState.Dead;
        // TODO play die animation
    }

    void Stagger()
    {
        State = EnemyState.Stagger;
        staggerTimer = 0;
        // TODO play stagger animation
    }

    void HandleStaggering()
    {
        if (State == EnemyState.Stagger)
        {
            staggerTimer += Time.deltaTime;
            if (staggerTimer >= StaggerTime)
            {
                State = EnemyState.Ready;
            }
        }
    }
}

public enum EnemyState
{
    Ready,
    Stagger,
    Dead
}