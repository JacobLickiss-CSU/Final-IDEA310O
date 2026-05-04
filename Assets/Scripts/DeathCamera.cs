using UnityEngine;

public class DeathCamera : MonoBehaviour
{
    public static DeathCamera Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().aspect = ((float)Screen.width) / Screen.height;
    }
}
