using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHelicopterController : MonoBehaviour
{
    [Header("Flight Settings")]
    public float liftForce = 5f;             // Force to keep the helicopter in the air
    public float moveSpeed = 10f;           // Speed of movement
    public float turnSpeed = 2f;            // Speed of turning
    public float hoverHeight = 10f;         // Target height for hovering
    public float hoverSmoothness = 2f;      // How smoothly it adjusts height
    public float altitudeSmoothing = 0.5f;  // How quickly the altitude correction happens
    public float waypointSpeed = 10f;       // Speed at which the helicopter moves toward waypoints
    public float rotationSpeed = 2f;        // Speed at which the helicopter turns toward waypoints
    public float waypointThreshold = 2f;    // Distance to a waypoint before moving to the next one

    [Header("Waypoints")]
    public Transform[] waypoints;           // List of waypoints for the helicopter to follow

    private int currentWaypointIndex = 0;   // Index of the current waypoint
    private Rigidbody rb;
    private float currentHeight;            // Current height of the helicopter

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Optional: Disable gravity if you don't want the helicopter to fall
        rb.useGravity = false;

        // Initialize current height
        currentHeight = transform.position.y;
    }

    void FixedUpdate()
    {
        // Maintain hover height with smooth adjustments
        MaintainHover();

        // Move towards the current waypoint
        MoveToWaypoint();
    }

    private void MaintainHover()
    {
        // Smoothly adjust the helicopter's altitude to hoverHeight
        currentHeight = Mathf.Lerp(currentHeight, hoverHeight, Time.deltaTime * hoverSmoothness);

        // Calculate the vertical force needed to maintain the hover height
        float heightDifference = currentHeight - transform.position.y;
        Vector3 lift = Vector3.up * heightDifference * liftForce * Time.deltaTime;
        rb.AddForce(lift, ForceMode.Acceleration);
    }

    private void MoveToWaypoint()
    {
        if (waypoints.Length == 0) return;

        // Get the current waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Calculate direction to the waypoint
        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;

        // Move forward towards the waypoint
        Vector3 forwardMovement = directionToWaypoint * waypointSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMovement);

        // Rotate smoothly towards the waypoint
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Check if the helicopter is close enough to the waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distanceToWaypoint < waypointThreshold)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    // Method to allow dynamic control over hover height (if needed)
    public void SetHoverHeight(float newHoverHeight)
    {
        hoverHeight = newHoverHeight;
    }

    // Optional: If you want to add a smoother transition for speed or other parameters, you can create methods for those
    public void AdjustSpeed(float newMoveSpeed)
    {
        moveSpeed = newMoveSpeed;
    }
}

