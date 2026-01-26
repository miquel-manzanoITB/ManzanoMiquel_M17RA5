using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(MoveBehaviour))]
[RequireComponent(typeof(CameraSwitcher))]

public class PlayerControler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;
    [SerializeField] private Transform cameraTransform;
    private MoveBehaviour _mb;
    private CameraSwitcher _cs;

    private Vector3 moveInput;

    public void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.SetCallbacks(this);
        _mb = GetComponent<MoveBehaviour>();
        _cs = GetComponent<CameraSwitcher>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Change camera action triggered");
        if (context.performed)
        {
            _cs.ToggleCamera();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        Debug.Log("Crouch action triggered");
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact action triggered");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump action triggered");
        if (context.performed)
        {
            _mb.JumpCharacter();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Debug.Log("Look action triggered");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        Debug.Log("Next action triggered");
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        Debug.Log("Previous action triggered");
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        Debug.Log("Sprint action triggered");
    }

    public void OnEnable()
    {
        inputActions.Player.Enable();
    }

    public void OnDisable()
    {
        inputActions.Player.Disable();
    }


    void FixedUpdate()
    {
        Vector3 camForward = cameraTransform.transform.forward;
        Vector3 camRight = cameraTransform.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        _mb.MoveCharacter(moveDir);
    }
}
