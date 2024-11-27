using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/** Utils to estimate remaining path to selected POI **/
public class PathEstimationUtils : MonoBehaviour
{
    public static PathEstimationUtils instance;

    public NavMeshAgent agent;

    // distance
    float remainingDistance = 0;
    Vector3[] path;

    // time estimation
    float estimatedArrivalDuration = 0;
    float defaultVelocity = 0.45f; // we substract, since you will be slower
    // default 1.34f; // in m/s source: https://www.healthline.com/health/exercise-fitness/average-walking-speed
    public Camera ARCamera;

    public Slider progressSlider;

    float startingDistance = 0; // distance when starting navigation
    bool estimationStarted = false;

    private void Awake()
    {
        instance = this;
    }

    void FixedUpdate()
    {
        HandleProgress();

        // TODO: maybe add avarage speed later
        //if (_path != null && _path.Length > 0 && estimationStarted)
        //{
        //    // get current speed of NavMeshAgent
        //    currentVelocity = ARCamera.velocity.magnitude;
        //}
    }

    public void HandleProgress()
    {
        if (estimationStarted)
        {
            float currentDistance = startingDistance - remainingDistance;
            progressSlider.value = currentDistance / startingDistance + 0.03f;
        }
    }

    /**
     * Estimates time from first to last position of given Vector3 path, e.g. from NavMeshAgent.
     */
    public void UpdateEstimation(Vector3[] path)
    {
        this.path = path;

        if (path.Length > 1)
        {
            float remainingPathTotal = 0;
            // loop through path and add up distance between Vectors
            for (int i = 0; i < path.Length; i++)
            {
                if (i < path.Length - 1) // we always comparing with next one
                {
                    remainingPathTotal += Vector3.Distance(path[i], path[i + 1]);
                }
            }
            remainingDistance = remainingPathTotal;
            //estimatedArrivalDuration = estimatedArrivalDistance / averageVelocity;
            // hotfix because average velocity is 0 when user is standing still
            estimatedArrivalDuration = remainingDistance / defaultVelocity;

            if (!estimationStarted)
            {
                startingDistance = remainingDistance;
                estimationStarted = true;
            }
        }
    }

    public void ResetEstimation()
    {
        estimationStarted = false;
        remainingDistance = 0;
        estimatedArrivalDuration = 0;
    }

    public int getRemainingDistanceMeters()
    {
        return (int)remainingDistance;
    }

    public int getRemainingDurationSeconds()
    {
        return (int)estimatedArrivalDuration;
    }

    public float EstimateDistanceToPosition(POI destination)
    {
        if (destination == null || destination.poiCollider == null)
        {
            Debug.LogError("Destination or its collider is null!");
            return -1;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Agent is not on a valid NavMesh!");
            return -1;
        }

        NavMeshPath navMeshPath = new NavMeshPath();
        agent.isStopped = true;

        bool pathFound = NavMesh.CalculatePath(agent.transform.position, destination.poiCollider.transform.position, NavMesh.AllAreas, navMeshPath);

        if (!pathFound || navMeshPath.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogWarning("Path to destination is invalid!");
            return -1;
        }

        if (navMeshPath.status == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning("Partial path detected: Destination may be unreachable.");
            return -2;
        }

        if (navMeshPath.corners.Length > 1)
        {
            float totalDistance = 0f;
            for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
            {
                totalDistance += Vector3.Distance(navMeshPath.corners[i], navMeshPath.corners[i + 1]);
            }

            Debug.Log($"Calculated distance to {destination.poiName}: {totalDistance} meters");
            return totalDistance;
        }

        return -1;
    }

}