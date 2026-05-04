using UnityEngine;

public class LockOn : MonoBehaviour
{
    public float SpinSpeed = 90f;

    private float magic = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerManager.Instance != null)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            GameObject camera = PlayerManager.Instance.Camera.gameObject;
            transform.LookAt(camera.transform);

            magic += Time.deltaTime * SpinSpeed;
            transform.Rotate(transform.forward, magic, Space.World);
        }
    }

    public void Show()
    {
        GetComponent<Renderer>().enabled = true;
    }

    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;
    }

    public void SetPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }
}
