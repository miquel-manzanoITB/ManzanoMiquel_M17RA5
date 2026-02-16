using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerJumpBehaviour : MonoBehaviour
{
    [Header("Salt")]
    [SerializeField] private float jumpPower = 5f;

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker _groundChecker;
    private PlayerDanceBehaviour _dance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _dance = GetComponent<PlayerDanceBehaviour>();

        _input.OnJumpEvent += HandleJump;
    }

    private void OnDestroy()
    {
        _input.OnJumpEvent -= HandleJump;
    }

    private void HandleJump()
    {
        // No es pot saltar durant el ball
        if (_dance != null && _dance.IsDancing) return;
        if (!_groundChecker.IsGrounded) return;

        _rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }
}