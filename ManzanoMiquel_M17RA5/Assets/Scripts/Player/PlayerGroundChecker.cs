using UnityEngine;

/// <summary>
/// Proporciona la propietat IsGrounded mitjançant un SphereCast cap avall.
/// Component independent perquè qualsevol Behaviour el pugui llegir sense acoblament.
/// </summary>
public class PlayerGroundChecker : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private float checkDistance = 0.15f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private Transform checkOrigin;   // Transform als peus del capsule

    public bool IsGrounded { get; private set; }

    private void FixedUpdate()
    {
        Vector3 origin = checkOrigin != null ? checkOrigin.position : transform.position;
        IsGrounded = Physics.SphereCast(origin, 0.25f, Vector3.down, out _, checkDistance, groundMask);
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = checkOrigin != null ? checkOrigin.position : transform.position;

        // Verd = a terra, vermell = a l'aire
        Gizmos.color = IsGrounded ? Color.green : Color.red;

        // Esfera inicial (on comença el cast)
        Gizmos.DrawWireSphere(origin, 0.25f);

        // Esfera final (fins on arriba el cast)
        Gizmos.DrawWireSphere(origin + Vector3.down * checkDistance, 0.25f);

        // Línia que connecta les dues esferes
        Gizmos.DrawLine(origin, origin + Vector3.down * checkDistance);
    }
}