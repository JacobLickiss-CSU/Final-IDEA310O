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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        // TODO test
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (PlayerManager.Instance != null)
        {
            //agent.destination = PlayerManager.Instance.transform.position;
        }
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