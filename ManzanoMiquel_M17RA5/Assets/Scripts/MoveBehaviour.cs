using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MoveBehaviour : MonoBehaviour
{
    private Rigidbody _rb;

    public float speed;
    public float jumpForce;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        //_sr = GetComponent<SpriteRenderer>();
        //animator = GetComponent<Animator>();
    }

    public void MoveCharacter(Vector3 direction)
    {
        //Debug.Log("Moving character in direction: " + direction);
        _rb.AddForce(direction.normalized * speed, ForceMode.VelocityChange);
        //_rb.linearVelocity = new Vector3(direction.normalized.x * speed, direction.normalized.y * speed, direction.normalized.z * speed);
    }

    public void JumpCharacter()
    {
        _rb.linearVelocity = new Vector3(0f, 0f, 0f);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);     
    }
}
