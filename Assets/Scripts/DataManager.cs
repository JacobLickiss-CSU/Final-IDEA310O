using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            RestoreDefaults();
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RestoreDefaults()
    {
        
    }
}
