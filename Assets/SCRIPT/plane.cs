using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

[RequireComponent(typeof(ARPlaneManager))]
public class FloorPlaneDetector : MonoBehaviour
{
    private ARPlaneManager planeManager;
    private List<ARPlane> detectedPlanes = new List<ARPlane>();
    private ARPlane currentFloorPlane;

    [SerializeField]
    private float maxVerticalOffset = 0.1f; // Maximum allowed vertical angle offset for horizontal planes

    void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Handle added planes
        foreach (ARPlane plane in args.added)
        {
            // Only consider horizontal planes (floors)
            if (IsHorizontalPlane(plane))
            {
                detectedPlanes.Add(plane);
                UpdateFloorPlane();
            }
            else
            {
                // Hide non-horizontal planes
                plane.gameObject.SetActive(false);
            }
        }

        // Handle updated planes
        foreach (ARPlane plane in args.updated)
        {
            if (IsHorizontalPlane(plane))
            {
                if (!detectedPlanes.Contains(plane))
                {
                    detectedPlanes.Add(plane);
                }
                UpdateFloorPlane();
            }
            else
            {
                plane.gameObject.SetActive(false);
                detectedPlanes.Remove(plane);
            }
        }

        // Handle removed planes
        foreach (ARPlane plane in args.removed)
        {
            detectedPlanes.Remove(plane);
            if (plane == currentFloorPlane)
            {
                currentFloorPlane = null;
                UpdateFloorPlane();
            }
        }
    }

    private bool IsHorizontalPlane(ARPlane plane)
    {
        // Check if the plane is roughly horizontal by comparing its normal to the up vector
        return Vector3.Angle(plane.normal, Vector3.up) <= maxVerticalOffset;
    }

    private void UpdateFloorPlane()
    {
        if (detectedPlanes.Count == 0)
        {
            currentFloorPlane = null;
            return;
        }

        // Find the plane with minimum Y position
        ARPlane lowestPlane = detectedPlanes[0];
        float lowestY = lowestPlane.center.y;

        foreach (ARPlane plane in detectedPlanes)
        {
            if (plane.center.y < lowestY)
            {
                lowestY = plane.center.y;
                lowestPlane = plane;
            }
        }

        // Update current floor plane
        if (currentFloorPlane != lowestPlane)
        {
            if (currentFloorPlane != null)
            {
                currentFloorPlane.gameObject.SetActive(false);
            }
            currentFloorPlane = lowestPlane;
            currentFloorPlane.gameObject.SetActive(true);
        }

        // Hide all other planes
        foreach (ARPlane plane in detectedPlanes)
        {
            if (plane != currentFloorPlane)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }

    // Public method to get the current floor plane
    public ARPlane GetCurrentFloorPlane()
    {
        return currentFloorPlane;
    }
}