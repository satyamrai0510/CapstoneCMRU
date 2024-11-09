using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POI : ListItemData
{
    int id;
    public int identification;                  // a unique identification for this point, e.g. room number - INFO: currently ignored in code!
    public string poiName;                      // title of Point of interest (POI)
    public string description;                  // description of POI
    public POIType type;                        // type of the POI
    public POICollider poiCollider;             // object for nav mesh agent calculation and detect user arrival
    public POISign sign;                        // sign of POI
    AugmentedSpace space;                       // space in which POI is located

    private void Awake()
    {
        base.listTitle = poiName;
        id = identification; //TODO: change this, set id from data source
        sign.SetPOI(this);
        poiCollider.SetPOI(this);
        space = gameObject.GetComponentInParent<AugmentedSpace>();
    }

    public int GetId()
    {
        return id;
    } 

    /**
    * Handles arrival of user at poi.
    */
    public void Arrived()
    {
        if (ARNavController.instance.currentDestination != null && ARNavController.instance.currentDestination.GetId() == id)
        {
            // arrived at the selected POI
            ARNavController.instance.ArrivedAtDestination();
        }
    }

}


public enum POIType { TutorialRoom, LectureHall, Staircase, Elevator }
