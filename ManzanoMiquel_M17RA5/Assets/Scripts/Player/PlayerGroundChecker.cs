using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// PlayerGroundChecker
// Detecta si el jugador és a terra amb SphereCast.
// Inclou un buffer anti-flicker per evitar canvis bruscos a l'animació.
// ════════════════════════════════════════════════════════════════════════════

public class PlayerGroundChecker : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private float checkDistance = 0.15f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private Transform checkOrigin;
    [SerializeField] private float sphereRadius = 0.25f;

    [Header("Anti-flicker")]
    [Tooltip("Frames consecutius sense terra abans de considerar-se a l'aire.")]
    [SerializeField] private int ungroundedFrames = 3;

    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;

    private int _ungroundedCounter;

    private void FixedUpdate()
    {
        Vector3 origin = checkOrigin != null ? checkOrigin.position : transform.position;

        bool hit = Physics.SphereCast(
            origin, sphereRadius, Vector3.down,
            out RaycastHit hitInfo, checkDistance, groundMask,
            QueryTriggerInteraction.Ignore);

        if (hit)
        {
            _ungroundedCounter = 0;
            IsGrounded = true;
            GroundNormal = hitInfo.normal;
        }
        else
        {
            _ungroundedCounter++;
            if (_ungroundedCounter >= ungroundedFrames)
                IsGrounded = false;
            GroundNormal = Vector3.up;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = checkOrigin != null ? checkOrigin.position : transform.position;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.DrawWireSphere(origin + Vector3.down * checkDistance, sphereRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * checkDistance);
    }
}