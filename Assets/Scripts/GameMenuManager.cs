using UnityEngine;
using UnityEngine.EventSystems;

public class GameMenuManager : MonoBehaviour
{
    public GameObject SaveWarning;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Keep the cursor unlocked while active
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        SaveWarning.SetActive(false);

        GameObject ContinueButton = GameObject.Find("ContinueButton");
        if (ContinueButton)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(ContinueButton);
        }

        Time.timeScale = 0.0f;
    }

    public void Continue()
    {
        // Unpause time
        Time.timeScale = 1.0f;

        // Hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Close the menu
        gameObject.SetActive(false);
    }

    public void MainMenu()
    {
        if(SaveWarning.activeSelf)
        {
            // Unpause time
            Time.timeScale = 1.0f;

            // Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            DataManager.Instance.LoadMenu();
        }
        else
        {
            SaveWarning.SetActive(true);
        }
    }
}
