using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotController : MonoBehaviour
{
    public enum LotState { Reveal, Bidding, Resolve }

    public LotState CurrentState { get; private set; }
    public ItemData CurrentItem => currentItem;
    public float BaseResaleMarkup => baseResaleMarkup;

    [SerializeField] private float revealDuration = 1f;
    [SerializeField] private float biddingDuration = 15f;
    [SerializeField] private float maxIncrementMultiplier = 1.5f;
    [SerializeField] private float bidTimeExtension = 3f;
    [SerializeField] private float bidTimeExtensionDecay = 0.85f;
    [SerializeField] private float minBidTimeExtension = 0.5f;
    [SerializeField] private float baseResaleMarkup = 1f;
    [SerializeField] private float startingPriceRatio = 0.3f;
    [SerializeField] private float cursedBidIncrementMultiplier = 3f;

    public float MinBidIncrement => currentMinBidIncrement;
    public float MaxBidIncrement => currentMaxBidIncrement;

    [Header("NPC bidders")]
    [SerializeField] private int npcBidderCount = 4;
    [SerializeField] private float npcTickInterval = 1f;

    [Header("Auctioneer calling thresholds (seconds remaining)")]
    [SerializeField] private float callingOnceThreshold = 6f;
    [SerializeField] private float callingTwiceThreshold = 3f;

    public float currentPrice;
    public float timeRemaining;
    public string currentHighBidder;

    [Header("Day-scoped modifiers (set by GameManager)")]
    public float dayBidIncrementSurcharge;
    public float dayTimeExtensionBonusMultiplier = 1f;

    public event System.Action<ItemData, bool, float> OnLotResolved;
    public event System.Action OnBiddingStarted;
    public event System.Action OnBiddingEnded;
    public event System.Action OnCallingOnce;
    public event System.Action OnCallingTwice;

    private ItemData currentItem;
    private float currentTrueValue;
    private float extendMultiplier;
    private readonly List<NPCBidder> npcBidders = new List<NPCBidder>();
    private float npcTickTimer;
    private int extendingBidCount;
    private bool calledOnce;
    private bool calledTwice;
    private float currentMinBidIncrement;
    private float currentMaxBidIncrement;

    public void StartLot(ItemData item)
    {
        CurrentState = LotState.Reveal;
        currentItem = item;
        currentPrice = item.basePrice * startingPriceRatio;
        currentHighBidder = null;
        extendingBidCount = 0;
        currentTrueValue = GetTrueValue(item);
        currentMinBidIncrement = BidIncrementCalculator.ComputeMinIncrement(item.basePrice);
        currentMaxBidIncrement = currentMinBidIncrement * maxIncrementMultiplier;
        extendMultiplier = item.mysticTraits.Contains(MysticTrait.SpiritBlessed) ? 2f : 1f;
        SpawnNpcBidders(item);
        Debug.Log($"[Lot] Reveal: {item.itemName}");
        StartCoroutine(RunLot());
    }

    private static readonly NPCPersonality[] Personalities =
    {
        NPCPersonality.Wizard,
        NPCPersonality.Alchemist,
        NPCPersonality.Noble,
        NPCPersonality.Blacksmith
    };

    private float GetTrueValue(ItemData item)
    {
        float flatTotal = 0f;
        float multiplierTotal = baseResaleMarkup;

        foreach (ItemMaterial material in item.materials)
        {
            multiplierTotal *= ItemMaterialInfo.GetMultiplier(material);
        }

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

    private float GetVisibleValue(ItemData item)
    {
        float multiplierTotal = baseResaleMarkup;

        foreach (ItemMaterial material in item.materials)
        {
            multiplierTotal *= ItemMaterialInfo.GetMultiplier(material);
        }

        return Mathf.Max(0f, item.basePrice * multiplierTotal);
    }

    private void SpawnNpcBidders(ItemData item)
    {
        npcBidders.Clear();
        npcTickTimer = 0f;

        float visibleValue = GetVisibleValue(item);

        for (int i = 0; i < npcBidderCount; i++)
        {
            NPCPersonality personality = Personalities[i % Personalities.Length];
            npcBidders.Add(new NPCBidder($"{personality} NPC", personality, visibleValue, item));
        }
    }

    private IEnumerator RunLot()
    {
        yield return new WaitForSeconds(revealDuration);

        CurrentState = LotState.Bidding;
        timeRemaining = biddingDuration;
        calledOnce = false;
        calledTwice = false;
        Debug.Log("[Lot] Bidding opened");
        OnBiddingStarted?.Invoke();
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
            OnBiddingEnded?.Invoke();
            ResolveLot();
            return;
        }

        if (!calledOnce && timeRemaining <= callingOnceThreshold)
        {
            calledOnce = true;
            OnCallingOnce?.Invoke();
        }

        if (!calledTwice && timeRemaining <= callingTwiceThreshold)
        {
            calledTwice = true;
            OnCallingTwice?.Invoke();
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
            if (currentHighBidder == npc.Name)
            {
                continue;
            }

            if (npc.WantsToBid(currentPrice, timeRemaining))
            {
                float increment = Random.Range(currentMinBidIncrement, currentMaxBidIncrement);
                PlaceBid(npc.Name, increment, extendsTimer: false);
                break;
            }
        }
    }

    public void PlaceBid(string bidderName, float incrementAmount, bool extendsTimer = true)
    {
        if (CurrentState != LotState.Bidding)
        {
            return;
        }

        if (bidderName == "Player" && currentItem.mysticTraits.Contains(MysticTrait.Cursed))
        {
            incrementAmount *= cursedBidIncrementMultiplier;
        }

        currentPrice += incrementAmount + dayBidIncrementSurcharge;
        currentHighBidder = bidderName;
        Debug.Log($"[Lot] {bidderName} bids +{incrementAmount:0} -> {currentPrice}");

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
        calledOnce = false;
        calledTwice = false;
        Debug.Log($"[Lot] Timer extended by {seconds}s -> {timeRemaining}s remaining");
    }
}
