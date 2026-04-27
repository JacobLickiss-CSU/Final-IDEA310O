using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public CameraManager Camera;

    public GameObject model;

    private InputAction moveAction;

    public Animator modelAnimator;

    public float gravityValue = -9.81f;

    public float walkSpeed = -1f; // TODO

    public float runSpeed = 5f; // TODO

    public float sprintSpeed = -1f; // TODO

    public float turnSpeed = 1f;

    public PlayerState State { get; private set; } = PlayerState.Ready;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupInputs();
    }

    void SetupInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (moveValue.magnitude > 0)
        {
            // TODO move somewhere more organized
            modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Run"));

            Vector3 cameraForward = Camera.GetForward();
            Vector3 cameraRight = Camera.GetRight();

            Vector2 moveForward = new Vector2(cameraForward.x, cameraForward.z);
            moveForward.Normalize();

            Vector2 moveRight = new Vector2(cameraRight.x, cameraRight.z);
            moveRight.Normalize();

            Vector2 forwardMovement = moveValue.y * moveForward;
            Vector2 sideMovement = moveValue.x * moveRight;

            Vector2 totalMovement = forwardMovement + sideMovement;
            totalMovement.Normalize();

            Vector3 lookDirection = new Vector3(totalMovement.x, 0, totalMovement.y);
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(lookDirection), turnSpeed * Time.deltaTime);

            Vector3 finalMove = new Vector3(totalMovement.x, 0, totalMovement.y) * runSpeed;

            finalMove.y += gravityValue;

            GetComponent<CharacterController>().Move(finalMove * Time.deltaTime);
        }
        else
        {
            // TODO move somewhere more organized
            modelAnimator.Play(Animator.StringToHash("RobogirlArmature|Idle"));
        }
    }

    public bool CanLook()
    {
        // TODO
        return Cursor.lockState == CursorLockMode.Locked && !Cursor.visible;
        //return true;
    }
}

public enum PlayerState
{
    Ready,
    Attacking,
    Hit,
    Rolling,
    Dead,
    Frozen
}
