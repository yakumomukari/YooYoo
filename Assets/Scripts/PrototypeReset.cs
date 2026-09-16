using UnityEngine;

public sealed class PrototypeReset : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private YoyoController yoyo;
    [SerializeField] private Vector2 resetPosition = new Vector2(-12f, -2f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || playerBody.position.y < -20f)
        {
            yoyo.Recall();
            playerBody.position = resetPosition;
            playerBody.velocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
        }
    }
}
