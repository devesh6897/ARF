using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation; // Make sure to include AR Foundation

public class ARWindZone : MonoBehaviour
{
    [Header("AR Setup")]
    public ARPlaneManager planeManager;
    public float heightAbovePlane = 0.5f; // How high above the plane the wind zone should be
    public Vector2 windZoneSize = new Vector2(3, 3); // X,Z size of the wind zone
    public float windZoneHeight = 2f; // Height of the wind zone

    [Header("Wind Settings")]
    [Tooltip("Base wind force")]
    public Vector3 windDirection = new Vector3(1, 0.1f, 0);
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
    public Color debugLineColor = Color.cyan;

    // Store all rigidbodies in the zone
    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    private Dictionary<Rigidbody, float> originalDrag = new Dictionary<Rigidbody, float>();
    private GameObject currentWindZone;
    private BoxCollider windZoneCollider;


    //rain
    public GameObject RAINprefab;
    private GameObject rainInstance;
    public float rainheight = 0.5f;

    //cloud
    public GameObject cloud;
    private GameObject cloudInstance;
    public float cloudheight = 3f;

    private void Start()
    {
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
        }

        // Subscribe to plane detected event
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
        else
        {
            Debug.LogError("ARPlaneManager not found! Please assign it in the inspector.");
        }

        // Create the wind zone collider on this object
        SetupWindZoneCollider();
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Position wind zone when a new plane is detected
        foreach (ARPlane plane in args.added)
        {
            PositionWindZoneAbovePlane(plane);
            SpawnRainAbovePlane(plane);
            SpawnCloudAbovePlane(plane);


            break; // Just use the first detected plane
        }
    }

    private void SpawnRainAbovePlane(ARPlane plane)
    {
        if (RAINprefab == null)
        {
            Debug.LogError("Rain prefab is not assigned!");
            return;
        }

        Vector3 rainPosition = plane.center + Vector3.up * (rainheight + 2f);
        if (rainInstance == null)
        {
            rainInstance = Instantiate(RAINprefab, rainPosition, Quaternion.identity);
        }
        else
        {
            rainInstance.transform.position = rainPosition;
        }
    }

    private void SpawnCloudAbovePlane(ARPlane plane)
    {
        if (cloud == null)
        {
            Debug.LogError("Rain prefab is not assigned!");
            return;
        }

        Vector3 rainPosition = plane.center + Vector3.up * (cloudheight + 2f);
        if (cloudInstance == null)
        {
            cloudInstance = Instantiate(cloud, rainPosition, Quaternion.identity);
        }
        else
        {
            cloudInstance.transform.position = rainPosition;
        }
    }
    private void SetupWindZoneCollider()
    {
        // Add box collider to this GameObject
        windZoneCollider = gameObject.AddComponent<BoxCollider>();
        windZoneCollider.size = new Vector3(windZoneSize.x, windZoneHeight, windZoneSize.y);
        windZoneCollider.isTrigger = true;
    }

    private void PositionWindZoneAbovePlane(ARPlane plane)
    {
        // Position it above the plane
        Vector3 planeCenter = plane.center;
        planeCenter.y += heightAbovePlane + (windZoneHeight / 2);
        transform.position = planeCenter;

        Debug.Log("Wind zone positioned above AR plane");
    }

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

        // Calculate current wind force with variation
        Vector3 currentWindForce = CalculateCurrentWindForce();

        foreach (Rigidbody rb in affectedObjects)
        {
            if (rb == null) continue;

            // Scale force by object size
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

        // Calculate an approximation of the cross-sectional area
        Vector3 size = bounds.size;

        // Dot product to determine how much of each axis faces the wind
        float xFactor = Mathf.Abs(Vector3.Dot(windDirection, Vector3.right));
        float yFactor = Mathf.Abs(Vector3.Dot(windDirection, Vector3.up));
        float zFactor = Mathf.Abs(Vector3.Dot(windDirection, Vector3.forward));

        // Calculate the cross-sectional area
        float area = (size.y * size.z * xFactor) + (size.x * size.z * yFactor) + (size.x * size.y * zFactor);

        return area * 0.1f; // Scale factor
    }

    private void OnDrawGizmos()
    {
        if (!showDebugWindVectors || windZoneCollider == null) return;

        Gizmos.color = debugLineColor;

        // Draw main wind direction
        Gizmos.DrawLine(transform.position, transform.position + windDirection.normalized * windZoneHeight);

        // Draw a grid of wind vectors within the collider
        Vector3 size = windZoneCollider.size;
        Vector3 center = windZoneCollider.center;

        for (float x = -size.x / 2; x <= size.x / 2; x += size.x / 4)
        {
            for (float y = -size.y / 2; y <= size.y / 2; y += size.y / 4)
            {
                for (float z = -size.z / 2; z <= size.z / 2; z += size.z / 4)
                {
                    Vector3 localPoint = center + new Vector3(x, y, z);
                    Vector3 worldPoint = transform.TransformPoint(localPoint);

                    Vector3 windAtPoint = CalculateCurrentWindForce().normalized * windZoneHeight * 0.2f;
                    Gizmos.DrawLine(worldPoint, worldPoint + windAtPoint);
                }
            }
        }
    }
}