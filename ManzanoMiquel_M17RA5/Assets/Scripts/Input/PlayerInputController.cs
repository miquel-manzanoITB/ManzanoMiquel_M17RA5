using static InputSystem_Actions;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
/// Llegeix tots els inputs via el New Input System i els exposa com a events.
/// Cap altre Behaviour llegeix input directament — tot passa per aquí.
/// </summary>
public class PlayerInputController : MonoBehaviour, IPlayerActions
{
    // ── Moviment ──────────────────────────────────────────────────────────────
    public event UnityAction<Vector2> OnMoveEvent = delegate { };
    public event UnityAction<Vector2> OnLookEvent = delegate { };
    public event UnityAction OnJumpEvent = delegate { };

    // ── Combat ────────────────────────────────────────────────────────────────
    public event UnityAction OnAttackEvent = delegate { };  // Només animació
    public event UnityAction<bool> OnAimEvent = delegate { };  // true=apuntar, false=deixar

    // ── Ball ──────────────────────────────────────────────────────────────────
    public event UnityAction OnDanceEvent = delegate { };

    // ── Interacció ────────────────────────────────────────────────────────────
    public event UnityAction OnInteractEvent = delegate { };

    public event UnityAction OnSkipVideoEvent = delegate { };

    // ── Global (static perquè UIManager no necessita referència) ─────────────
    public static event UnityAction OnPauseGameEvent;

    // ─────────────────────────────────────────────────────────────────────────
    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.SetCallbacks(this);
    }

    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

    // ── Callbacks de IPlayerActions ───────────────────────────────────────────

    public void OnMove(InputAction.CallbackContext context)
        => OnMoveEvent.Invoke(context.ReadValue<Vector2>());

    public void OnLook(InputAction.CallbackContext context)
        => OnLookEvent.Invoke(context.ReadValue<Vector2>());

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log($"Jump input");
        if (context.performed) OnJumpEvent.Invoke();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed) OnAttackEvent.Invoke();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed) OnAimEvent.Invoke(true);
        if (context.canceled) OnAimEvent.Invoke(false);
    }

    public void OnDance(InputAction.CallbackContext context)
    {
        if (context.performed) OnDanceEvent.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log($"Interact input");
        if (context.performed) OnInteractEvent.Invoke();
    }

    public void OnPauseGame(InputAction.CallbackContext context)
    {
        if (context.performed) OnPauseGameEvent?.Invoke();
    }

    public void OnSkipVideo(InputAction.CallbackContext context)
    {
        if (context.performed) OnSkipVideoEvent.Invoke();
    }
}