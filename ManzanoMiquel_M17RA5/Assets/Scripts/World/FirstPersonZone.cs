using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// FirstPersonZone
// Col·loca aquest script en un GameObject amb un Collider marcat com Trigger.
// Quan el jugador entra, força la vista en primera persona cridant
// PlayerLookBehaviour.SetFirstPerson(true) directament.
// Quan surt, restaura la tercera persona.
//
// Setup:
//  - Crea un GameObject amb BoxCollider (Is Trigger = true)
//  - Afegeix aquest script
//  - El jugador ha de tenir el tag "Player"
// ════════════════════════════════════════════════════════════════════════════

public class FirstPersonZone : MonoBehaviour
{
    [Header("Configuració")]
    [Tooltip("Tag del jugador per filtrar el trigger.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si true, el jugador no pot sortir manualment de 1a persona mentre és dins la zona.")]
    [SerializeField] private bool lockInsideZone = true;

    private PlayerLookBehaviour _lookBehaviour;
    private bool _playerInside = false;

    private void Awake()
    {
        // Assegurem que el collider és trigger
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[FirstPersonZone] '{name}': El collider no era Trigger, s'ha activat automàticament.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_playerInside) return;

        _lookBehaviour = other.GetComponent<PlayerLookBehaviour>();
        if (_lookBehaviour == null) return;

        _playerInside = true;
        _lookBehaviour.SetFirstPerson(true);

        if (lockInsideZone)
            _lookBehaviour.LockFirstPerson(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!_playerInside) return;

        _playerInside = false;

        if (_lookBehaviour == null) return;

        if (lockInsideZone)
            _lookBehaviour.LockFirstPerson(false);

        _lookBehaviour.SetFirstPerson(false);
        _lookBehaviour = null;
    }

    // Gizmo per veure la zona a l'editor
    private void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider>();
        if (col == null) return;

        Gizmos.color = _playerInside
            ? new Color(0f, 1f, 0f, 0.2f)
            : new Color(0f, 0.5f, 1f, 0.15f);

        Gizmos.matrix = Matrix4x4.TRS(
            transform.TransformPoint(col.center),
            transform.rotation,
            transform.lossyScale);
        Gizmos.DrawCube(Vector3.zero, col.size);

        Gizmos.color = _playerInside ? Color.green : new Color(0f, 0.5f, 1f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, col.size);
    }
}