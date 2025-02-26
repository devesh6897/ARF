using UnityEngine;
using System.Collections.Generic;

public class WindZone : MonoBehaviour
{
    [Header("Wind Settings")]
    [Tooltip("Base wind force")]
    public Vector3 windDirection = new Vector3(1, 0, 0);
    [Range(0, 10)]
    public float windStrength = 2.0f;
    [Range(0, 2)]
    public float turbulence = 0.5f;
    [Range(0, 3)]
    public float pulseMagnitude = 1.0f;
    [Range(0.1f, 5.0f)]
    public float pulseFrequency = 1.0f;

    [Header("Debug")]
    public bool showDebugWindVectors = false;
    public float debugLineLength = 1.0f;
    public Color debugLineColor = Color.cyan;

    // Store all rigidbodies in the zone
    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    // Store their drag values to restore when they exit
    private Dictionary<Rigidbody, float> originalDrag = new Dictionary<Rigidbody, float>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !affectedObjects.Contains(rb))
        {
            affectedObjects.Add(rb);
            originalDrag[rb] = rb.drag; // Store original drag
            rb.drag *= 0.5f; // Reduce drag in wind
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && affectedObjects.Contains(rb))
        {
            affectedObjects.Remove(rb);
            // Restore original drag
            if (originalDrag.ContainsKey(rb))
            {
                rb.drag = originalDrag[rb];
                originalDrag.Remove(rb);
            }
        }
    }

    private void FixedUpdate()
    {
        ApplyWindForceToObjects();
    }

    private void ApplyWindForceToObjects()
    {
        if (affectedObjects.Count == 0) return;

        // Calculate current wind force with some variation
        Vector3 currentWindForce = CalculateCurrentWindForce();

        foreach (Rigidbody rb in affectedObjects)
        {
            if (rb == null) continue;

            // Scale force by object size (cross-sectional area facing the wind)
            float crossSection = CalculateCrossSectionalArea(rb, currentWindForce.normalized);
            Vector3 scaledForce = currentWindForce * crossSection;

            // Apply the force
            rb.AddForce(scaledForce, ForceMode.Force);
        }
    }

    private Vector3 CalculateCurrentWindForce()
    {
        // Base wind direction normalized and scaled by strength
        Vector3 baseWind = windDirection.normalized * windStrength;

        // Add time-based variation (turbulence)
        float xVar = Mathf.PerlinNoise(Time.time * turbulence, 0) * 2 - 1;
        float yVar = Mathf.PerlinNoise(0, Time.time * turbulence) * 2 - 1;
        float zVar = Mathf.PerlinNoise(Time.time * turbulence, Time.time * turbulence) * 2 - 1;

        Vector3 windVar = new Vector3(xVar, yVar, zVar) * turbulence;

        // Add pulsing effect
        float pulse = Mathf.Sin(Time.time * pulseFrequency) * pulseMagnitude;

        return baseWind + windVar + (baseWind.normalized * pulse);
    }

    private float CalculateCrossSectionalArea(Rigidbody rb, Vector3 windDirection)
    {
        // Get bounds of the object
        Bounds bounds = new Bounds(rb.position, Vector3.zero);

        foreach (Collider collider in rb.GetComponentsInChildren<Collider>())
        {
            bounds.Encapsulate(collider.bounds);
        }

        // Calculate an approximation of the cross-sectional area facing the wind
        Vector3 size = bounds.size;

        // Dot product to determine how much of each axis faces the wind
        float xFactor = Mathf.Abs(Vector3.Dot(windDirection, Vector3.right));
        float yFactor = Mathf.Abs(Vector3.Dot(windDirection, Vector3.up));
        float zFactor = Mathf.Abs(Vector3.Dot(windDirection, Vector3.forward));

        // Calculate the cross-sectional area based on the wind direction
        float area = (size.y * size.z * xFactor) + (size.x * size.z * yFactor) + (size.x * size.y * zFactor);

        // Scale the force by mass to make it more realistic
        return area * 0.1f; // Scale factor to make the force reasonable
    }

    private void OnDrawGizmos()
    {
        if (!showDebugWindVectors) return;

        Gizmos.color = debugLineColor;

        // Draw main wind direction
        Gizmos.DrawLine(transform.position, transform.position + windDirection.normalized * debugLineLength);

        // Draw a grid of wind vectors within the collider if it's a box collider
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Vector3 size = boxCollider.size;
            Vector3 center = boxCollider.center;

            for (float x = -size.x / 2; x <= size.x / 2; x += size.x / 4)
            {
                for (float y = -size.y / 2; y <= size.y / 2; y += size.y / 4)
                {
                    for (float z = -size.z / 2; z <= size.z / 2; z += size.z / 4)
                    {
                        Vector3 localPoint = center + new Vector3(x, y, z);
                        Vector3 worldPoint = transform.TransformPoint(localPoint);

                        Vector3 windAtPoint = CalculateCurrentWindForce().normalized * debugLineLength * 0.5f;
                        Gizmos.DrawLine(worldPoint, worldPoint + windAtPoint);
                    }
                }
            }
        }
    }
}