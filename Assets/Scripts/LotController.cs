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
    [SerializeField] private float playerBidCooldownDuration = 3f;
    [SerializeField] private float lateSnipeThreshold = 3f;
    [SerializeField] private float npcBidCutoffThreshold = 2.5f;
    [SerializeField] private float dailyBudgetMultiplier = 2.5f;

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
    private float playerBidCooldownRemaining;

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
        PrepareNpcBiddersForItem(item);
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

    public void InitializeDailyNpcBidders(IList<ItemData> dayItems)
    {
        npcBidders.Clear();

        float totalVisibleValue = 0f;
        foreach (ItemData item in dayItems)
        {
            totalVisibleValue += GetVisibleValue(item);
        }

        float avgVisibleValue = dayItems.Count > 0 ? totalVisibleValue / dayItems.Count : 0f;

        for (int i = 0; i < npcBidderCount; i++)
        {
            NPCPersonality personality = Personalities[i % Personalities.Length];
            NPCBidder npc = new($"{personality} NPC", personality);

            int preferredCount = 0;
            foreach (ItemData item in dayItems)
            {
                if (npc.MatchesPreference(item))
                {
                    preferredCount++;
                }
            }

            npc.SetPreferredItemCountToday(preferredCount);
            npc.SetDailyBudget(avgVisibleValue * dailyBudgetMultiplier);
            npcBidders.Add(npc);
        }
    }

    private void PrepareNpcBiddersForItem(ItemData item)
    {
        npcTickTimer = 0f;
        float visibleValue = GetVisibleValue(item);

        foreach (NPCBidder npc in npcBidders)
        {
            npc.PrepareForItem(visibleValue, item);
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

        if (playerBidCooldownRemaining > 0f)
        {
            playerBidCooldownRemaining -= Time.deltaTime;
        }

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
        else if (!string.IsNullOrEmpty(currentHighBidder))
        {
            NPCBidder winner = npcBidders.Find(npc => npc.Name == currentHighBidder);
            winner?.OnWonBid(currentPrice);
        }

        OnLotResolved?.Invoke(currentItem, playerWon, profit);
    }

    private void TickNpcBidders()
    {
        if (playerBidCooldownRemaining > 0f)
        {
            return;
        }

        if (timeRemaining < npcBidCutoffThreshold)
        {
            return;
        }

        if (timeRemaining <= lateSnipeThreshold)
        {
            TickLateSnipe();
            return;
        }

        foreach (NPCBidder npc in npcBidders)
        {
            if (npc.WantsToBid(currentPrice, timeRemaining, currentHighBidder))
            {
                float increment = Random.Range(currentMinBidIncrement, currentMaxBidIncrement);
                PlaceBid(npc.Name, increment, extendsTimer: false);
                break;
            }
        }
    }

    private void TickLateSnipe()
    {
        List<NPCBidder> eligible = new();
        foreach (NPCBidder npc in npcBidders)
        {
            if (npc.PrefersItem && currentHighBidder != npc.Name)
            {
                eligible.Add(npc);
            }
        }

        if (eligible.Count == 0)
        {
            return;
        }

        NPCBidder chosen = eligible[Random.Range(0, eligible.Count)];
        if (chosen.WantsToBid(currentPrice, timeRemaining, currentHighBidder))
        {
            float increment = Random.Range(currentMinBidIncrement, currentMaxBidIncrement);
            PlaceBid(chosen.Name, increment, extendsTimer: false);
        }
    }

    public void PlaceBid(string bidderName, float incrementAmount, bool extendsTimer = true)
    {
        if (CurrentState != LotState.Bidding)
        {
            return;
        }

        if (bidderName == "Player")
        {
            if (currentItem.mysticTraits.Contains(MysticTrait.Cursed))
            {
                incrementAmount *= cursedBidIncrementMultiplier;
            }

            playerBidCooldownRemaining = playerBidCooldownDuration;
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
