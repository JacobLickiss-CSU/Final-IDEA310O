using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public AudioSource MusicPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject PlayButton = GameObject.Find("PlayButton");
        if (PlayButton)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(PlayButton);
        }
    }

    void Awake()
    {
        if(MusicManager.Instance == null || MusicManager.Instance.gameObject == null)
        {
            MusicPlayer.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Keep the cursor unlocked while active
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void StartGame()
    {
        if(MusicManager.Instance != null && MusicManager.Instance.gameObject != null)
        {
            Destroy(MusicManager.Instance.gameObject);
        }

        DataManager.Instance.LoadGame();
    }
}
