using UnityEngine;



public class FireballProjectile : MonoBehaviour

{

    [Header("Projectile Settings")]

    [SerializeField] private GameObject fireballPrefab;

    [SerializeField] private Transform firePoint;

    [SerializeField] private float projectileSpeed = 15f;

    [SerializeField] private float arcHeight = 1f;

    [SerializeField] private bool debugVisuals = true;  // Toggle for debug visualization



    [Header("Effects")]

    [SerializeField] private ParticleSystem muzzleEffect;

    [SerializeField] private AudioClip fireSound;



    private Camera mainCamera;

    private AudioSource audioSource;



    private void Awake()

    {

        mainCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)

        {

            audioSource = gameObject.AddComponent<AudioSource>();

        }

    }



    private void Update()

    {

        // Check for mouse click

        if (Input.GetMouseButtonDown(0))

        {

            FireProjectile();

        }

    }



    private void FireProjectile()

    {

        // Create a ray from the camera to the mouse position

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;



        // Use default target if nothing is hit

        Vector3 targetPosition = ray.GetPoint(100f);



        // If something is hit, use that position

        if (Physics.Raycast(ray, out hit))

        {

            targetPosition = hit.point;

        }



        // Visualize target in scene view if debug is enabled

        if (debugVisuals)

        {

            Debug.DrawLine(mainCamera.transform.position, targetPosition, Color.red, 1f);

            Debug.DrawRay(targetPosition, Vector3.up, Color.green, 1f);

        }



        // Create the fireball

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);



        // Get projectile component

        Projectile projectileComponent = fireball.GetComponent<Projectile>();

        if (projectileComponent == null)

        {

            projectileComponent = fireball.AddComponent<Projectile>();

        }



        // Set debug flag

        projectileComponent.SetDebugVisuals(debugVisuals);



        // Calculate and set trajectory

        projectileComponent.Launch(firePoint.position, targetPosition, projectileSpeed, arcHeight);



        // Play effects

        if (muzzleEffect != null)

        {

            muzzleEffect.Play();

        }



        if (fireSound != null)

        {

            audioSource.PlayOneShot(fireSound);

        }

    }

}



// Projectile class to handle movement

public class Projectile : MonoBehaviour

{

    [SerializeField] private ParticleSystem trailEffect;

    [SerializeField] private ParticleSystem impactEffect;



    private Vector3 startPosition;

    private Vector3 targetPosition;

    private float timeOfFlight;

    private float launchTime;

    private float gravity;

    private bool isLaunched = false;

    private bool debugVisuals = false;

    private LineRenderer trajectoryLine;



    private void Awake()

    {

        // Add a line renderer component for visualization if it doesn't exist

        trajectoryLine = GetComponent<LineRenderer>();

        if (trajectoryLine == null && debugVisuals)

        {

            trajectoryLine = gameObject.AddComponent<LineRenderer>();

            trajectoryLine.startWidth = 0.1f;

            trajectoryLine.endWidth = 0.1f;

            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));

            trajectoryLine.startColor = Color.yellow;

            trajectoryLine.endColor = Color.yellow;

        }

    }



    public void SetDebugVisuals(bool enabled)

    {

        debugVisuals = enabled;

        if (trajectoryLine != null)

        {

            trajectoryLine.enabled = enabled;

        }

        else if (enabled)

        {

            trajectoryLine = gameObject.AddComponent<LineRenderer>();

            trajectoryLine.startWidth = 0.1f;

            trajectoryLine.endWidth = 0.1f;

            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));

            trajectoryLine.startColor = Color.yellow;

            trajectoryLine.endColor = Color.yellow;

        }

    }



    public void Launch(Vector3 start, Vector3 target, float speed, float arcHeight)

    {

        startPosition = start;

        targetPosition = target;

        isLaunched = true;

        launchTime = Time.time;



        // Calculate time of flight based on distance and speed

        float distance = Vector3.Distance(start, target);

        timeOfFlight = distance / speed;



        // Force a positive arc height, regardless of target elevation

        // This ensures the arc always goes upward

        gravity = 4 * arcHeight / (timeOfFlight * timeOfFlight);



        // Start trail effect if available

        if (trailEffect != null)

        {

            trailEffect.Play();

        }



        // Visualize the trajectory if debug is enabled

        if (debugVisuals && trajectoryLine != null)

        {

            DrawTrajectory();

        }

    }



    private void DrawTrajectory()

    {

        // Create points along the trajectory for visualization

        int numPoints = 50;

        trajectoryLine.positionCount = numPoints;



        for (int i = 0; i < numPoints; i++)

        {

            float normalizedTime = (float)i / (numPoints - 1);

            Vector3 point = CalculatePositionAtTime(normalizedTime);

            trajectoryLine.SetPosition(i, point);

        }

    }



    private Vector3 CalculatePositionAtTime(float normalizedTime)

    {

        Vector3 position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);



        // Modified arc calculation that's always positive (upward)

        // Using a sin curve for the first half, and then decreasing for the second half

        float arcFactor = Mathf.Sin(normalizedTime * Mathf.PI);

        float heightOffset = gravity * timeOfFlight * timeOfFlight * arcFactor;



        position.y += heightOffset;

        return position;

    }



    private void Update()

    {

        if (!isLaunched) return;



        // Calculate how far along the flight path we are

        float timeElapsed = Time.time - launchTime;

        float normalizedTime = timeElapsed / timeOfFlight;



        // If we've reached the target, trigger impact but don't destroy

        if (normalizedTime >= 1.0f)

        {

            OnImpact();

            isLaunched = false;  // Stop updating position

            return;

        }



        // Calculate the current position along the linear path

        Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, normalizedTime);



        // Add arc using a sine curve that's always positive

        float arcFactor = Mathf.Sin(normalizedTime * Mathf.PI);

        float heightOffset = gravity * timeOfFlight * timeOfFlight * arcFactor;



        currentPosition.y += heightOffset;



        // Update position and rotation

        transform.position = currentPosition;



        // Orient towards travel direction

        if (timeElapsed > 0)

        {

            Vector3 lastPosition = transform.position - transform.forward;

            Vector3 direction = (transform.position - lastPosition).normalized;

            transform.rotation = Quaternion.LookRotation(direction);

        }

    }



    private void OnImpact()

    {

        // Play impact effect if available

        if (impactEffect != null)

        {

            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);

            // Note: We're still destroying the impact effect after it completes

            Destroy(impact.gameObject, impact.main.duration);

        }



        // Apply damage or other effects here

        // For example, find nearby enemies and apply damage



        // The projectile itself is no longer destroyed

        if (trailEffect != null)

        {

            trailEffect.Stop();  // Stop the trail effect

        }



        // Disable the line renderer if it exists

        if (trajectoryLine != null)

        {

            trajectoryLine.enabled = false;

        }

    }



    private void OnCollisionEnter(Collision collision)

    {

        // Trigger impact but don't destroy

        isLaunched = false;

        OnImpact();

    }

}