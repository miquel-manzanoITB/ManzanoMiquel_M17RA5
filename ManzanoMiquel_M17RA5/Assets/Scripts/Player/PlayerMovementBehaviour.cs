using UnityEngine;

/// <summary>
/// Movimiento estilo Half-Life 2 / Mirror's Edge con fix de protuberancias.
///
/// FIXES para suelos con baches/protuberancias:
///
///   1. PhysicsMaterial automático — crea en Awake un material con fricción 0
///      en el collider del jugador. Sin fricción, el Rigidbody no "tropieza"
///      con bordes pequeños y no recibe empujones laterales inesperados.
///
///   2. Step offset (subir escalones) — antes de mover, hace un SphereCast
///      hacia adelante a ras de suelo. Si detecta un obstáculo bajo (< stepHeight),
///      teletransporta suavemente el personaje hacia arriba para subirlo.
///      Permite pasar por encima de protuberancias sin quedarse atascado.
///
///   3. Velocidad proyectada sobre el suelo — cuando hay suelo, la velocidad
///      horizontal se proyecta en el plano de la normal del suelo para que
///      el personaje no "vuele" al bajar rampas ni frene en ellas.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerMovementBehaviour : MonoBehaviour
{
    [Header("Velocidad")]
    [SerializeField] private float runSpeed = 8f;

    [Header("Aceleración")]
    [Tooltip("Recomendado 60-100. Alto = respuesta rápida con inercia.")]
    [SerializeField] private float acceleration = 80f;
    [Tooltip("Un poco menos que acceleration para que haya deslizamiento al parar.")]
    [SerializeField] private float deceleration = 55f;

    [Header("Control aéreo")]
    [Tooltip("0 = sin control en el aire. 1 = igual que en suelo.\nHL2≈0.15 | Mirror's Edge≈0.5")]
    [SerializeField] [Range(0f, 1f)] private float airControl = 0.35f;

    [Header("Gravedad extra")]
    [SerializeField] private float fallMultiplier    = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2.0f;

    [Header("Step offset (subir escalones/baches)")]
    [Tooltip("Altura máxima de obstáculo que el personaje sube automáticamente.")]
    [SerializeField] private float stepHeight = 0.35f;
    [Tooltip("Qué tan suave es la subida del escalón. Más alto = más instantáneo.")]
    [SerializeField] private float stepSmooth = 12f;
    [Tooltip("LayerMask del suelo/geometría para el step cast.")]
    [SerializeField] private LayerMask stepMask = ~0;

    [Header("Anti-wall stick")]
    [SerializeField] private float wallNormalThreshold = 0.25f;

    // Leído por PlayerLookBehaviour para el FOV boost
    public float MaxSpeed => runSpeed;

    // Referencias
    private Rigidbody             _rb;
    private PlayerInputController _input;
    private PlayerGroundChecker   _ground;
    private PlayerDanceBehaviour  _dance;
    private PlayerJumpBehaviour   _jump;
    private Camera                _camera;
    private Collider              _collider;
    private CapsuleCollider       _capsule;

    // Estado
    private Vector2 _moveInput;
    private Vector3 _wallNormal;

    private void Awake()
    {
        _rb       = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _collider = GetComponent<Collider>();
        _capsule  = GetComponent<CapsuleCollider>();
        _input    = GetComponent<PlayerInputController>();
        _ground   = GetComponent<PlayerGroundChecker>();
        _dance    = GetComponent<PlayerDanceBehaviour>();
        _jump     = GetComponent<PlayerJumpBehaviour>();
        _camera   = Camera.main;

        _input.OnMoveEvent += v => _moveInput = v;

        // FIX 1: PhysicsMaterial con fricción 0 para evitar que el Rigidbody
        // "trope" con bordes y reciba impulsos laterales inesperados.
        // DynamicFriction = 0 también evita que el personaje se quede pegado
        // a paredes inclinadas cuando lo empujan.
        SetupPhysicsMaterial();

        // El step height no puede superar el radio de la cápsula o el Rigidbody
        // puede recibir impulsos verticales incorrectos al subir escalones
        if (_capsule != null)
            stepHeight = Mathf.Min(stepHeight, _capsule.radius * 0.9f);
    }

    private void SetupPhysicsMaterial()
    {
        if (_collider == null) return;

        // Solo creamos uno nuevo si no tiene ya uno asignado
        if (_collider.sharedMaterial == null)
        {
            var mat = new PhysicsMaterial("PlayerNoFriction")
            {
                dynamicFriction = 0f,
                staticFriction  = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness      = 0f,
                bounceCombine   = PhysicsMaterialCombine.Minimum,
            };
            _collider.material = mat;
        }
    }

    private void OnDestroy() => _input.OnMoveEvent -= v => _moveInput = v;

    private void FixedUpdate()
    {
        if (_dance != null && _dance.IsDancing) return;

        ApplyBetterGravity();
        ApplyHorizontalMovement();

        // FIX 2: Step offset — subir escalones/baches pequeños
        if (_ground.IsGrounded && _moveInput.magnitude > 0.01f)
            TryStepUp();
    }

    // ── Gravedad mejorada ─────────────────────────────────────────────────────

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

    // ── Movimiento horizontal ─────────────────────────────────────────────────
    private void ApplyHorizontalMovement()
    {
        Vector3 camRight = _camera.transform.right;
        Vector3 camForward = _camera.transform.forward;
        camRight.y = 0f; camRight.Normalize();
        camForward.y = 0f; camForward.Normalize();

        Vector3 wishDir = (camRight * _moveInput.x + camForward * _moveInput.y).normalized;

        if (_wallNormal != Vector3.zero && wishDir != Vector3.zero
            && Vector3.Dot(wishDir, _wallNormal) < 0f)
        {
            wishDir = Vector3.ProjectOnPlane(wishDir, _wallNormal).normalized;
        }

        bool grounded = _ground.IsGrounded;

        if (grounded && _ground.GroundNormal != Vector3.up)
        {
            float slopeAngle = Vector3.Angle(Vector3.up, _ground.GroundNormal);
            if (slopeAngle < 45f)
                wishDir = Vector3.ProjectOnPlane(wishDir, _ground.GroundNormal).normalized;
        }

        // ESTO ES LO QUE FALTABA — aceleración, deceleración y rotación
        float accelDt = (grounded ? acceleration : acceleration * airControl) * Time.fixedDeltaTime;
        float decelDt = (grounded ? deceleration : deceleration * airControl) * Time.fixedDeltaTime;

        Vector3 hVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (wishDir.magnitude > 0.01f)
        {
            Vector3 force = Vector3.ClampMagnitude(wishDir * runSpeed - hVel, accelDt);
            _rb.AddForce(force, ForceMode.VelocityChange);

            float rotSpeed = grounded ? 14f : 5f;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(wishDir),
                rotSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _rb.AddForce(Vector3.ClampMagnitude(-hVel, decelDt), ForceMode.VelocityChange);
        }
        // Al final de ApplyHorizontalMovement, antes de cerrar el método:
        if (grounded && _rb.linearVelocity.y < 0f && _rb.linearVelocity.y > -2f)
        {
            // Suprimir pequeñas velocidades verticales negativas causadas por baches
            Vector3 vel = _rb.linearVelocity;
            vel.y = 0f;
            _rb.linearVelocity = vel;
        }
    }

    // ── Step offset ───────────────────────────────────────────────────────────

    /// <summary>
    /// Detecta obstáculos bajos justo delante del jugador y sube por encima de ellos.
    /// Funciona lanzando dos raycasts:
    ///   - Uno bajo (a ras de suelo) que detecta si hay obstáculo
    ///   - Uno alto (a stepHeight) que confirma que hay espacio libre arriba
    /// Si el obstáculo cabe dentro de stepHeight, movemos el personaje hacia arriba.
    /// </summary>
    private void TryStepUp()
    {
        Vector3 moveDir = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).normalized;
        if (moveDir == Vector3.zero) return;

        float radius = _capsule != null
            ? _capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z)
            : 0.3f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        bool hitLow = Physics.SphereCast(
            origin, radius * 0.5f, moveDir,
            out RaycastHit hitLowInfo, radius + 0.1f,
            stepMask, QueryTriggerInteraction.Ignore);

        if (!hitLow) return;

        // FIX CRÍTICO: Si la superficie que golpeamos es muy vertical (pared),
        // no intentamos subirla. Solo subimos si la normal apunta bastante hacia arriba.
        if (hitLowInfo.normal.y < 0.1f) return; // Es pared, no escalón

        Vector3 originHigh = origin + Vector3.up * stepHeight;
        bool hitHigh = Physics.SphereCast(
            originHigh, radius * 0.5f, moveDir,
            out _, radius + 0.1f,
            stepMask, QueryTriggerInteraction.Ignore);

        if (hitHigh) return;

        Vector3 targetPos = transform.position + Vector3.up * stepHeight;
        transform.position = Vector3.Lerp(transform.position, targetPos, stepSmooth * Time.fixedDeltaTime);
    }

    // ── Detección de paredes ──────────────────────────────────────────────────

    private void OnCollisionStay(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
        {
            // Solo consideramos pared si la normal es MUY horizontal
            // Subimos el threshold de 0.25 a 0.15 para ignorar baches del terreno
            if (c.normal.y < 0.15f)
            {
                _wallNormal = c.normal;
                return;
            }
        }
        _wallNormal = Vector3.zero;
    }

    private void OnCollisionExit(Collision col) => _wallNormal = Vector3.zero;
}
