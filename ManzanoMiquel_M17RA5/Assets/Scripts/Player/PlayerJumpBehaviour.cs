using UnityEngine;

/// <summary>
/// Salto con mecánicas pro:
///
///   Coyote time     — puedes saltar hasta X segundos DESPUÉS de caer de un borde.
///                     Elimina la sensación de "no me ha cogido el salto".
///
///   Jump buffering  — si pulsas espacio justo ANTES de tocar el suelo,
///                     el salto se ejecuta en cuanto aterrizas (ventana de X ms).
///                     Hace que el juego "lea" mejor tus intenciones.
///
///   IsJumpHeld      — leído por PlayerMovementBehaviour para el variable jump height.
///                     Soltar espacio durante la subida recorta el salto.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerJumpBehaviour : MonoBehaviour
{
    [Header("Salto")]
    [SerializeField] private float jumpForce = 6f;

    [Header("Coyote time")]
    [Tooltip("Segundos después de caer de un borde en los que todavía puedes saltar.")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Jump buffer")]
    [Tooltip("Segundos antes de tocar el suelo en los que puedes pre-pulsar el salto.")]
    [SerializeField] private float jumpBufferTime = 0.15f;

    // Leído por PlayerMovementBehaviour para el salto variable
    public bool IsJumpHeld { get; private set; }

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker _ground;
    private PlayerDanceBehaviour _dance;

    private float _coyoteTimer;     // Tiempo desde que dejó de estar en tierra
    private float _jumpBufferTimer; // Tiempo desde que se pulsó el salto

    private bool _wasGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _ground = GetComponent<PlayerGroundChecker>();
        _dance = GetComponent<PlayerDanceBehaviour>();

        _input.OnJumpEvent += HandleJumpPressed;
    }

    private void OnDestroy() => _input.OnJumpEvent -= HandleJumpPressed;

    private void Update()
    {
        // Detectar si el botón de salto sigue pulsado (para variable jump height)
        // Usamos el Input System directamente aquí para leer el estado continuo
        IsJumpHeld = UnityEngine.InputSystem.Keyboard.current != null
                     && UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed;

        UpdateTimers();
        TryJump();
    }

    private void UpdateTimers()
    {
        bool grounded = _ground.IsGrounded;

        // Coyote time: empieza a contar cuando dejamos de estar en tierra
        if (grounded)
        {
            _coyoteTimer = coyoteTime;
            _wasGrounded = true;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        // Jump buffer: cuenta hacia atrás desde el último press
        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;
    }

    private void TryJump()
    {
        bool canJump = _coyoteTimer > 0f;  // En tierra o en ventana coyote
        bool wantsJump = _jumpBufferTimer > 0f; // Hay un jump en el buffer

        if (canJump && wantsJump)
            ExecuteJump();
    }

    private void HandleJumpPressed()
    {
        if (_dance != null && _dance.IsDancing) return;

        // Guardamos la intención en el buffer
        _jumpBufferTimer = jumpBufferTime;
    }

    private void ExecuteJump()
    {
        // Consumir el buffer y el coyote time
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;

        // Reseteamos velocidad vertical antes del impulso para que el salto
        // siempre tenga la misma altura independientemente de si bajábamos
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
