
// ════════════════════════════════════════════════════════════════════════════
// InteractionDetector
// Adjunta'l al Player. Fa Raycast des de la càmera i detecta objectes interactuables.
// Mostra el prompt a la UI i crida OnInteract() quan es prem E.
// ════════════════════════════════════════════════════════════════════════════

using TMPro;
using UnityEngine;


[RequireComponent(typeof(PlayerInputController))]
public class InteractionDetector : MonoBehaviour
{
    [Header("Detecció")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float rayHeightOffset = 1.5f;  // Alçada des d'on surt el raig (alçada dels ulls)

    [Header("UI")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private PlayerInputController _input;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        _input = GetComponent<PlayerInputController>();
        _input.OnInteractEvent += TryInteract;

        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
    }

    private void OnDestroy()
    {
        _input.OnInteractEvent -= TryInteract;
    }

    private void Update()
    {
        DetectInteractable();
    }

    private void DetectInteractable()
    {
        // Raycast des de la posició del Player cap endavant
        Vector3 rayOrigin = transform.position + Vector3.up * rayHeightOffset;
        Vector3 rayDirection = transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, interactionRange, interactableLayers))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                _currentInteractable = interactable;
                ShowPrompt(interactable.GetInteractionPrompt());
                return;
            }
        }

        _currentInteractable = null;
        HidePrompt();
    }

    private void TryInteract()
    {
        Debug.Log("Intentant interactuar...");
        if (_currentInteractable != null)
        {
            _currentInteractable.OnInteract(gameObject);
            HidePrompt();
        }
    }

    private void ShowPrompt(string message)
    {
        if (interactionPromptUI == null) return;
        interactionPromptUI.SetActive(true);
        if (promptText != null)
            promptText.text = message;
    }

    private void HidePrompt()
    {
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
    }

    // Debug visual en Scene
    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * rayHeightOffset;
        Gizmos.color = _currentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(origin, transform.forward * interactionRange);
    }
}