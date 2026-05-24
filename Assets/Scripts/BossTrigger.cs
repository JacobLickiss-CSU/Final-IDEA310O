using UnityEngine;

public class BossTrigger : MonoBehaviour, IReset
{
    public BossIntroManager intro;

    private bool triggered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DataManager.Instance.RegisterReset(this);
    }

    public void DoReset()
    {
        triggered = false;
        intro.DoReset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && !triggered)
        {
            triggered = true;
            intro.StartIntro();
        }
    }
}
