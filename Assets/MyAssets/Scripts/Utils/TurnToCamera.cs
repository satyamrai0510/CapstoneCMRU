using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Turns game object to main camera while enabled.
 */
public class TurnToCamera : MonoBehaviour
{
    Camera ARCamera;                                // camera of scene

    private void Start()
    {
        ARCamera = Camera.main;
    }

    private void Update()
    {
        Transform target = ARCamera.transform;
        Vector3 targetPostition = new Vector3(target.position.x,
                                        this.transform.position.y,
                                        target.position.z);
        this.transform.LookAt(targetPostition);
    }
}
