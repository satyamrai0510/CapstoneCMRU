

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ListItemUI : MonoBehaviour
{
    public TextMeshProUGUI title;               // title visible in UI
    public GameObject startNavigationButton;    // button to start navigation
    public Button itemSelectButton;             // button to select item, e.g. location
    public ListItemData dataObject;             // object containing data
    public TextMeshProUGUI distance;

    /**
     * Set variables for this list item.
     * Should be called during rendering of item list.
     */
    public void SetListItemData(ListItemData data)
    {
        dataObject = data;
        title.text = data.listTitle;

        // only enable go button if data object is poi
        startNavigationButton.SetActive(data is POI);

        if (data is POI)
        {
            EstimateDistance();
        }
        else
        {
            distance.text = "";
        }
    }

    public void EstimateDistance()
    {
        distance.text = GetDistance();
    }


    public void Go()
    {
        if (dataObject is POI)
        {
            NavUIController.instance.ClickedStartNavigation((dataObject as POI));
        }
    }

    public string GetDistance()
    {
        if (ARStateController.instance.IsLocalized())
        {
            float distance = PathEstimationUtils.instance.EstimateDistanceToPosition(dataObject as POI);
            if (distance > 0)
            {
                return (int)distance + " m";
            }
            else if (distance == -2)
            {
                return "Unreachable";
            }
            else
            {
                return "";
            }
        }
        else
        {
            return "";
        }
    }
}
