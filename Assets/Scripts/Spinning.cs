using UnityEngine;

public class Spinning : MonoBehaviour
{
    public Vector3 Speed = new Vector3(0f, 0f, 90f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += (Time.deltaTime * Speed);
    }
}
