using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInterface : MonoBehaviour
{
    public static PlayerInterface Instance { get; private set; }

    public UnityEngine.UI.Image GameOverFilter;

    public RawImage GameOverCharacter;

    public UnityEngine.UI.Image GameOverTextBack;

    public TextMeshProUGUI GameOverText;

    public UIDocument MainHUD;

    public GameMenuManager MenuManager;

    private InputAction menuAction;

    private Dictionary<VisualElement, Color> baseColors = new Dictionary<VisualElement, Color>();

    private Dictionary<VisualElement, Color> baseBackgroundColors = new Dictionary<VisualElement, Color>();

    private Dictionary<VisualElement, Color> baseTints = new Dictionary<VisualElement, Color>();

    private IInteractable interactable = null;

    public string CurrentScheme = "Keyboard&Mouse";

    public Texture Interact_Keyboard;

    public Texture Interact_Gamepad;

    public Texture Heal_Keyboard;

    public Texture Heal_Gamepad;

    public bool IsMenuOpen
    {
        get
        {
            return MenuManager.gameObject.activeSelf;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        SaveColors();

        menuAction = InputSystem.actions.FindAction("Menu");
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
        if(menuAction.triggered)
        {
            if (MenuManager.gameObject.activeSelf)
            {
                MenuManager.Continue();
            }
            else
            {
                MenuManager.Open();
            }
        }

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

            UpdateInputPrompt();
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

    public void InteractableInRange(IInteractable interactable)
    {
        if(this.interactable != null && this.interactable != interactable)
        {
            HideInteractable(interactable);
        }

        ShowInteractable(interactable);
    }

    public void InteractableExitRange(IInteractable interactable)
    {
        if (this.interactable == interactable)
        {
            HideInteractable(interactable);
        }
    }

    void ShowInteractable(IInteractable interactable)
    {
        this.interactable = interactable;

        VisibilityInteractPrompt(interactable.GetInteractVerb(), true);
    }

    void HideInteractable(IInteractable interactable)
    {
        VisibilityInteractPrompt("", false);

        this.interactable = null;
    }

    void VisibilityInteractPrompt(string message, bool visible)
    {
        if (MainHUD != null)
        {
            var interactPrompt = MainHUD.rootVisualElement.Q<UnityEngine.UIElements.VisualElement>("InteractPrompt");
            var interactMessage = MainHUD.rootVisualElement.Q<UnityEngine.UIElements.Label>("InteractText");

            if(interactPrompt != null && interactMessage != null)
            {
                interactPrompt.visible = visible;
                interactMessage.text = message;
            }
        }
    }

    void UpdateInputPrompt()
    {
        if(PlayerManager.Instance != null)
        {
            if(PlayerManager.Instance.State != PlayerState.Ready)
            {
                VisibilityInteractPrompt("", false);
            }
            else if(interactable != null)
            {
                ShowInteractable(this.interactable);
            }
        }
    }

    void OnControlsChanged(PlayerInput input)
    {
        if(input == null) return;

        if(CurrentScheme != input.currentControlScheme)
        {
            SetControlsUI(input.currentControlScheme);
            CurrentScheme = input.currentControlScheme;
        }
    }

    void SetControlsUI(string controlScheme)
    {
        if(controlScheme == null) return;

        if(MainHUD != null)
        {
            var interactImage = MainHUD.rootVisualElement.Q<UnityEngine.UIElements.Image>("InteractImage");

            switch(controlScheme)
            {
                case "Keyboard&Mouse": { interactImage.image = Interact_Keyboard; break; }
                case "Gamepad": { interactImage.image = Interact_Gamepad; break; }
                default: { interactImage.image = Interact_Keyboard; break; }
            }

            var healImage = MainHUD.rootVisualElement.Q<UnityEngine.UIElements.Image>("HealPrompt");

            switch (controlScheme)
            {
                case "Keyboard&Mouse": { healImage.image = Heal_Keyboard; break; }
                case "Gamepad": { healImage.image = Heal_Gamepad; break; }
                default: { healImage.image = Heal_Keyboard; break; }
            }
        }
    }

    
}
