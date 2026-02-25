using UnityEngine;

// ════════════════════════════════════════════════════════════════════════════
// MinimapCamera
// Càmera ortogràfica que segueix el jugador des de dalt en temps real.
// ════════════════════════════════════════════════════════════════════════════

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float height = 30f;

    private void Awake()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y + height, target.position.z);
        transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }
}