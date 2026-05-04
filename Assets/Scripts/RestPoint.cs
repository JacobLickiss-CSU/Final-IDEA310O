using UnityEngine;
using UnityEngine.InputSystem;

public class RestPoint : MonoBehaviour
{
    public FogEffect fog;

    private InputAction interactAction;

    public float InteractDistance = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerManager.Instance != null)
        {
            CheckForInteract();
        }
    }

    void CheckForInteract()
    {
        Vector3 playerPosition = PlayerManager.Instance.gameObject.transform.position;
        float distance = Vector3.Distance(playerPosition, transform.position);

        PlayerState state = PlayerManager.Instance.State;

        if(state == PlayerState.Ready && distance <= InteractDistance)
        {
            DisplayInteractPrompt();

            if(interactAction.IsPressed())
            {
                Interact();
            }
        }
        else
        {
            HideInteractPrompt();
        }
    }

    void DisplayInteractPrompt()
    {
        // TODO
    }

    void HideInteractPrompt()
    {
        // TODO
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
}
