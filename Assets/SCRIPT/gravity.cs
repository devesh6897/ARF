using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleGravity : MonoBehaviour
{
    private Rigidbody rb;
    public float g;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector3.down * g * Time.fixedDeltaTime);
    }
}