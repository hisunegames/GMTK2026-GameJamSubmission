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
    public float Budget { get; }
    public float PerceivedValue { get; }

    private readonly float baseBidChance;
    private bool canBidLast5s;
    private const float SnipeTimeThreshold = 4f;
    private const float SnipeBidChance = 0.85f;

    public NPCBidder(string name, NPCPersonality personality, float trueValue , ItemData item)
    {
        Name = name;
        Personality = personality;
        canBidLast5s = false;

        switch (personality)
        {
            case NPCPersonality.Wizard:
                PerceivedValue = trueValue + Random.Range(-10f, 10f);
                Budget = trueValue * 1.1f;
                baseBidChance = 0.3f;
                if (item.materials.Contains(ItemMaterial.Ceramic) || item.materials.Contains(ItemMaterial.Gemstone))
                {
                    PerceivedValue *= 1.5f;
                    Budget *= 1.5f;
                    baseBidChance = 0.65f;
                    canBidLast5s = true;
                }
                break;

            case NPCPersonality.Alchemist:
                PerceivedValue = trueValue + Random.Range(-20f, 30f);
                Budget = trueValue * 1.4f;
                baseBidChance = 0.5f;
                if (item.materials.Contains(ItemMaterial.Gold) || item.materials.Contains(ItemMaterial.Silver) || item.materials.Contains(ItemMaterial.MonsterPart) || item.materials.Contains(ItemMaterial.Bronze))
                {
                    PerceivedValue *= 1.6f;
                    Budget *= 1.5f;
                    baseBidChance = 0.7f;
                    canBidLast5s = true;
                }
                break;

            case NPCPersonality.Jewelry:
                PerceivedValue = trueValue + Random.Range(-20f, 50f);
                Budget = trueValue * 1.5f;
                baseBidChance = 0.55f;
                if (item.materials.Contains(ItemMaterial.Gemstone))
                {
                    PerceivedValue *= 1.8f;
                    Budget *= 1.8f;
                    baseBidChance = 0.75f;
                    canBidLast5s = true;
                }
                break;

            case NPCPersonality.Blacksmith:
                PerceivedValue = trueValue + Random.Range(-15f, 25f);
                Budget = trueValue * 1.3f;
                baseBidChance = 0.45f;
                if (item.materials.Contains(ItemMaterial.Bronze) || item.materials.Contains(ItemMaterial.Metal) || item.materials.Contains(ItemMaterial.Silver))
                {
                    PerceivedValue *= 1.5f;
                    Budget *= 1.5f;
                    baseBidChance = 0.6f;
                    canBidLast5s = true;
                }
                break;

            case NPCPersonality.Noble:
                PerceivedValue = trueValue + Random.Range(-5f, 80f);
                Budget = trueValue * 1.8f;
                baseBidChance = 0.6f;
                if (item.materials.Contains(ItemMaterial.Gold) || item.materials.Contains(ItemMaterial.MonsterPart) || item.materials.Contains(ItemMaterial.Gemstone))
                {
                    PerceivedValue *= 2f;
                    Budget *= 2f;
                    baseBidChance = 0.85f;
                    canBidLast5s = true;
                }
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

        if (canBidLast5s && timeRemaining <= 5f)
        {
            chance = 1f;
            canBidLast5s = false;
        }

        return Random.value < chance;
    }
}
