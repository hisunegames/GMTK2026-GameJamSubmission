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

    public static string GetDescription(ItemDefect defect)
    {
        switch (defect)
        {
            case ItemDefect.Scratched:
                return "Visible scratches - a value hit that scales with how bad they are.";
            case ItemDefect.Broken:
                return "A break or structural damage - a heavy value hit.";
            case ItemDefect.Fake:
                return "Not the genuine article - worthless to a real buyer.";
            case ItemDefect.Pristine:
                return "Flawless condition - commands a premium.";
            default:
                return string.Empty;
        }
    }
}
