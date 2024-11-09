using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

/**
  * Visualizes path between two points on NavMesh in AR.
  *
  * Path is calculated every 0.1 second, Source: https://docs.unity3d/com/ScriptReference/AI.NavMesh.CalculatePath.html
  * 
  * Line is drawn with LineRenderer using path.corners, Source: https://gamedev.stackexchange.com/a/86255
  * 
  * Off screen target works only with this Unity asset: https://assetstore.unity.com/packages/tools/gui/off-screen-target-indicator-71799
  */

public class ARPathVisualizer : MonoBehaviour
{
    public static ARPathVisualizer instance;

    //public ARStateController stateController;
    Camera ARCamera;

    //Line
    LineRenderer line;

    //public GameObject pin; TODO: enable later if wanted

    // path of agent
    NavMeshPath path;

    // timer
    float _elapsed = 0.0f;

    // parameter to control line
    public float LINE_HEIGHT_ABOVE_GROUND = 0.1f; // in meters

    // start and destination transforms
    Transform a = null;
    Transform b = null;

    // the object in the 3d world that we want to show to with a 2d arrow
    public Target offscreenTarget;

    // used to visualize the corners of the path
    public GameObject cornerVisualizationPrefab;

    // holds all current corner GameObjects that are visualized
    GameObject[] visibileCorners = {};

    // true if corners should be shown
    public bool showCornersToggle = false;

    // true when showCornersToggle was used, needed to track change so we don't loop all the time 
    bool cornerVisibilityHasChanged = false;

    void Awake()
    {
        ARCamera = Camera.main;
        instance = this;
        line = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        path = new NavMeshPath();
        line.enabled = false;
    }

    void Update()
    {
        if (a != null && b != null && ARStateController.instance.IsLocalized())
        {
            line.enabled = true;
            line.SetPosition(0, a.position); // set first point of line
            StartCoroutine(DrawPath(path));
            PathEstimationUtils.instance.UpdateEstimation(path.corners);

            // Calculate fastest way only every 0.1 second, because it is heavy calculation
            _elapsed += Time.deltaTime;
            if (_elapsed > 0.1f)
            {
                _elapsed -= 0.1f;
                NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path);
            }
        }
        else
        {
            line.enabled = false;
            //offscreenTarget.gameObject.SetActive(false);
        }
    }

    /**
    * Draws shortest line from NavMeshAgent to destination
    */
    IEnumerator DrawPath(NavMeshPath path)
    {
        yield return new WaitForEndOfFrame(); // wait for path to be drawn

        if (path.corners.Length < 2) // if the path has 1 or no corners, there is no need
            yield break;

        line.positionCount = path.corners.Length; // set the array of positions to the amount of corners

        if (showCornersToggle)
        {
            cornerVisibilityHasChanged = true;
            if (cornerVisibilityHasChanged)
            {
                SetCornerVisibility(true);
            }
            HandlePathCornerVisualization();
        } else
        {
            cornerVisibilityHasChanged = true;
            if (cornerVisibilityHasChanged)
            {
                SetCornerVisibility(false);
            }
        }

        for (var i = 0; i < path.corners.Length; i++)
        {
            // go through each corner and set that to the line renderer's position, a little bit over ground
            Vector3 linePosition = new Vector3(path.corners[i].x, path.corners[i].y + LINE_HEIGHT_ABOVE_GROUND, path.corners[i].z);
            line.SetPosition(i, linePosition);

            if (showCornersToggle)
            {
                UpdateVisibleCorner(i, linePosition);
            }
        }

        // offscreen target indicator shows to next corner in path
        // TODO: this can be done better by calculating a point on the line 1.5 or 2 meters away from AR camera
        if (this.path.corners.Length >= 2 && Vector3.Distance(ARCamera.transform.position, this.path.corners[1]) > 1)
        {
            // only point to arrow if at least 1 meter away
            //offscreenTarget.transform.position = _path.corners[1];
            //offscreenTarget.gameObject.SetActive(true);
        }
        else
        {
            //offscreenTarget.gameObject.SetActive(false);
        }
    }

    /**
     * Reset path.
     */
    public void ResetPath()
    {
        StopAllCoroutines();
        a = null;
        b = null;
        line.positionCount = 1;
    }

    // SETTERS

    public void SetPositionFrom(Transform from)
    {
        a = from;
    }

    public void SetPositionTo(Transform to)
    {
        b = to;

        if (b != null)
        {
            // already calculate because it calculates only every 0.1 second
            NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path);

            if (path.status == NavMeshPathStatus.PathPartial)
            {
                // handle unreachable route
                //NotificationController.instance.ShowNewNotification("Problem calculating route. Please contact the publisher (see imprint).");
                b = null;
                ARNavController.instance.StopNavigation();
            }
        }
    }

    /**
     * Handles the visualization of the path corners.
     * For debugging purposes e.g.
     */
    void HandlePathCornerVisualization()
    {
        // handle visualized corners
        int pathCornersCount = path.corners.Length;
        if (pathCornersCount > visibileCorners.Length)
        {
            Debug.Log("There are MORE CORNERS");
            // new corners we haven't visualized yet
            if (visibileCorners.Length == 0)
            {
                // there are no corners yet
                visibileCorners = new GameObject[pathCornersCount];
            }
            else
            {
                // we need to create new array with current size and copy over old objects
                GameObject[] newVisibleCorners = new GameObject[pathCornersCount];
                for (int i = 0; i < visibileCorners.Length; i++)
                {
                    newVisibleCorners[i] = visibileCorners[i];
                }
                visibileCorners = newVisibleCorners;
            }
        }
        else if (pathCornersCount < visibileCorners.Length)
        {
            Debug.Log("There are LESS CORNERS");
            // there are less corners in the path, delete the once that are not used, source: https://www.c-sharpcorner.com/article/how-to-remove-an-element-from-an-array-in-c-sharp/
            int elementsToRemoveCount = visibileCorners.Length - pathCornersCount;
            GameObject[] newVisibleCorners = new GameObject[visibileCorners.Length - elementsToRemoveCount];

            for (int i = 0; i < visibileCorners.Length; i++)
            {
                if (i < newVisibleCorners.Length)
                {
                    // copy old visible corner to new one
                    newVisibleCorners[i] = visibileCorners[i];
                } else
                {
                   // remove deleted corner
                   Destroy(visibileCorners[i]);
                }
            }
            visibileCorners = newVisibleCorners;
        }
        else
        {
            // amount of corners stayed the same, do nothing
        }
    }

    /**
     * Updates the position of a corner
     */
    void UpdateVisibleCorner(int i, Vector3 newPosition)
    {
        if (visibileCorners[i] == null)
        {
            // there is no instantiated corner yet
            GameObject newCorner = GameObject.Instantiate(cornerVisualizationPrefab, newPosition, Quaternion.identity);
            visibileCorners[i] = newCorner;
        }
        else
        {
            // update the previously instatiated corner
            visibileCorners[i].gameObject.transform.position = newPosition;
        }
    }

    /**
     * Set the visibility of corners.
     */
    void SetCornerVisibility(bool show)
    {
        cornerVisibilityHasChanged = false;
        foreach (var corner in visibileCorners)
        {
            corner.gameObject.SetActive(show);
        }
    }
}
