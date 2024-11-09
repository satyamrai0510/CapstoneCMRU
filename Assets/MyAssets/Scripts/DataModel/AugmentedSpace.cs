using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using Vuforia;

/**
 * Represents an area in real life that is covered by an Area Target (AT) and can be augmented.
 * 
 * This script must be placed on object with AreaTargetBehaviour script.
 */
public class AugmentedSpace : MonoBehaviour
{
    /** Title of the space **/
    public string title;

    /** Behaviour script of corresponding AT covering this space **/
    AreaTargetBehaviour areaTargetBehaviour;

    /** Which method was used to generate AT, not necessary, but could be halpful for example for comparison **/
    public AreaTargetType generationMethod;

    /** With which Vuforia version this AT was created **/
    public string CreatedWithVuforiaVersion;

    /** What date AT was created **/
    public string CreationDate;

    /** POIs inside this space **/
    POI[] pois = { };

    /** holding all object for augmentation in this space **/
    public GameObject augmentation;

    void Awake()
    {
        pois = augmentation.GetComponentsInChildren<POI>(true);
        areaTargetBehaviour = GetComponent<AreaTargetBehaviour>();
        augmentation.SetActive(false);
    }

    /**
     * Returns POIs of this space.
     */
    public POI[] GetPOIs()
    {
        return pois;
    }

    /**
     * Returns AT
     */
    public AreaTargetBehaviour GetAT()
    {
        return areaTargetBehaviour;
    }
}

/** Describes how an AT was created **/
public enum AreaTargetType { ProCamera, Handheld, Runtime };
