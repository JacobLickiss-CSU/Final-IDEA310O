using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour, IInteractable
{
    private InputAction interactAction;

    public float InteractDistance = 1.5f;

    public LeverDoor leverDoor;

    public Transform leverBone;

    public Transform StandSpot;

    public Vector3 leverOnRotation;

    public bool Position = false;

    public float SwitchSpeed = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");

        interactAction.started += AttemptInteract;
    }

    void OnDestroy()
    {
        interactAction.started -= AttemptInteract;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager.Instance != null)
        {
            CheckForInteract();
        }

        if (Position)
        {
            //if(Vector3.Distance(leverBone.localEulerAngles, leverOnRotation) > 3)
            //{
            //    leverBone.localEulerAngles = Vector3.Lerp(leverBone.localEulerAngles, leverOnRotation, SwitchSpeed * Time.deltaTime);
            //}
            if(leverOnRotation.x > 0)
            {
                leverBone.Rotate(new Vector3(-SwitchSpeed * Time.deltaTime, 0, 0));
                leverOnRotation.x -= SwitchSpeed * Time.deltaTime;
            }
        }
    }

    public void Switch()
    {
        leverDoor?.Open();
        Position = true;
    }

    protected void OnTriggerEnter(Collider collider)
    {
        if (PlayerManager.Instance.IsAttackActive && collider.tag == "PlayerWeapon")
        {
            Switch();
        }
    }

    public float GetInteractDistance()
    {
        return InteractDistance;
    }

    public string GetInteractVerb()
    {
        return "FLIP";
    }

    void CheckForInteract()
    {
        if (Position)
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
        if (Position)
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
        PlayerManager.Instance.StartSwitch(this);
    }
}
