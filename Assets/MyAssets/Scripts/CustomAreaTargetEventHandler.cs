using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

/**
 * A custom handler that inherits from the DefaultObserverEventHandler class.
 * 
 * We don't disable renderings here, because we disable them ourselves in ARNavigationController
 * 
 * OnTargetFound&Lost don't have to be set on GameObject, since we call our own function of ARStateController
 */
public class CustomAreaTargetEventHandler : DefaultObserverEventHandler
{
    AreaTargetBehaviour atBehaviour; // AT behaviour on same object to tell ARStateController

    private void Awake()
    {
        atBehaviour = GetComponent<AreaTargetBehaviour>();
    }

    protected override void OnTrackingFound()
    {
        //SetAugmentationRendering(true);
        ARStateController.instance.OnTargetFound(atBehaviour);
        OnTargetFound?.Invoke();
    }

    protected override void OnTrackingLost()
    {
        //SetAugmentationRendering(false);
        ARStateController.instance.OnTargetLost(atBehaviour);
        OnTargetLost?.Invoke();
    }

    /**
     * 
     * Methods from DefaultAreaTargeEventHandler
     * 
    void SetAugmentationRendering(bool value)
    {
        for (var i = 0; i < transform.childCount; i++)
            SetEnabledOnChildComponents(transform.GetChild(i), value);
        SetVuforiaRenderingComponents(value);
    }

    void SetEnabledOnChildComponents(Transform augmentationTransform, bool value)
    {
        var augmentationRenderer = augmentationTransform.GetComponent<VuforiaAugmentationRenderer>();
        if (augmentationRenderer != null)
        {
            augmentationRenderer.SetActive(value);
            return;
        }

        if (mObserverBehaviour)
        {
            var rendererComponent = augmentationTransform.GetComponent<Renderer>();
            if (rendererComponent != null)
                rendererComponent.enabled = value;
            var canvasComponent = augmentationTransform.GetComponent<Canvas>();
            if (canvasComponent != null)
                canvasComponent.enabled = value;
            var colliderComponent = augmentationTransform.GetComponent<Collider>();
            if (colliderComponent != null)
                colliderComponent.enabled = value;
        }

        for (var i = 0; i < augmentationTransform.childCount; i++)
            SetEnabledOnChildComponents(augmentationTransform.GetChild(i), value);
    }

    void SetVuforiaRenderingComponents(bool value)
    {
        var augmentationRendererComponents = mObserverBehaviour.GetComponentsInChildren<VuforiaAugmentationRenderer>(false);
        foreach (var component in augmentationRendererComponents)
            component.SetActive(value);
    }
    **/
}
