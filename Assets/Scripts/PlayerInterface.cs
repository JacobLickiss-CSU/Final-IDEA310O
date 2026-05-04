using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInterface : MonoBehaviour
{
    public static PlayerInterface Instance { get; private set; }

    public Image GameOverFilter;

    public RawImage GameOverCharacter;

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
        SetRenderTextureDimensions();

        GameOverFilter.color = new Color(0f, 0f, 0f, progress);
        GameOverCharacter.color = new Color(GameOverText.color.r, GameOverText.color.g, GameOverText.color.b, showText ? progress : 0);
        GameOverTextBack.color = new Color(0f, 0f, 0f, showText ? progress : 0);
        GameOverText.color = new Color(GameOverText.color.r, GameOverText.color.g, GameOverText.color.b, showText ? progress : 0);

        bool DeathCamEnabled = GameOverCharacter.color.a > 0;
        GameOverCharacter.gameObject.SetActive(DeathCamEnabled);
        DeathCamera.Instance?.gameObject.SetActive(DeathCamEnabled);
    }

    void SetRenderTextureDimensions()
    {
        RenderTexture renderTexture = GameOverCharacter.mainTexture as RenderTexture;
        if(renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        {
            renderTexture.Release();
            renderTexture.width = Screen.width;
            renderTexture.height = Screen.height;
            renderTexture.Create();
        }
    }
}
