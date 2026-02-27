using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// ════════════════════════════════════════════════════════════════════════════
// GameManager
// Singleton persistent (DontDestroyOnLoad).
// Gestiona: música (amb control de volum), col·leccionables, escenes i save.
// ════════════════════════════════════════════════════════════════════════════

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mainTheme;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;

    [Header("Sons")]
    [SerializeField] private AudioClip sceneChangeClip;

    // Substituïm el bool per un HashSet que suporta múltiples col·leccionables
    private HashSet<string> _collectedItems = new HashSet<string>();

    public float MusicVolume => musicVolume;

    /// <summary>Retorna true si el col·leccionable amb aquest nom ja ha estat recollit.</summary>
    public bool HasCollectible(string itemName) => _collectedItems.Contains(itemName);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        PlayMusic(mainTheme);
        SetMusicVolume(musicVolume);
    }

    private void OnEnable()
    {
        PlayerInputController.OnSaveGameEvent += SaveGameFromInput;
        PlayerInputController.OnLoadGameEvent += LoadGameFromInput;
    }

    private void OnDisable()
    {
        PlayerInputController.OnSaveGameEvent -= SaveGameFromInput;
        PlayerInputController.OnLoadGameEvent -= LoadGameFromInput;
    }

    // ── Col·leccionables ─────────────────────────────────────────────────────

    /// <summary>Registra un col·leccionable amb FX (recollit en joc).</summary>
    public void RegisterCollectible(CollectibleData data, Transform player)
    {
        _collectedItems.Add(data.itemName);
        UIManager.Instance?.AddItemToHUD(data.icon);

        AttachWeapon(data, player);
    }

    /// <summary>Registra un col·leccionable sense FX (restaurat al carregar partida).</summary>
    public void RegisterCollectibleSilent(CollectibleData data, Transform player)
    {
        _collectedItems.Add(data.itemName);
        UIManager.Instance?.AddItemToHUD(data.icon);

        AttachWeapon(data, player);
    }

    private void AttachWeapon(CollectibleData data, Transform player)
    {
        if (data.weaponPrefab == null || player == null) return;
        Transform hand = player.Find("Armature/Hips/Spine/RightHand");
        if (hand == null) return;

        GameObject weapon = Instantiate(data.weaponPrefab, hand);
        weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    // ── Música ────────────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>Ajusta el volum de la música (0-1). Assignable des de la UI.</summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.PlayOneShot(clip);
    }

    // ── Escenes ───────────────────────────────────────────────────────────────

    public void LoadScene(string sceneName)
    {
        PlaySFX(sceneChangeClip);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() => Application.Quit();

    // ── Guardar / Carregar ────────────────────────────────────────────────────

    /// <summary>Crida des de SaveTrigger (checkpoint) o tecla G.</summary>
    public void SaveGame(Transform player)
    {
        PlayerPrefs.SetFloat("px", player.position.x);
        PlayerPrefs.SetFloat("py", player.position.y);
        PlayerPrefs.SetFloat("pz", player.position.z);
        PlayerPrefs.SetFloat("ry", player.eulerAngles.y);

        // Guardem tots els col·leccionables com una cadena separada per comes
        string collected = string.Join(",", _collectedItems);
        PlayerPrefs.SetString("collectedItems", collected);

        PlayerPrefs.Save();
        UIManager.Instance?.ShowHint("Partida guardada! [G]");
        Debug.Log($"[GameManager] Partida guardada — items: {collected}");
    }

    /// <summary>Crida des de LoadGameTrigger o tecla P.</summary>
    public void LoadGame(Transform player)
    {
        if (!PlayerPrefs.HasKey("px"))
        {
            UIManager.Instance?.ShowHint("No hi ha cap partida guardada.");
            return;
        }

        // Restaurar posició i rotació
        player.position = new Vector3(
            PlayerPrefs.GetFloat("px"),
            PlayerPrefs.GetFloat("py"),
            PlayerPrefs.GetFloat("pz"));
        player.rotation = Quaternion.Euler(0f, PlayerPrefs.GetFloat("ry"), 0f);

        // Restaurar col·leccionables
        string saved = PlayerPrefs.GetString("collectedItems", "");
        _collectedItems = new HashSet<string>(
            saved.Split(',', System.StringSplitOptions.RemoveEmptyEntries));

        UIManager.Instance?.ShowHint("Partida carregada! [P]");
        Debug.Log($"[GameManager] Partida carregada — items: {saved}");
    }

    // ── Helpers privats per als events d'input ────────────────────────────────

    private void SaveGameFromInput()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) SaveGame(player.transform);
    }

    private void LoadGameFromInput()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) LoadGame(player.transform);
    }
}