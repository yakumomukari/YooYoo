using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private YoyoTuning tuning;
    [SerializeField] private LayerMask groundMask = ~(1 << 3);

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private float horizontalInput;
    private readonly RaycastHit2D[] orbitCastHits = new RaycastHit2D[8];

    public Rigidbody2D Body => body;
    public bool IsGrounded { get; private set; }
    public bool YoyoOrbitActive { get; set; }

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

        if (IsGrounded && !YoyoOrbitActive)
        {
            float targetX = horizontalInput * tuning.groundMoveSpeed;
            float newX = Mathf.MoveTowards(body.velocity.x, targetX,
                tuning.groundAcceleration * Time.fixedDeltaTime);
            body.velocity = new Vector2(newX, body.velocity.y);
            PreserveOrbitCollisions();
            return;
        }

        // Air control adds acceleration without braking or clamping existing
        // momentum, so tangential speed can survive and build during an orbit.
        if (!YoyoOrbitActive && Mathf.Abs(horizontalInput) > 0.01f)
        {
            Vector2 force = Vector2.right *
                (horizontalInput * tuning.groundAcceleration * tuning.airControl * body.mass);
            body.AddForce(force, ForceMode2D.Force);
        }

        PreserveOrbitCollisions();
    }

    private void PreserveOrbitCollisions()
    {
        if (!YoyoOrbitActive || body.velocity.sqrMagnitude < 0.0001f)
            return;

        Vector2 direction = body.velocity.normalized;
        float travelDistance = body.velocity.magnitude * Time.fixedDeltaTime + 0.02f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundMask);
        filter.useTriggers = false;

        int hitCount = body.Cast(direction, filter, orbitCastHits, travelDistance);
        float nearestDistance = float.PositiveInfinity;
        Vector2 nearestNormal = Vector2.zero;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = orbitCastHits[i];
            if (hit.collider == null || hit.distance >= nearestDistance)
                continue;

            nearestDistance = hit.distance;
            nearestNormal = hit.normal;
        }

        if (nearestDistance == float.PositiveInfinity)
            return;

        float velocityIntoSurface = Vector2.Dot(body.velocity, nearestNormal);
        if (velocityIntoSurface < 0f)
            body.velocity -= nearestNormal * velocityIntoSurface;
    }
}
