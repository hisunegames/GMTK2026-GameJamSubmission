using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InpectItemScript : MonoBehaviour
{
    [SerializeField] private Button inspectButton;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image artifactImage;

    public event Action<bool> OnInspectZoomChanged;
    public bool IsZoomedIn => isZoomedIn;

    private bool isZoomedIn;

    private void Start()
    {
        if (inspectButton != null)
        {
            inspectButton.onClick.AddListener(OnInspectButtonClicked);
        }
    }

    private void OnInspectButtonClicked()
    {
        ItemData item = GameManager.Instance != null ? GameManager.Instance.LotController.CurrentItem : null;

        if (item == null)
        {
            Debug.Log("[Inspect] No item to inspect right now.");
            return;
        }

        string summary = BuildDefectSummary(item);
        Debug.Log($"[Inspect] {summary}");

        if (resultText != null)
        {
            resultText.text = summary;
        }

        isZoomedIn = !isZoomedIn;

        if (artifactImage != null)
        {
            Sprite defectVariant = item.defectSprite != null ? item.defectSprite : item.sprite;
            artifactImage.sprite = isZoomedIn ? defectVariant : item.sprite;
        }

        OnInspectZoomChanged?.Invoke(isZoomedIn);
    }

    private static string BuildDefectSummary(ItemData item)
    {
        if (item.defects == null || item.defects.Count == 0)
        {
            return $"{item.itemName}: no visible defects.";
        }

        System.Text.StringBuilder builder = new();
        builder.Append(item.itemName).Append(": ");

        for (int i = 0; i < item.defects.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(item.defects[i]);
        }

        return builder.ToString();
    }
}
