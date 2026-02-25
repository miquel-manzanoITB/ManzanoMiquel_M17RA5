using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// PlayerJumpBehaviour
// Salt amb coyote time i jump buffering.
//
//   Coyote time   — pots saltar uns ms DESPRÉS de caure d'una vora.
//   Jump buffer   — si prems espai just ABANS d'aterrar, el salt s'executa
//                   en cuanto toques terra.
//   Salt variable — soltar espai durant la pujada retalla el salt.
//                   L'estat IsJumpHeld ara ve de PlayerInputController,
//                   sense accedir al Input System directament des d'aquí.
// ════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerJumpBehaviour : MonoBehaviour
{
    [Header("Salt")]
    [SerializeField] private float jumpForce = 6f;

    [Header("Coyote time")]
    [Tooltip("Segons després de caure d'una vora en els quals encara pots saltar.")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Jump buffer")]
    [Tooltip("Segons abans de tocar terra en els quals pots pre-prémer el salt.")]
    [SerializeField] private float jumpBufferTime = 0.15f;

    // Llegit per PlayerMovementBehaviour per al salt variable
    public bool IsJumpHeld => _input.IsJumpHeld;

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker _ground;
    private PlayerAnimationBehaviour _animation;

    private float _coyoteTimer;
    private float _jumpBufferTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _ground = GetComponent<PlayerGroundChecker>();
        _animation = GetComponent<PlayerAnimationBehaviour>();

        _input.OnJumpEvent += HandleJumpPressed;
    }

    private void OnDestroy() => _input.OnJumpEvent -= HandleJumpPressed;

    private void Update()
    {
        UpdateTimers();
        TryJump();
    }

    private void UpdateTimers()
    {
        if (_ground.IsGrounded)
            _coyoteTimer = coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;

        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;
    }

    private void TryJump()
    {
        if (_coyoteTimer > 0f && _jumpBufferTimer > 0f)
            ExecuteJump();
    }

    private void HandleJumpPressed()
    {
        // No saltem mentre ballem (IsDancing ve de PlayerAnimationBehaviour)
        if (_animation != null && _animation.IsDancing) return;
        _jumpBufferTimer = jumpBufferTime;
    }

    private void ExecuteJump()
    {
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}