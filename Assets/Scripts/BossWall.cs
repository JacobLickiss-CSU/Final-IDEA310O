using UnityEngine;
using System.Collections.Generic;

public class BossWall : MonoBehaviour
{
    public List<Enemy> Protectors = new List<Enemy>();

    public bool Eternal = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckWall();
    }

    void CheckWall()
    {
        bool wallClear = true;
        foreach (Enemy protector in Protectors)
        {
            if (protector.State != EnemyState.Dead)
            {
                wallClear = false;
                break;
            }
        }
        if(Eternal)
        {
            wallClear = false;
        }

        GetComponent<Collider>().enabled = !wallClear;
        GetComponent<MeshRenderer>().enabled = !wallClear;
    }
}
