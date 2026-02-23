// ════════════════════════════════════════════════════════════════════════════
// InteractionDetector
// Adjunta'l al Player. Fa Raycast des de la CÀMERA (no des del Player) per
// detectar objectes interactuables. Mostra el prompt a la UI i crida
// OnInteract() quan es prem E.
//
// FIX: El raig ara surt des de la càmera cap endavant, com fan tots els jocs
// en primera/tercera persona. Abans sortia des del transform del player i
// apuntava a transform.forward, que causava que el raig mai coincidís amb el
// centre de la pantalla quan la càmera estava en angle.
// ════════════════════════════════════════════════════════════════════════════

using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerInputController))]
public class InteractionDetector : MonoBehaviour
{
    [Header("Detecció")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private Transform rayOrigin;

    [Header("UI")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private PlayerInputController _input;
    private Camera _camera;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        _input = GetComponent<PlayerInputController>();
        _camera = Camera.main;

        _input.OnInteractEvent += TryInteract;

        HidePrompt();
    }

    private void OnDestroy()
    {
        _input.OnInteractEvent -= TryInteract;
    }

    private void Update()
    {
        DetectInteractable();
    }

    // ── Detecció ─────────────────────────────────────────────────────────────
    private void DetectInteractable()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        bool found = false;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayers, QueryTriggerInteraction.Ignore))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>()
                                      ?? hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                _currentInteractable = interactable;
                ShowPrompt(interactable.GetInteractionPrompt());
                found = true;
            }
        }

        if (!found)
        {
            _currentInteractable = null;
            HidePrompt();
        }
    }

    // ── Interacció ────────────────────────────────────────────────────────────

    private void TryInteract()
    {
        if (_currentInteractable == null) return;

        _currentInteractable.OnInteract(gameObject);

        // Refresquem la detecció immediatament per si l'objecte ha canviat d'estat
        HidePrompt();
        _currentInteractable = null;
    }

    // ── UI ───────────────────────────────────────────────────────────────────

    private void ShowPrompt(string message)
    {
        if (interactionPromptUI == null) return;
        interactionPromptUI.SetActive(true);
        if (promptText != null) promptText.text = message;
    }

    private void HidePrompt()
    {
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        if (!showDebugRay || rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        Gizmos.color = _currentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(ray.origin, ray.direction * interactionRange);
    }
}
