using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppraisalManualScript : MonoBehaviour
{
    [Serializable]
    private class DefectToggleEntry
    {
        public ItemDefect defect;
        public Toggle toggle;
        public TMP_Text label;
    }

    [Serializable]
    private class TraitToggleEntry
    {
        public MysticTrait trait;
        public Toggle toggle;
        public TMP_Text label;
    }

    [SerializeField] private List<DefectToggleEntry> defectToggles;
    [SerializeField] private List<TraitToggleEntry> traitToggles;
    [SerializeField] private TMP_Text estimatedPriceText;
    [SerializeField] private Button openManualButton;

    private LotController lotController;

    private void Awake()
    {
        SetManualInteractable(false);

        foreach (DefectToggleEntry entry in defectToggles)
        {
            if (entry.label != null)
            {
                entry.label.text = $"{entry.defect} - {ItemDefectInfo.GetDescription(entry.defect)}";
            }

            if (entry.toggle != null)
            {
                entry.toggle.SetIsOnWithoutNotify(false);
                entry.toggle.onValueChanged.AddListener(_ => RecalculateEstimate());
            }
        }

        foreach (TraitToggleEntry entry in traitToggles)
        {
            if (entry.label != null)
            {
                entry.label.text = $"{entry.trait} - {MysticTraitInfo.GetDescription(entry.trait)}";
            }

            if (entry.toggle != null)
            {
                entry.toggle.SetIsOnWithoutNotify(false);
                entry.toggle.onValueChanged.AddListener(_ => RecalculateEstimate());
            }
        }

        RecalculateEstimate();
    }

    private void OnEnable()
    {
        lotController = GameManager.Instance != null ? GameManager.Instance.LotController : null;
        Debug.Log($"[Manual] OnEnable - lotController={(lotController != null ? "OK" : "NULL")}");

        if (lotController == null)
        {
            return;
        }

        lotController.OnBiddingStarted += HandleBiddingStarted;
        lotController.OnBiddingEnded += HandleBiddingEnded;

        // Catch up in case bidding already started/ended while this panel was inactive
        // and we missed the event firing.
        bool isBidding = lotController.CurrentState == LotController.LotState.Bidding;
        Debug.Log($"[Manual] OnEnable - CurrentState={lotController.CurrentState}, CurrentItem={(lotController.CurrentItem != null ? lotController.CurrentItem.itemName : "NULL")}, setting interactable={isBidding}");
        SetManualInteractable(isBidding);
        RecalculateEstimate();
    }

    private void OnDisable()
    {
        Debug.Log("[Manual] OnDisable");
        if (lotController != null)
        {
            lotController.OnBiddingStarted -= HandleBiddingStarted;
            lotController.OnBiddingEnded -= HandleBiddingEnded;
        }
    }

    private void HandleBiddingStarted()
    {
        Debug.Log("[Manual] HandleBiddingStarted - unlocking");
        ResetToggles();
        SetManualInteractable(true);
    }

    private void HandleBiddingEnded()
    {
        Debug.Log("[Manual] HandleBiddingEnded - locking");
        SetManualInteractable(false);
    }

    private void SetManualInteractable(bool interactable)
    {
        if (openManualButton != null)
        {
            openManualButton.interactable = interactable;
        }

        foreach (DefectToggleEntry entry in defectToggles)
        {
            if (entry.toggle != null)
            {
                entry.toggle.interactable = interactable;
            }
        }

        foreach (TraitToggleEntry entry in traitToggles)
        {
            if (entry.toggle != null)
            {
                entry.toggle.interactable = interactable;
            }
        }
    }

    private void ResetToggles()
    {
        foreach (DefectToggleEntry entry in defectToggles)
        {
            if (entry.toggle != null)
            {
                entry.toggle.SetIsOnWithoutNotify(false);
            }
        }

        foreach (TraitToggleEntry entry in traitToggles)
        {
            if (entry.toggle != null)
            {
                entry.toggle.SetIsOnWithoutNotify(false);
            }
        }

        RecalculateEstimate();
    }

    private void RecalculateEstimate()
    {
        if (estimatedPriceText == null)
        {
            return;
        }

        ItemData item = lotController != null ? lotController.CurrentItem : null;
        Debug.Log($"[Manual] RecalculateEstimate - lotController={(lotController != null ? "OK" : "NULL")}, CurrentState={(lotController != null ? lotController.CurrentState.ToString() : "N/A")}, item={(item != null ? "OK" : "NULL")}");

        if (item == null)
        {
            Debug.Log("[Manual] RecalculateEstimate - item is NULL, showing placeholder");
            estimatedPriceText.text = "Estimated sell value: -";
            return;
        }

        float flatTotal = 0f;
        float multiplierTotal = lotController.BaseResaleMarkup;

        foreach (ItemMaterial material in item.materials)
        {
            multiplierTotal *= ItemMaterialInfo.GetMultiplier(material);
        }

        foreach (DefectToggleEntry entry in defectToggles)
        {
            if (entry.toggle != null && entry.toggle.isOn)
            {
                ValueModifier modifier = ItemDefectInfo.GetEstimatedModifier(entry.defect);
                flatTotal += modifier.FlatAdjustment;
                multiplierTotal *= modifier.Multiplier;
            }
        }

        foreach (TraitToggleEntry entry in traitToggles)
        {
            if (entry.toggle != null && entry.toggle.isOn)
            {
                ValueModifier modifier = MysticTraitInfo.GetModifier(entry.trait);
                flatTotal += modifier.FlatAdjustment;
                multiplierTotal *= modifier.Multiplier;
            }
        }

        float estimatedSellValue = Mathf.Max(0f, (item.basePrice + flatTotal) * multiplierTotal);
        estimatedPriceText.text = $"Estimated sell value: <wiggle a=0.1 f=0.1>{estimatedSellValue:0}</>";
    }
}
