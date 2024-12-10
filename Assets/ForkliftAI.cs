using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftAI : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform waypointA;
    public Transform waypointB;

    [Header("Settings")]
    public float moveSpeed = 1f;
    public float rotationSpeed = 80f;  
    public float arrivalThreshold = 1f;
    public float waitTimeAtWaypoint = 3f;

    private Transform currentTarget;
    private bool isWaiting = false;
    private float originalX;
    private float originalZ;

    private enum State
    {
        RotateToTarget,
        MoveToTarget,
        WaitAtTarget
    }

    private State currentState;

    void Start()
    {
        originalX = transform.eulerAngles.x;
        originalZ = transform.eulerAngles.z;
        currentTarget = waypointA;
        currentState = State.RotateToTarget;

    }

    void Update()
    {
        if (isWaiting) return;

        switch (currentState)
        {
            case State.RotateToTarget:
                RotateTowardsTarget();
                break;
            case State.MoveToTarget:
                MoveTowardsTarget();
                break;
               
        }
    }

    /*Responsible for rotating the forklift so it can start moving towards its waypoint.
     * 
     */
    void RotateTowardsTarget()
    {
        Vector3 directionToTarget = (currentTarget.position - transform.position);
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        float currentY = transform.eulerAngles.y;
        float targetY = targetRotation.eulerAngles.y;

        float newY = Mathf.MoveTowardsAngle(currentY, targetY, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(originalX, newY, originalZ);

        if (Mathf.Abs(Mathf.DeltaAngle(newY, targetY)) < 1f)
        {
            currentState = State.MoveToTarget;
        }
    }

    /*Moves the forklift towards the next waypoint.
     * 
     */
    void MoveTowardsTarget()
    {
        // Calculate the direction to the current target
        Vector3 directionToTarget = (currentTarget.position - transform.position);
        directionToTarget.y = 0; // Ensure movement remains on the horizontal plane
        directionToTarget.Normalize();

        // Move toward the target dynamically
        transform.position += directionToTarget * moveSpeed * Time.deltaTime;

        // Check if close enough to the target
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= arrivalThreshold)
        {
            // Arrived at waypoint
            StartCoroutine(WaitAtWaypoint());
        }
    }



    /*Waits a little bit at each waypoint to simulate "working"
     */
    IEnumerator WaitAtWaypoint()
    {
        currentState = State.WaitAtTarget;
        isWaiting = true;

        // Wait for the specified time
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        // Switch target
        currentTarget = (currentTarget == waypointA) ? waypointB : waypointA;

        // After waiting, go back to rotation phase
        currentState = State.RotateToTarget;
        isWaiting = false;
    }
}
