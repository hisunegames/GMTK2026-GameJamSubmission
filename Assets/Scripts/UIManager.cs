using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private LotController lotController;
    [SerializeField] private TMP_Text bidTimerText;
    [SerializeField] private TMP_Text currentBidText;
    [SerializeField] private Button bidButton;

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
            currentBidText.text = $"Bid: {lotController.currentPrice:0}";
        }
    }

    private void OnBidClicked()
    {
        lotController.PlaceBid("Player");
    }
}
