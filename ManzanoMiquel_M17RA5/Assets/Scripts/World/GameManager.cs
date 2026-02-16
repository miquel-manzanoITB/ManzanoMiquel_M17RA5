using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// ════════════════════════════════════════════════════════════════════════════
// GameManager
// Singleton persistent (DontDestroyOnLoad).
// Gestiona: música, col·leccionable, canvi d'escena, guardar/carregar.
// ════════════════════════════════════════════════════════════════════════════

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mainTheme;

    [Header("Sons d'escena")]
    [SerializeField] private AudioClip sceneChangeClip;   // So en canviar d'escena

    public bool HasCollectible { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PlayMusic(mainTheme);
    }

    // ── Col·leccionable ───────────────────────────────────────────────────────

    /// <summary>
    /// Crida CollectibleBehaviour en recollir l'objecte.
    /// Actualitza el HUD i afegeix el mesh visual a la mà del personatge.
    /// </summary>
    public void RegisterCollectible(CollectibleData data, Transform player)
    {
        HasCollectible = true;
        UIManager.Instance?.AddItemToHUD(data.icon);

        // Visual: instancia el prefab de l'arma al hueso de la mà
        if (data.weaponPrefab == null) return;
        Transform hand = player.Find("Armature/Hips/Spine/RightHand");
        if (hand == null) return;

        GameObject weapon = Instantiate(data.weaponPrefab, hand);
        weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    // ── So ────────────────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.PlayOneShot(clip);
    }

    // ── Canvi d'escena ────────────────────────────────────────────────────────

    public void LoadScene(string sceneName)
    {
        PlaySFX(sceneChangeClip);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() => Application.Quit();

    // ── Guardar / Carregar ────────────────────────────────────────────────────
    // Guarda: posició, rotació i l'estat del col·leccionable de l'escena.
    // (L'inventari del personatge és opcional; no s'inclou aquí.)

    public void SaveGame(Transform player)
    {
        PlayerPrefs.SetFloat("px", player.position.x);
        PlayerPrefs.SetFloat("py", player.position.y);
        PlayerPrefs.SetFloat("pz", player.position.z);
        PlayerPrefs.SetFloat("ry", player.eulerAngles.y);
        PlayerPrefs.SetInt("hasCollectible", HasCollectible ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadGame(Transform player)
    {
        if (!PlayerPrefs.HasKey("px")) return;

        player.position = new Vector3(
            PlayerPrefs.GetFloat("px"),
            PlayerPrefs.GetFloat("py"),
            PlayerPrefs.GetFloat("pz"));
        player.rotation = Quaternion.Euler(0f, PlayerPrefs.GetFloat("ry"), 0f);
        HasCollectible = PlayerPrefs.GetInt("hasCollectible") == 1;
    }
}

// ════════════════════════════════════════════════════════════════════════════
// SaveTrigger
// Adjunta-ho a l'element de Shader/Partícules del checkpoint.
// Quan el jugador el toca guarda posició, rotació i col·leccionable.
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