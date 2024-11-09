using UnityEngine;
using UnityEngine.AI;

/**
 * Keeps NavMeshAgent always on NavMesh where AR camera is currently located.
 */
public class NavMeshAgentHelper : MonoBehaviour
{
    /** ARCamera GameObject **/
    GameObject ARcamera;

    /** NavMeshAgent component on same GameObject **/
    NavMeshAgent agent;

    /** mesh of agent **/
    MeshRenderer mesh;

    void Awake()
    {
        ARcamera = Camera.main.gameObject;
        agent = GetComponent<NavMeshAgent>();
        // TODO: mesh = GetComponent<MeshRenderer>();
        // TODO: mesh.enabled = false;
    }

    void Update()
    {
        //if (ARStateController.instance.IsLocalized())
        //{
        // teleport agent to camera x & z, but take y from agent object
        agent.Warp(new Vector3(ARcamera.transform.position.x, agent.gameObject.transform.position.y, ARcamera.transform.position.z));


        // if agent is too far away from camera reset the position of agent, local position of agent is checked, because agent is child of camera
        if (agent.gameObject.transform.localPosition.y > 0 && agent.gameObject.transform.localPosition.y > 1.5)
        {
        // 1.5 is when camera is down but agent on upper floor
        agent.Warp(new Vector3(ARcamera.transform.position.x, ARcamera.transform.position.y - 1.5f, ARcamera.transform.position.z));
        } else if (agent.gameObject.transform.localPosition.y < 0 && agent.gameObject.transform.localPosition.y < -3.5)
        {
            // -3.5 is when camera is up, but agent is on floor below
            agent.Warp(new Vector3(ARcamera.transform.position.x, ARcamera.transform.position.y + 3.5f, ARcamera.transform.position.z));
        }
    }

    /**
     * Toggles visibility of nav mesh agent
     */
    public void ToggleNavMeshAgentVisibility()
    {
        mesh.enabled = !mesh.enabled;
    }
}
