using UnityEngine;

public static class BidIncrementCalculator
{
    private const float Ratio = 0.05f;
    private const float FloorStep = 10f;
    private const float MinimumIncrement = 10f;

    public static float ComputeMinIncrement(float basePrice)
    {
        float raw = basePrice * Ratio;
        float floored = Mathf.Floor(raw / FloorStep) * FloorStep;
        return Mathf.Max(MinimumIncrement, floored);
    }
}
