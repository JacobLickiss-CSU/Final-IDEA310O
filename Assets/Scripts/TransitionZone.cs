using UnityEngine;

public class TransitionZone : MonoBehaviour
{
    public TransitionType transition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            TriggerTransition();
        }
    }

    public void TriggerTransition()
    {
        TransitionManager.Instance.PerformTransition(transition);
    }
}