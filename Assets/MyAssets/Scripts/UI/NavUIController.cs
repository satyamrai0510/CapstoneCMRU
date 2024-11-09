using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

enum NavigationState
{
    searching, navigation, poi, explorer
}

public class NavUIController : MonoBehaviour
{
    // localization
    public GameObject pleaseScanOverlay;                // scanning environment overlay to inform user about idealy pointing device camera on environment
    public TextMeshProUGUI localizationStatus;          // status information about localization

    // navigation
    public TextMeshProUGUI remainingDistance;           // remaining distance
    public TextMeshProUGUI remainingDuration;           // remaining duration

    public static NavUIController instance;

    public GameObject stopButton;

    public GameObject destinationViewButton;

    public SelectList poiList;

    public GameObject DestinationSelectUI;

    public TextMeshProUGUI destinationName;

    public GameObject navigationProgressSlider;
    public GameObject backgroundWithProgress;
    public GameObject backgroundNoProgress;

    // settings
    public GameObject settingsUI;
    public GameObject settingsButton;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        settingsUI.SetActive(false);
        ShowNavigationUIElements(false);
        DestinationSelectUI.SetActive(false);

        destinationName.text = "";
    }

    private void Update()
    {
        HandleNavigationState();
        UpdateRemainingDistanceAndDuration();
    }

    void HandleNavigationState()
    {
        if (ARNavController.instance.IsCurrentlyNavigating())
        {
            destinationName.text = ARNavController.instance.currentDestination.poiName;
            return;
        }
        destinationName.text = "";
    }

    /**
     * Toggles visibility of destination select UI.
     */
    public void ToggleDestinationSelectUI()
    {
        DestinationSelectUI.SetActive(!DestinationSelectUI.activeSelf);

        if (!DestinationSelectUI.activeSelf)
        {
            Debug.Log("destination list is invisible");
            poiList.ResetPOISearch();
            return;
        }

        if (settingsUI.activeSelf)
        {
            settingsUI.SetActive(false);
        }

        poiList.RenderPOIs();
    }

    public void ClickedBackButton()
    {
        poiList.ResetPOISearch();
    }

    /**
     * User clicked on poi to start navigation.
     */
    public void ClickedStartNavigation(POI poi)
    {
        ARNavController.instance.SetPOIForNavigation(poi);
        ToggleDestinationSelectUI();

        ShowNavigationUIElements(true);
    }

    /**
     * User clicked on stop
     */
    public void ClickedStopButton()
    {
        ShowNavigationUIElements(false);
        ARNavController.instance.StopNavigation();
    }

    public void ShowNavigationUIElements(bool isVisible)
    {
        backgroundWithProgress.SetActive(isVisible);
        backgroundNoProgress.SetActive(!isVisible);
        settingsButton.SetActive(!isVisible);
        destinationViewButton.SetActive(!isVisible);

        // for navigation
        navigationProgressSlider.SetActive(isVisible);
        stopButton.SetActive(isVisible);
    }

    /**
     * User clicked on category.
     */
    public void ClickedCategory()
    {
        // TODO filter pois according to this category and reneder list
    }

    public void ShowSettings(bool isVisible)
    {
        settingsUI.SetActive(isVisible);

        if (isVisible && DestinationSelectUI.activeSelf)
        {
            DestinationSelectUI.SetActive(false);
        }
    }

    /**
     * Set localization status text.
     */
    public void SetLocalizationStatus(string newStatus)
    {
        localizationStatus.text = newStatus;
    }

    /**
     * Sets visibility of scan overlay.
     */
    public void ShowScanOverlay(bool isShown)
    {
        pleaseScanOverlay.SetActive(isShown);

        // hide distance and duration during scanning
        remainingDistance.GetComponent<CanvasGroup>().alpha = isShown ? 0 : 1;
        remainingDuration.GetComponent<CanvasGroup>().alpha = isShown ? 0 : 1;
    }

    /**
 * Update info about remaining distance and duration.
 */
    private void UpdateRemainingDistanceAndDuration()
    {
        if (!ARNavController.instance.IsCurrentlyNavigating())
        {
            remainingDistance.SetText("");
            remainingDuration.SetText("");
            return;
        }

        // distance
        int distance = PathEstimationUtils.instance.getRemainingDistanceMeters();
        string distanceText = distance + "";
        if (distance <= 1)
        {
            distanceText += " meter remaining";
        }
        else
        {
            distanceText += " meters remaining";
        }
        remainingDistance.text = distanceText;

        // duration
        int remainingSeconds = PathEstimationUtils.instance.getRemainingDurationSeconds();
        int remainingMin = remainingSeconds / 60;
        if (remainingMin <= 0)
        {
            remainingDuration.text = "<" + " 1 min";
        }
        else
        {
            remainingDuration.text = remainingMin + " min";
        }
    }

    /**
     * Should be called when arrived at a destination (when navigated).
     */
    public void ShowArrivedState()
    {
        NotificationController.instance.ShowNewNotification("You arrived at the destination!");
    }

    /**
     * Toggle visibility of NavMeshs in scene.
     */
    public void ToggleNavMeshVisibility()
    {
        ShowNavMesh.instance.ToggleVisibility();
    }
}
