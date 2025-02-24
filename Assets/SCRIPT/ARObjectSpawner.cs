using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("List of prefabs to randomly instantiate on a plane at the touch location.")]
    List<GameObject> m_PlacedPrefabs = new List<GameObject>();

    [SerializeField]
    GameObject visualObject;

    [SerializeField]
    [Tooltip("Maximum Y height difference from the lowest detected plane to consider as floor")]
    float maxFloorHeightDifference = 0.1f;

    UnityEvent placementUpdate;

    private ARPlaneManager planeManager;
    private bool hasSpawnedThisTouch = false;
    private Vector2 lastTouchPosition;
    private float lowestPlaneY = float.MaxValue;

    public List<GameObject> placedPrefabs
    {
        get { return m_PlacedPrefabs; }
        set { m_PlacedPrefabs = value; }
    }

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (placementUpdate == null)
            placementUpdate = new UnityEvent();
        placementUpdate.AddListener(DisableVisual);

        // Subscribe to the planesChanged event
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDestroy()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Update the lowest plane Y position when new planes are added
        foreach (var plane in args.added)
        {
            if (plane.center.y < lowestPlaneY)
            {
                lowestPlaneY = plane.center.y;
            }
        }
    }

    bool IsFloorPlane(ARRaycastHit hit)
    {
        // If this is one of the first planes detected, consider it as a potential floor
        if (lowestPlaneY == float.MaxValue)
        {
            lowestPlaneY = hit.pose.position.y;
            return true;
        }

        // Check if the hit plane is close enough to the lowest detected plane
        return Mathf.Abs(hit.pose.position.y - lowestPlaneY) <= maxFloorHeightDifference;
    }

    void Update()
    {
        if (Input.touchCount == 0)
        {
            hasSpawnedThisTouch = false;
            return;
        }

        Touch touch = Input.GetTouch(0);
        lastTouchPosition = touch.position;

        if (touch.phase == TouchPhase.Ended && !hasSpawnedThisTouch)
        {
            SpawnPrefab();
            hasSpawnedThisTouch = true;
        }
    }

    private void SpawnPrefab()
    {
        if (m_RaycastManager.Raycast(lastTouchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Find the first hit that corresponds to a floor plane
            ARRaycastHit floorHit = new ARRaycastHit();
            bool foundFloor = false;

            foreach (var hit in s_Hits)
            {
                if (IsFloorPlane(hit))
                {
                    floorHit = hit;
                    foundFloor = true;
                    break;
                }
            }

            // Only spawn if we hit a floor plane
            if (foundFloor && m_PlacedPrefabs != null && m_PlacedPrefabs.Count > 0)
            {
                int randomIndex = Random.Range(0, m_PlacedPrefabs.Count);
                GameObject prefabToSpawn = m_PlacedPrefabs[randomIndex];

                Instantiate(prefabToSpawn, floorHit.pose.position, floorHit.pose.rotation);
                placementUpdate.Invoke();
            }
        }
    }

    public void DisableVisual()
    {
        visualObject.SetActive(false);
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    ARRaycastManager m_RaycastManager;
}