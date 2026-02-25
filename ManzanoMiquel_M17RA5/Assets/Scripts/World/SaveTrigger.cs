using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// SaveTrigger
// Adjunta-ho a un checkpoint. Quan el jugador el toca guarda la partida.
// ════════════════════════════════════════════════════════════════════════════

[RequireComponent(typeof(Collider))]
public class SaveTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem saveParticles;
    [SerializeField] private AudioClip saveClip;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance?.SaveGame(other.transform);
        saveParticles?.Play();

        if (saveClip != null)
            AudioSource.PlayClipAtPoint(saveClip, transform.position);

        UIManager.Instance?.ShowHint("Partida guardada!");
    }
}