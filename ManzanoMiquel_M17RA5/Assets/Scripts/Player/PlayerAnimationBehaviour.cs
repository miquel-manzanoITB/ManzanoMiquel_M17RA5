using System.Collections;
using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// PlayerAnimationBehaviour
// Condueix l'Animator i gestiona les seqüències d'atac i ball.
//
// REFACTOR: S'ha eliminat PlayerDanceBehaviour (estava duplicat aquí).
//           Tota la lògica del ball viu en aquest script.
//           PlayerJumpBehaviour ara consulta IsDancing directament aquí.
//
// Paràmetres requerits a l'Animator:
//   Float   – Speed, VelocityY
//   Bool    – IsGrounded, Aiming, Dancing, IsAttacking
//   Trigger – Jump, Fire
// ════════════════════════════════════════════════════════════════════════════

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
    [Tooltip("Durada de l'animació de ball (s). Ha de coincidir amb el clip.")]
    [SerializeField] private float danceDuration = 5f;

    // Hashes en caché — evita cerques de string cada frame
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
    private PlayerGroundChecker _ground;
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
        _ground = GetComponent<PlayerGroundChecker>();
        _look = GetComponent<PlayerLookBehaviour>();

        _input.OnJumpEvent += () => animator.SetTrigger(H_Jump);
        _input.OnAttackEvent += HandleAttack;
        _input.OnAimEvent += HandleAim;
        _input.OnDanceEvent += HandleDance;
    }

    private void OnDestroy()
    {
        _input.OnJumpEvent -= () => animator.SetTrigger(H_Jump);
        _input.OnAttackEvent -= HandleAttack;
        _input.OnAimEvent -= HandleAim;
        _input.OnDanceEvent -= HandleDance;
    }

    private void Update()
    {
        Vector3 hVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        animator.SetFloat(H_Speed, hVel.magnitude);
        animator.SetFloat(H_VelocityY, _rb.linearVelocity.y);
        animator.SetBool(H_IsGrounded, _ground.IsGrounded);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void HandleAim(bool aiming)
    {
        _isAiming = aiming;
        animator.SetBool(H_Aiming, aiming);
    }

    private void HandleAttack()
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
        _input.SetInputEnabled(false, blockLook: false); // càmera lliure durant atac
        animator.SetBool(H_IsAttacking, true);

        yield return new WaitForSeconds(attackDuration);

        animator.SetBool(H_IsAttacking, false);
        _isAttacking = false;
        _input.SetInputEnabled(true);
    }

    private IEnumerator DanceRoutine()
    {
        IsDancing = true;
        _input.SetInputEnabled(false, blockLook: true); // càmera i accions bloquejades

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