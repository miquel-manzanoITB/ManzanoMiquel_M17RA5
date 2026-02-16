using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona tota la UI: menú d'inici, pausa, GameOver/Win, HUD i minimapa.
/// Es subscriu a events estàtics a OnEnable/OnDisable — mai fa polling.
/// Tot és obligatori excepte el minimap (però l'enunciat el demana).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    [SerializeField] private Transform inventoryContainer;   // Layout horitzontal
    [SerializeField] private Image itemIconPrefab;
    [SerializeField] private Text hintText;

    [Header("Minimapa")]
    [SerializeField] private RawImage minimapImage;          // Connecta amb Render Texture

    [Header("Panells")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;            // GameOver (guanyar la partida)

    private bool _isPaused;

    private void Awake()
    {
        Instance = this;
        // Mostra el menú d'inici, amaga la resta
        mainMenuPanel?.SetActive(true);
        pausePanel?.SetActive(false);
        winPanel?.SetActive(false);
        hintText?.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerInputController.OnPauseGameEvent += TogglePause;
    }

    private void OnDisable()
    {
        PlayerInputController.OnPauseGameEvent -= TogglePause;
    }

    // ── HUD – Inventari ───────────────────────────────────────────────────────

    public void AddItemToHUD(Sprite icon)
    {
        if (itemIconPrefab == null || inventoryContainer == null) return;
        Image img = Instantiate(itemIconPrefab, inventoryContainer);
        img.sprite = icon;
    }

    // ── HUD – Missatges d'hint ────────────────────────────────────────────────

    public void ShowHint(string message)
    {
        StopAllCoroutines();
        StartCoroutine(HintRoutine(message));
    }

    private IEnumerator HintRoutine(string message)
    {
        hintText.text = message;
        hintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        hintText.gameObject.SetActive(false);
    }

    // ── Menú d'inici ──────────────────────────────────────────────────────────

    // Connecta el botó "Jugar" al menú d'inici
    public void OnStartPressed()
    {
        mainMenuPanel?.SetActive(false);
        Time.timeScale = 1f;
        GameManager.Instance?.LoadScene("Exterior");
    }

    // Connecta el botó "Sortir" al menú d'inici
    public void OnQuitFromMenuPressed() => GameManager.Instance?.QuitGame();

    // ── Pausa ─────────────────────────────────────────────────────────────────

    private void TogglePause()
    {
        // No es pot pausar al menú principal
        if (mainMenuPanel != null && mainMenuPanel.activeSelf) return;

        _isPaused = !_isPaused;
        pausePanel?.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    // Connecta el botó "Reprendre" del menú de pausa
    public void OnResumePressed() => TogglePause();

    // Connecta el botó "Tornar a començar" del menú de pausa
    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.LoadScene("MainMenu");
    }

    // Connecta el botó "Sortir" del menú de pausa
    public void OnQuitPressed() => GameManager.Instance?.QuitGame();

    // ── Win / GameOver ────────────────────────────────────────────────────────

    // Crida-ho des de la zona de trigger a l'interior (quan el ball acaba)
    public void ShowWin()
    {
        winPanel?.SetActive(true);
        Time.timeScale = 0f;
    }

    // Connecta el botó "Tornar a jugar" del panell de Win
    public void OnPlayAgainPressed()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.LoadScene("MainMenu");
    }

    // Connecta el botó "Sortir" del panell de Win
    public void OnQuitFromWinPressed() => GameManager.Instance?.QuitGame();
}