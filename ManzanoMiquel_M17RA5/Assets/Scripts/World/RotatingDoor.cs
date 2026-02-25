using System.Collections;
using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// RotatingDoor
// Porta que rota NOMÉS en Y al interactuar, preservant els angles X i Z
// originals del mesh (important si el mesh ja té rotació prèvia com X:-90).
// Activa/desactiva el BoxCollider de pas segons si la porta està oberta.
// ════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Collider))]
public class RotatingDoor : MonoBehaviour, IInteractable
{
    [Header("Interacció")]
    [SerializeField] private string promptTancat = "Prem E per obrir la porta";
    [SerializeField] private string promptObert = "Prem E per tancar la porta";

    [Header("Mesh de la porta")]
    [Tooltip("El fill amb el MeshRenderer. El pivot és el punt de rotació (frontissa).")]
    [SerializeField] private Transform doorMesh;

    [Header("Rotació")]
    [SerializeField] private float angleObert = 90f;
    [SerializeField] private float duracio = 0.5f;
    [SerializeField] private AnimationCurve corba = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Col·lisionador de pas")]
    [SerializeField] private BoxCollider passCollider;

    // ── Estat intern ─────────────────────────────────────────────────────────
    private bool _estaOberta = false;
    private bool _animant = false;
    private float _angleActual = 0f;
    private float _xOriginal;   // preservem X del doorMesh
    private float _zOriginal;   // preservem Z del doorMesh

    // ── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (doorMesh == null)
        {
            Debug.LogWarning($"[RotatingDoor] '{name}': Assigna el Door Mesh al Inspector.");
        }
        else
        {
            // Llegim els angles locals actuals i guardem X i Z intocables
            _xOriginal = doorMesh.localEulerAngles.x;
            _zOriginal = doorMesh.localEulerAngles.z;
            _angleActual = doorMesh.localEulerAngles.y;  // punt de partida en Y
        }

        if (passCollider == null)
        {
            passCollider = GetComponentInChildren<BoxCollider>();
            if (passCollider == null)
                Debug.LogWarning($"[RotatingDoor] '{name}': No s'ha trobat cap BoxCollider.");
        }

        SetPassCollider(true); // tancada per defecte
    }

    // ── IInteractable ─────────────────────────────────────────────────────────
    public string GetInteractionPrompt() => _estaOberta ? promptObert : promptTancat;

    public void OnInteract(GameObject player)
    {
        if (_animant) return;
        StartCoroutine(AnimarPorta());
    }

    // ── Animació ──────────────────────────────────────────────────────────────
    private IEnumerator AnimarPorta()
    {
        _animant = true;

        float angleOrigen = _angleActual;
        float angleDesti = _estaOberta
            ? _angleActual - angleObert   // tanca: torna enrere
            : _angleActual + angleObert;  // obre: avança

        float temps = 0f;
        while (temps < duracio)
        {
            temps += Time.deltaTime;
            float t = corba.Evaluate(Mathf.Clamp01(temps / duracio));
            AplicarY(Mathf.Lerp(angleOrigen, angleDesti, t));
            yield return null;
        }

        AplicarY(angleDesti);
        _angleActual = angleDesti;

        _estaOberta = !_estaOberta;
        SetPassCollider(!_estaOberta); // collider actiu = porta tancada

        _animant = false;
    }

    // Modifica NOMÉS Y, X i Z es queden tal com estaven al principi
    private void AplicarY(float y)
    {
        if (doorMesh == null) return;
        doorMesh.localEulerAngles = new Vector3(_xOriginal, y, _zOriginal);
    }

    private void SetPassCollider(bool actiu)
    {
        if (passCollider != null)
            passCollider.enabled = actiu;
    }

    private void OnDrawGizmosSelected()
    {
        if (doorMesh == null) return;
        Gizmos.color = _estaOberta ? Color.green : Color.red;
        Gizmos.DrawWireSphere(doorMesh.position, 0.1f);
    }
}