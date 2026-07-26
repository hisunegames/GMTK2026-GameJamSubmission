using UnityEngine;

public static class ItemDefectInfo
{
    private static readonly float[] ScratchedTiers = { -10f, -20f, -30f };
    private static readonly float[] BrokenTiers = { -30f, -50f };

    public static ValueModifier GetModifier(ItemDefect defect)
    {
        switch (defect)
        {
            case ItemDefect.Scratched:
                return new ValueModifier(ScratchedTiers[Random.Range(0, ScratchedTiers.Length)], 1f);
            case ItemDefect.Broken:
                return new ValueModifier(BrokenTiers[Random.Range(0, BrokenTiers.Length)], 1f);
            case ItemDefect.Fake:
                return new ValueModifier(0f, 0f);
            case ItemDefect.Pristine:
                return new ValueModifier(0f, 1.2f);
            default:
                return new ValueModifier(0f, 1f);
        }
    }

    public static ValueModifier GetEstimatedModifier(ItemDefect defect)
    {
        switch (defect)
        {
            case ItemDefect.Scratched:
                return new ValueModifier(Average(ScratchedTiers), 1f);
            case ItemDefect.Broken:
                return new ValueModifier(Average(BrokenTiers), 1f);
            default:
                return GetModifier(defect);
        }
    }

    private static float Average(float[] values)
    {
        float total = 0f;
        foreach (float value in values)
        {
            total += value;
        }

        return total / values.Length;
    }

    public static string GetDescription(ItemDefect defect)
    {
        switch (defect)
        {
            case ItemDefect.Scratched:
                return "Visible signs of scratching and cracking (-10, -20, -30)";
            case ItemDefect.Broken:
                return "Holes and broken parts shown (-30, -50)";
            case ItemDefect.Fake:
                return "When inspected, the item seemed to change (x0)";
            case ItemDefect.Pristine:
                return "White sparkle aura (x1.2)";
            default:
                return string.Empty;
        }
    }
}
