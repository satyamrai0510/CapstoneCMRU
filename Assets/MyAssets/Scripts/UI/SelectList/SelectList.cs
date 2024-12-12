using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

/**
 * Shows & handles UI of a list with different items o select
 */
public class SelectList : MonoBehaviour
{
    public List<POI> emergencyExits;
    // to render stuff
    public RectTransform content;      // parent of spawn point
    public Transform SpawnPoint;       // spawn point of items
    public GameObject spawnItem;       // prefab of item to be spawned
    public int heightOfPrefab;         // height of spawnItem

    // additional UI
    public TMP_InputField searchField;
    public GameObject resetButtonSearchField;
    public GameObject placeholder;

    // title and back button
    public TextMeshProUGUI titleNoBackButton;
    //public GameObject backButton;
    public TextMeshProUGUI titleWithBackButton;

    public List<ListItemData> pois; // all items available for list

    List<ListItemData> currentItemsTotal;

    public void Awake()
    {
        PrepareAllData();
    }

    private void PrepareAllData()
    {
        pois = new List<ListItemData>();

        foreach (var poi in ARNavController.instance.agumentedSpace.GetPOIs())
        {
            pois.Add(poi);
        }
    }

    public void RenderPOIs()
    {
        // reset UI elements from previous renders
        titleNoBackButton.gameObject.SetActive(false);
        //backButton.SetActive(true);
        //titlewithbackbutton.gameobject.setactive(true);
        //titlewithbackbutton.text = location.title;
        // render list
        // RenderList(pois);
        // currentItemsTotal = pois;
        // Filter out emergency exits from the list
        List<ListItemData> nonEmergencyPOIs = pois.FindAll(poi => !(poi is POI) || !(poi as POI).isEmergencyExit);
        // Render only non-emergency POIs
        RenderList(nonEmergencyPOIs);
        currentItemsTotal = nonEmergencyPOIs; // Update the current visible list
    }

    /**
     * Renders given items as a list
     */
    public void RenderList(List<ListItemData> items)
    {
        Debug.Log("Render POIs!");
        // sort pois alphabetically
        items.Sort(CompareItemTitle);

        // remove previous items first
        foreach (Transform child in SpawnPoint.transform)
        {
            Destroy(child.gameObject);
        }

        int poisCount = items.Count;

        // loop over pois of this space
        for (int i = 0; i < poisCount; i++)
        {
            ListItemData item = items[i];

            // y where to spawn destinations
            float spawnY = (i * heightOfPrefab); // calculate new spawn point
            Vector3 pos = new Vector3(SpawnPoint.localPosition.x, -spawnY, SpawnPoint.localPosition.z);

            //instantiate Prefab at spawn point
            GameObject SpawnedItem = Instantiate(spawnItem, pos, SpawnPoint.rotation);

            //set parent
            SpawnedItem.transform.SetParent(SpawnPoint, false);

            // set poi item for reference
            ListItemUI itemUI = SpawnedItem.GetComponent<ListItemUI>();
            itemUI.SetListItemData(item);
        }

        //set content holder height
        content.sizeDelta = new Vector2(0, poisCount * heightOfPrefab);
    }

    /**
     * Resets list.
     */
    public void ResetPOISearch()
    {
        searchField.text = "";
        resetButtonSearchField.SetActive(false);
        RenderList(currentItemsTotal);
        placeholder.SetActive(true);
    }

    /**
     * Selects input search field.
     */
    public void SelectSearchInputField()
    {
        searchField.Select();
    }

    /**
     * Call when search string changed.
     */
    public void SearchPOIOnSearchChanged(string search)
    {
        if (search == "")
        {
            resetButtonSearchField.SetActive(false);
        }
        else
        {
            resetButtonSearchField.SetActive(true);
        }

        RenderList(FilterByTitle(search));
    }

    /**
     * Filters poi list by title.
     */
    private List<ListItemData> FilterByTitle(string searchTerm)
    {
        string search = searchTerm.ToLower();
        List<ListItemData> filteredItems = currentItemsTotal.FindAll(x =>
        {
            if (x.listTitle.ToLower().Contains(search))
            {
                return true;
            }
            else
            {
                return false;
            }
        });
        return filteredItems;
    }

    /**
     * Call to reset search.
     */
    public void ResetSearch()
    {
        searchField.text = "";
        if (placeholder != null)
        {
            placeholder.SetActive(true);
        }
    }

    /**
     * Sorting by item titl.
     */
    int CompareItemTitle(ListItemData a, ListItemData b)
    {
        // Here we sort two times at once, first one the first item, then on the second.
        // ... Compare the first items of each element.
        var part1 = a.listTitle;
        var part2 = b.listTitle;
        var compareResult = part1.CompareTo(part2);
        // If the first items are equal (have a CompareTo result of 0) then compare on the second item.
        if (compareResult == 0)
        {
            return b.listTitle.CompareTo(a.listTitle);
        }
        // Return the result of the first CompareTo.
        return compareResult;
    }
    public void AddEmergencyExit(POI exit)
    {
        if (!emergencyExits.Contains(exit))
        {
            emergencyExits.Add(exit);
        }
    }

    public POI GetNearestEmergencyExit(Vector3 currentPosition)
    {
        if (emergencyExits == null || emergencyExits.Count == 0)
    {
        Debug.LogWarning("No emergency exits available!");
        return null;
    }

    POI nearestExit = null;
    float minDistance = Mathf.Infinity;

    foreach (var exit in emergencyExits)
    {
        if (exit == null) continue;

        float distance = PathEstimationUtils.instance.EstimateDistanceToPosition(exit);

        if (distance >= 0 && distance < minDistance)
        {
            minDistance = distance;
            nearestExit = exit;
        }
    }

    if (nearestExit == null)
    {
        Debug.LogWarning("No reachable emergency exits found!");
    }
    else
    {
        Debug.Log($"Nearest emergency exit: {nearestExit.name}, Distance: {minDistance}");
    }

    return nearestExit;
    }
}
