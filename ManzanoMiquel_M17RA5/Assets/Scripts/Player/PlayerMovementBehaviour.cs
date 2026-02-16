using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerMovementBehaviour : MonoBehaviour
{
    [Header("Velocitat")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Acceleració")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;

    // Llegit per PlayerLookBehaviour per al FOV boost
    public float MaxSpeed => runSpeed;

    [Header("Rotació del cos")]
    [SerializeField] private float rotationSpeed = 12f;   // Velocitat amb la que el cos gira cap a la direcció de moviment

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerDanceBehaviour _dance;
    private Camera _camera;

    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _input = GetComponent<PlayerInputController>();
        _dance = GetComponent<PlayerDanceBehaviour>();
        _camera = Camera.main;

        _input.OnMoveEvent += input => _moveInput = input;
    }

    private void OnDestroy()
    {
        _input.OnMoveEvent -= input => _moveInput = input;
    }

    private void FixedUpdate()
    {
        if (_dance != null && _dance.IsDancing) return;

        // Direcció calculada des dels eixos de la CÀMERA, no del personatge.
        // Així prémer "endavant" sempre mou cap on mira la càmera.
        Vector3 camRight = _camera.transform.right;
        Vector3 camForward = _camera.transform.forward;

        // Ignorem la component vertical perquè el moviment és horitzontal
        camRight.y = 0f; camRight.Normalize();
        camForward.y = 0f; camForward.Normalize();

        Vector3 wishDir = (camRight * _moveInput.x + camForward * _moveInput.y).normalized;

        Vector3 horizontalVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (wishDir.magnitude > 0f)
        {
            // Aplica forces cap a la direcció desitjada
            Vector3 velocityDiff = wishDir * runSpeed - horizontalVel;
            Vector3 accel = Vector3.ClampMagnitude(velocityDiff, acceleration * Time.fixedDeltaTime);
            _rb.AddForce(accel, ForceMode.VelocityChange);

            // Gira el COS del personatge suaument cap a la direcció de moviment
            Quaternion targetRot = Quaternion.LookRotation(wishDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 brake = Vector3.ClampMagnitude(-horizontalVel, deceleration * Time.fixedDeltaTime);
            _rb.AddForce(brake, ForceMode.VelocityChange);
        }
    }
}