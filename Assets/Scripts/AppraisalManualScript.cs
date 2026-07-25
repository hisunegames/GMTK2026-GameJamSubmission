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
    }

    [Serializable]
    private class TraitToggleEntry
    {
        public MysticTrait trait;
        public Toggle toggle;
    }

    [SerializeField] private List<DefectToggleEntry> defectToggles;
    [SerializeField] private List<TraitToggleEntry> traitToggles;
    [SerializeField] private TMP_Text estimatedPriceText;

    private LotController lotController;

    private void Start()
    {
        lotController = GameManager.Instance != null ? GameManager.Instance.LotController : null;

        if (lotController != null)
        {
            lotController.OnBiddingStarted += ResetToggles;
        }

        foreach (DefectToggleEntry entry in defectToggles)
        {
            if (entry.toggle != null)
            {
                entry.toggle.onValueChanged.AddListener(_ => RecalculateEstimate());
            }
        }

        foreach (TraitToggleEntry entry in traitToggles)
        {
            if (entry.toggle != null)
            {
                entry.toggle.onValueChanged.AddListener(_ => RecalculateEstimate());
            }
        }

        RecalculateEstimate();
    }

    private void OnDestroy()
    {
        if (lotController != null)
        {
            lotController.OnBiddingStarted -= ResetToggles;
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

        if (item == null)
        {
            estimatedPriceText.text = "Estimated price: -";
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

        float estimatedPrice = Mathf.Max(0f, (item.basePrice + flatTotal) * multiplierTotal);
        estimatedPriceText.text = $"Estimated price: <wiggle a=0.1 f=0.1>{estimatedPrice:0}</>";
    }
}
