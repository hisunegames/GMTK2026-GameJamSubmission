using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private LotController lotController;
    [SerializeField] private TMP_Text bidTimerText;
    [SerializeField] private TMP_Text currentBidText;
    [SerializeField] private TMP_Text fundsText;
    [SerializeField] private Button bidButton;
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

        if (artifactImage != null && lotController.CurrentItem != shownItem)
        {
            shownItem = lotController.CurrentItem;
            artifactImage.sprite = shownItem != null ? shownItem.sprite : null;
        }
    }

    private void OnBidClicked()
    {
        lotController.PlaceBid("Player");
    }
}
