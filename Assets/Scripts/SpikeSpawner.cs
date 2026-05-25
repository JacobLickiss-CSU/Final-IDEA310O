using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    public GameObject[] Spikes = new GameObject[0];

    public Vector3 Vector = new Vector3(0f, 0f, 0f);

    public float Speed = 10f;

    public float Lifetime = 1f;

    public float Frequency = 10f;

    public float Radius = 5f;

    private float lifeTimer = 0f;

    private float spawnTimer = 0f;

    private System.Random random = new System.Random();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lifeTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
            return;
        }

        while(spawnTimer >= 1f / Frequency)
        {
            spawnTimer -= (1f / Frequency);

            Spawn();
        }

        transform.Translate(Vector.normalized * Speed * Time.deltaTime);
    }

    void Spawn()
    {
        Vector3 spawnPoint = GetSpawnPoint();
        GameObject selectedSpike = Spikes[random.Next(Spikes.Length)];

        GameObject spike = Instantiate(selectedSpike);
        spike.transform.position = spawnPoint;
    }

    Vector3 GetSpawnPoint()
    {
        return transform.position + new Vector3(UnityEngine.Random.Range(-Radius, Radius), 0f, UnityEngine.Random.Range(-Radius, Radius));
    }
}
