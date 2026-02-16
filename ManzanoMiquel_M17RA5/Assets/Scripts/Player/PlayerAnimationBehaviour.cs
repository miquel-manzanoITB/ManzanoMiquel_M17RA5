using UnityEngine;

/// <summary>
/// Condueix l'Animator a partir de l'estat del Rigidbody i els events d'input.
/// Atacar només activa un Trigger — sense hitbox ni dany.
///
/// Paràmetres requerits a l'Animator:
///   Float   – Speed       (magnitud de la velocitat horitzontal)
///   Float   – VelocityY   (velocitat vertical → fases del salt)
///   Bool    – IsGrounded
///   Bool    – Aiming      (apuntar → canvi de càmera)
///   Bool    – Dancing     (ball de la victòria)
///   Trigger – Jump
///   Trigger – Attack      (cop cuerpo a cuerpo — només animació)
///   Trigger – Fire        (disparo mentre s'apunta — només animació)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerAnimationBehaviour : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Hashes en caché — mai strings en runtime
    private static readonly int H_Speed = Animator.StringToHash("Speed");
    private static readonly int H_VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int H_IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int H_Aiming = Animator.StringToHash("Aiming");
    private static readonly int H_Dancing = Animator.StringToHash("Dancing");
    private static readonly int H_Jump = Animator.StringToHash("Jump");
    private static readonly int H_Attack = Animator.StringToHash("Attack");
    private static readonly int H_Fire = Animator.StringToHash("Fire");

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker _groundChecker;

    private bool _isAiming;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _groundChecker = GetComponent<PlayerGroundChecker>();

        _input.OnJumpEvent += () => animator.SetTrigger(H_Jump);
        _input.OnAttackEvent += HandleAttackAnimation;
        _input.OnAimEvent += HandleAim;
    }

    private void OnDestroy()
    {
        _input.OnJumpEvent -= () => animator.SetTrigger(H_Jump);
        _input.OnAttackEvent -= HandleAttackAnimation;
        _input.OnAimEvent -= HandleAim;
    }

    private void Update()
    {
        Vector3 horizontalVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        animator.SetFloat(H_Speed, horizontalVel.magnitude);
        animator.SetFloat(H_VelocityY, _rb.linearVelocity.y);
        animator.SetBool(H_IsGrounded, _groundChecker.IsGrounded);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void HandleAim(bool aiming)
    {
        _isAiming = aiming;
        animator.SetBool(H_Aiming, aiming);
    }

    // Apuntant → animació de disparo; sense apuntar → animació de cop
    private void HandleAttackAnimation()
    {
        if (_isAiming) animator.SetTrigger(H_Fire);
        else animator.SetTrigger(H_Attack);
    }

    // ── API pública (crida PlayerDanceBehaviour) ──────────────────────────────

    public void SetDancing(bool dancing) => animator.SetBool(H_Dancing, dancing);
}