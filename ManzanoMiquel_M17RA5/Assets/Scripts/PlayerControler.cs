using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MoveBehaviour))]

public class PlayerControler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;
    private MoveBehaviour _mb;

    private Vector3 vectorInput;

    public void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.SetCallbacks(this);
        _mb = GetComponent<MoveBehaviour>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Attack action triggered");
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
        Vector2 vector2Input = context.ReadValue<Vector2>();

        vectorInput = new Vector3(vector2Input.x, vectorInput.y, vector2Input.y);
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
        _mb.MoveCharacter(vectorInput);
    }
}
