using UnityEngine;
using Vuforia;

public class CameraFocusController : MonoBehaviour
{
    void Start()
    {
        SetFocusMode();
    }

    private void SetFocusMode()
    {
        if (VuforiaBehaviour.Instance != null && VuforiaBehaviour.Instance.CameraDevice != null)
        {
            bool focusSet = VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

            if (!focusSet)
            {
                Debug.LogWarning("Continuous autofocus is not supported on this device. Default focus mode is set.");
            }
            else
            {
                Debug.Log("Continuous autofocus enabled.");
            }
        }
        else
        {
            Debug.LogError("Vuforia or Camera Device is not initialized.");
        }
    }
}
