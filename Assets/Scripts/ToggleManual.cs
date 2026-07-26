using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ToggleManual : MonoBehaviour
{
    [SerializeField] private GameObject manualPanel;

    private void Awake()
    {
        manualPanel.SetActive(false);

        if (DialogueManager.instance == null)
        {
            Debug.LogWarning("[ToggleManual] DialogueManager.instance is null in Awake - could not subscribe to conversationStarted.");
            return;
        }

        DialogueManager.instance.conversationStarted += OnConversationStarted;
        Debug.Log($"[ToggleManual] Subscribed to conversationStarted. manualPanel={(manualPanel != null ? manualPanel.name : "NULL")}");
    }

    private void OnDestroy()
    {
        if (DialogueManager.hasInstance)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
        }
    }

    private void OnConversationStarted(Transform actor)
    {
        Debug.Log($"[ToggleManual] conversationStarted fired - closing manual panel (currently active: {(manualPanel != null ? manualPanel.activeSelf.ToString() : "NULL")}).");
        CloseManualPanel();
    }

    public void ToggleManualPanel()
    {
        if (manualPanel != null)
        {
            bool opening = !manualPanel.activeSelf;
            manualPanel.SetActive(opening);

            if (opening && SfxManager.Instance != null)
            {
                SfxManager.Instance.PlayScrollOpen();
            }
        }
    }

    public void CloseManualPanel()
    {
        if (manualPanel != null)
        {
            manualPanel.SetActive(false);
        }
    }
}
