using System.Collections;
using UnityEngine;

/// <summary>
/// Condueix l'Animator a partir de l'estat del Rigidbody i els events d'input.
///
/// Paràmetres requerits a l'Animator:
///   Float   – Speed       (magnitud de la velocitat horitzontal)
///   Float   – VelocityY   (velocitat vertical → fases del salt)
///   Bool    – IsGrounded
///   Bool    – Aiming
///   Bool    – Dancing
///   Bool    – IsAttacking  ← FIX: ara és Bool (s'activa i es desactiva automàticament)
///   Trigger – Jump
///   Trigger – Fire        (disparo mentre s'apunta)
///
/// CANVI: "Attack" era un Trigger. Ara és un Bool (IsAttacking) que s'activa
/// quan el jugador ataca i es desactiva quan l'animació de l'atac acaba.
/// Això evita que el Trigger es quedi "penjat" a la cua de l'Animator.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerAnimationBehaviour : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Atac")]
    [Tooltip("Nom exacte de l'estat d'atac a l'Animator (per llegir la durada del clip).")]
    [SerializeField] private string attackStateName = "Attack";
    [Tooltip("Si no es pot llegir la durada del clip, s'usa aquest temps per defecte (s).")]
    [SerializeField] private float fallbackAttackDuration = 0.5f;

    // Hashes en caché — mai strings en runtime
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

    private bool _isAiming;
    private bool _isAttacking;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _groundChecker = GetComponent<PlayerGroundChecker>();

        _input.OnJumpEvent += HandleJumpAnimation;
        _input.OnAttackEvent += HandleAttackAnimation;
        _input.OnAimEvent += HandleAim;
    }

    private void OnDestroy()
    {
        _input.OnJumpEvent -= HandleJumpAnimation;
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

    private void HandleJumpAnimation()
    {
        animator.SetTrigger(H_Jump);
    }

    private void HandleAim(bool aiming)
    {
        _isAiming = aiming;
        animator.SetBool(H_Aiming, aiming);
    }

    /// <summary>
    /// Si s'apunta → Trigger de Fire. Si no → activa IsAttacking (Bool) i el desactiva
    /// quan acaba el clip d'atac, de manera que la transició és neta.
    /// </summary>
    private void HandleAttackAnimation()
    {
        if (_isAiming)
        {
            animator.SetTrigger(H_Fire);
            return;
        }

        // Evitem encadenar atacs mentre n'hi ha un en curs
        if (_isAttacking) return;

        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        animator.SetBool(H_IsAttacking, true);

        // Esperem un frame perquè l'Animator entri a l'estat d'atac
        yield return null;

        // Llegim la durada del clip d'atac directament de l'Animator
        float duration = GetCurrentClipLength(attackStateName);

        yield return new WaitForSeconds(duration);

        animator.SetBool(H_IsAttacking, false);
        _isAttacking = false;
    }

    /// <summary>
    /// Retorna la durada del clip que s'està reproduint (o fallback si no el troba).
    /// </summary>
    private float GetCurrentClipLength(string stateName)
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
            return clipInfo[0].clip.length;

        // Fallback: busquem pel nom de l'estat
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Contains(stateName))
                return clip.length;
        }

        return fallbackAttackDuration;
    }

    // ── API pública ──────────────────────────────────────────────────────────

    public void SetDancing(bool dancing) => animator.SetBool(H_Dancing, dancing);

    /// <summary>Retorna si hi ha un atac en curs (útil per bloquejar altres accions).</summary>
    public bool IsAttacking => _isAttacking;
}
