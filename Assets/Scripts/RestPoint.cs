using UnityEngine;
using UnityEngine.InputSystem;

public class RestPoint : MonoBehaviour, IInteractable
{
    public FogEffect fog;

    private InputAction interactAction;

    public float InteractDistance = 1.5f;

    public bool IsDefaultRespawn = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");

        interactAction.started += AttemptInteract;

        if (IsDefaultRespawn)
        {
            DataManager.Instance.SetDefaultRespawn(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerManager.Instance != null)
        {
            CheckForInteract();
        }
    }

    void OnDestroy()
    {
        interactAction.started -= AttemptInteract;
    }

    public float GetInteractDistance()
    {
        return InteractDistance;
    }

    public string GetInteractVerb()
    {
        return "REST";
    }

    void CheckForInteract()
    {
        Vector3 playerPosition = PlayerManager.Instance.gameObject.transform.position;
        float distance = Vector3.Distance(playerPosition, transform.position);

        PlayerState state = PlayerManager.Instance.State;

        if(state == PlayerState.Ready && distance <= InteractDistance && !PlayerInterface.Instance.IsMenuOpen)
        {
            DisplayInteractPrompt();

            if(interactAction.IsPressed())
            {
                //Interact();
            }
        }
        else
        {
            HideInteractPrompt();
        }
    }

    void AttemptInteract(InputAction.CallbackContext context)
    {
        Vector3 playerPosition = PlayerManager.Instance.gameObject.transform.position;
        float distance = Vector3.Distance(playerPosition, transform.position);

        PlayerState state = PlayerManager.Instance.State;

        if (state == PlayerState.Ready && distance <= InteractDistance && !PlayerInterface.Instance.IsMenuOpen)
        {
            Interact();
        }
    }

    void DisplayInteractPrompt()
    {
        PlayerInterface.Instance.InteractableInRange(this);
    }

    void HideInteractPrompt()
    {
        PlayerInterface.Instance.InteractableExitRange(this);
    }

    void Interact()
    {
        PlayerManager.Instance.StartRest(this);
    }

    public void PlayEnable()
    {
        fog.PlayEnable();
    }

    public void PlayDisable()
    {
        fog.PlayDisable();
    }

    public void PrepareArea()
    {
        if(TransitionManager.Instance != null)
        {
            TransitionManager.Instance.InitState(transform.parent.gameObject);
        }
    }
}
