using UnityEngine;

[CreateAssetMenu(menuName = "Yooooooooo/Yoyo Tuning", fileName = "YoyoTuning")]
public sealed class YoyoTuning : ScriptableObject
{
    [Header("Defaults Synchronization")]
    [Tooltip("When enabled, Unity copies the defaults declared in this script to this asset after scripts reload.")]
    public bool automaticallySyncCodeDefaults = true;

    [Header("Player")]
    public float groundMoveSpeed = 12f;
    public float groundAcceleration = 40f;
    [Range(0f, 1f)] public float airControl = 0.30f;
    public float gravityScale = 1f;
    public float groundCheckDistance = 0.08f;

    [Header("Throw")]
    public float minimumThrowSpeed = 12f;
    public float maximumThrowSpeed = 36f;
    public float chargeDuration = 1.1f;
    public float maximumFlightTime = 1f;
    public float yoyoVisualSpinSpeed = 1080f;

    [Header("Bullet Time")]
    [Range(0.05f, 1f)] public float bulletTimeScale = 0.2f;

    [Header("Rope")]
    public bool ropeCollidesWithConnectedBody = true;

    [Header("Phase Two Angular Acceleration Experiment")]
    public bool enableAngularAccelerationExperiment = true;
    [Min(0f)] public float angularAcceleration = 1.2f;
    [Min(0f)] public float maximumAngularSpeed = 4f;
    [Min(0f)] public float minimumTangentialSpeed = 0.1f;

}
