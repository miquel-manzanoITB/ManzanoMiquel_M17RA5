// ════════════════════════════════════════════════════════════════════════════
// CollectibleBehaviour
// Efecte float + rotació per destacar a l'entorn (requerit per l'enunciat).
// En recollir-lo: adjunta el mesh a la mà, notifica GameManager, dispara event.
// ════════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollectibleBehaviour : MonoBehaviour
{
    [Header("Dades (ScriptableObject)")]
    [SerializeField] private CollectibleData data;

    [Header("Animació float")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private float spinSpeed = 60f;

    [Header("FX")]
    [SerializeField] private ParticleSystem pickupParticles;
    [SerializeField] private AudioClip pickupClip;

    [Header("Events")]
    public UnityEvent OnCollected;   // Connecta aquí la porta que s'ha d'obrir

    private Vector3 _startPos;
    private bool _collected;

    private void Start()
    {
        _startPos = transform.position;
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected || !other.CompareTag("Player")) return;
        _collected = true;

        // Notifica el GameManager (actualitza HUD + afegeix visualment a la mà)
        GameManager.Instance?.RegisterCollectible(data, other.transform);

        // Dispara l'event (la porta pot subscriure-s'hi a l'Inspector)
        OnCollected.Invoke();

        // FX
        if (pickupParticles != null)
            Instantiate(pickupParticles, transform.position, Quaternion.identity);
        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);

        gameObject.SetActive(false);
    }
}