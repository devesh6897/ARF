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
    }
    void Update()
    {
        // Determine the direction of rotation
        float direction = rotateClockwise ? -1.0f : 1.0f;
        // Calculate rotation amount this frame
        float rotationAmount = rotationSpeed * direction * Time.deltaTime;
        // Apply rotation around the Z axis
        transform.Rotate(0, rotationAmount, 0);
    }
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector3.down * g * Time.fixedDeltaTime);
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
            if (debugMode)
            {
                Debug.Log("Collision detected with object on layer '" + targetLayerName + "': " + collidedObject.name);
            }
            // Make sure we have an animator and it's not already activated
            if (animator != null && !animatorActivated)
            {
                air.SetActive(false);
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