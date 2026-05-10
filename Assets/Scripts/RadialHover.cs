using UnityEngine;

public class RadialHover : MonoBehaviour
{
    public float MinMagnitude = -3f;

    public float MaxMagnitude = 3f;

    private float magnitude;

    public float MinSpeed = .1f;

    public float MaxSpeed = 1f;

    private float speed;

    private Vector3 startPosition;

    private Vector3 originDirection;

    private float magic = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        magnitude = Random.Range(MinMagnitude, MaxMagnitude);
        speed = Random.Range(MinSpeed, MaxSpeed);
        startPosition = transform.localPosition;
        originDirection = -transform.localPosition;
        originDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        magic += Time.deltaTime * speed;

        transform.localPosition = startPosition + (originDirection * magnitude * Mathf.Sin(magic));
    }
}
