using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class YoyoProjectile : MonoBehaviour
{
    private Rigidbody2D body;
    private Collider2D projectileCollider;
    private float worldRadius;
    private float visualSpinSpeed;
    private bool isDeployed;

    [SerializeField] private Transform visual;
    private float launchedAt;
    private float maximumFlightTime;
    private Action<Vector2> hitCallback;
    private Action timeoutCallback;

    public bool IsFlying { get; private set; }
    public float WorldRadius => worldRadius;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        worldRadius = Mathf.Max(projectileCollider.bounds.extents.x,
            projectileCollider.bounds.extents.y);
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        gameObject.SetActive(false);
    }

    public void Launch(Vector2 origin, Vector2 velocity, float flightTime, float spinSpeed,
        Action<Vector2> onHit, Action onTimeout)
    {
        transform.position = origin;
        gameObject.SetActive(true);
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.velocity = velocity;
        launchedAt = Time.unscaledTime;
        maximumFlightTime = flightTime;
        visualSpinSpeed = spinSpeed;
        hitCallback = onHit;
        timeoutCallback = onTimeout;
        IsFlying = true;
        isDeployed = true;
    }

    public void StopAt(Vector2 point)
    {
        IsFlying = false;
        body.velocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.position = point;
    }

    public void Hide()
    {
        IsFlying = false;
        isDeployed = false;
        body.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isDeployed && visual != null)
            visual.Rotate(0f, 0f, -visualSpinSpeed * Time.deltaTime);

        if (IsFlying && Time.unscaledTime - launchedAt >= maximumFlightTime)
            timeoutCallback?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsFlying || collision.contactCount == 0)
            return;

        Vector2 exactPoint = collision.GetContact(0).point;
        StopAt(exactPoint);
        hitCallback?.Invoke(exactPoint);
    }
}
