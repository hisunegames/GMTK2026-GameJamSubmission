using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InpectItemScript : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler
{
    [SerializeField] private Button inspectButton;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text materialText;
    [SerializeField] private GameObject materialTextBox;
    [SerializeField] public Image artifactImage;

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

        if (!isZoomedIn && materialText != null)
        {
            materialText.gameObject.SetActive(false);
            materialText.text = string.Empty;

            if (materialTextBox != null)
            {
                materialTextBox.SetActive(false);
            }
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
    public void OnPointerEnter(PointerEventData eventData)
    {
        string pointerEnterName = eventData.pointerEnter != null ? eventData.pointerEnter.name : "null";
        Debug.Log($"[Inspect] OnPointerEnter - isZoomedIn={isZoomedIn}, pointerEnter target={pointerEnterName}");

        if (!isZoomedIn)
        {
            return;
        }

        ItemData item = GameManager.Instance != null ? GameManager.Instance.LotController.CurrentItem : null;
        if (item == null)
        {
            return;
        }

        materialText.text = "Material: ";
        for (int i = 0 ; i < item.materials.Count ; i++)
        {
            if (i > 0)
            {
                materialText.text += ", ";
            }
            materialText.text += item.materials[i];
        }
        materialText.gameObject.SetActive(true);

        if (materialTextBox != null)
        {
            materialTextBox.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[Inspect] OnPointerExit - isZoomedIn={isZoomedIn}");

        materialText.gameObject.SetActive(false);
        materialText.text = string.Empty;

        if (materialTextBox != null)
        {
            materialTextBox.SetActive(false);
        }
    }
}
