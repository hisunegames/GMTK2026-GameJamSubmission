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
            bidTimerText.text = $"Timer\n{lotController.timeRemaining:0.0}";
        }

        if (currentBidText != null)
        {
            string highBidder = string.IsNullOrEmpty(lotController.currentHighBidder) ? "-" : lotController.currentHighBidder;
            currentBidText.text = $"Bid: {lotController.currentPrice:0} ({highBidder})";
        }

        if (fundsText != null && GameManager.Instance != null)
        {
            fundsText.text = $"Funds: {GameManager.Instance.cash:0}";
        }

        if (dayText != null && GameManager.Instance != null)
        {
            dayText.text = $"Day {GameManager.Instance.currentDay}";
        }

        if (quotaText != null && GameManager.Instance != null)
        {
            quotaText.text = $"Quota: {GameManager.Instance.WeekProgress:0}/{GameManager.Instance.WeeklyQuotaTarget:0}";
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
