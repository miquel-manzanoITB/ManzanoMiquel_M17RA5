using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerLookBehaviour : MonoBehaviour
{
    [Header("Sensibilitat")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private bool invertY = false;

    [Header("Càmera – Tercera persona")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform thirdPersonPivot;     // Fill buit a l'alçada del pit
    [SerializeField] private float orbitDistance = 4f;
    [SerializeField] private float minOrbitDistance = 0.5f;
    [SerializeField] private LayerMask cameraCollisionMask;

    [Header("Càmera – Primera persona (apuntar)")]
    [SerializeField] private Transform firstPersonPivot;    // Fill buit a l'alçada del cap

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
    private bool _isFrontCamera;

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

        // El cos del personatge NO rota amb la càmera.
        // PlayerMovementBehaviour girarà el cos quan es mogui.
        if (_isFirstPerson) UpdateFirstPersonCamera();
        else UpdateThirdPersonCamera();

        UpdateFov();
    }

    // ── Modes de càmera ───────────────────────────────────────────────────────

    private void UpdateThirdPersonCamera()
    {
        Quaternion orbitRot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos = thirdPersonPivot.position
                               + orbitRot * new Vector3(0f, 0f, -orbitDistance);

        if (Physics.Linecast(thirdPersonPivot.position, desiredPos,
                             out RaycastHit hit, cameraCollisionMask))
        {
            float safeDist = Mathf.Max(hit.distance - 0.2f, minOrbitDistance);
            desiredPos = thirdPersonPivot.position
                        + orbitRot * new Vector3(0f, 0f, -safeDist);
        }

        playerCamera.transform.position = desiredPos;
        playerCamera.transform.LookAt(thirdPersonPivot.position);
    }

    private void UpdateFirstPersonCamera()
    {
        playerCamera.transform.position = firstPersonPivot.position;
        playerCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // Vista frontal bloquejada per al ball de la victòria
    private void UpdateFrontCamera()
    {
        Vector3 frontPos = transform.position
                         + transform.forward * 3f
                         + Vector3.up * 1.5f;
        playerCamera.transform.position = frontPos;
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    // ── FOV ───────────────────────────────────────────────────────────────────

    private void UpdateFov()
    {
        float speed = _rb.linearVelocity.magnitude;
        float maxSpeed = _movement != null ? _movement.MaxSpeed : 8f;
        playerCamera.fieldOfView = speed >= maxSpeed - fovBoostThreshold ? fovBoost : fov;
    }

    // ── API pública ────────────────────────────────────────────────────────────

    /// <summary>Canvia entre 1a i 3a persona quan s'apunta.</summary>
    public void SetFirstPerson(bool fp) => _isFirstPerson = fp;

    /// <summary>Activa/desactiva la vista frontal durant el ball.</summary>
    public void SetFrontCamera(bool front) => _isFrontCamera = front;
}