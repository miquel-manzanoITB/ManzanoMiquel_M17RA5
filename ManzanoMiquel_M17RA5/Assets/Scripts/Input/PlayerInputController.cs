using static InputSystem_Actions;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// PlayerInputController
// Llegeix tots els inputs via el New Input System i els exposa com a events.
// Cap altre Behaviour llegeix input directament — tot passa per aquí.
// ════════════════════════════════════════════════════════════════════════════

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

    // ── Global (estàtic per a UIManager) ─────────────────────────────────────
    public static event UnityAction OnPauseGameEvent;

    // ── Estat del salt (llegit per PlayerJumpBehaviour) ───────────────────────
    public bool IsJumpHeld { get; private set; }

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
    /// Bloqueja/desbloqueja les accions.
    /// blockLook=true també bloqueja la càmera (ball).
    /// blockLook=false deixa la càmera lliure (atac).
    /// </summary>
    public void SetInputEnabled(bool enabled, bool blockLook = false)
    {
        _actionsEnabled = enabled;
        _lookEnabled = enabled || !blockLook;

        if (!enabled)
            OnMoveEvent.Invoke(Vector2.zero);
    }

    // ── Callbacks IPlayerActions ──────────────────────────────────────────────

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (_actionsEnabled) OnMoveEvent.Invoke(ctx.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (_lookEnabled) OnLookEvent.Invoke(ctx.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!_actionsEnabled) return;
        if (ctx.performed) OnJumpEvent.Invoke();
        // Exposem l'estat del botó per al salt variable (sense accedir al Input System des de fora)
        IsJumpHeld = ctx.performed || ctx.started;
        if (ctx.canceled) IsJumpHeld = false;
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_actionsEnabled && ctx.performed) OnAttackEvent.Invoke();
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (!_actionsEnabled) return;
        if (ctx.performed) OnAimEvent.Invoke(true);
        if (ctx.canceled) OnAimEvent.Invoke(false);
    }

    public void OnDance(InputAction.CallbackContext ctx)
    {
        if (_actionsEnabled && ctx.performed) OnDanceEvent.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_actionsEnabled && ctx.started) OnInteractEvent.Invoke();
    }

    public void OnPauseGame(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnPauseGameEvent?.Invoke();
    }

    public void OnSkipVideo(InputAction.CallbackContext ctx)
    {
        if (_actionsEnabled && ctx.performed) OnSkipVideoEvent.Invoke();
    }
}