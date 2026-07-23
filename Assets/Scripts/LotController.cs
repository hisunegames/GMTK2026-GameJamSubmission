using System.Collections;
using UnityEngine;

public class LotController : MonoBehaviour
{
    public enum LotState { Reveal, Bidding, Resolve }

    public LotState CurrentState { get; private set; }

    [SerializeField] private float revealDuration = 1f;
    [SerializeField] private float biddingDuration = 15f;
    [SerializeField] private float bidIncrement = 10f;
    [SerializeField] private float bidTimeExtension = 3f;

    public float currentPrice;
    public float timeRemaining;
    public string currentHighBidder;

    public void StartLot(ItemData item)
    {
        CurrentState = LotState.Reveal;
        currentPrice = item.basePrice;
        currentHighBidder = null;
        Debug.Log($"[Lot] Reveal: {item.itemName}");
        StartCoroutine(RunLot());
    }

    private IEnumerator RunLot()
    {
        yield return new WaitForSeconds(revealDuration);

        CurrentState = LotState.Bidding;
        timeRemaining = biddingDuration;
        Debug.Log("[Lot] Bidding opened");
    }

    private void Update()
    {
        if (CurrentState != LotState.Bidding)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            CurrentState = LotState.Resolve;
            Debug.Log($"[Lot] Resolve - winner: {currentHighBidder ?? "no one"} at {currentPrice}");
        }
    }

    public void PlaceBid(string bidderName)
    {
        if (CurrentState != LotState.Bidding)
        {
            return;
        }

        currentPrice += bidIncrement;
        currentHighBidder = bidderName;
        Debug.Log($"[Lot] {bidderName} bids -> {currentPrice}");
        ExtendTimer(bidTimeExtension);
    }

    public void ExtendTimer(float seconds)
    {
        timeRemaining += seconds;
        Debug.Log($"[Lot] Timer extended by {seconds}s -> {timeRemaining}s remaining");
    }
}
