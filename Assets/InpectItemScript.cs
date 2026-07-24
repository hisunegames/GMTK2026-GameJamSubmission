using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InpectItemScript : MonoBehaviour
{
    [Serializable]
    private class DefectSpriteEntry
    {
        public ItemDefect defect;
        public Sprite sprite;
    }

    [SerializeField] private Button inspectButton;
    [SerializeField] private TMP_Text resultText;

    [Header("Defect sprites (hook - assign sprites per defect when art exists)")]
    [SerializeField] private RectTransform defectSpriteContainer;
    [SerializeField] private List<DefectSpriteEntry> defectSprites;

    public event Action<bool> OnInspectZoomChanged;

    private readonly List<GameObject> spawnedDefectSprites = new();
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

        ShowDefectSprites(item);

        isZoomedIn = !isZoomedIn;
        OnInspectZoomChanged?.Invoke(isZoomedIn);
    }

    private void ShowDefectSprites(ItemData item)
    {
        foreach (GameObject spawned in spawnedDefectSprites)
        {
            Destroy(spawned);
        }

        spawnedDefectSprites.Clear();

        if (defectSpriteContainer == null || item.defects == null)
        {
            return;
        }

        foreach (ItemDefect defect in item.defects)
        {
            Sprite sprite = GetDefectSprite(defect);
            if (sprite == null)
            {
                continue;
            }

            GameObject go = new($"{defect} Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(defectSpriteContainer, false);
            go.GetComponent<Image>().sprite = sprite;
            spawnedDefectSprites.Add(go);
        }
    }

    private Sprite GetDefectSprite(ItemDefect defect)
    {
        if (defectSprites == null)
        {
            return null;
        }

        foreach (DefectSpriteEntry entry in defectSprites)
        {
            if (entry.defect == defect)
            {
                return entry.sprite;
            }
        }

        return null;
    }

    private static string BuildDefectSummary(ItemData item)
    {
        if (item.defects == null || item.defects.Count == 0)
        {
            return $"{item.itemName}: no visible defects.";
        }

        StringBuilder builder = new();
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
