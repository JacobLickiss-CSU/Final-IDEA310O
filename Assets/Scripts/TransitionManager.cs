using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    public GameObject Level1 = null;

    public GameObject Level2 = null;

    public GameObject Level3 = null;

    public GameObject Transition1_2 = null;

    public GameObject Transition2_3 = null;

    public float SkyboxTransitionTime = 5f;

    private float skyboxTimer = 0f;

    public Material DefaultSkybox;

    public Material Level3Skybox;

    private Material TargetSkybox;

    private float originAtmosphereThickness;

    private Color originSkyTint;

    private Color originGroundColor;

    private float originExposure;

    void Start()
    {
        Instance = this;

        // Copy skybox to prevent changing the original material
        RenderSettings.skybox = new Material(RenderSettings.skybox);

        InitState();
    }

    public void InitState(GameObject level = null)
    {
        if (level == null) level = Level1;

        if (Level1 != null && level == Level1) EventLevel1Threshhold();
        if (Level2 != null && level == Level2) EventLevel2Threshhold();
        if (Level3 != null && level == Level3) EventLevel3Threshhold();
    }

    // Update is called once per frame
    void Update()
    {
        if(skyboxTimer > 0f)
        {
            UpdateSkybox();
        }
    }

    public void PerformTransition(TransitionType transition)
    {
        switch(transition)
        {
            case TransitionType.Level1Threshhold: EventLevel1Threshhold(); return;
            case TransitionType.Level2Threshhold: EventLevel2Threshhold(); return;
            case TransitionType.Level3Threshhold: EventLevel3Threshhold(); return;
            case TransitionType.Transition1_2: EventTransition1_2(); return;
            case TransitionType.Transition2_3_Inner: EventTransition2_3_Inner(); return;
            case TransitionType.Transition2_3_Outer: EventTransition2_3_Outer(); return;
        }
    }

    public void EventLevel1Threshhold()
    {
        if (Level1 != null) Level1.SetActive(true);
        if (Level2 != null) Level2.SetActive(false);
        if (Level3 != null) Level3.SetActive(false);
        if (Transition1_2 != null) Transition1_2.SetActive(true);
        if (Transition2_3 != null) Transition2_3.SetActive(false);
        MusicManager.Instance.StartLevelMusic(1);
    }

    public void EventTransition1_2()
    {
        if (Level1 != null) Level1.SetActive(true);
        if (Level2 != null) Level2.SetActive(true);
        if (Level3 != null) Level3.SetActive(false);
        if (Transition1_2 != null) Transition1_2.SetActive(true);
        if (Transition2_3 != null) Transition2_3.SetActive(false);
        MusicManager.Instance.StartLevelMusic(0);
    }

    public void EventLevel2Threshhold()
    {
        if (Level1 != null) Level1.SetActive(false);
        if (Level2!= null) Level2.SetActive(true);
        if (Level3 != null) Level3.SetActive(false);
        if (Transition1_2 != null) Transition1_2.SetActive(true);
        if (Transition2_3 != null) Transition2_3.SetActive(true);
        MusicManager.Instance.StartLevelMusic(2);
    }

    public void EventTransition2_3_Inner()
    {
        EventTransition2_3();
        StartSkyboxTransition(DefaultSkybox);
        
        //RenderSettings.skybox = DefaultSkybox;
        //DynamicGI.UpdateEnvironment();
    }

    public void EventTransition2_3_Outer()
    {
        EventTransition2_3();
        StartSkyboxTransition(Level3Skybox);
        
        //RenderSettings.skybox = Level3Skybox;
        //DynamicGI.UpdateEnvironment();
    }

    public void EventTransition2_3()
    {
        if (Level1 != null) Level1.SetActive(false);
        if (Level2 != null) Level2.SetActive(true);
        if (Level3 != null) Level3.SetActive(true);
        if (Transition1_2 != null) Transition1_2.SetActive(false);
        if (Transition2_3 != null) Transition2_3.SetActive(true);
        MusicManager.Instance.StartLevelMusic(0);
    }

    public void EventLevel3Threshhold()
    {
        if (Level1 != null) Level1.SetActive(false);
        if (Level2 != null) Level2.SetActive(false);
        if (Level3 != null) Level3.SetActive(true);
        if (Transition1_2 != null) Transition1_2.SetActive(false);
        if (Transition2_3 != null) Transition2_3.SetActive(true);
        MusicManager.Instance.StartLevelMusic(3);
    }

    void StartSkyboxTransition(Material skybox)
    {
        TargetSkybox = skybox;
        skyboxTimer = SkyboxTransitionTime;

        originAtmosphereThickness = RenderSettings.skybox.GetFloat("_AtmosphereThickness");
        originSkyTint = RenderSettings.skybox.GetColor("_SkyTint");
        originGroundColor = RenderSettings.skybox.GetColor("_GroundColor");
        originExposure = RenderSettings.skybox.GetFloat("_Exposure");
}

    void UpdateSkybox()
    {
        float progress = (SkyboxTransitionTime - skyboxTimer) / SkyboxTransitionTime;

        float atmosphereThickness = Mathf.Lerp(originAtmosphereThickness, TargetSkybox.GetFloat("_AtmosphereThickness"), progress);
        Color skyTint = Color.Lerp(originSkyTint, TargetSkybox.GetColor("_SkyTint"), progress);
        Color groundColor = Color.Lerp(originGroundColor, TargetSkybox.GetColor("_GroundColor"), progress);
        float exposure = Mathf.Lerp(originExposure, TargetSkybox.GetFloat("_Exposure"), progress);

        RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphereThickness);
        RenderSettings.skybox.SetColor("_SkyTint", skyTint);
        RenderSettings.skybox.SetColor("_GroundColor", groundColor);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);

        DynamicGI.UpdateEnvironment();

        if(skyboxTimer <= 0) return;
        skyboxTimer -= Time.deltaTime;

        // Finish off the transition
        if(skyboxTimer <= 0)
        {
            skyboxTimer = 0f;
            UpdateSkybox();
        }
    }
}

public enum TransitionType
{
    Level1Threshhold,
    Level2Threshhold,
    Level3Threshhold,
    Transition1_2,
    Transition2_3_Inner,
    Transition2_3_Outer,
}