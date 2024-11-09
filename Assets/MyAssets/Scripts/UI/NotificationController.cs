using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/**
 * Shows notification messages.
 */
public class NotificationController : MonoBehaviour
{
    public static NotificationController instance;

    public GameObject Notification;
    public TextMeshProUGUI NotificationText;
    public GameObject QuitNotification;
    bool isQuitNotification;
    readonly int NOTIFICATION_TIMER = 4;

    void Awake()
    {
        instance = this;
        Notification.SetActive(false);
        QuitNotification.SetActive(false);
    }

    public void ShowNewNotification(string text)
    {
        NotificationText.text = text;
        isQuitNotification = false;
        StartCoroutine(ShowTimedNotification(isQuitNotification));
    }

    public void ShowQuitNotification()
    {
        isQuitNotification = true;
        StartCoroutine(ShowTimedNotification(isQuitNotification));
    }

    /**
     * Shows a notification only for a brief moment.
     */
    IEnumerator ShowTimedNotification(bool isQuitNotification)
    {

        // fade in
        if (!isQuitNotification)
        {
            Notification.SetActive(true);
        }
        else
        {
            QuitNotification.SetActive(true);
        }
        StartCoroutine(FadeIn(isQuitNotification));

        // timeout
        float counter = NOTIFICATION_TIMER;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }

        // fade out
        StartCoroutine(FadeOut(isQuitNotification));
        StopCoroutine(ShowTimedNotification(isQuitNotification));
    }

    // Fades in the Notification
    IEnumerator FadeIn(bool isQuitNotification)
    {
        // 1 is opacity 100%
        float timeout = 0.01f;
        float fadeAmount = 1 / (0.4f / timeout);

        for (float f = fadeAmount; f <= 1; f += fadeAmount)
        {
            if (!isQuitNotification)
            {
                Image background = Notification.GetComponent<Image>();
                Color colorImage = background.color;
                colorImage.a = f;
                background.color = colorImage;

                Color colorText = NotificationText.color;
                colorText.a = f;
                NotificationText.color = colorText;
            }
            else
            {
                Image background = QuitNotification.GetComponent<Image>();
                Color colorImage = background.color;
                colorImage.a = f;
                background.color = colorImage;
            }
            yield return new WaitForSeconds(timeout);
        }
        StopCoroutine(FadeIn(isQuitNotification));
    }

    // Fades out the Notification
    IEnumerator FadeOut(bool isQuitNotification)
    {
        // 1 is opacity 100%
        float timeout = 0.01f;
        float fadeAmount = 1 / (0.4f / timeout);

        for (float f = 1; f > 0; f -= fadeAmount)
        {
            if (!isQuitNotification)
            {
                Image background = Notification.GetComponent<Image>();
                Color colorImage = background.color;
                colorImage.a = f;
                background.color = colorImage;

                Color colorText = NotificationText.color;
                colorText.a = f;
                NotificationText.color = colorText;
                Notification.SetActive(false);
            }
            else
            {
                Image background = QuitNotification.GetComponent<Image>();
                Color colorImage = background.color;
                colorImage.a = f;
                background.color = colorImage;
                QuitNotification.SetActive(false);
            }
            yield return new WaitForSeconds(timeout);
        }
        StopCoroutine(FadeOut(isQuitNotification));
    }
}
