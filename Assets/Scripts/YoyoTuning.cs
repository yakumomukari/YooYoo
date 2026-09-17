using UnityEngine;

[CreateAssetMenu(menuName = "Yooooooooo/Yoyo Tuning", fileName = "YoyoTuning")]
public sealed class YoyoTuning : ScriptableObject
{
    [Header("Player")]
    public float groundMoveSpeed = 12f;
    public float groundAcceleration = 70f;
    [Range(0f, 1f)] public float airControl = 0.65f;
    public float gravityScale = 3f;
    public float groundCheckDistance = 0.08f;

    [Header("Throw")]
    public float minimumThrowSpeed = 12f;
    public float maximumThrowSpeed = 36f;
    public float chargeDuration = 1.1f;
    public float maximumFlightTime = 2.5f;
    public float yoyoVisualSpinSpeed = 1080f;

    [Header("Bullet Time")]
    [Range(0.05f, 1f)] public float bulletTimeScale = 0.2f;

    [Header("Rope")]
    public bool ropeCollidesWithConnectedBody = true;

    [Header("Phase Two Angular Acceleration Experiment")]
    public bool enableAngularAccelerationExperiment = true;
    [Min(0f)] public float angularAcceleration = 4f;
    [Min(0f)] public float maximumAngularSpeed = 8f;
    [Min(0f)] public float minimumTangentialSpeed = 0.1f;
}
