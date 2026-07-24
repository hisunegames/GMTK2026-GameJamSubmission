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
                return "A malevolent curse - unsellable, and buying it burdens every item that follows it today.";
            case MysticTrait.Holy:
                return "A holy blessing - commands an exceptional price.";
            case MysticTrait.SpiritBlessed:
                return "Favored by spirits - valuable, and its bidding clock runs slower.";
            default:
                return string.Empty;
        }
    }
}
