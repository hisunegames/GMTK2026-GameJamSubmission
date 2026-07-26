using System.Collections.Generic;
using UnityEngine;

public enum NPCPersonality
{
    Wizard,
    Jewelry,
    Alchemist,
    Noble,
    Blacksmith
}

public class NPCBidder
{
    public string Name { get; }
    public NPCPersonality Personality { get; }
    public float Budget { get; private set; }
    public float PerceivedValue { get; private set; }
    public float RemainingBudget { get; private set; }
    public bool PrefersItem => prefersItem;

    private float baseBidChance;
    private bool prefersItem;
    private int preferredItemCountToday;

    private const float SnipeWindowStart = 5f;
    private const float SnipeBidChance = 0.85f;

    // Applied when this item doesn't match the NPC's preference but at least one
    // of today's other lots does - conserves budget for the items it actually wants.
    private const float ConservationChanceMultiplier = 0.4f;

    private static readonly Dictionary<NPCPersonality, ItemMaterial[]> PreferredMaterials = new()
    {
        { NPCPersonality.Wizard, new[] { ItemMaterial.Ceramic, ItemMaterial.Gemstone } },
        { NPCPersonality.Alchemist, new[] { ItemMaterial.Gold, ItemMaterial.Silver, ItemMaterial.MonsterPart, ItemMaterial.Bronze } },
        { NPCPersonality.Jewelry, new[] { ItemMaterial.Gemstone } },
        { NPCPersonality.Blacksmith, new[] { ItemMaterial.Bronze, ItemMaterial.Metal, ItemMaterial.Silver } },
        { NPCPersonality.Noble, new[] { ItemMaterial.Gold, ItemMaterial.MonsterPart, ItemMaterial.Gemstone } },
    };

    public NPCBidder(string name, NPCPersonality personality)
    {
        Name = name;
        Personality = personality;
    }

    public bool MatchesPreference(ItemData item)
    {
        foreach (ItemMaterial material in PreferredMaterials[Personality])
        {
            if (item.materials.Contains(material))
            {
                return true;
            }
        }

        return false;
    }

    public void SetDailyBudget(float amount)
    {
        RemainingBudget = amount;
    }

    public void SetPreferredItemCountToday(int count)
    {
        preferredItemCountToday = count;
    }

    public void OnWonBid(float amountSpent)
    {
        RemainingBudget = Mathf.Max(0f, RemainingBudget - amountSpent);
    }

    public void PrepareForItem(float trueValue, ItemData item)
    {
        prefersItem = MatchesPreference(item);

        switch (Personality)
        {
            case NPCPersonality.Wizard:
                PerceivedValue = trueValue + Random.Range(-10f, 10f);
                Budget = trueValue * 1.1f;
                baseBidChance = 0.2f;
                if (prefersItem)
                {
                    PerceivedValue *= 1.5f;
                    Budget *= 1.5f;
                    baseBidChance = 0.5f;
                }
                break;

            case NPCPersonality.Alchemist:
                PerceivedValue = trueValue + Random.Range(-20f, 30f);
                Budget = trueValue * 1.4f;
                baseBidChance = 0.35f;
                if (prefersItem)
                {
                    PerceivedValue *= 1.6f;
                    Budget *= 1.5f;
                    baseBidChance = 0.55f;
                }
                break;

            case NPCPersonality.Jewelry:
                PerceivedValue = trueValue + Random.Range(-20f, 50f);
                Budget = trueValue * 1.5f;
                baseBidChance = 0.4f;
                if (prefersItem)
                {
                    PerceivedValue *= 1.8f;
                    Budget *= 1.8f;
                    baseBidChance = 0.6f;
                }
                break;

            case NPCPersonality.Blacksmith:
                PerceivedValue = trueValue + Random.Range(-15f, 25f);
                Budget = trueValue * 1.3f;
                baseBidChance = 0.3f;
                if (prefersItem)
                {
                    PerceivedValue *= 1.5f;
                    Budget *= 1.5f;
                    baseBidChance = 0.45f;
                }
                break;

            case NPCPersonality.Noble:
                PerceivedValue = trueValue + Random.Range(-5f, 80f);
                Budget = trueValue * 1.8f;
                baseBidChance = 0.4f;
                if (prefersItem)
                {
                    PerceivedValue *= 2f;
                    Budget *= 2f;
                    baseBidChance = 0.6f;
                }
                break;
        }

        if (!prefersItem && preferredItemCountToday > 0)
        {
            baseBidChance *= ConservationChanceMultiplier;
        }
    }

    public bool WantsToBid(float currentPrice, float timeRemaining , string currentHighBidder)
    {
        if (currentPrice >= PerceivedValue || currentPrice >= Budget || currentPrice >= RemainingBudget || currentHighBidder == Name)
        {
            return false;
        }

        float chance = baseBidChance;

        if (prefersItem && timeRemaining <= SnipeWindowStart)
        {
            chance = SnipeBidChance;
        }

        return Random.value < chance;
    }
}
