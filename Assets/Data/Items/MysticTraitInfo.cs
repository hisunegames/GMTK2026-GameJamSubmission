public static class MysticTraitInfo
{
    public static ValueModifier GetModifier(MysticTrait trait)
    {
        switch (trait)
        {
            case MysticTrait.Cursed:
                return new ValueModifier(0f, 0f);
            case MysticTrait.Holy:
                return new ValueModifier(0f, 3f);
            case MysticTrait.SpiritBlessed:
                return new ValueModifier(0f, 1.5f);
            default:
                return new ValueModifier(0f, 1f);
        }
    }

    public static string GetDescription(MysticTrait trait)
    {
        switch (trait)
        {
            case MysticTrait.Cursed:
                return "Purple ugly aura (x0), increases fixing bid the next day (x1 to x3)";
            case MysticTrait.Holy:
                return "Rainbow aura (x3)";
            case MysticTrait.SpiritBlessed:
                return "Yellowish aura (x1.5), slows time in the current auction";
            default:
                return string.Empty;
        }
    }
}
