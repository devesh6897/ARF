using UnityEngine;

public class fireball : MonoBehaviour
{


    void Start()
    {

    }

    // Handle regular collisions
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Destroy(gameObject);
        }

    }

    // Handle trigger collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ground"))
        {
            Destroy(gameObject);


        }

    }


}