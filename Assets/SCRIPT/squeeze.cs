using UnityEngine;

public class GhostManipulator : MonoBehaviour
{
    private readonly Vector2[] lastTouchPositions = new Vector2[2];

    private bool isManipulating = false;

    void Update()
    {
        // Handle two-finger gestures for scale and rotation.
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (!isManipulating)
            {
                // Record the initial positions of both touches.
                lastTouchPositions[0] = touchZero.position;
                lastTouchPositions[1] = touchOne.position;
                isManipulating = true;
            }
            else
            {
                // Calculate the previous and current distances between touches.
                float prevDistance = (lastTouchPositions[0] - lastTouchPositions[1]).magnitude;
                float currentDistance = (touchZero.position - touchOne.position).magnitude;
                float deltaDistance = currentDistance - prevDistance;

                // Adjust the scale based on the change in distance.
                float scaleFactor = 1 + (deltaDistance / 500.0f); // Adjust the denominator to control sensitivity.
                transform.localScale *= scaleFactor;

                // For rotation, compare the angle between the two fingers.
                Vector2 prevDir = lastTouchPositions[1] - lastTouchPositions[0];
                Vector2 currentDir = touchOne.position - touchZero.position;
                float angleDiff = Vector2.SignedAngle(prevDir, currentDir);
                transform.Rotate(Vector3.up, angleDiff);

                // Update last touch positions for the next frame.
                lastTouchPositions[0] = touchZero.position;
                lastTouchPositions[1] = touchOne.position;
            }
        }
        else
        {
            // Reset the manipulation flag if fewer than 2 touches are detected.
            isManipulating = false;
        }

        // Optionally: Handle one-finger drag for moving the ghost.
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            // Check if the touch is a drag and update position accordingly.
            if (touch.phase == TouchPhase.Moved)
            {
                // Cast a ray from the touch position into the AR scene.
                Ray ray = Camera.main.ScreenPointToRay(touch.position);

                // This example assumes there is a collider on the plane or a layer that you can hit.
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Update the ghost’s position while keeping its current Y (or adjust as needed).
                    Vector3 newPos = hit.point;
                    transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
                }
            }
        }
    }
}