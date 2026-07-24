using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotController : MonoBehaviour
{
    public enum LotState { Reveal, Bidding, Resolve }

    public LotState CurrentState { get; private set; }
    public ItemData CurrentItem => currentItem;

    [SerializeField] private float revealDuration = 1f;
    [SerializeField] private float biddingDuration = 15f;
    [SerializeField] private float bidIncrement = 10f;
    [SerializeField] private float bidTimeExtension = 3f;
    [SerializeField] private float bidTimeExtensionDecay = 0.85f;
    [SerializeField] private float minBidTimeExtension = 0.5f;

    [Header("NPC bidders")]
    [SerializeField] private int npcBidderCount = 5;
    [SerializeField] private float npcTickInterval = 1f;

    public float currentPrice;
    public float timeRemaining;
    public string currentHighBidder;

    [Header("Day-scoped modifiers (set by GameManager)")]
    public float dayBidIncrementSurcharge;
    public float dayTimeExtensionBonusMultiplier = 1f;

    public event System.Action<ItemData, bool, float> OnLotResolved;

    private ItemData currentItem;
    private float currentTrueValue;
    private float extendMultiplier;
    private readonly List<NPCBidder> npcBidders = new List<NPCBidder>();
    private float npcTickTimer;
    private int extendingBidCount;

    public void StartLot(ItemData item)
    {
        CurrentState = LotState.Reveal;
        currentItem = item;
        currentPrice = item.basePrice;
        currentHighBidder = null;
        extendingBidCount = 0;
        currentTrueValue = GetTrueValue(item);
        extendMultiplier = item.mysticTraits.Contains(MysticTrait.SpiritBlessed) ? 2f : 1f;
        SpawnNpcBidders(item);
        Debug.Log($"[Lot] Reveal: {item.itemName}");
        StartCoroutine(RunLot());
    }

    private static readonly NPCPersonality[] Personalities = (NPCPersonality[])System.Enum.GetValues(typeof(NPCPersonality));

    private static float GetTrueValue(ItemData item)
    {
        float flatTotal = 0f;
        float multiplierTotal = 1f;

        foreach (ItemDefect defect in item.defects)
        {
            ValueModifier modifier = ItemDefectInfo.GetModifier(defect);
            flatTotal += modifier.FlatAdjustment;
            multiplierTotal *= modifier.Multiplier;
        }

        foreach (MysticTrait trait in item.mysticTraits)
        {
            ValueModifier modifier = MysticTraitInfo.GetModifier(trait);
            flatTotal += modifier.FlatAdjustment;
            multiplierTotal *= modifier.Multiplier;
        }

        return Mathf.Max(0f, (item.basePrice + flatTotal) * multiplierTotal);
    }

    private void SpawnNpcBidders(ItemData item)
    {
        npcBidders.Clear();
        npcTickTimer = 0f;

        float trueValue = currentTrueValue;

        for (int i = 0; i < npcBidderCount; i++)
        {
            NPCPersonality personality = Personalities[i % Personalities.Length];
            npcBidders.Add(new NPCBidder($"{personality} NPC", personality, trueValue));
        }
    }

    private IEnumerator RunLot()
    {
        yield return new WaitForSeconds(revealDuration);

        CurrentState = LotState.Bidding;
        timeRemaining = biddingDuration;
        Debug.Log("[Lot] Bidding opened");
    }

    private void Update()
    {
        if (CurrentState != LotState.Bidding)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            CurrentState = LotState.Resolve;
            ResolveLot();
            return;
        }

        npcTickTimer += Time.deltaTime;
        if (npcTickTimer >= npcTickInterval)
        {
            npcTickTimer = 0f;
            TickNpcBidders();
        }
    }

    private void ResolveLot()
    {
        bool playerWon = currentHighBidder == "Player";
        float profit = playerWon ? currentTrueValue - currentPrice : 0f;

        Debug.Log($"[Lot] Resolve - true value: {currentTrueValue:0}, winner: {currentHighBidder ?? "no one"} at {currentPrice}");

        if (playerWon)
        {
            Debug.Log(profit >= 0f ? $"[Lot] Player profit: +{profit:0}" : $"[Lot] Player loss: {profit:0}");
        }

        OnLotResolved?.Invoke(currentItem, playerWon, profit);
    }

    private void TickNpcBidders()
    {
        foreach (NPCBidder npc in npcBidders)
        {
            if (npc.WantsToBid(currentPrice, timeRemaining))
            {
                PlaceBid(npc.Name, extendsTimer: false);
                break;
            }
        }
    }

    public void PlaceBid(string bidderName, bool extendsTimer = true)
    {
        if (CurrentState != LotState.Bidding)
        {
            return;
        }

        currentPrice += bidIncrement + dayBidIncrementSurcharge;
        currentHighBidder = bidderName;
        Debug.Log($"[Lot] {bidderName} bids -> {currentPrice}");

        if (extendsTimer)
        {
            float extension = Mathf.Max(minBidTimeExtension, bidTimeExtension * Mathf.Pow(bidTimeExtensionDecay, extendingBidCount));
            extension *= extendMultiplier * dayTimeExtensionBonusMultiplier;
            extendingBidCount++;
            ExtendTimer(extension);
        }
    }

    public void ExtendTimer(float seconds)
    {
        timeRemaining += seconds;
        Debug.Log($"[Lot] Timer extended by {seconds}s -> {timeRemaining}s remaining");
    }
}
