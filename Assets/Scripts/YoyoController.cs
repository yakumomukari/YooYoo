using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public sealed class YoyoController : MonoBehaviour
{
    public enum YoyoState { Ready, Aiming, Flying, Anchored }

    [SerializeField] private YoyoTuning tuning;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private YoyoProjectile projectile;
    [SerializeField] private BulletTimeController bulletTime;
    [SerializeField] private Transform launchOrigin;
    [SerializeField] private LineRenderer ropeLine;

    private DistanceJoint2D ropeJoint;
    private Collider2D playerCollider;
    private Camera mainCamera;
    private float chargeStartedAt;

    public YoyoState State { get; private set; } = YoyoState.Ready;
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public float Charge01 { get; private set; }
    public Vector2 AnchorPoint { get; private set; }
    public float RopeLength => ropeJoint.distance;
    public bool AngularAccelerationEnabled => tuning.enableAngularAccelerationExperiment;
    public float TangentialSpeed { get; private set; }
    public float AngularSpeed { get; private set; }
    public int RotationDirection { get; private set; }

    private void Awake()
    {
        ropeJoint = GetComponent<DistanceJoint2D>();
        playerCollider = player.GetComponent<Collider2D>();
        ropeJoint.enabled = false;
        ropeJoint.autoConfigureConnectedAnchor = false;
        // The hit distance is the orbit radius. World collision remains enabled
        // separately so the player still collides with terrain while constrained.
        ropeJoint.maxDistanceOnly = false;
        ropeJoint.enableCollision = tuning.ropeCollidesWithConnectedBody;
        mainCamera = Camera.main;
        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());
    }

    private void Update()
    {
        UpdateAimDirection();

        if (Input.GetMouseButtonDown(0))
            BeginAim();

        if (State == YoyoState.Aiming)
        {
            Charge01 = Mathf.Clamp01((Time.unscaledTime - chargeStartedAt) / tuning.chargeDuration);
            if (Input.GetMouseButtonUp(0))
                Throw();
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.E))
            Recall();

        UpdateRopeLine();
    }

    private void UpdateAimDirection()
    {
        Vector3 mouse = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = (Vector2)mouse - (Vector2)launchOrigin.position;
        if (delta.sqrMagnitude > 0.0001f)
            AimDirection = delta.normalized;
    }

    private void BeginAim()
    {
        if (State == YoyoState.Flying || State == YoyoState.Anchored)
            DetachAndHide();

        State = YoyoState.Aiming;
        Charge01 = 0f;
        chargeStartedAt = Time.unscaledTime;
        bulletTime.SetAiming(true);
    }

    private void Throw()
    {
        bulletTime.SetAiming(false);
        float speed = Mathf.Lerp(tuning.minimumThrowSpeed, tuning.maximumThrowSpeed, Charge01);
        projectile.Launch(GetLaunchPosition(), AimDirection * speed,
            tuning.maximumFlightTime, tuning.yoyoVisualSpinSpeed, OnProjectileHit, Recall);
        State = YoyoState.Flying;
    }

    private Vector2 GetLaunchPosition()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 extents = bounds.extents;
        float playerEdgeDistance = Mathf.Abs(AimDirection.x) * extents.x +
                                   Mathf.Abs(AimDirection.y) * extents.y;
        return (Vector2)bounds.center + AimDirection *
            (playerEdgeDistance + projectile.WorldRadius + 0.01f);
    }

    private void OnProjectileHit(Vector2 point)
    {
        AnchorPoint = point;
        ropeJoint.connectedBody = null;
        ropeJoint.connectedAnchor = point;
        ropeJoint.distance = Vector2.Distance(player.Body.position, point);
        ropeJoint.enabled = true;
        player.YoyoOrbitActive = true;
        State = YoyoState.Anchored;
    }

    private void FixedUpdate()
    {
        if (State != YoyoState.Anchored)
        {
            ResetOrbitDebugValues();
            return;
        }

        Vector2 radial = player.Body.position - AnchorPoint;
        float radius = radial.magnitude;
        if (radius < 0.0001f)
        {
            ResetOrbitDebugValues();
            return;
        }

        Vector2 counterClockwiseTangent = new Vector2(-radial.y, radial.x) / radius;
        TangentialSpeed = Vector2.Dot(player.Body.velocity, counterClockwiseTangent);
        AngularSpeed = TangentialSpeed / radius;
        RotationDirection = Mathf.Abs(TangentialSpeed) >= tuning.minimumTangentialSpeed
            ? (TangentialSpeed > 0f ? 1 : -1)
            : 0;

        if (!tuning.enableAngularAccelerationExperiment || RotationDirection == 0)
            return;

        float remainingAngularSpeed = tuning.maximumAngularSpeed - Mathf.Abs(AngularSpeed);
        if (remainingAngularSpeed <= 0f)
            return;

        // Angular acceleration is converted to tangential acceleration at the
        // current rope radius. The final physics step is reduced so the drive
        // cannot push its own contribution beyond the configured speed cap.
        float appliedAngularAcceleration = Mathf.Min(tuning.angularAcceleration,
            remainingAngularSpeed / Time.fixedDeltaTime);
        Vector2 driveDirection = counterClockwiseTangent * RotationDirection;
        Vector2 force = driveDirection *
            (appliedAngularAcceleration * radius * player.Body.mass);
        player.Body.AddForce(force, ForceMode2D.Force);
    }

    private void ResetOrbitDebugValues()
    {
        TangentialSpeed = 0f;
        AngularSpeed = 0f;
        RotationDirection = 0;
    }

    public void Recall()
    {
        if (State == YoyoState.Aiming)
            bulletTime.SetAiming(false);
        DetachAndHide();
        State = YoyoState.Ready;
        Charge01 = 0f;
    }

    private void DetachAndHide()
    {
        // Disabling the joint does not overwrite the player's current velocity.
        ropeJoint.enabled = false;
        player.YoyoOrbitActive = false;
        projectile.Hide();
    }

    private void UpdateRopeLine()
    {
        bool visible = State == YoyoState.Flying || State == YoyoState.Anchored;
        ropeLine.enabled = visible;
        if (!visible) return;
        ropeLine.SetPosition(0, launchOrigin.position);
        ropeLine.SetPosition(1, projectile.transform.position);
    }

    private void OnDisable()
    {
        bulletTime.SetAiming(false);
    }
}
