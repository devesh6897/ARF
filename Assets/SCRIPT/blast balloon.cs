using UnityEngine;

public class SimpleObjectToggler : MonoBehaviour
{
    [Tooltip("The first object that can be toggled")]
    public GameObject object1;
    [Tooltip("The second object that can be toggled")]
    public GameObject object2;
    [Tooltip("The camera used for raycasting")]
    public Camera mainCamera;
    [Tooltip("Enable debug logs for troubleshooting")]
    public bool debugMode = false;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (object1 == null || object2 == null)
        {
            Debug.LogError("SimpleObjectToggler: Please assign both object references in the inspector!");
            return;
        }

        object1.SetActive(true);
        object2.SetActive(false);

        if (debugMode)
        {
            Debug.Log("SimpleObjectToggler initialized. Object1 active, Object2 inactive.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleInput(touch.position);
            }
        }
    }

    void HandleInput(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (debugMode)
            {
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
            }

            // Check if the hit object is part of this prefab/gameObject instance
            SimpleObjectToggler hitToggler = hit.collider.GetComponentInParent<SimpleObjectToggler>();

            // Only toggle if we hit THIS specific instance of the prefab
            if (hitToggler == this)
            {
                object1.SetActive(false);
                object2.SetActive(true);

                if (debugMode)
                {
                    Debug.Log("Toggled objects on: " + gameObject.name);
                }
            }
        }
    }
}