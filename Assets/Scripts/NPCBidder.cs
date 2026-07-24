using UnityEngine;

public enum NPCPersonality
{
    Cautious,
    Aggressive,
    Reckless,
    Sniper,
    BudgetConscious
}

public class NPCBidder
{
    public string Name { get; }
    public NPCPersonality Personality { get; }
    public float Budget { get; }
    public float PerceivedValue { get; }

    private readonly float baseBidChance;
    private const float SnipeTimeThreshold = 4f;
    private const float SnipeBidChance = 0.85f;

    public NPCBidder(string name, NPCPersonality personality, float trueValue)
    {
        Name = name;
        Personality = personality;

        switch (personality)
        {
            case NPCPersonality.Cautious:
                // Accurate appraiser, rarely bids, won't chase past a slim margin over true value.
                PerceivedValue = trueValue + Random.Range(-10f, 10f);
                Budget = trueValue * 1.1f;
                baseBidChance = 0.15f;
                break;

            case NPCPersonality.Aggressive:
                // Bids often and is willing to pay well over true value.
                PerceivedValue = trueValue + Random.Range(-20f, 30f);
                Budget = trueValue * 1.6f;
                baseBidChance = 0.6f;
                break;

            case NPCPersonality.Reckless:
                // Bad appraiser - frequently overvalues the item, then backs it with a big budget.
                PerceivedValue = trueValue + Random.Range(-20f, 80f);
                Budget = trueValue * 1.8f;
                baseBidChance = 0.5f;
                break;

            case NPCPersonality.Sniper:
                // Sits out almost the whole auction, then jumps in hard once the clock is nearly out.
                PerceivedValue = trueValue + Random.Range(-15f, 25f);
                Budget = trueValue * 1.5f;
                baseBidChance = 0.05f;
                break;

            case NPCPersonality.BudgetConscious:
                // Appraises fairly well but is cash-strapped, so it drops out on budget, not valuation.
                PerceivedValue = trueValue + Random.Range(-10f, 40f);
                Budget = trueValue * 1.05f;
                baseBidChance = 0.35f;
                break;
        }
    }

    public bool WantsToBid(float currentPrice, float timeRemaining)
    {
        if (currentPrice >= PerceivedValue || currentPrice >= Budget)
        {
            return false;
        }

        float chance = baseBidChance;
        if (Personality == NPCPersonality.Sniper && timeRemaining <= SnipeTimeThreshold)
        {
            chance = SnipeBidChance;
        }

        return Random.value < chance;
    }
}
