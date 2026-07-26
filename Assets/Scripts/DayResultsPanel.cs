using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayResultsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text ledgerText;
    [SerializeField] private TMP_Text profitText;
    [SerializeField] private MoneyCountUpText profitCountUp;
    [SerializeField] private TMP_Text quotaStatusText;
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

    public void Show(int day, IReadOnlyList<DayLedgerEntry> ledger, float profit, float remainingToWeeklyQuota)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (dayText != null)
        {
            dayText.text = $"Day {day} Results";
        }

        if (ledgerText != null)
        {
            StringBuilder builder = new();

            foreach (DayLedgerEntry entry in ledger)
            {
                string outcome = entry.PlayerWon
                    ? (entry.Profit >= 0f ? $"+{entry.Profit:0}" : $"{entry.Profit:0}")
                    : "not won";

                builder.AppendLine($"{entry.ItemName} - {outcome}");
            }

            builder.AppendLine();
            builder.Append(profit >= 0f ? $"Grand total: +{profit:0}" : $"Grand total: {profit:0}");

            ledgerText.text = builder.ToString();
        }

        if (profitCountUp != null)
        {
            profitCountUp.CountTo(profit, v => v >= 0f ? $"Profit: +{v:0}" : $"Loss: {v:0}");
        }
        else if (profitText != null)
        {
            profitText.text = profit >= 0f ? $"Profit: +{profit:0}" : $"Loss: {profit:0}";
        }

        if (quotaStatusText != null)
        {
            quotaStatusText.text = remainingToWeeklyQuota > 0f
                ? $"{remainingToWeeklyQuota:0} left to hit weekly quota"
                : "Weekly quota met!";
        }
    }

    private void OnContinueClicked()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        GameManager.Instance.ContinueFromDayResults();
    }
}
