using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// IInteractable
// Interfície que implementen tots els objectes amb els quals es pot interactuar.
// ════════════════════════════════════════════════════════════════════════════

public interface IInteractable
{
    /// <summary>Nom que es mostra a la UI ("Obrir porta", "Agafar clau", etc.)</summary>
    string GetInteractionPrompt();

    /// <summary>Cridat quan el jugador prem E/Interact mentre mira l'objecte</summary>
    void OnInteract(GameObject player);
}