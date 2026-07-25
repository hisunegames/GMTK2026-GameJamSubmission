using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

[RequireComponent(typeof(LotController))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, DayIntro, LotLoop, Intermission, DayResults, NextDay, GameOver }

    public GameState CurrentState { get; private set; }

    [Header("Week/day loop")]
    public int currentWeek = 1;
    public int dayOfWeek = 1;
    [SerializeField] private int daysPerWeek = 3;
    [SerializeField] private float quotaTarget = 200f;
    [SerializeField] private int maxMissedQuotaDays = 3;
    [SerializeField] private float intermissionDuration = 1f;

    public int currentDay = 1;
    public float cash;

    [SerializeField] private LotController lotController;
    public LotController LotController => lotController;

    [Header("Item bundles - hand-authored sets of items, one picked at random per day")]
    [SerializeField] private List<ItemBundle> itemBundles;
    [SerializeField] private ItemData testItem;

    [Header("Dialogue")]
    [SerializeField] private string lotStartConversation = "ItemAnnouncement";
    [SerializeField] private Transform dialogueActor;
    [SerializeField] private Transform dialogueConversant;
    [SerializeField] private int firstItemEntryID = -1;
    [SerializeField] private int nextItemEntryID = -1;
    [SerializeField] private int lastItemEntryID = -1;
    [SerializeField] private int callingOnceEntryID = -1;
    [SerializeField] private int callingTwiceEntryID = -1;
    [SerializeField] private int soldEntryID = -1;
    [SerializeField] private int noBidEntryID = -1;
    [SerializeField] private InpectItemScript inspectItemScript;

    private float dayBidIncrementSurcharge;
    private float dayTimeExtensionBonusMultiplier = 1f;
    private float cashAtDayStart;
    private int lotsCompletedToday;
    private int lotsPerDay;
    private int missedQuotaStreak;
    private ItemBundle currentBundle;
    private int bundleIndex;
    private ItemData pendingLotItem;

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

        if (inspectItemScript == null)
        {
            inspectItemScript = FindFirstObjectByType<InpectItemScript>();
        }

        lotController.OnLotResolved += HandleLotResolved;
        lotController.OnBiddingStarted += () => DialogueLua.SetVariable("IsInBiddingStage", true);
        lotController.OnBiddingEnded += () => DialogueLua.SetVariable("IsInBiddingStage", false);
        lotController.OnCallingOnce += () => TriggerAuctioneerBark(callingOnceEntryID);
        lotController.OnCallingTwice += () => TriggerAuctioneerBark(callingTwiceEntryID);
    }

    private bool ShouldSkipAuctioneerCall()
    {
        if (inspectItemScript != null && inspectItemScript.IsZoomedIn)
        {
            Debug.Log("[GameManager] Skipping auctioneer call - player is inspecting");
            return true;
        }

        return false;
    }

    private void TriggerAuctioneerBark(int entryID)
    {
        if (entryID < 0 || ShouldSkipAuctioneerCall())
        {
            return;
        }

        DialogueManager.Bark(lotStartConversation, dialogueConversant, dialogueActor, entryID);
    }

    private bool TriggerAuctioneerConversation(int entryID)
    {
        if (entryID < 0 || ShouldSkipAuctioneerCall())
        {
            return false;
        }

        DialogueManager.StartConversation(lotStartConversation, dialogueActor, dialogueConversant, entryID);
        return true;
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
                bundleIndex = 0;
                currentBundle = PickRandomBundle();
                lotsPerDay = currentBundle != null ? currentBundle.items.Count : (testItem != null ? 1 : 0);

                Debug.Log($"[GameManager] Week {currentWeek}, day {dayOfWeek} - {lotsPerDay} lots" + (currentBundle != null ? $" (bundle: {currentBundle.name})" : ""));

                SetState(GameState.LotLoop);
                break;

            case GameState.LotLoop:
                StartNextLot();
                break;

            case GameState.Intermission:
                StartCoroutine(RunIntermission());
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

    private ItemBundle PickRandomBundle()
    {
        if (itemBundles == null || itemBundles.Count == 0)
        {
            return null;
        }

        return itemBundles[Random.Range(0, itemBundles.Count)];
    }

    private void StartNextLot()
    {
        ItemData item = null;

        if (currentBundle != null && bundleIndex < currentBundle.items.Count)
        {
            item = currentBundle.items[bundleIndex];
            bundleIndex++;
        }
        else if (testItem != null)
        {
            item = testItem;
        }

        if (item == null)
        {
            Debug.LogWarning("[GameManager] No item source assigned - skipping to day results");
            SetState(GameState.DayResults);
            return;
        }

        int lotIndex = lotsCompletedToday + 1;
        DialogueLua.SetVariable("ArtifactName", item.itemName);
        DialogueLua.SetVariable("LotIndex", lotIndex);
        DialogueLua.SetVariable("LotCount", lotsPerDay);

        int announcementEntryID = GetLotAnnouncementEntryID(lotIndex, lotsPerDay);
        Debug.Log($"[GameManager] Starting lot conversation - LotIndex={lotIndex}, LotCount={lotsPerDay}, item={item.itemName}, entryID={announcementEntryID}");

        if (!string.IsNullOrEmpty(lotStartConversation) && announcementEntryID >= 0)
        {
            pendingLotItem = item;
            DialogueManager.instance.conversationEnded += OnLotIntroConversationEnded;
            DialogueManager.StartConversation(lotStartConversation, dialogueActor, dialogueConversant, announcementEntryID);
        }
        else
        {
            lotController.StartLot(item);
        }
    }

    private int GetLotAnnouncementEntryID(int lotIndex, int lotCount)
    {
        if (lotIndex >= lotCount)
        {
            return lastItemEntryID;
        }

        return lotIndex <= 1 ? firstItemEntryID : nextItemEntryID;
    }

    private void OnLotIntroConversationEnded(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= OnLotIntroConversationEnded;
        lotController.StartLot(pendingLotItem);
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

        bool noBidder = string.IsNullOrEmpty(lotController.currentHighBidder);
        DialogueLua.SetVariable("BidWinner", noBidder ? "no one" : lotController.currentHighBidder);

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

        int resolveEntryID = noBidder ? noBidEntryID : soldEntryID;

        if (TriggerAuctioneerConversation(resolveEntryID))
        {
            DialogueManager.instance.conversationEnded += OnResolveConversationEnded;
        }
        else
        {
            AdvanceAfterLotResolved();
        }
    }

    private void OnResolveConversationEnded(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= OnResolveConversationEnded;
        AdvanceAfterLotResolved();
    }

    private void AdvanceAfterLotResolved()
    {
        SetState(lotsCompletedToday < lotsPerDay ? GameState.Intermission : GameState.DayResults);
    }

    private IEnumerator RunIntermission()
    {
        // Hook point for between-item dialogue/flavor - currently just a timed pause.
        yield return new WaitForSeconds(intermissionDuration);
        SetState(GameState.LotLoop);
    }
}
