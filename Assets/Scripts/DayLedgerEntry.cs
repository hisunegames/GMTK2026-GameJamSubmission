public struct DayLedgerEntry
{
    public string ItemName;
    public bool PlayerWon;
    public float Profit;

    public DayLedgerEntry(string itemName, bool playerWon, float profit)
    {
        ItemName = itemName;
        PlayerWon = playerWon;
        Profit = profit;
    }
}
