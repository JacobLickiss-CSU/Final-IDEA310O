using UnityEngine;

public class Spinning : MonoBehaviour
{
    public Vector3 Speed = new Vector3(0f, 0f, 90f);

    public bool DoLocal = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(DoLocal)
        {
            transform.Rotate(Speed * Time.deltaTime, Space.Self);
            //transform.localEulerAngles += (Time.deltaTime * Speed);
        }
        else
        {
            transform.eulerAngles += (Time.deltaTime * Speed);
        }
    }
}
