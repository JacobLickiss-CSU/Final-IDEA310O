using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System.Collections.Generic;

public class PlayerInterface : MonoBehaviour
{
    public static PlayerInterface Instance { get; private set; }

    public UnityEngine.UI.Image GameOverFilter;

    public RawImage GameOverCharacter;

    public UnityEngine.UI.Image GameOverTextBack;

    public TextMeshProUGUI GameOverText;

    public UIDocument MainHUD;

    private Dictionary<VisualElement, Color> baseColors = new Dictionary<VisualElement, Color>();

    private Dictionary<VisualElement, Color> baseBackgroundColors = new Dictionary<VisualElement, Color>();

    private Dictionary<VisualElement, Color> baseTints = new Dictionary<VisualElement, Color>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        SaveColors();
    }

    void SaveColors()
    {
        if (MainHUD != null)
        {
            MainHUD.rootVisualElement.Query<VisualElement>().ForEach(element =>
            {
                StyleColor baseColor = element.resolvedStyle.color;
                StyleColor baseBackgroundColor = element.resolvedStyle.backgroundColor;
                StyleColor baseTint = element.resolvedStyle.unityBackgroundImageTintColor;
                baseColors.Add(element, new Color(baseColor.value.r, baseColor.value.g, baseColor.value.b, baseColor.value.a));
                baseBackgroundColors.Add(element, new Color(baseBackgroundColor.value.r, baseBackgroundColor.value.g, baseBackgroundColor.value.b, baseBackgroundColor.value.a));
                baseTints.Add(element, new Color(baseTint.value.r, baseTint.value.g, baseTint.value.b, baseTint.value.a));
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (MainHUD != null && DataManager.Instance != null)
        {
            float healthWidth = (float)DataManager.Instance.CurrentHealth / (float)DataManager.Instance.MaxHealth;
            var healthBarOver = MainHUD.rootVisualElement.Q<VisualElement>("HealthBarOver");
            healthBarOver.style.width = Length.Percent(healthWidth * 100);

            var healthBarUnder = MainHUD.rootVisualElement.Q<VisualElement>("HealthBarUnder");
            healthBarUnder.style.width = Length.Percent(healthWidth * 100);

            float staminaWidth = (float)DataManager.Instance.CurrentStamina / (float)DataManager.Instance.MaxStamina;
            var staminaBar = MainHUD.rootVisualElement.Q<VisualElement>("StaminaBar");
            staminaBar.style.width = Length.Percent(staminaWidth * 100);

            var healingText = MainHUD.rootVisualElement.Q<Label>("HealCount");
            healingText.text = ""+DataManager.Instance.CurrentHeals;
        }
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

        if (MainHUD != null)
        {
            MainHUD.rootVisualElement.Query<VisualElement>().ForEach(element =>
            {
                Color baseColor = baseColors[element];
                Color baseBackgroundColor = baseBackgroundColors[element];
                Color baseTint = baseTints[element];
                element.style.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - progress));
                element.style.backgroundColor = new Color(baseBackgroundColor.r, baseBackgroundColor.g, baseBackgroundColor.b, baseBackgroundColor.a * (1f - progress));
                element.style.unityBackgroundImageTintColor = new Color(baseTint.r, baseTint.g, baseTint.b, baseTint.a * (1f - progress));
            });
        }
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
