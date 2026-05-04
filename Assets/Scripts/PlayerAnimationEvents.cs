using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EventHeal()
    {
        PlayerManager.Instance.EventHeal();
    }

    public void EventFinishHealing()
    {
        PlayerManager.Instance.EventFinishHealing();
    }

    public void EventFinishResting()
    {
        PlayerManager.Instance.EventFinishResting();
    }

    public void EventStartReseting()
    {
        PlayerManager.Instance.EventStartReseting();
    }

    public void EventGameOver()
    {
        PlayerManager.Instance.EventGameOver();
    }
}
