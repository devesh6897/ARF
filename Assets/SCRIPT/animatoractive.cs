using UnityEngine;

public class CollisionAnimatorActivator : MonoBehaviour
{
    [Tooltip("The Animator component that should be activated on collision")]
    public Animator animator;
    [Tooltip("The layer name to check for collision with")]
    public string targetLayerName = "g";
    [Tooltip("Enable for debug messages")]
    public bool debugMode = false;
    private int targetLayerNumber;
    private bool animatorActivated = false;
    public GameObject air;
    private Rigidbody rb;
    public float g;
    public float rotationSpeed = 15.0f;
    public bool rotateClockwise = false;

    // Random movement parameters
    [Header("Random Movement")]
    [Tooltip("Maximum distance the balloon can move randomly on X axis")]
    public float maxRandomX = 2.0f;
    [Tooltip("Maximum distance the balloon can move randomly on Z axis")]
    public float maxRandomZ = 2.0f;
    [Tooltip("Speed of random movement")]
    public float randomMovementSpeed = 1.0f;

    // Variables for random movement
    private Vector3 randomOffset;
    private float nextDirectionChangeTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // Find the animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("No Animator component found! Please attach an Animator to this GameObject or assign one in the inspector.");
            }
        }
        // Get the layer number from the layer name
        targetLayerNumber = LayerMask.NameToLayer(targetLayerName);
        if (targetLayerNumber == -1)
        {
            Debug.LogError("Layer '" + targetLayerName + "' does not exist! Please create this layer in the Unity editor.");
        }
        if (debugMode)
        {
            Debug.Log("CollisionAnimatorActivator initialized. Waiting for collision with layer: " + targetLayerName);
        }

        // Initialize random movement
        ChangeRandomDirection();
    }

    void Update()
    {
        // Determine the direction of rotation
        float direction = rotateClockwise ? -1.0f : 1.0f;
        // Calculate rotation amount this frame
        float rotationAmount = rotationSpeed * direction * Time.deltaTime;
        // Apply rotation around the Y axis
        transform.Rotate(0, rotationAmount, 0);

        // Check if it's time to change direction
        if (Time.time > nextDirectionChangeTime && !animatorActivated)
        {
            ChangeRandomDirection();
        }
    }

    void FixedUpdate()
    {
        if (!animatorActivated)
        {
            // Calculate downward movement (gravity)
            Vector3 gravityMovement = Vector3.down * g * Time.fixedDeltaTime;

            // Calculate random movement on X and Z
            Vector3 randomMovement = new Vector3(
                randomOffset.x * randomMovementSpeed * Time.fixedDeltaTime,
                0,
                randomOffset.z * randomMovementSpeed * Time.fixedDeltaTime
            );

            // Combine movements
            Vector3 combinedMovement = gravityMovement + randomMovement;

            // Apply movement
            rb.MovePosition(rb.position + combinedMovement);
        }
        else
        {
            // Only apply gravity when not activated
            rb.MovePosition(rb.position + Vector3.down * g * Time.fixedDeltaTime);
        }
    }

    void ChangeRandomDirection()
    {
        // Generate random values between -1 and 1 for X and Z
        randomOffset = new Vector3(
            Random.Range(-1f, 1f) * maxRandomX,
            0,
            Random.Range(-1f, 1f) * maxRandomZ
        );

        // Normalize to get a direction with maximum length of 1
        if (randomOffset.magnitude > 0)
        {
            randomOffset.Normalize();
        }

        // Set next time to change direction (between 1-3 seconds)
        nextDirectionChangeTime = Time.time + Random.Range(1f, 3f);
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CheckCollision(other.gameObject);
    }

    void CheckCollision(GameObject collidedObject)
    {
        // Check if we collided with an object on the target layer
        if (collidedObject.layer == targetLayerNumber)
        {
            air.SetActive(false);

            if (debugMode)
            {
                Debug.Log("Collision detected with object on layer '" + targetLayerName + "': " + collidedObject.name);
            }
            // Make sure we have an animator and it's not already activated
            if (animator != null && !animatorActivated)
            {
                // Enable the animator
                animator.enabled = true;
                g = 0f;
                rotationSpeed = 0f;
                // You can also trigger a specific animation if needed
                // animator.SetTrigger("YourTriggerName");
                animatorActivated = true;
                if (debugMode)
                {
                    Debug.Log("Animator activated!");
                }
                // Destroy object after 2 seconds
                Destroy(gameObject, 2f);
            }
        }
    }
}