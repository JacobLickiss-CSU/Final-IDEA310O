using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    public GameObject Level1 = null;

    public GameObject Level2 = null;

    public GameObject Level3 = null;

    public GameObject Transition1_2 = null;

    public GameObject Transition2_3 = null;

    void Start()
    {
        Instance = this;

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
        
    }

    public void PerformTransition(TransitionType transition)
    {
        switch(transition)
        {
            case TransitionType.Level1Threshhold: EventLevel1Threshhold(); return;
            case TransitionType.Level2Threshhold: EventLevel2Threshhold(); return;
            case TransitionType.Level3Threshhold: EventLevel3Threshhold(); return;
            case TransitionType.Transition1_2: EventTransition1_2(); return;
            case TransitionType.Transition2_3: EventTransition2_3(); return;
        }
    }

    public void EventLevel1Threshhold()
    {
        if (Level1 != null) Level1.SetActive(true);
        if (Level2 != null) Level2.SetActive(false);
        if (Level3 != null) Level3.SetActive(false);
        if (Transition1_2 != null) Transition1_2.SetActive(true);
        if (Transition2_3 != null) Transition2_3.SetActive(false);
    }

    public void EventTransition1_2()
    {
        if (Level1 != null) Level1.SetActive(true);
        if (Level2 != null) Level2.SetActive(true);
        if (Level3 != null) Level3.SetActive(false);
        if (Transition1_2 != null) Transition1_2.SetActive(true);
        if (Transition2_3 != null) Transition2_3.SetActive(false);
    }

    public void EventLevel2Threshhold()
    {
        if (Level1 != null) Level1.SetActive(false);
        if (Level2!= null) Level2.SetActive(true);
        if (Level3 != null) Level3.SetActive(false);
        if (Transition1_2 != null) Transition1_2.SetActive(true);
        if (Transition2_3 != null) Transition2_3.SetActive(true);
    }

    public void EventTransition2_3()
    {
        if (Level1 != null) Level1.SetActive(false);
        if (Level2 != null) Level2.SetActive(true);
        if (Level3 != null) Level3.SetActive(true);
        if (Transition1_2 != null) Transition1_2.SetActive(false);
        if (Transition2_3 != null) Transition2_3.SetActive(true);
    }

    public void EventLevel3Threshhold()
    {
        if (Level1 != null) Level1.SetActive(false);
        if (Level2 != null) Level2.SetActive(false);
        if (Level3 != null) Level3.SetActive(true);
        if (Transition1_2 != null) Transition1_2.SetActive(false);
        if (Transition2_3 != null) Transition2_3.SetActive(true);
    }
}

public enum TransitionType
{
    Level1Threshhold,
    Level2Threshhold,
    Level3Threshhold,
    Transition1_2,
    Transition2_3
}