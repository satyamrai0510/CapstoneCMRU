using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/**
 * Represents sign of poi.
 * Handles clicking on sign of poi.
 */
public class POISign : MonoBehaviour
{
    POI poi; // corresponding poi

    //public Image imageNormal;
    //public Image imageLibrary;

    public TextMeshProUGUI title;

    void Update()
    {
        //always check for touchcount first, before checking array
        if (gameObject.activeSelf && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            Ray raycast;
            if (Input.touchCount > 0)
            {
                raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else
            {
                raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            if (Physics.Raycast(raycast, out RaycastHit raycastHit))
            {
                if (raycastHit.collider.name == poi.gameObject.name)
                {
                    Debug.Log("Clicked POI: " + poi.poiName);

                    // TODO: show info panel about this POI
                }
            }
        }
    }

    /**
     * Set poi from parent
     */
    public void SetPOI(POI aPoi)
    {
        poi = aPoi;
        title.text = aPoi.poiName; // TODO: get translation
        //TODO: handle custom image for POI HandleImage();
    }
    
    ///**
    // * Sets specific color of poi image
    // */
    //void HandleImage()
    //{
    //    if (poi.type == POIType.BookShelf)
    //    {
    //        imageNormal.gameObject.SetActive(false);
    //        imageLibrary.gameObject.SetActive(true);
    //    }
    //}
}
