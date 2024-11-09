using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles arrival of user (ARCamera) in collider of poi.
 * 
 * Note: Is only triggered when collider of camera is enabled (during navigation).
 */
public class POICollider : MonoBehaviour
{
    POI poi;

    /**
     * Detect if user (respectively ARCamera) hits poi collider.
     */
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ARCamera")
        {
            Debug.Log("User visited " + poi.poiName);
            poi.Arrived();
        }
    }

    /**
     * Detect if user (respectively ARCamera) left poi collider.
     */
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "ARCamera")
        {
            Debug.Log("User left " + poi.poiName);
        }
    }

    /**
     * Set poi from POI script.
     */
    public void SetPOI(POI aPoi)
    {
        poi = aPoi;
    }
}
