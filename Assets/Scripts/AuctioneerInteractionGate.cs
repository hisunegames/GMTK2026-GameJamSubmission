using PixelCrushers.DialogueSystem;
using UnityEngine;

public class AuctioneerInteractionGate : MonoBehaviour
{
    [SerializeField] private Usable usable;

    private void Start()
    {
        Debug.Log($"[AuctioneerGate] Start - usable={(usable != null ? "OK" : "NULL")}, GameManager.Instance={(GameManager.Instance != null ? "OK" : "NULL")}");

        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        bool shouldBeUsable = state == GameManager.GameState.LotLoop;
        Debug.Log($"[AuctioneerGate] HandleStateChanged - state={state}, setting usable.enabled={shouldBeUsable}");

        if (usable != null)
        {
            usable.enabled = shouldBeUsable;
        }
    }
}
