using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public float Lifetime = 1.0f;

    private float lifeTimer = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lifeTimer = Lifetime;

        // TODO set and play sounds
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if(lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
