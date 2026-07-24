public readonly struct ValueModifier
{
    public readonly float FlatAdjustment;
    public readonly float Multiplier;

    public ValueModifier(float flatAdjustment, float multiplier)
    {
        FlatAdjustment = flatAdjustment;
        Multiplier = multiplier;
    }
}
