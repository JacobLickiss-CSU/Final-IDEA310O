using UnityEngine;

public class Sun : MonoBehaviour
{
    public GameObject SunLight;

    public float distance = 100f;

    private float baseIntensity = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SunLight != null)
        {
            baseIntensity = SunLight.GetComponent<Light>().intensity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(SunLight != null && PlayerManager.Instance != null)
        {
            transform.position = PlayerManager.Instance.transform.position - SunLight.transform.forward * distance;
            transform.LookAt(PlayerManager.Instance.transform.position);

            // Dim light below 0f
            float depth = PlayerManager.Instance.transform.position.y;
            if (depth < 0)
            {
                if (depth <= -10)
                {
                    SunLight.GetComponent<Light>().intensity = 0f;
                }
                else
                {
                    SunLight.GetComponent<Light>().intensity = baseIntensity * ((10f + depth) / 10f);
                }
            }
            else
            {
                SunLight.GetComponent<Light>().intensity = baseIntensity;
            }
        }
    }
}
