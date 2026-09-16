using UnityEngine;
using UnityEngine.UI;

public sealed class PrototypeHUD : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private YoyoController yoyo;
    [SerializeField] private Text debugText;
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private RectTransform aimArrow;
    [SerializeField] private bool showDebug = true;

    private void Awake()
    {
        if (player != null && yoyo != null && debugText != null &&
            chargeSlider != null && aimArrow != null)
            return;

        Debug.LogError("PrototypeHUD is missing scene references. Rebuild the phase-one scene from the Yooooooooo menu.", this);
        enabled = false;
    }

    private void Update()
    {
        chargeSlider.gameObject.SetActive(yoyo.State == YoyoController.YoyoState.Aiming);
        chargeSlider.value = yoyo.Charge01;
        aimArrow.gameObject.SetActive(yoyo.State == YoyoController.YoyoState.Aiming);
        aimArrow.localRotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(yoyo.AimDirection.y, yoyo.AimDirection.x) * Mathf.Rad2Deg);

        debugText.gameObject.SetActive(showDebug);
        if (!showDebug) return;
        Vector2 velocity = player.Body.velocity;
        string anchor = yoyo.State == YoyoController.YoyoState.Anchored
            ? $"{yoyo.AnchorPoint:F2}" : "--";
        debugText.text = $"Velocity: {velocity:F2}  Speed: {velocity.magnitude:F2}\n" +
            $"Yoyo: {yoyo.State}  Anchor: {anchor}\n" +
            $"Rope: {(yoyo.State == YoyoController.YoyoState.Anchored ? yoyo.RopeLength.ToString("F2") : "--")}" +
            $"  Charge: {yoyo.Charge01:P0}  Time: {Time.timeScale:F2}";
    }
}
