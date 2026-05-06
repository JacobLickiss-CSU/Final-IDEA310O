using UnityEngine;

public class Sun : MonoBehaviour
{
    public GameObject SunLight;

    public float distance = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(SunLight != null && PlayerManager.Instance != null)
        {
            transform.position = PlayerManager.Instance.transform.position - SunLight.transform.forward * distance;
            transform.LookAt(PlayerManager.Instance.transform.position);
        }
    }
}
