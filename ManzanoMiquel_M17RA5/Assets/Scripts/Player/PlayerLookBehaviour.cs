using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// PlayerLookBehaviour
// Gestiona la càmera en 3a i 1a persona amb transició suau entre modes.
//
// 3a persona — òrbita al voltant de thirdPersonPivot amb col·lisió.
// 1a persona — posició a firstPersonPivot, el cos segueix el yaw.
// Transició  — interpolació de posició/rotació via _transitionT (0→1).
// Frontal    — vista fixa per al ball de victòria.
// ════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerLookBehaviour : MonoBehaviour
{
    [Header("Sensibilitat")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private bool invertY = false;

    [Header("Càmera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform thirdPersonPivot;
    [SerializeField] private Transform firstPersonPivot;

    [Header("3a persona")]
    [SerializeField] private float orbitDistance = 4f;
    [SerializeField] private float minOrbitDistance = 0.5f;
    [SerializeField] private LayerMask cameraCollisionMask;

    [Header("Transició 1a ↔ 3a persona")]
    [Tooltip("Velocitat de la transició. 6 = natural, 12 = ràpid.")]
    [SerializeField] private float transitionSpeed = 6f;

    [Header("FOV")]
    [SerializeField] private float fov = 60f;
    [SerializeField] private float fovBoost = 80f;
    [SerializeField] private float fovBoostThreshold = 2f;

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerMovementBehaviour _movement;

    private float _yaw;
    private float _pitch;
    private Vector2 _lookInput;

    private bool _isFirstPerson;
    private bool _lockedFirstPerson;  // bloquejat per FirstPersonZone
    private bool _isFrontCamera;

    // 0 = 3a persona, 1 = 1a persona
    private float _transitionT;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _movement = GetComponent<PlayerMovementBehaviour>();

        playerCamera.fieldOfView = fov;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _input.OnLookEvent += input => _lookInput = input;
        _input.OnAimEvent += SetFirstPerson;
    }

    private void OnDestroy()
    {
        _input.OnLookEvent -= input => _lookInput = input;
        _input.OnAimEvent -= SetFirstPerson;
    }

    private void LateUpdate()
    {
        if (_isFrontCamera) { UpdateFrontCamera(); return; }

        _yaw += _lookInput.x * mouseSensitivity * Time.deltaTime;
        _pitch += (invertY ? _lookInput.y : -_lookInput.y) * mouseSensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, -maxLookAngle, maxLookAngle);

        float targetT = _isFirstPerson ? 1f : 0f;
        _transitionT = Mathf.MoveTowards(_transitionT, targetT, transitionSpeed * Time.deltaTime);

        UpdateCamera();
        UpdateFov();
    }

    // ── Càmera ────────────────────────────────────────────────────────────────

    private void UpdateCamera()
    {
        (Vector3 thirdPos, Quaternion thirdRot) = GetThirdPersonPosRot();
        Vector3 firstPos = firstPersonPivot.position;
        Quaternion firstRot = Quaternion.Euler(_pitch, _yaw, 0f);

        // SmoothStep per a una corba més natural (accelera i frena)
        float t = Mathf.SmoothStep(0f, 1f, _transitionT);

        playerCamera.transform.position = Vector3.Lerp(thirdPos, firstPos, t);
        playerCamera.transform.rotation = Quaternion.Slerp(thirdRot, firstRot, t);

        // El cos segueix el yaw proporcionalment a quant estem en 1a persona
        if (_transitionT > 0.05f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0f, _yaw, 0f),
                _transitionT * 20f * Time.deltaTime);
        }
    }

    private (Vector3 pos, Quaternion rot) GetThirdPersonPosRot()
    {
        Quaternion orbitRot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desired = thirdPersonPivot.position
                             + orbitRot * new Vector3(0f, 0f, -orbitDistance);

        if (Physics.Linecast(thirdPersonPivot.position, desired,
                             out RaycastHit hit, cameraCollisionMask))
        {
            float safeDist = Mathf.Max(hit.distance - 0.2f, minOrbitDistance);
            desired = thirdPersonPivot.position + orbitRot * new Vector3(0f, 0f, -safeDist);
        }

        return (desired, Quaternion.LookRotation(thirdPersonPivot.position - desired));
    }

    private void UpdateFrontCamera()
    {
        Vector3 frontPos = transform.position + transform.forward * 3f + Vector3.up * 1.5f;
        playerCamera.transform.position = frontPos;
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    // ── FOV ───────────────────────────────────────────────────────────────────

    private void UpdateFov()
    {
        float speed = _rb.linearVelocity.magnitude;
        float maxSpeed = _movement != null ? _movement.MaxSpeed : 8f;
        float targetFov = speed >= maxSpeed - fovBoostThreshold ? fovBoost : fov;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView, targetFov, 10f * Time.deltaTime);
    }

    // ── API pública ────────────────────────────────────────────────────────────

    public bool IsFirstPerson => _isFirstPerson;

    /// <summary>Canvia entre 1a i 3a persona (via OnAimEvent o FirstPersonZone).</summary>
    public void SetFirstPerson(bool fp)
    {
        if (_lockedFirstPerson && !fp) return;
        _isFirstPerson = fp;
    }

    /// <summary>Bloqueja 1a persona (cridat per FirstPersonZone).</summary>
    public void LockFirstPerson(bool locked)
    {
        _lockedFirstPerson = locked;
        if (locked) _isFirstPerson = true;
    }

    /// <summary>Vista frontal fixa per al ball de victòria.</summary>
    public void SetFrontCamera(bool front) => _isFrontCamera = front;
}