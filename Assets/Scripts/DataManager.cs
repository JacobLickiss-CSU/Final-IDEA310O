using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    public int MaxHeals = 3;

    public int CurrentHeals = 3;

    public int HealAmount = 50;

    private RestPoint respawnPoint = null;

    private RestPoint defaultRespawnPoint = null;

    private HashSet<IReset> resetRegistry = new HashSet<IReset>();

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

    public void LoadGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void LoadMenu()
    {
        RestoreDefaults();

        SceneManager.LoadScene("MainMenu");
    }

    public void RestoreDefaults()
    {
        resetRegistry.Clear();

        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        CurrentPoise = MaxPoise;
        CurrentHeals = MaxHeals;
    }

    public void RegisterReset(IReset reset)
    {
        resetRegistry.Add(reset);
    }

    void InitiateReset()
    {
        foreach (IReset reset in resetRegistry)
        {
            if (reset != null)
            {
                reset.DoReset();
            }
        }
    }

    public void SetRespawn(RestPoint point)
    {
        respawnPoint = point;
    }

    public RestPoint GetRespawnPoint()
    {
        if (respawnPoint == null) return defaultRespawnPoint;

        return respawnPoint;
    }

    public void SetDefaultRespawn(RestPoint point)
    {
        defaultRespawnPoint = point;
    }

    public void ResetWorld()
    {
        //TODO
        RestoreHealing();
        RestoreVitals();
        InitiateReset();
    }

    void RestoreHealing()
    {
        CurrentHeals = MaxHeals;
    }

    void RestoreVitals()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        CurrentPoise = MaxPoise;
    }
}
