using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// PlayerMovementBehaviour
// Moviment estil Half-Life 2 / Mirror's Edge.
//
// Fixes inclosos:
//   1. PhysicsMaterial sense fricció — evita que el Rigidbody "tropessi"
//      amb vores i rebi impulsos laterals inesperats.
//   2. Step offset — puja escalons i baches petits automàticament.
//   3. Velocitat projectada sobre el pendent — no "vola" en rampes.
//   4. En 1a persona, NO rota el cos des d'aquí (ho fa PlayerLookBehaviour).
// ════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerMovementBehaviour : MonoBehaviour
{
    [Header("Velocitat")]
    [SerializeField] private float runSpeed = 8f;

    [Header("Acceleració")]
    [Tooltip("60-100 recomanat. Alt = resposta ràpida amb inèrcia.")]
    [SerializeField] private float acceleration = 80f;
    [Tooltip("Una mica menys que acceleration per tenir lliscament en aturar.")]
    [SerializeField] private float deceleration = 55f;

    [Header("Control aeri")]
    [Tooltip("0 = sense control a l'aire. 1 = igual que a terra.")]
    [SerializeField][Range(0f, 1f)] private float airControl = 0.35f;

    [Header("Gravetat extra")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2.0f;

    [Header("Step offset")]
    [Tooltip("Altura màxima d'obstacle que el personatge puja automàticament.")]
    [SerializeField] private float stepHeight = 0.35f;
    [SerializeField] private float stepSmooth = 12f;
    [SerializeField] private LayerMask stepMask = ~0;

    // Llegit per PlayerLookBehaviour per al FOV boost
    public float MaxSpeed => runSpeed;

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker _ground;
    private PlayerJumpBehaviour _jump;
    private PlayerAnimationBehaviour _animation;
    private PlayerLookBehaviour _look;
    private Camera _camera;
    private CapsuleCollider _capsule;
    private Collider _collider;

    private Vector2 _moveInput;
    private Vector3 _wallNormal;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _collider = GetComponent<Collider>();
        _capsule = GetComponent<CapsuleCollider>();
        _input = GetComponent<PlayerInputController>();
        _ground = GetComponent<PlayerGroundChecker>();
        _jump = GetComponent<PlayerJumpBehaviour>();
        _animation = GetComponent<PlayerAnimationBehaviour>();
        _look = GetComponent<PlayerLookBehaviour>();
        _camera = Camera.main;

        _input.OnMoveEvent += v => _moveInput = v;

        SetupPhysicsMaterial();

        if (_capsule != null)
            stepHeight = Mathf.Min(stepHeight, _capsule.radius * 0.9f);
    }

    private void OnDestroy() => _input.OnMoveEvent -= v => _moveInput = v;

    private void SetupPhysicsMaterial()
    {
        if (_collider == null || _collider.sharedMaterial != null) return;

        _collider.material = new PhysicsMaterial("PlayerNoFriction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounciness = 0f,
            bounceCombine = PhysicsMaterialCombine.Minimum,
        };
    }

    private void FixedUpdate()
    {
        if (_animation != null && _animation.IsDancing) return;

        ApplyBetterGravity();
        ApplyHorizontalMovement();

        if (_ground.IsGrounded && _moveInput.magnitude > 0.01f)
            TryStepUp();
    }

    // ── Gravetat millorada ────────────────────────────────────────────────────

    private void ApplyBetterGravity()
    {
        float vy = _rb.linearVelocity.y;

        if (vy < 0f)
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y
                                * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (vy > 0f && !(_jump != null && _jump.IsJumpHeld))
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y
                                * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    // ── Moviment horitzontal ──────────────────────────────────────────────────

    private void ApplyHorizontalMovement()
    {
        Vector3 camRight = _camera.transform.right;
        Vector3 camForward = _camera.transform.forward;
        camRight.y = 0f; camRight.Normalize();
        camForward.y = 0f; camForward.Normalize();

        Vector3 wishDir = (camRight * _moveInput.x + camForward * _moveInput.y).normalized;

        // Anti-wall stick
        if (_wallNormal != Vector3.zero && wishDir != Vector3.zero
            && Vector3.Dot(wishDir, _wallNormal) < 0f)
        {
            wishDir = Vector3.ProjectOnPlane(wishDir, _wallNormal).normalized;
        }

        bool grounded = _ground.IsGrounded;

        // Projecta sobre el pendent per no frenar en rampes
        if (grounded && _ground.GroundNormal != Vector3.up)
        {
            float angle = Vector3.Angle(Vector3.up, _ground.GroundNormal);
            if (angle < 45f)
                wishDir = Vector3.ProjectOnPlane(wishDir, _ground.GroundNormal).normalized;
        }

        float accelDt = (grounded ? acceleration : acceleration * airControl) * Time.fixedDeltaTime;
        float decelDt = (grounded ? deceleration : deceleration * airControl) * Time.fixedDeltaTime;

        Vector3 hVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (wishDir.magnitude > 0.01f)
        {
            _rb.AddForce(Vector3.ClampMagnitude(wishDir * runSpeed - hVel, accelDt),
                         ForceMode.VelocityChange);

            // En 1a persona el cos el gira PlayerLookBehaviour.
            // Si ho fem aquí també, lluiten i provoquen glitch.
            if (_look == null || !_look.IsFirstPerson)
            {
                float rotSpeed = grounded ? 14f : 5f;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(wishDir),
                    rotSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            _rb.AddForce(Vector3.ClampMagnitude(-hVel, decelDt), ForceMode.VelocityChange);
        }

        // Suprimeix petites velocitats verticals negatives causades per baches
        if (grounded && _rb.linearVelocity.y < 0f && _rb.linearVelocity.y > -2f)
        {
            Vector3 vel = _rb.linearVelocity;
            vel.y = 0f;
            _rb.linearVelocity = vel;
        }
    }

    // ── Step offset ───────────────────────────────────────────────────────────

    private void TryStepUp()
    {
        Vector3 moveDir = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).normalized;
        if (moveDir == Vector3.zero) return;

        float radius = _capsule != null
            ? _capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z)
            : 0.3f;

        Vector3 origin = transform.position + Vector3.up * 0.05f;

        bool hitLow = Physics.SphereCast(origin, radius * 0.5f, moveDir,
            out RaycastHit hitLowInfo, radius + 0.1f, stepMask, QueryTriggerInteraction.Ignore);

        if (!hitLow || hitLowInfo.normal.y < 0.1f) return;

        Vector3 originHigh = origin + Vector3.up * stepHeight;
        bool hitHigh = Physics.SphereCast(originHigh, radius * 0.5f, moveDir,
            out _, radius + 0.1f, stepMask, QueryTriggerInteraction.Ignore);

        if (hitHigh) return;

        transform.position = Vector3.Lerp(
            transform.position,
            transform.position + Vector3.up * stepHeight,
            stepSmooth * Time.fixedDeltaTime);
    }

    // ── Detecció de parets ────────────────────────────────────────────────────

    private void OnCollisionStay(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
        {
            if (c.normal.y < 0.15f) { _wallNormal = c.normal; return; }
        }
        _wallNormal = Vector3.zero;
    }

    private void OnCollisionExit(Collision col) => _wallNormal = Vector3.zero;
}