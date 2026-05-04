using UnityEngine;

public class FogEffect : MonoBehaviour
{
    public float EnableSize = 12f;

    public float DisableSize = 1200f;

    public float VisibilityCutoff = 0.9f;

    public float AnimationSpeed = 3f;

    public float OutroEasing = 30f;

    public float EasingBleed = 3f;

    private float targetSize;

    private float easeOutLevel = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetSize = DisableSize;
        transform.localScale = new Vector3(targetSize, targetSize, targetSize);
    }

    // Update is called once per frame
    void Update()
    {
        RunPlay();
    }

    public void PlayEnable()
    {
        targetSize = EnableSize;
    }

    public void PlayDisable()
    {
        targetSize = DisableSize;
        easeOutLevel = OutroEasing;
    }

    void RunPlay()
    {
        Vector3 targetSizeVector = new Vector3(targetSize, targetSize, targetSize);

        float easing = 1f;
        if(transform.localScale.x < targetSize)
        {
            easing = easeOutLevel;
            easeOutLevel -= Time.deltaTime * EasingBleed;
            if(easeOutLevel < 1) easeOutLevel = 1;
        }
        transform.localScale = Vector3.Lerp(transform.localScale, targetSizeVector, Time.deltaTime * AnimationSpeed / easing);


        CheckVisible();
        SetAlpha();
    }

    void CheckVisible()
    {
        if(transform.localScale.x < (DisableSize * VisibilityCutoff))
        {
            GetComponent<Renderer>().enabled = true;
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

    void SetAlpha()
    {
        GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, GetProgress());
    }

    float GetProgress()
    {
        float minSize = EnableSize;
        float range = (DisableSize * VisibilityCutoff) - minSize;
        float currentSize = transform.localScale.x;

        return 1f - (currentSize - minSize) / range;
    }
}
