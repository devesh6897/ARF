using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARPlaneSelection : MonoBehaviour
{
    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

    private ARPlane selectedPlane;

    void Start()
    {
        // Get AR Plane and Raycast Managers
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();

        if (arPlaneManager == null || arRaycastManager == null)
        {
            Debug.LogError("AR Managers are missing in the scene.");
            return;
        }
    }

    void Update()
    {
        // Detect touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            DetectPlaneSelection(touchPosition);
        }
    }

    void DetectPlaneSelection(Vector2 touchPosition)
    {
        if (arRaycastManager.Raycast(touchPosition, hitResults, TrackableType.PlaneWithinPolygon))
        {
            // Get the ARPlane from the hit result
            ARPlane tappedPlane = arPlaneManager.GetPlane(hitResults[0].trackableId);

            if (tappedPlane != null)
            {
                SelectMainPlane(tappedPlane);
            }
        }
    }

    void SelectMainPlane(ARPlane mainPlane)
    {
        selectedPlane = mainPlane;
        selectedPlane.gameObject.GetComponent<MeshRenderer>().material.color = Color.green; // Highlight selected plane

        Debug.Log("Main Plane Selected: " + selectedPlane.gameObject.name);

        // Expand the selected plane if needed
        selectedPlane.transform.localScale *= 1.5f;

        // Disable all other planes
        foreach (ARPlane plane in arPlaneManager.trackables)
        {
            if (plane != selectedPlane)
            {
                Destroy(plane.gameObject);
            }
        }

        // Disable further plane detection
        arPlaneManager.enabled = false;
    }
}
