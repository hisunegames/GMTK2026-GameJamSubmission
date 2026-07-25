using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Virtual cameras (Cinemachine) - exactly one active at a time, swapped by index")]
    [SerializeField] private List<GameObject> virtualCameras;
    [SerializeField] private int defaultCameraIndex = 0;
    [SerializeField] private int inspectZoomCameraIndex = 1;
    [SerializeField] private int dialogueCameraIndex = 2;

    [SerializeField] private InpectItemScript inspectItemScript;

    private void Start()
    {
        if (inspectItemScript == null)
        {
            inspectItemScript = FindFirstObjectByType<InpectItemScript>();
        }

        if (inspectItemScript != null)
        {
            inspectItemScript.OnInspectZoomChanged += OnInspectZoomChanged;
        }

        DialogueManager.instance.conversationStarted += OnConversationStarted;
        DialogueManager.instance.conversationEnded += OnConversationEnded;

        ActivateVirtualCamera(defaultCameraIndex);
    }

    private void OnDestroy()
    {
        if (inspectItemScript != null)
        {
            inspectItemScript.OnInspectZoomChanged -= OnInspectZoomChanged;
        }

        if (DialogueManager.hasInstance)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
            DialogueManager.instance.conversationEnded -= OnConversationEnded;
        }
    }

    private void OnInspectZoomChanged(bool isZoomedIn)
    {
        ActivateVirtualCamera(isZoomedIn ? inspectZoomCameraIndex : defaultCameraIndex);
    }

    private void OnConversationStarted(Transform actor)
    {
        ActivateVirtualCamera(dialogueCameraIndex);
    }

    private void OnConversationEnded(Transform actor)
    {
        ActivateVirtualCamera(defaultCameraIndex);
    }

    public void ActivateVirtualCamera(int index)
    {
        if (virtualCameras == null)
        {
            return;
        }

        for (int i = 0; i < virtualCameras.Count; i++)
        {
            if (virtualCameras[i] != null)
            {
                virtualCameras[i].SetActive(i == index);
            }
        }
    }
}
