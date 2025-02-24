using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class GrassSpawner : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject grassPrefab; // Assign a 3D grass prefab in the Inspector
    public int grassDensity = 20; // Number of grass patches per plane

    void Start()
    {
        // Listen for new planes being detected
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        planeManager.planesChanged -= OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Spawn grass on newly detected planes
        foreach (var plane in args.added)
        {
            SpawnGrassOnPlane(plane);
        }
    }

    private void SpawnGrassOnPlane(ARPlane plane)
    {
        Vector3 planeCenter = plane.center; // Get center of the plane
        Vector2 planeSize = plane.size; // Get width and height of the plane

        for (int i = 0; i < grassDensity; i++)
        {
            // Random position within plane boundaries
            float xOffset = Random.Range(-planeSize.x / 2, planeSize.x / 2);
            float zOffset = Random.Range(-planeSize.y / 2, planeSize.y / 2);
            Vector3 spawnPosition = planeCenter + new Vector3(xOffset, 0, zOffset);

            // Spawn grass at random positions on the plane
            Instantiate(grassPrefab, spawnPosition, Quaternion.identity, plane.transform);
        }
    }
}
