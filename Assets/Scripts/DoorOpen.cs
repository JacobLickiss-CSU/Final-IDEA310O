using UnityEngine;
using UnityEngine.InputSystem;

public class DoorOpen : MonoBehaviour, IInteractable
{
    private InputAction interactAction;

    public GameObject Door;

    public GameObject DoorFacing;

    public float InteractDistance = 1.5f;

    private bool open = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");

        interactAction.started += AttemptInteract;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager.Instance != null)
        {
            CheckForInteract();
        }
    }

    void OnDestroy()
    {
        interactAction.started -= AttemptInteract;
    }

    public bool IsOpen()
    {
        return open;
    }

    public float GetInteractDistance()
    {
        return InteractDistance;
    }

    public string GetInteractVerb()
    {
        return "OPEN";
    }

    void CheckForInteract()
    {
        if(IsOpen())
        {
            HideInteractPrompt();
            return;
        }

        Vector3 playerPosition = PlayerManager.Instance.gameObject.transform.position;
        float distance = Vector3.Distance(playerPosition, transform.position);

        PlayerState state = PlayerManager.Instance.State;

        if (state == PlayerState.Ready && distance <= InteractDistance && !PlayerInterface.Instance.IsMenuOpen)
        {
            DisplayInteractPrompt();
        }
        else
        {
            HideInteractPrompt();
        }
    }

    void AttemptInteract(InputAction.CallbackContext context)
    {
        if (IsOpen())
        {
            return;
        }

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
        PlayerManager.Instance.StartOpen(this);

        Animation animation = Door.GetComponent<Animation>();
        foreach (AnimationState state in animation)
        {
            state.speed = 1f;
        }
        animation.Play("Level1DoorOpen");
    }

    public void PlayInterrupt()
    {
        if (IsOpen()) return;

        Animation animation = Door.GetComponent<Animation>();
        if(animation.isPlaying)
        {
            foreach (AnimationState state in animation)
            {
                state.speed = -1f;
            }
        }
    }

    public void SetOpen()
    {
        open = true;
    }
}
