using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MoveBehaviour : MonoBehaviour
{
    private Rigidbody _rb;
    public Animator animator;

    public float velocity;
    //public float speed;
    public float jumpForce;
    public float rotSpeed = 10f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        //_sr = GetComponent<SpriteRenderer>();
        //animator = GetComponent<Animator>();
    }

    public void FixedUpdate()
    {
        //Debug.Log("Current Velocity: " + _rb.velocity);
        velocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
        animator.SetFloat("velocity", velocity);
    }

    public void MoveCharacter(Vector3 direction)
    {
        //Debug.Log("Moving character in direction: " + direction);
        _rb.AddForce(direction.normalized, ForceMode.VelocityChange);


        // Girrar camara con la direccion del movimiento
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotSpeed * Time.deltaTime
            );
        }

        //_rb.linearVelocity = new Vector3(direction.normalized.x * speed, direction.normalized.y * speed, direction.normalized.z * speed);
    }

    public void JumpCharacter()
    {
        _rb.linearVelocity = new Vector3(0f, 0f, 0f);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);     
    }
}
