using System.Collections;
using UnityEngine;

/// <summary>
/// Condueix l'Animator a partir de l'estat del Rigidbody i els events d'input.
/// També gestiona el ball de victòria (abans a PlayerDanceBehaviour).
///
/// Durant l'ATAC:  bloqueja accions però permet moure la càmera.
/// Durant el BALL: bloqueja accions i càmera.
///
/// Paràmetres requerits a l'Animator:
///   Float   – Speed
///   Float   – VelocityY
///   Bool    – IsGrounded
///   Bool    – Aiming
///   Bool    – Dancing
///   Bool    – IsAttacking
///   Trigger – Jump
///   Trigger – Fire
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerAnimationBehaviour : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Atac")]
    [Tooltip("Durada de l'animació d'atac (s).")]
    [SerializeField] private float attackDuration = 0.5f;

    [Header("Ball")]
    [Tooltip("Durada de l'animació de ball (s). Ha de coincidir amb el clip de Mixamo.")]
    [SerializeField] private float danceDuration = 5f;

    // Hashes en caché
    private static readonly int H_Speed = Animator.StringToHash("Speed");
    private static readonly int H_VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int H_IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int H_Aiming = Animator.StringToHash("Aiming");
    private static readonly int H_Dancing = Animator.StringToHash("Dancing");
    private static readonly int H_Jump = Animator.StringToHash("Jump");
    private static readonly int H_IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int H_Fire = Animator.StringToHash("Fire");

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker _groundChecker;
    private PlayerLookBehaviour _look;

    private bool _isAiming;
    private bool _isAttacking;
    private Coroutine _attackCoroutine;

    public bool IsAttacking => _isAttacking;
    public bool IsDancing { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _look = GetComponent<PlayerLookBehaviour>();

        _input.OnJumpEvent += HandleJumpAnimation;
        _input.OnAttackEvent += HandleAttackAnimation;
        _input.OnAimEvent += HandleAim;
        _input.OnDanceEvent += HandleDance;
    }

    private void OnDestroy()
    {
        _input.OnJumpEvent -= HandleJumpAnimation;
        _input.OnAttackEvent -= HandleAttackAnimation;
        _input.OnAimEvent -= HandleAim;
        _input.OnDanceEvent -= HandleDance;
    }

    private void Update()
    {
        Vector3 horizontalVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        animator.SetFloat(H_Speed, horizontalVel.magnitude);
        animator.SetFloat(H_VelocityY, _rb.linearVelocity.y);
        animator.SetBool(H_IsGrounded, _groundChecker.IsGrounded);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void HandleJumpAnimation() => animator.SetTrigger(H_Jump);

    private void HandleAim(bool aiming)
    {
        _isAiming = aiming;
        animator.SetBool(H_Aiming, aiming);
    }

    private void HandleAttackAnimation()
    {
        if (_isAiming)
        {
            animator.SetTrigger(H_Fire);
            return;
        }

        if (_isAttacking) return;

        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private void HandleDance()
    {
        if (IsDancing) return;
        StartCoroutine(DanceRoutine());
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        // blockLook: false → la càmera segueix funcionant durant l'atac
        _input.SetInputEnabled(false, blockLook: false);
        animator.SetBool(H_IsAttacking, true);

        yield return new WaitForSeconds(attackDuration);

        animator.SetBool(H_IsAttacking, false);
        _isAttacking = false;
        _input.SetInputEnabled(true);
    }

    private IEnumerator DanceRoutine()
    {
        IsDancing = true;
        // blockLook: true → càmera i accions bloquejades durant el ball
        _input.SetInputEnabled(false, blockLook: true);

        _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        animator.SetBool(H_Dancing, true);
        _look?.SetFrontCamera(true);

        yield return new WaitForSeconds(danceDuration);

        _look?.SetFrontCamera(false);
        animator.SetBool(H_Dancing, false);
        IsDancing = false;
        _input.SetInputEnabled(true);
    }

    // ── API pública ───────────────────────────────────────────────────────────

    public void SetDancing(bool dancing) => animator.SetBool(H_Dancing, dancing);
}