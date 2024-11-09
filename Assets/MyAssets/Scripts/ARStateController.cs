using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using TMPro;
using UnityEngine.Events;

/**
 * Handles different states of Positional Device Tracker according to Vuforia documentation:
 * https://library.vuforia.com/content/vuforia-library/en/articles/Solution/tracking-state.html
 * 
 * Shows current state in UI.
 * 
 * IMPORTANT: The DeviceTrackerARController currently doesn't change state when running in Unity simulator.
 */
public class ARStateController : MonoBehaviour
{
    public static ARStateController instance;

    // AR state debug information
    //public GameObject debugParent;
    //public TextMeshProUGUI stateText;
    //public TextMeshProUGUI stateInfoText;
    //public TextMeshProUGUI debugMessageText;
    //public TextMeshProUGUI trackedATName;
    //public TextMeshProUGUI appVersionText;

    // developer settings
    //public GameObject developerSettings;

    bool isLocalized = false; // true when device is localized enough to show AR
    readonly float LOCALIZATION_TIME_OUT = 10f; // how many seconds should be waited max for (re)localization
    bool isLocalizationCountdownStarted = false;

    // event to signal that device was localized
    public UnityEvent PositionFoundEvent = new UnityEvent();

    // event to signal that Area Target was lost
    public UnityEvent PositionLostEvent = new UnityEvent();

    // holds name of Area Target is currently being tracked
    string currentlyTrackedATname = "";

    // holds currently tracked Area Target Behaviour
    AreaTargetBehaviour currentlyTrackedAT = null;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        /*debugParent.SetActive(false);*/
        // navigation state information
        //targetFoundMessage.text = ""; I have no clue why but when this is on the GameObject is not shown...
        //stateText.text = "";
        //stateInfoText.text = "";
        //debugMessageText.text = "";

        //appVersionText.text = Application.version;

        // register observer method
        VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged += OnStatusChanged;
    }

    /**
     * Handles changes in state of DevicePoseBehaviour.
     */
    void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        Status status = behaviour.TargetStatus.Status;
        StatusInfo statusInfo = behaviour.TargetStatus.StatusInfo;

        Debug.LogFormat("Status is: {0}, statusInfo is: {1}", "" + status, "" + statusInfo);

        SetARStateText("" + status);
        SetARStateInfoText("" + statusInfo);


        // Source of states: https://developer.vuforia.com/library/getting-started/pose-status-and-status-info-unity
        if (status == Status.TRACKED)
        {
            // A reliable device pose is provided, and experiences are anchored with respect to the environment.
            SetARStateInformation("Device localization is stable.", false);
        }
        else if (status == Status.LIMITED)
        {
            // The device pose is of degraded quality. The application may advise the user to help recover a better device tracking.

            if (statusInfo == StatusInfo.UNKNOWN)
            {
                // Vuforia is not capable of providing information on the cause of the limited pose.
                SetARStateInformation("Please move device smoothly, go to areas with more light and scan environment with more features.", true);

            }
            else if (statusInfo == StatusInfo.INITIALIZING)
            {
                // The device tracker is initializing.
                SetARStateInformation("The device is initializing. Please scan your surroundings.", true);
            }
            else if (statusInfo == StatusInfo.EXCESSIVE_MOTION)
            {
                // The device is being moved too fast.
                SetARStateInformation("Too much motion. Please move device more smoothly.", true);
            }
            else if (statusInfo == StatusInfo.INSUFFICIENT_FEATURES)
            {
                // The device is pointed at an area with very few visual features. ARKit only
                SetARStateInformation("Insufficent features. Please scan area with more features.", true);
            }
            else if (statusInfo == StatusInfo.INSUFFICIENT_LIGHT)
            {
                // Motion tracking is lost due to poor lighting conditions. ARCore only
                SetARStateInformation("Low light detected. Please go to area with more light.", true);
            }
        }
        else if (status == Status.NO_POSE)
        {
            // No device pose available.

            if (statusInfo == StatusInfo.UNKNOWN)
            {
                // Vuforia cannot determine a device pose or provide information on the reason.

                // TODO reset PositionalDeviceTracker
                SetARStateInformation("Please move device smoothly, go to areas with more light and scan environment with more features.", true);
                if (!isLocalizationCountdownStarted)
                {
                    isLocalizationCountdownStarted = true;
                    StartCoroutine(LocalizationAttemptsCountdown());
                }
            }
            else if (statusInfo == StatusInfo.INITIALIZING)
            {
                // The device tracker is initializing.
                SetARStateInformation("The device is initializing. Please scan your surroundings.", true);
            }
            else if (statusInfo == StatusInfo.RELOCALIZING)
            {
                // The device is trying to re-attach to the world and restore Anchor locations. ARCore only

                // TODO reset PositionalDeviceTracker
                SetARStateInformation("Having difficulty to localize device. Please go back to a previous area that worked.", true);

                if (!isLocalizationCountdownStarted)
                {
                    isLocalizationCountdownStarted = true;
                    StartCoroutine(LocalizationAttemptsCountdown());
                }
            }
        }
        else if (status == Status.EXTENDED_TRACKED)
        {
            // Target is not in sight anymore

            SetARStateInformation("Running on extended tracking. Keep going.", false);
        }
        else
        {
            // something else

            SetARStateInformation("Unkown state", false);
        }
    }

    /**
     * Called when a given Area Target was found.
     * This callback is independent of OnStatusChanged because it is called from 
     * Default Trackable Event Handler.
     */
    public void OnTargetFound(AreaTargetBehaviour areaTarget)
    {
        string nameOfAT = GetATName(areaTarget);
        currentlyTrackedATname = nameOfAT;
        currentlyTrackedAT = areaTarget;
        SetTrackedATName(nameOfAT);

        PositionFoundEvent.Invoke();
        ShowAugmentations(true);

        if (!isLocalized)
        {
            // first time we find location
            isLocalized = true;

            // show message for user if not in Editor (because there Area Targets are simulated as Found)
#if !UNITY_EDITOR

            NotificationController.instance.ShowNewNotification("Position found");
#endif
        }
        NavUIController.instance.SetLocalizationStatus("Position is being tracked");
    }

    /**
     * Called when a given Area Target was lost.
     * This callback is independent of OnStatusChanged because it is called from 
     * Default Trackable Event Handler.
     */
    public void OnTargetLost(AreaTargetBehaviour areaTarget)
    {
        string nameOfAT = GetATName(areaTarget);
        if (nameOfAT == currentlyTrackedATname)
        {
            // we lost position in currently localized area target
            ShowAugmentations(false);
            currentlyTrackedATname = "";
            currentlyTrackedAT = null;
            SetTrackedATName("-");
            PositionLostEvent.Invoke();
            isLocalized = false;
            NavUIController.instance.SetLocalizationStatus("Please scan surroundings");
        }
        else if (currentlyTrackedATname == "" || currentlyTrackedAT == null)
        {
            // we didn't have a pose yet
            isLocalized = false;
            SetTrackedATName("-");
            PositionLostEvent.Invoke();
            NavUIController.instance.SetLocalizationStatus("Please scan surroundings");
        }
        else
        {
            // lost position of another area target we are tracking
            Debug.Log("Another AT lost position.");
        }
    }

    /**
     * Sets the currently tracked AT name in the debug info UI.
     */
    void SetTrackedATName(string name)
    {
        //trackedATName.text = name;
    }

    /**
     * Returns true if AR experience is ready to be shown.
     */
    public bool IsLocalized()
    {
        return isLocalized;
    }

    void ShowLocalizationFailedMessage()
    {
        SetARStateInformation("Device could not be localized. Please restart AR experience.", true);
    }

    /**
     * Shows a message to user after waiting some time for (re)localizing device.
     */
    IEnumerator LocalizationAttemptsCountdown()
    {
        float counter = LOCALIZATION_TIME_OUT;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }
        ShowLocalizationFailedMessage();
        isLocalizationCountdownStarted = false;
    }

    /* GETTERS or SETTERS */

    void SetARStateText(string newState)
    {
        //stateText.text = "State: " + newState;
    }

    void SetARStateInfoText(string newStateInfo)
    {
        //stateInfoText.text = "State Info: " + newStateInfo;
    }

    /**
     * Sets new AR state information.
     * 
     */
    void SetARStateInformation(string info, bool isVisibleForUser)
    {
        //debugMessageText.text = info;
        if (isVisibleForUser)
        {
            //NotificationController.instance.ShowNewNotification(info);
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    /**
     * Toggles visibility of developer UI.
     */
    public void ToggleDeveloperUI()
    {
        //this.debugParent.SetActive(!this.debugParent.activeSelf);
        //this.developerSettings.SetActive(this.debugParent.activeSelf);
    }

    /**
     * Return name for given AT.
     */
    string GetATName(AreaTargetBehaviour at)
    {
        AugmentedSpace space = at.gameObject.GetComponent<AugmentedSpace>();
        return space.title;
    }

    /**
     * Returns AT that is currently tracked.
     */
    public AreaTargetBehaviour GetTrackedAT()
    {
        return currentlyTrackedAT;
    }

    /**
     * Hides augmenteation of all spaces.
     */
    void ShowAugmentations(bool isShown)
    {
        ARNavController.instance.agumentedSpace.augmentation.SetActive(isShown);
    }
}
