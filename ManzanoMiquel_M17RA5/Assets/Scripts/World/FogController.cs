// ════════════════════════════════════════════════════════════════════════════
// FogController
// Configura la boira lineal de Unity en iniciar l'escena.
// "Aplica boira, renderitza els objectes en funció de la vista de la càmera
//  o millora el rendiment de visualització d'exteriors" — requerit.
// ════════════════════════════════════════════════════════════════════════════

using UnityEngine;

public class FogController : MonoBehaviour
{
    [Header("Boira")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color fogColor = new Color(0.7f, 0.8f, 0.9f);
    [SerializeField] private float fogStart = 30f;
    [SerializeField] private float fogEnd = 120f;

    private void Start()
    {
        RenderSettings.fog = enableFog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;
    }
}