using UnityEngine;

public class Spinning : MonoBehaviour
{
    public float Speed = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (Time.deltaTime * Speed));
    }
}
