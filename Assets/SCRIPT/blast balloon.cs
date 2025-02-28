using UnityEngine;

public class SimpleFireTagToggler : MonoBehaviour
{
    [Tooltip("The first object that will be disabled when hit by fire")]
    public GameObject object1;

    [Tooltip("The second object that will be activated when hit by fire")]
    public GameObject object2;

    [Tooltip("Enable debug logs for troubleshooting")]
    public bool debugMode = false;

    void Start()
    {
        // Check for missing references
        if (object1 == null || object2 == null)
        {
            Debug.LogError("SimpleFireTagToggler: Please assign both object references in the inspector!");
            return;
        }

        // Set initial state
        object1.SetActive(true);
        object2.SetActive(false);

        if (debugMode)
        {
            Debug.Log("SimpleFireTagToggler initialized. Object1 active, Object2 inactive.");
        }
    }

    // Handle regular collisions
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("ball"))
        {
            ToggleObjects();
            Destroy(other.gameObject);
            if (debugMode)
            {
                Debug.Log("Collision with 'fire' tag, toggling objects.");
            }
        }

    }

    // Handle trigger collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ball"))
        {
            Destroy(other.gameObject);

            ToggleObjects();


        }

    }

    // Toggle objects method
    void ToggleObjects()
    {
        object1.SetActive(false);
        object2.SetActive(true);
    }
}