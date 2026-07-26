using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private LotController lotController;
    [SerializeField] private TMP_Text bidTimerText;
    [SerializeField] private TMP_Text currentBidText;
    [SerializeField] private TMP_Text fundsText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text quotaText;
    [SerializeField] private Button bidButton;
    [SerializeField] private TMP_InputField bidIncrementInput;
    [SerializeField] private Image artifactImage;

    [Header("Top bidder chibi head")]
    [SerializeField] private Image currentBidderImage;
    [SerializeField] private Sprite playerBidderSprite;
    [SerializeField] private List<BidderSprite> npcBidderSprites = new();

    [System.Serializable]
    private class BidderSprite
    {
        public NPCPersonality personality;
        public Sprite sprite;
    }

    private ItemData shownItem;

    private void Awake()
    {
        if (lotController == null)
        {
            lotController = FindFirstObjectByType<LotController>();
        }

        if (bidButton != null)
        {
            bidButton.onClick.AddListener(OnBidClicked);
        }

        if (bidIncrementInput != null)
        {
            bidIncrementInput.onEndEdit.AddListener(OnBidIncrementInputEndEdit);
        }
    }

    private void SyncBidIncrementInput()
    {
        if (bidIncrementInput == null || lotController == null)
        {
            return;
        }

        bidIncrementInput.text = lotController.MinBidIncrement.ToString("0");
    }

    private void OnBidIncrementInputEndEdit(string text)
    {
        if (lotController == null)
        {
            return;
        }

        float maxAffordable = GameManager.Instance != null ? GameManager.Instance.cash : lotController.MaxBidIncrement;
        float value = float.TryParse(text, out float parsed) ? parsed : lotController.MinBidIncrement;
        value = Mathf.Clamp(value, lotController.MinBidIncrement, maxAffordable);
        bidIncrementInput.text = value.ToString("0");
    }

    private void Update()
    {
        if (lotController == null)
        {
            return;
        }

        if (bidTimerText != null)
        {
            bidTimerText.text = $"{lotController.timeRemaining:0.0}";
        }

        if (currentBidText != null)
        {
            currentBidText.text = $"{lotController.currentPrice:0}";
        }

        if (currentBidderImage != null)
        {
            Sprite bidderSprite = GetBidderSprite(lotController.currentHighBidder);
            currentBidderImage.sprite = bidderSprite;
            currentBidderImage.enabled = bidderSprite != null;
        }

        if (fundsText != null && GameManager.Instance != null)
        {
            fundsText.text = $"{GameManager.Instance.cash:0}";
        }

        if (dayText != null && GameManager.Instance != null)
        {
            dayText.text = $"{GameManager.Instance.currentDay}";
        }

        if (quotaText != null && GameManager.Instance != null)
        {
            quotaText.text = $"{GameManager.Instance.WeekProgress:0}/{GameManager.Instance.WeeklyQuotaTarget:0}";
        }

        if (lotController.CurrentItem != shownItem)
        {
            shownItem = lotController.CurrentItem;

            if (artifactImage != null)
            {
                artifactImage.sprite = shownItem != null ? shownItem.sprite : null;
            }

            SyncBidIncrementInput();
        }
    }

    private Sprite GetBidderSprite(string bidderName)
    {
        if (string.IsNullOrEmpty(bidderName))
        {
            return null;
        }

        if (bidderName == "Player")
        {
            return playerBidderSprite;
        }

        if (System.Enum.TryParse(bidderName, out NPCPersonality personality))
        {
            foreach (BidderSprite entry in npcBidderSprites)
            {
                if (entry.personality == personality)
                {
                    return entry.sprite;
                }
            }
        }

        return null;
    }

    private void OnBidClicked()
    {
        float maxAffordable = GameManager.Instance != null ? GameManager.Instance.cash : lotController.MaxBidIncrement;
        float increment = lotController.MinBidIncrement;

        if (bidIncrementInput != null && float.TryParse(bidIncrementInput.text, out float typedValue))
        {
            increment = Mathf.Clamp(typedValue, lotController.MinBidIncrement, maxAffordable);
        }

        lotController.PlaceBid("Player", increment);
    }
}
