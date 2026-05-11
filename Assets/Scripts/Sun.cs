using UnityEngine;

public class Sun : MonoBehaviour
{
    public GameObject SunLight;

    public float distance = 100f;

    public float DepthFull = 0f;

    public float DepthVanish = -10f;

    private float baseIntensity = 1.0f;

    private bool directionDown = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        directionDown = DepthVanish < DepthFull;

        if (SunLight != null)
        {
            baseIntensity = SunLight.GetComponent<Light>().intensity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Transform targetTransform = PlayerManager.Instance != null ? PlayerManager.Instance.transform : Camera.main.transform;

        if(SunLight != null)
        {
            transform.position = targetTransform.position - SunLight.transform.forward * distance;
            transform.LookAt(targetTransform.position);

            // Dim light within depth targets
            float depth = targetTransform.position.y;
            if((directionDown && depth > DepthFull) || (!directionDown && depth < DepthFull))
            {
                SunLight.GetComponent<Light>().intensity = baseIntensity;
            }
            else if((directionDown && depth < DepthVanish) || (!directionDown && depth > DepthVanish))
            {
                SunLight.GetComponent<Light>().intensity = 0f;
            }
            else
            {
                if(directionDown)
                {
                    float progress = (depth - DepthVanish) / Mathf.Abs(DepthFull - DepthVanish);
                    SunLight.GetComponent<Light>().intensity = baseIntensity * progress;
                }
                else
                {
                    float progress = 1.0f - ((depth - DepthFull) / Mathf.Abs(DepthVanish - DepthFull));
                    SunLight.GetComponent<Light>().intensity = baseIntensity * progress;
                }
            }
        }
    }
}
