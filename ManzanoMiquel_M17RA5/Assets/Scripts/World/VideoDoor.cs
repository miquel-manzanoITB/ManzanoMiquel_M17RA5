
// ════════════════════════════════════════════════════════════════════════════
// VideoDoor
// Porta que reprodueix un vídeo quan s'interactua amb ella.
// Pausa el joc, mostra el vídeo a pantalla completa i reprèn quan acaba.
// ════════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(Collider))]
public class VideoDoor : MonoBehaviour, IInteractable
{
    [Header("Interacció")]
    [SerializeField] private string interactionPrompt = "Prem E per obrir la porta";

    [Header("Vídeo")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private VideoPlayer videoPlayer;    // VideoPlayer al Canvas
    [SerializeField] private GameObject videoUI;         // Panel que conté el VideoPlayer (desactivat per defecte)

    [Header("Després del vídeo")]
    [SerializeField] private bool openDoorAfterVideo = true;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTrigger = "Open";

    [Header("Saltar vídeo")]
    [SerializeField] private GameObject skipButton;  // Botó UI "Prem Espai per saltar"

    [Header("Referència al Player")]
    [SerializeField] private PlayerInputController playerInputController;

    private bool _videoPlaying;

    private void Awake()
    {
        if (playerInputController == null)
            Debug.LogWarning("VideoDoor: No s'ha trobat PlayerInputController a l'escena!");

        if (videoUI != null)
            videoUI.SetActive(false);
        if (skipButton != null)
            skipButton.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public void OnInteract(GameObject player)
    {
        if (_videoPlaying) return;
        PlayVideo();
    }

    private void PlayVideo()
    {
        if (videoClip == null || videoPlayer == null)
        {
            Debug.LogWarning("VideoDoor: falta VideoClip o VideoPlayer!");
            return;
        }

        _videoPlaying = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pausa el joc
        Time.timeScale = 0f;

        // Configura el VideoPlayer
        videoPlayer.clip = videoClip;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;

        // Subscriu als events
        videoPlayer.loopPointReached += OnVideoFinished;
        if (playerInputController != null)
            playerInputController.OnSkipVideoEvent += HandleSkipVideo;

        // Mostra la UI i reprodueix
        if (videoUI != null)
            videoUI.SetActive(true);

        // Mostra el botó de saltar
        if (skipButton != null)
            skipButton.SetActive(true);

        videoPlayer.Play();

        // Amaga el cursor mentre es reprodueix
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Dessubscriu
        vp.loopPointReached -= OnVideoFinished;
        if (playerInputController != null)
            playerInputController.OnSkipVideoEvent -= HandleSkipVideo;

        // Amaga la UI del vídeo
        if (videoUI != null)
            videoUI.SetActive(false);

        // Amaga el botó de saltar
        if (skipButton != null)
            skipButton.SetActive(false);

        // Reprèn el joc
        Time.timeScale = 1f;
        _videoPlaying = false;

        // Obre la porta si està configurat
        if (openDoorAfterVideo && doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTrigger);
        }
    }

    private void HandleSkipVideo()
    {
        if (!_videoPlaying) return;

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            OnVideoFinished(videoPlayer);
        }
    }
}
