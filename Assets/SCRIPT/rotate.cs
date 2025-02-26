using UnityEngine;

public class RotateAroundZ : MonoBehaviour
{
    [Tooltip("Speed of rotation in degrees per second")]
    public float rotationSpeed = 45.0f;

    [Tooltip("Set to true to rotate clockwise, false for counter-clockwise")]
    public bool rotateClockwise = false;

    void Update()
    {
        // Determine the direction of rotation
        float direction = rotateClockwise ? -1.0f : 1.0f;

        // Calculate rotation amount this frame
        float rotationAmount = rotationSpeed * direction * Time.deltaTime;

        // Apply rotation around the Z axis
        transform.Rotate(0, rotationAmount, 0);
    }
}