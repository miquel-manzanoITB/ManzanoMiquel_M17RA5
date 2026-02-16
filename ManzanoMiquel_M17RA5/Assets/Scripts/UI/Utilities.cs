using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// MinimapCamera
// Càmera ortogràfica que segueix el jugador des de dalt en temps real.
// La posició del jugador es representa al minimapa (requerit per l'enunciat).
// ════════════════════════════════════════════════════════════════════════════

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float height = 30f;

    private void LateUpdate()
    {
        if (player == null) return;
        transform.position = new Vector3(
            player.position.x,
            player.position.y + height,
            player.position.z);
        // Rota amb el jugador per a una vista top-down sempre orientada correctament
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}

