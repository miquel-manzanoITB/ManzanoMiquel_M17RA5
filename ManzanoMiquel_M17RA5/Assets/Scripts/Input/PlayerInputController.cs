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
    public event UnityAction OnAttackEvent = delegate { };
    public event UnityAction<bool> OnAimEvent = delegate { };

    // ── Ball ──────────────────────────────────────────────────────────────────
    public event UnityAction OnDanceEvent = delegate { };

    // ── Interacció ────────────────────────────────────────────────────────────
    public event UnityAction OnInteractEvent = delegate { };
    public event UnityAction OnSkipVideoEvent = delegate { };

    // ── Global ────────────────────────────────────────────────────────────────
    public static event UnityAction OnPauseGameEvent;

    // ─────────────────────────────────────────────────────────────────────────
    private InputSystem_Actions _inputActions;
    private bool _actionsEnabled = true;
    private bool _lookEnabled = true;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.SetCallbacks(this);
    }

    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

    /// <summary>
    /// Bloqueja o desbloqueja les accions del jugador.
    /// blockLook = true també bloqueja la càmera (útil per al ball).
    /// blockLook = false deixa la càmera lliure (útil per a l'atac).
    /// </summary>
    public void SetInputEnabled(bool enabled, bool blockLook = false)
    {
        _actionsEnabled = enabled;
        _lookEnabled = enabled || !blockLook;

        // Si estem bloquejant, forcem un event de moviment a zero perquè
        // el personatge s'aturi immediatament encara que mantingui la tecla.
        if (!enabled)
            OnMoveEvent.Invoke(Vector2.zero);
    }

    // ── Callbacks de IPlayerActions ───────────────────────────────────────────

    public void OnMove(InputAction.CallbackContext context)
    {
        if (_actionsEnabled)
            OnMoveEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (_lookEnabled)
            OnLookEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_actionsEnabled && context.performed)
        {
            Debug.Log("Jump input");
            OnJumpEvent.Invoke();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (_actionsEnabled && context.performed) OnAttackEvent.Invoke();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (!_actionsEnabled) return;
        if (context.performed) OnAimEvent.Invoke(true);
        if (context.canceled) OnAimEvent.Invoke(false);
    }

    public void OnDance(InputAction.CallbackContext context)
    {
        if (_actionsEnabled && context.performed) OnDanceEvent.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (_actionsEnabled && context.started) OnInteractEvent.Invoke();
    }

    public void OnPauseGame(InputAction.CallbackContext context)
    {
        if (context.performed) OnPauseGameEvent?.Invoke();
    }

    public void OnSkipVideo(InputAction.CallbackContext context)
    {
        if (_actionsEnabled && context.performed) OnSkipVideoEvent.Invoke();
    }
}