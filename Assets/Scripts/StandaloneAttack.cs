using UnityEngine;

public class StandaloneAttack : MonoBehaviour
{
    public int Damage = 15;

    public bool ForceStagger = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Connected()
    {
        if(GetComponent<Spike>() != null)
        {
            GetComponent<Spike>().ForceShrink();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 GetFocusPosition()
    {
        return transform.position;
    }
}
