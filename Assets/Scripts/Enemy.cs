using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject TargetFocus = null;

    public int MaxHealth = 50;

    public int CurrentHealth = 50;

    public EnemyState State { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        State = EnemyState.Ready;
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

public enum EnemyState
{
    Ready,
    Hit,
    Dead
}