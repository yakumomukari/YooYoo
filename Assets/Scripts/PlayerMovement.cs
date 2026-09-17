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
    public bool YoyoOrbitActive { get; set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        // Scene serialization may preserve an older all-layers mask. Never let
        // the feet probe classify this player's own collider as ground.
        groundMask &= ~(1 << gameObject.layer);
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

        if (IsGrounded && !YoyoOrbitActive)
        {
            float targetX = horizontalInput * tuning.groundMoveSpeed;
            float newX = Mathf.MoveTowards(body.velocity.x, targetX,
                tuning.groundAcceleration * Time.fixedDeltaTime);
            body.velocity = new Vector2(newX, body.velocity.y);
            return;
        }

        // Air input can steer toward the normal ground-speed range, but no input
        // leaves horizontal momentum untouched. Momentum already above that range
        // is preserved and cannot be increased further in the same direction.
        if (!YoyoOrbitActive && Mathf.Abs(horizontalInput) > 0.01f)
        {
            float targetX = horizontalInput * tuning.groundMoveSpeed;
            float controlledX = Mathf.MoveTowards(body.velocity.x, targetX,
                tuning.groundAcceleration * tuning.airControl * Time.fixedDeltaTime);
            float velocityChange = controlledX - body.velocity.x;
            body.AddForce(Vector2.right * (velocityChange * body.mass), ForceMode2D.Impulse);
        }

    }
}
