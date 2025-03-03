using System.Collections;
using UnityEngine;

public class HelicopterTransport : MonoBehaviour
{
    public Transform[] pathPoints; // Waypoints for fixed path
    public bool useParachuteDrop = false; // Set in Inspector
    public GameObject player;
    public Transform dropPoint;
    public Transform landingPoint;
    public GameObject parachutePrefab;
    public float speed = 10f;
    public float landingDelay = 3f;
    public float rotationSpeed = 2f;

    private int currentPathIndex = 0;
    private bool missionCompleted = false;

    private Vector3 playerOffset = new Vector3(0, 1, 0);  // Keep player slightly above the helicopter

    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    void Start()
    {
        playerRigidbody = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<Collider>();

        if (pathPoints.Length > 0)
        {
            transform.position = pathPoints[0].position;
            StartCoroutine(FollowPath());
        }
    }

    IEnumerator FollowPath()
    {
        while (currentPathIndex < pathPoints.Length)
        {
            Transform targetPoint = pathPoints[currentPathIndex];
            while (Vector3.Distance(transform.position, targetPoint.position) > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);
                Quaternion targetRotation = Quaternion.LookRotation(targetPoint.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }
            currentPathIndex++;
        }

        if (!missionCompleted)
        {
            if (useParachuteDrop)
                StartCoroutine(ParachuteDrop());
            else
                StartCoroutine(LandAndExit());
        }
    }

    IEnumerator ParachuteDrop()
    {
        yield return new WaitForSeconds(1f);

        // Temporarily disable the player's physics and collider while they are attached
        playerRigidbody.isKinematic = true;
        playerCollider.enabled = false;

        // Attach the player to the helicopter
        player.transform.SetParent(transform);

        // Set player's position relative to the helicopter
        player.transform.localPosition = playerOffset;

        // Instantiate the parachute and move the player to it after drop
        GameObject parachute = Instantiate(parachutePrefab, dropPoint.position, Quaternion.identity);
        Rigidbody rb = parachute.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, -2f, 0);
        }

        // Parent player to the parachute
        player.transform.SetParent(parachute.transform);
        missionCompleted = true;

        // Re-enable physics and collider once the player is dropped
        playerRigidbody.isKinematic = false;
        playerCollider.enabled = true;
    }

    IEnumerator LandAndExit()
    {
        yield return new WaitForSeconds(landingDelay);

        // Temporarily disable the player's physics and collider while they are attached
        playerRigidbody.isKinematic = true;
        playerCollider.enabled = false;

        // Attach the player to the helicopter
        player.transform.SetParent(transform);

        // Move the helicopter to the landing position
        transform.position = landingPoint.position;

        yield return new WaitForSeconds(2f);

        // After landing, re-enable the player's physics and collider, and detach
        player.transform.SetParent(null);
        playerRigidbody.isKinematic = false;
        playerCollider.enabled = true;
        missionCompleted = true;
    }
}

