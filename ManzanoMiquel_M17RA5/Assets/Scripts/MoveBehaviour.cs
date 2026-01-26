using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MoveBehaviour : MonoBehaviour
{
    private Rigidbody _rb;
    public Animator animator;

    // Player velocity info
    public float horizontalVelocity;
    public float verticalVelocity;

    // Ground check info
    private bool isGrounded;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    
    public float jumpForce;
    public float rotSpeed = 10f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        //_sr = GetComponent<SpriteRenderer>();
        //animator = GetComponent<Animator>();
    }

    public void Update()
    {
        isGrounded = CheckGround();
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("verticalVelocity", _rb.linearVelocity.y);
        animator.SetFloat("horizontalVelocity", horizontalVelocity);
        Debug.Log("IsGrounded: " + isGrounded);
    }

    public void FixedUpdate()
    {
        //Debug.Log("Current Velocity: " + _rb.velocity);
        horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
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
        if (!isGrounded) return;

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        isGrounded = false;
    }

    private bool CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

}
