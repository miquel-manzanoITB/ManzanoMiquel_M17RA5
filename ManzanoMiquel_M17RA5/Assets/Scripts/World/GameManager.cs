using UnityEngine;
using UnityEngine.SceneManagement;

// ════════════════════════════════════════════════════════════════════════════
// GameManager
// Singleton persistent (DontDestroyOnLoad).
// Gestiona: música (amb control de volum), col·leccionable, escenes i save.
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

    public bool HasCollectible { get; private set; }
    public float MusicVolume => musicVolume;

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

    // ── Col·leccionable ───────────────────────────────────────────────────────

    public void RegisterCollectible(CollectibleData data, Transform player)
    {
        HasCollectible = true;
        UIManager.Instance?.AddItemToHUD(data.icon);

        if (data.weaponPrefab == null) return;
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