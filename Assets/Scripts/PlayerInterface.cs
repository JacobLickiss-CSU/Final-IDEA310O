using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInterface : MonoBehaviour
{
    public static PlayerInterface Instance { get; private set; }

    public Image GameOverFilter;

    public Image GameOverTextBack;

    public TextMeshProUGUI GameOverText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGameOverProgress(float progress, bool showText = true)
    {
        GameOverFilter.color = new Color(0f, 0f, 0f, progress);
        GameOverTextBack.color = new Color(0f, 0f, 0f, showText ? progress : 0);
        GameOverText.color = new Color(GameOverText.color.r, GameOverText.color.g, GameOverText.color.b, showText ? progress : 0);
    }
}
