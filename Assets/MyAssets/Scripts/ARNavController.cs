using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Linq;


/**
 * Handles the agent and other controllers to navigate a user in AR to a selected destination.
 */
public class ARNavController : MonoBehaviour
{
    private Coroutine emergencyTrackingCoroutine;
    private Vector3 previousPosition;
    public bool isEmergencyMode = false;
    public SelectList selectList;
    public static ARNavController instance;

    /** AR camera of scene **/
    Camera ARCamera;

    /** navmesh agent **/
    public NavMeshAgent agent;

    /** holds current destination of navigation **/
    public POI currentDestination;

    /** collider of the ARCamera to detect POI arrival **/
    SphereCollider ARCameraCollider;

    /**
     * Augmented space where we can navigated
     */
    public AugmentedSpace agumentedSpace;

    private void Awake()
    {
        instance = this;
        ARCamera = Camera.main;

        // Initialize previous position
        previousPosition = agent.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        ARCameraCollider = ARCamera.GetComponent<SphereCollider>();

        ARStateController.instance.PositionFoundEvent.AddListener(() =>
        {
            NavUIController.instance.ShowScanOverlay(false);

        });

        ARStateController.instance.PositionLostEvent.AddListener(() =>
        {
            NavUIController.instance.ShowScanOverlay(true);
        });

        // invoke to get started
        ARStateController.instance.PositionLostEvent.Invoke();
        foreach (POI exit in selectList.emergencyExits)
        {
            if (exit != null)
            {
                exit.SetVisibility(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ARStateController.instance.IsLocalized())
        {
            agent.gameObject.SetActive(true);
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                // TODO: add debug function to simulate the agent walking to destination
            }
        }
        else
        {
            // agent is deactivated when not localized because no NavMesh is available
            agent.gameObject.SetActive(false);
        }

        if (IsCurrentlyNavigating() && ARStateController.instance.IsLocalized() && agent.isOnNavMesh)
        {
            agent.destination = currentDestination.poiCollider.transform.position;

            // when we are navigating and we are localized path needs to go from curent agent position
            ARPathVisualizer.instance.SetPositionFrom(agent.transform);
            ARPathVisualizer.instance.SetPositionTo(currentDestination.poiCollider.transform);

            // enable collider to detect arrival
            ARCameraCollider.enabled = true;
        }
        else if (!IsCurrentlyNavigating() && ARStateController.instance.IsLocalized())
        {
            // localized but not navigating, collider should also be active to detect continuation or end of AT
            ARCameraCollider.enabled = true;
        } else if (IsCurrentlyNavigating() && ARStateController.instance.IsLocalized() && !agent.isOnNavMesh)
        {
            // TODO: handle not being on navmesh
        }
        else
        {
            // not localized, not navigating
            ARCameraCollider.enabled = false;
        }
    }

    /**
     * Sets a POI for navigation and gets ready for navigation.
     */
    public void SetPOIForNavigation(POI aPOI)
    {
        currentDestination = aPOI;
        if (ARStateController.instance.IsLocalized())
        {
            if (isEmergencyMode)
        {
            // For Emergency Mode, update navigation dynamically
            UpdateNavigationPath(aPOI);
        }
        else
        {
            // Normal navigation setup
            StartNavigation();
        }
        }
        else
        {
            // TODO: show error, not localized
        }
    }

    public void StartNavigation()
    {
        // set positions for path visualizer
        ARPathVisualizer.instance.SetPositionFrom(agent.transform);
        ARPathVisualizer.instance.SetPositionTo(currentDestination.poiCollider.transform);
        //NavigationUIController.instance.ShowNavigationUI(true);
    }

    /**
     * Stops navigation.
     */
    public void StopNavigation()
    {
        if (isEmergencyMode)
    {
        isEmergencyMode = false;
        if (emergencyTrackingCoroutine != null)
        {
            StopCoroutine(emergencyTrackingCoroutine);
            emergencyTrackingCoroutine = null;
        }
    }
        if (currentDestination != null)
        {
            currentDestination = null;
            ARPathVisualizer.instance.ResetPath();
            PathEstimationUtils.instance.ResetEstimation();
        }
        foreach (POI poi in selectList.pois)
    {
        if (poi != null)
        {
            poi.SetVisibility(!poi.isEmergencyExit); // Show non-emergency POIs, hide emergency exits
        }
    }
        
    }

    /**
     * Hanles destination arrival
     */
    public void ArrivedAtDestination()
    {
        StopNavigation();
        NavUIController.instance.ShowArrivedState();
        NavUIController.instance.stopEmergencybutton.SetActive(false);
        NavUIController.instance.emergencyModeIndicator.SetActive(false);
        NavUIController.instance.ClickedStopButton();

    }

    /**
     * Returns true when user is currently navigating.
     */
    public bool IsCurrentlyNavigating()
    {
        return currentDestination != null;
    }


    public void ActivateEmergencyMode()
    {
        isEmergencyMode = true;
        foreach (POI poi in selectList.pois)
    {
        if (poi != null)
        {
            if (poi.isEmergencyExit)
            {
                poi.SetVisibility(false); // Initially hide all emergency exits
            }
            else
            {
                poi.SetVisibility(false); // Hide all other POIs
            }
        }
    }
        // POI nearestExit = selectList.GetNearestEmergencyExit(transform.position);
        // if (nearestExit != null)
        // {
        //     nearestExit.SetVisibility(true); // Show only the nearest exit
        //     SetPOIForNavigation(nearestExit);
        // }
        // else
        // {
        //     Debug.LogWarning("No emergency exits found!");
        // }
        if (emergencyTrackingCoroutine == null)
    {
        emergencyTrackingCoroutine = StartCoroutine(TrackNearestExit());
    }
    }

    public void DeactivateEmergencyMode()
    {
        isEmergencyMode = false;
        StopNavigation();

        if (emergencyTrackingCoroutine != null)
        {
        StopCoroutine(emergencyTrackingCoroutine);
        emergencyTrackingCoroutine = null;
        NavUIController.instance.ClickedStopButton();
        }

        // Hide all emergency exits
        foreach (POI poi in selectList.pois)
        {
        if (poi != null)
        {
            poi.SetVisibility(!poi.isEmergencyExit); // Show non-emergency POIs, hide emergency exits
        }
        }
        foreach (POI exit in selectList.emergencyExits)
        {
        if (exit != null)
            {
            exit.SetVisibility(false);
            }
        }
    }





    private IEnumerator TrackNearestExit()
{
    POI currentNearestExit = null;

    while (isEmergencyMode)
    {
        // Get the user's current speed
        float userSpeed = instance.CalculateSpeed();

        // Adjust the wait time based on the user's speed
        float adjustedWaitTime = Mathf.Lerp(3f, 0.5f, Mathf.Clamp(userSpeed / 5f, 0f, 1f));

        // Find the nearest emergency exit
        POI nearestExit = selectList.GetNearestEmergencyExit(transform.position);

        if (nearestExit != null && nearestExit != currentNearestExit)
        {
            currentNearestExit = nearestExit;

            // Update POI visibility: hide all, show only the nearest exit
            foreach (POI poi in selectList.pois)
            {
                if (poi != null)
                {
                    poi.SetVisibility(poi == currentNearestExit); // Show only the nearest POI
                }
            }

            // Dynamically update the path to the nearest exit
            SetPOIForNavigation(currentNearestExit);
            Debug.Log($"Updated path to new nearest exit: {currentNearestExit.name}");
        }

        // Check if the user has arrived at the destination
        if (currentNearestExit != null && HasArrivedAtDestination(currentNearestExit))
        {
            Debug.Log($"Arrived at destination: {currentNearestExit.name}");
            StopNavigation();
            yield break; // Exit the coroutine
        }

        // Adjust frequency as needed
        // yield return new WaitForSeconds(1);

        // Wait based on the user's speed (adaptive frequency)
        yield return new WaitForSeconds(adjustedWaitTime);
    }

    emergencyTrackingCoroutine = null;
}






private void UpdateNavigationPath(POI destination)
{
    if (!isEmergencyMode) return; // Only update dynamically in Emergency Mode

    if (destination == null)
    {
        Debug.LogError("Destination is null! Cannot update path.");
        return;
    }

    // Recalculate the path for the updated destination
    Vector3[] pathPoints = CalculatePathPoints(destination);
    if (pathPoints != null)
    {
        ARPathVisualizer.instance.DisplayEmergencyPath(pathPoints.ToList()); // Update path visuals
        Debug.Log($"Path updated dynamically for {destination.poiName}");
    }
    else
    {
        Debug.LogWarning("Failed to update path for the new destination.");
    }
}




private Vector3[] CalculatePathPoints(POI destination)
{
    NavMeshPath path = new NavMeshPath();
    if (NavMesh.CalculatePath(agent.transform.position, destination.poiCollider.transform.position, NavMesh.AllAreas, path))
    {
        return path.corners; // Return calculated path points
    }
    return null; // Path could not be calculated
}




private bool HasArrivedAtDestination(POI destination)
{
    if (destination == null) return false;

    float distanceToDestination = Vector3.Distance(transform.position, destination.transform.position);
    return distanceToDestination <= 1f; // Adjust threshold as needed (e.g., 1.5 meters)
}




public float CalculateSpeed()
    {
        float distanceMoved = Vector3.Distance(agent.transform.position, previousPosition);
        float speed = distanceMoved / Time.deltaTime;  // speed = distance / time (per frame)

        previousPosition = agent.transform.position;  // Update previous position
        Debug.Log($"speed of agent {speed}");
        return speed;
    }
}
