using UnityEngine;

public sealed class BulletTimeController : MonoBehaviour
{
    [SerializeField] private YoyoTuning tuning;
    private float normalFixedDeltaTime;

    private void Awake()
    {
        normalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void SetAiming(bool aiming)
    {
        float scale = aiming ? tuning.bulletTimeScale : 1f;
        Time.timeScale = scale;
        Time.fixedDeltaTime = normalFixedDeltaTime * scale;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }
}
