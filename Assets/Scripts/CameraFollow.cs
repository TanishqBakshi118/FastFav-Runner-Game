using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Tooltip("The base offset from the player. X:0, Y:3, Z:-6 is recommended.")]
    public Vector3 offset = new Vector3(0, 3f, -6f);

    [Tooltip("How smoothly the camera catches up to the player.")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null) return;

        // We track the player's X and Y, but keep Z locked to the offset since the player doesn't move forward
        Vector3 targetPosition = new Vector3(
            player.position.x + offset.x,
            player.position.y + offset.y,
            offset.z
        );

        // Smoothly interpolate between the current camera position and the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}