using UnityEngine;

[RequireComponent(typeof(LotController))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, DayIntro, LotLoop, DayResults, NextDay, GameOver }

    public GameState CurrentState { get; private set; }

    [Header("Week/day loop")]
    public int currentWeek = 1;
    public int dayOfWeek = 1;
    [SerializeField] private int daysPerWeek = 3;
    [SerializeField] private Vector2Int lotsPerDayRange = new Vector2Int(4, 5);
    [SerializeField] private float quotaTarget = 200f;
    [SerializeField] private int maxMissedQuotaDays = 3;

    public int currentDay = 1;
    public float cash;

    [SerializeField] private LotController lotController;
    public LotController LotController => lotController;

    [Header("Temporary item source - replace with a real item catalog later")]
    [SerializeField] private ItemData testItem;
    [SerializeField] private ItemGeneratorConfig itemGeneratorConfig;

    private float dayBidIncrementSurcharge;
    private float dayTimeExtensionBonusMultiplier = 1f;
    private float cashAtDayStart;
    private int lotsCompletedToday;
    private int lotsPerDay;
    private int missedQuotaStreak;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (lotController == null)
        {
            lotController = GetComponent<LotController>();
        }

        lotController.OnLotResolved += HandleLotResolved;
    }

    private void Start()
    {
        SetState(GameState.Menu);
        SetState(GameState.DayIntro);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Entered state: {newState}");

        switch (newState)
        {
            case GameState.Menu:
                break;

            case GameState.DayIntro:
                dayBidIncrementSurcharge = 0f;
                dayTimeExtensionBonusMultiplier = 1f;
                lotController.dayBidIncrementSurcharge = 0f;
                lotController.dayTimeExtensionBonusMultiplier = 1f;
                cashAtDayStart = cash;
                lotsCompletedToday = 0;
                lotsPerDay = Random.Range(lotsPerDayRange.x, lotsPerDayRange.y + 1);

                Debug.Log($"[GameManager] Week {currentWeek}, day {dayOfWeek} - {lotsPerDay} lots");

                SetState(GameState.LotLoop);
                break;

            case GameState.LotLoop:
                StartNextLot();
                break;

            case GameState.DayResults:
                EvaluateDayResults();
                break;

            case GameState.NextDay:
                dayOfWeek++;
                if (dayOfWeek > daysPerWeek)
                {
                    dayOfWeek = 1;
                    currentWeek++;
                }

                currentDay++;
                SetState(GameState.DayIntro);
                break;

            case GameState.GameOver:
                Debug.Log($"[GameManager] Game over - missed quota {missedQuotaStreak} days in a row");
                break;
        }
    }

    private void StartNextLot()
    {
        ItemData item = itemGeneratorConfig != null
            ? ProceduralItemGenerator.GenerateItem(itemGeneratorConfig)
            : testItem;

        if (item == null)
        {
            Debug.LogWarning("[GameManager] No item source assigned - skipping to day results");
            SetState(GameState.DayResults);
            return;
        }

        lotController.StartLot(item);
    }

    private void EvaluateDayResults()
    {
        float dayProfit = cash - cashAtDayStart;
        bool metQuota = dayProfit >= quotaTarget;

        Debug.Log($"[GameManager] Day {currentDay} results - profit {dayProfit:0}, quota {quotaTarget:0}, {(metQuota ? "met" : "missed")}");

        missedQuotaStreak = metQuota ? 0 : missedQuotaStreak + 1;

        SetState(missedQuotaStreak >= maxMissedQuotaDays ? GameState.GameOver : GameState.NextDay);
    }

    private void HandleLotResolved(ItemData item, bool playerWon, float profit)
    {
        lotsCompletedToday++;

        if (playerWon)
        {
            cash += profit;
            Debug.Log($"[GameManager] Cash updated -> {cash}");

            if (item.mysticTraits.Contains(MysticTrait.Cursed))
            {
                float surcharge = Random.Range(10f, 30f);
                dayBidIncrementSurcharge += surcharge;
                Debug.Log($"[GameManager] Cursed item bought - bids on today's remaining items cost +{surcharge:0} more");
            }

            if (item.mysticTraits.Contains(MysticTrait.SpiritBlessed))
            {
                dayTimeExtensionBonusMultiplier *= 2f;
                Debug.Log($"[GameManager] Spirit-blessed item bought - bid time extensions today are now x{dayTimeExtensionBonusMultiplier}");
            }

            lotController.dayBidIncrementSurcharge = dayBidIncrementSurcharge;
            lotController.dayTimeExtensionBonusMultiplier = dayTimeExtensionBonusMultiplier;
        }
        else
        {
            Debug.Log("[GameManager] Player did not win the lot");
        }

        if (CurrentState != GameState.LotLoop)
        {
            return;
        }

        SetState(lotsCompletedToday < lotsPerDay ? GameState.LotLoop : GameState.DayResults);
    }
}
