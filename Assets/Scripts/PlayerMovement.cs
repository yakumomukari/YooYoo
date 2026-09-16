using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private YoyoTuning tuning;
    [SerializeField] private LayerMask groundMask = ~(1 << 3);

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private float horizontalInput;

    public Rigidbody2D Body => body;
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        body.gravityScale = tuning.gravityScale;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y - tuning.groundCheckDistance * 0.5f);
        Vector2 size = new Vector2(bounds.size.x * 0.8f, tuning.groundCheckDistance);
        IsGrounded = Physics2D.OverlapBox(origin, size, 0f, groundMask) != null;

        float targetX = horizontalInput * tuning.groundMoveSpeed;
        float control = IsGrounded ? 1f : tuning.airControl;
        float newX = Mathf.MoveTowards(body.velocity.x, targetX,
            tuning.groundAcceleration * control * Time.fixedDeltaTime);
        body.velocity = new Vector2(newX, body.velocity.y);
    }
}
