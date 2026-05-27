using UnityEngine;

public class EnemyHealthbar : MonoBehaviour
{
    public Enemy hostEnemy;

    public Renderer Bar;

    public float MaxDistance = 50f;

    float fullBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fullBar = Bar.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        Transform targetTransform = Camera.main.transform;
        transform.LookAt(targetTransform.position);
        transform.Rotate(90f, 0f, 0f);

        if (hostEnemy.CurrentHealth > 0 && hostEnemy.CurrentHealth < hostEnemy.MaxHealth && Vector3.Distance(transform.position, targetTransform.position) < MaxDistance)
        {
            GetComponent<Renderer>().enabled = true;
            Bar.enabled = true;

            Bar.transform.localScale = new Vector3(fullBar * ((float)hostEnemy.CurrentHealth/(float)hostEnemy.MaxHealth), Bar.transform.localScale.y, Bar.transform.localScale.z);
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
            Bar.enabled = false;
        }
    }
}
