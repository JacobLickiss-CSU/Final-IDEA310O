using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public int MaxHealth = 100;

    public int CurrentHealth = 100;

    public int MaxStamina = 60;

    public int CurrentStamina = 60;

    public float StaminaRegen = 10f;

    public float MaxPoise = 30f;

    public float CurrentPoise = 30f;

    public float PoiseRegen = 5f;

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
