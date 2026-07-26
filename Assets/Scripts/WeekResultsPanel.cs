using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeekResultsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text weekText;
    [SerializeField] private TMP_Text profitText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(int week, float profit, float quotaTarget, bool passed)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (weekText != null)
        {
            weekText.text = $"Week {week} Results";
        }

        if (profitText != null)
        {
            profitText.text = profit >= 0f ? $"Total profit: +{profit:0}" : $"Total loss: {profit:0}";
        }

        if (resultText != null)
        {
            resultText.text = passed
                ? $"{profit:0} / {quotaTarget:0} - Congratulations, moving on to next week"
                : $"{profit:0} / {quotaTarget:0} - Game Over";
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(passed);
        }
    }

    private void OnContinueClicked()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        GameManager.Instance.ContinueFromWeekResults();
    }
}
