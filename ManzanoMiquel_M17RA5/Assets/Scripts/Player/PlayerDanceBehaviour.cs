using System.Collections;
using UnityEngine;

/// <summary>
/// Ball de la victòria: bloqueja el Rigidbody, activa la càmera frontal,
/// espera la durada de l'animació i ho restaura tot.
/// El jugador no es pot moure mentre balla (requerit per l'enunciat).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerDanceBehaviour : MonoBehaviour
{
    [Header("Ball")]
    [SerializeField] private float animationDuration = 5f;   // Ha de coincidir amb el clip de Mixamo

    public bool IsDancing { get; private set; }

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerAnimationBehaviour _animation;
    private PlayerLookBehaviour _look;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _animation = GetComponent<PlayerAnimationBehaviour>();
        _look = GetComponent<PlayerLookBehaviour>();

        _input.OnDanceEvent += HandleDance;
    }

    private void OnDestroy()
    {
        _input.OnDanceEvent -= HandleDance;
    }

    private void HandleDance()
    {
        if (IsDancing) return;
        StartCoroutine(DanceRoutine());
    }

    private IEnumerator DanceRoutine()
    {
        IsDancing = true;

        // Congela el Rigidbody perquè el personatge no es mogui
        _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        //_rb.isKinematic = true;

        _animation?.SetDancing(true);
        _look?.SetFrontCamera(true);

        yield return new WaitForSeconds(animationDuration);

        _look?.SetFrontCamera(false);
        _animation?.SetDancing(false);

        //_rb.isKinematic = false;
        IsDancing = false;
    }
}