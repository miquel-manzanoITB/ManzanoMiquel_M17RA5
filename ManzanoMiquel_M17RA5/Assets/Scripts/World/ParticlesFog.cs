using UnityEngine;

/// <summary>
/// Adjunta este script a un GameObject con un ParticleSystem configurado
/// como niebla. Seguirá al player infinitamente para que nunca se acabe.
/// </summary>
public class ParticlesFog : MonoBehaviour
{
    [Header("Seguimiento")]
    [SerializeField] private Transform target;          // Arrastra el Player aquí
    [SerializeField] private float followSpeed = 2f;    // Qué tan suave sigue al player
    [SerializeField] private Vector3 offset = Vector3.zero; // Offset respecto al player

    [Header("Zona de emisión")]
    [Tooltip("Cuando el player sale de este radio, la niebla se teletransporta")]
    [SerializeField] private float teleportDistance = 15f;

    private ParticleSystem _ps;
    private ParticleSystem.ShapeModule _shape;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        // Si el player se aleja mucho (ej: carga de escena, spawn),
        // teletransportamos directamente para no dejar zonas sin niebla
        if (Vector3.Distance(transform.position, desiredPos) > teleportDistance)
        {
            transform.position = desiredPos;
        }
        else
        {
            // Seguimiento suave en XZ, mantenemos Y fija para que
            // la niebla siempre esté al nivel del suelo
            Vector3 smoothed = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
            smoothed.y = desiredPos.y; // Y siempre al nivel del player (+ offset)
            transform.position = smoothed;
        }
    }

    // Opcional: visualizar el área en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, teleportDistance);
    }
}