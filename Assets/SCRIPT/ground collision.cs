using UnityEngine;

public class DestroyOnGroundHit : MonoBehaviour
{
    public GameObject particleEffect;
    public GameObject particleEffect2;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("g"))
        {
            GameObject effectInstance2 = Instantiate(particleEffect2, transform.position, Quaternion.identity);
            GameObject effectInstance = Instantiate(particleEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
            Destroy(effectInstance2, effectInstance2.GetComponent<ParticleSystem>().main.duration);
            Destroy(effectInstance, effectInstance.GetComponent<ParticleSystem>().main.duration);
        }
    }
}
