using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

[RequireComponent(typeof(LotController))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, DayIntro, LotLoop, Intermission, DayResults, WeekResults, NextDay, GameOver }

    public GameState CurrentState { get; private set; }
    public event System.Action<GameState> OnStateChanged;

    [Header("Week/day loop")]
    public int currentWeek = 1;
    public int dayOfWeek = 1;
    [SerializeField] private int daysPerWeek = 3;
    [SerializeField] private float quotaTarget = 200f;
    [SerializeField] private float weeklyQuotaTarget = 500f;
    [SerializeField] private float weeklyQuotaGrowthMultiplier = 2.2f;
    [SerializeField] private float intermissionDuration = 1f;

    public float QuotaTarget => quotaTarget;
    public float WeeklyQuotaTarget => weeklyQuotaTarget;
    public float WeekProgress => cash - cashAtWeekStart;

    public int currentDay = 1;
    public float cash;

    [SerializeField] private LotController lotController;
    public LotController LotController => lotController;

    [Header("Day/week summary panels")]
    [SerializeField] private DayResultsPanel dayResultsPanel;
    [SerializeField] private WeekResultsPanel weekResultsPanel;
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private ArcanaSurgeBoardPanel arcanaSurgeBoardPanel;

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
    [SerializeField] private List<NpcDialogueEntry> npcDialogueEntries = new();
    [SerializeField, Range(0f, 1f)] private float intermissionInteractionChance = 0.3f;
    [SerializeField] private List<int> intermissionInteractionEntryIDs = new();
    [SerializeField] private InpectItemScript inspectItemScript;
    [SerializeField] private ArcanaSurge arcanaSurgeScript;

    [System.Serializable]
    private class NpcDialogueEntry
    {
        public NPCPersonality personality;
        public int winEntryID = -1;
        public int loseEntryID = -1;
    }

    private float dayBidIncrementSurcharge;
    private float dayTimeExtensionBonusMultiplier = 1f;
    private float cashAtDayStart;
    private float cashAtWeekStart;
    private int lotsCompletedToday;
    private int lotsPerDay;
    private ItemBundle currentBundle;
    private int bundleIndex;
    private ItemData pendingLotItem;
    private readonly List<DayLedgerEntry> dayLedger = new List<DayLedgerEntry>();
    private ArcanaSurge.SurgeType surgeType;
    private bool npcWonThisLot;
    private string npcLoserName;
    private int npcWinEntryIDForLot = -1;
    private int npcLoseEntryIDForLot = -1;

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
    }

    public void StartGame()
    {
        SetState(GameState.DayIntro);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Entered state: {newState}");
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Menu:
                if (mainMenuPanel != null)
                {
                    mainMenuPanel.Show();
                }
                else
                {
                    StartGame();
                }
                break;

            case GameState.DayIntro:
                if (dayOfWeek == 1)
                {
                    cashAtWeekStart = cash;
                }

                dayBidIncrementSurcharge = 0f;
                dayTimeExtensionBonusMultiplier = 1f;
                lotController.dayBidIncrementSurcharge = 0f;
                lotController.dayTimeExtensionBonusMultiplier = 1f;
                cashAtDayStart = cash;
                lotsCompletedToday = 0;
                dayLedger.Clear();
                bundleIndex = 0;
                currentBundle = PickRandomBundle();
                lotsPerDay = currentBundle != null ? currentBundle.items.Count : (testItem != null ? 1 : 0);

                List<ItemData> dayItems = currentBundle != null
                    ? currentBundle.items
                    : (testItem != null ? new List<ItemData> { testItem } : new List<ItemData>());
                lotController.InitializeDailyNpcBidders(dayItems);

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

            case GameState.WeekResults:
                EvaluateWeekResults();
                break;

            case GameState.NextDay:
                dayOfWeek++;
                currentDay++;
                SetState(GameState.DayIntro);
                break;

            case GameState.GameOver:
                Debug.Log("[GameManager] Game over - weekly quota not met");
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
        DialogueLua.SetVariable("S_BidIncrement", BidIncrementCalculator.ComputeMinIncrement(item.basePrice));

        int announcementEntryID = GetLotAnnouncementEntryID(lotIndex, lotsPerDay);
        Debug.Log($"[GameManager] Starting lot conversation - LotIndex={lotIndex}, LotCount={lotsPerDay}, item={item.itemName}, entryID={announcementEntryID}");
        if (currentWeek >= 2)
        {
            surgeType = arcanaSurgeScript.RandomSurge();
            Debug.Log("randomsurgetype"+surgeType);
        }

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

        if (dayResultsPanel != null)
        {
            dayResultsPanel.Show(currentDay, dayLedger, dayProfit, metQuota, quotaTarget);
        }
        else
        {
            ContinueFromDayResults();
        }
    }

    public void ContinueFromDayResults()
    {
        SetState(dayOfWeek >= daysPerWeek ? GameState.WeekResults : GameState.NextDay);
    }

    private void EvaluateWeekResults()
    {
        float weekProfit = cash - cashAtWeekStart;
        bool metWeeklyQuota = weekProfit >= weeklyQuotaTarget;

        Debug.Log($"[GameManager] Week {currentWeek} results - profit {weekProfit:0}, weekly quota {weeklyQuotaTarget:0}, {(metWeeklyQuota ? "met" : "missed")}");

        weekResultsPanel?.Show(currentWeek, weekProfit, weeklyQuotaTarget, metWeeklyQuota);

        weeklyQuotaTarget = weekProfit * weeklyQuotaGrowthMultiplier;
        Debug.Log($"[GameManager] Next week's quota -> {weeklyQuotaTarget:0}");

        if (!metWeeklyQuota)
        {
            SetState(GameState.GameOver);
        }
        else if (weekResultsPanel == null)
        {
            ContinueFromWeekResults();
        }
    }

    public void ContinueFromWeekResults()
    {
        if (arcanaSurgeBoardPanel != null && arcanaSurgeScript != null)
        {
            ArcanaSurge.SurgeType nextSurge = arcanaSurgeScript.RandomSurge();
            arcanaSurgeBoardPanel.Show(nextSurge);
        }
        else
        {
            AdvanceToNextWeek();
        }
    }

    public void AdvanceToNextWeek()
    {
        dayOfWeek = 1;
        currentWeek++;
        currentDay++;
        SetState(GameState.DayIntro);
    }

    private NpcDialogueEntry FindNpcDialogueEntry(string npcName)
    {
        if (string.IsNullOrEmpty(npcName) || !System.Enum.TryParse(npcName, out NPCPersonality personality))
        {
            return null;
        }

        foreach (NpcDialogueEntry entry in npcDialogueEntries)
        {
            if (entry.personality == personality)
            {
                return entry;
            }
        }

        return null;
    }

    private void HandleLotResolved(ItemData item, bool playerWon, float profit)
    {
        lotsCompletedToday++;
        dayLedger.Add(new DayLedgerEntry(item.itemName, playerWon, playerWon ? profit : 0f));

        bool noBidder = string.IsNullOrEmpty(lotController.currentHighBidder);
        DialogueLua.SetVariable("BidWinner", noBidder ? "no one" : lotController.currentHighBidder);

        npcWonThisLot = !playerWon && !noBidder;
        npcWinEntryIDForLot = -1;
        npcLoseEntryIDForLot = -1;

        if (npcWonThisLot)
        {
            npcLoserName = lotController.GetRandomLoserName(lotController.currentHighBidder);
            DialogueLua.SetVariable("WinnerName", lotController.currentHighBidder);
            DialogueLua.SetVariable("LoserName", npcLoserName ?? "no one");

            NpcDialogueEntry winnerEntry = FindNpcDialogueEntry(lotController.currentHighBidder);
            npcWinEntryIDForLot = winnerEntry != null ? winnerEntry.winEntryID : -1;

            NpcDialogueEntry loserEntry = FindNpcDialogueEntry(npcLoserName);
            npcLoseEntryIDForLot = loserEntry != null ? loserEntry.loseEntryID : -1;
        }

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
            ContinueAfterSoldAnnouncement();
        }
    }

    private void OnResolveConversationEnded(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= OnResolveConversationEnded;
        ContinueAfterSoldAnnouncement();
    }

    private void ContinueAfterSoldAnnouncement()
    {
        if (npcWonThisLot && TriggerAuctioneerConversation(npcWinEntryIDForLot))
        {
            DialogueManager.instance.conversationEnded += OnNpcWinConversationEnded;
        }
        else
        {
            ContinueAfterNpcWinLine();
        }
    }

    private void OnNpcWinConversationEnded(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= OnNpcWinConversationEnded;
        ContinueAfterNpcWinLine();
    }

    private void ContinueAfterNpcWinLine()
    {
        if (npcWonThisLot && npcLoserName != null && TriggerAuctioneerConversation(npcLoseEntryIDForLot))
        {
            DialogueManager.instance.conversationEnded += OnNpcLoseConversationEnded;
        }
        else
        {
            npcWonThisLot = false;
            AdvanceAfterLotResolved();
        }
    }

    private void OnNpcLoseConversationEnded(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= OnNpcLoseConversationEnded;
        npcWonThisLot = false;
        AdvanceAfterLotResolved();
    }

    private void AdvanceAfterLotResolved()
    {
        SetState(lotsCompletedToday < lotsPerDay ? GameState.Intermission : GameState.DayResults);
    }

    private IEnumerator RunIntermission()
    {
        if (intermissionInteractionEntryIDs.Count > 0
            && Random.value < intermissionInteractionChance
            && lotController.TryGetRandomNpcPair(out string npcA, out string npcB))
        {
            DialogueLua.SetVariable("NpcA", npcA);
            DialogueLua.SetVariable("NpcB", npcB);

            int entryID = intermissionInteractionEntryIDs[Random.Range(0, intermissionInteractionEntryIDs.Count)];

            if (TriggerAuctioneerConversation(entryID))
            {
                bool conversationFinished = false;
                void HandleEnded(Transform actor) => conversationFinished = true;

                DialogueManager.instance.conversationEnded += HandleEnded;
                yield return new WaitUntil(() => conversationFinished);
                DialogueManager.instance.conversationEnded -= HandleEnded;
            }
        }

        yield return new WaitForSeconds(intermissionDuration);
        SetState(GameState.LotLoop);
    }
}
