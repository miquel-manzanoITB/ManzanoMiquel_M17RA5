using TMPro;
using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// InteractionDetector
// Adjunta'l al Player. Fa Raycast des de rayOrigin (apunta la càmera) per
// detectar IInteractable. Mostra el prompt i crida OnInteract() en prémer E.
// ════════════════════════════════════════════════════════════════════════════

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
    private IInteractable _currentInteractable;

    private void Awake()
    {
        _input = GetComponent<PlayerInputController>();
        _input.OnInteractEvent += TryInteract;
        HidePrompt();
    }

    private void OnDestroy() => _input.OnInteractEvent -= TryInteract;

    private void Update() => DetectInteractable();

    // ── Detecció ──────────────────────────────────────────────────────────────

    private void DetectInteractable()
    {
        if (rayOrigin == null) return;

        bool found = false;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward,
            out RaycastHit hit, interactionRange, interactableLayers, QueryTriggerInteraction.Ignore))
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
        HidePrompt();
        _currentInteractable = null;
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    private void ShowPrompt(string message)
    {
        if (interactionPromptUI == null) return;
        interactionPromptUI.SetActive(true);
        if (promptText != null) promptText.text = message;
    }

    private void HidePrompt()
    {
        interactionPromptUI?.SetActive(false);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!showDebugRay || rayOrigin == null) return;
        Gizmos.color = _currentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * interactionRange);
    }
}