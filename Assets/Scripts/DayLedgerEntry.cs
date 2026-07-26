using System.Collections.Generic;

public struct DayLedgerEntry
{
    public string ItemName;
    public bool PlayerWon;
    public float Profit;
    public List<ItemDefect> Defects;
    public List<MysticTrait> MysticTraits;

    public DayLedgerEntry(string itemName, bool playerWon, float profit, List<ItemDefect> defects, List<MysticTrait> mysticTraits)
    {
        ItemName = itemName;
        PlayerWon = playerWon;
        Profit = profit;
        Defects = defects;
        MysticTraits = mysticTraits;
    }
}
