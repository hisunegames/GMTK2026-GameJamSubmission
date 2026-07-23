using UnityEngine;

[RequireComponent(typeof(LotController))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, DayIntro, LotLoop, DayResults, NextDay, GameOver }

    public GameState CurrentState { get; private set; }

    public int currentDay = 1;
    public float cash;
    public float quotaTarget;

    [SerializeField] private LotController lotController;
    public LotController LotController => lotController;

    [Header("Temporary test hook - remove once LotLoop drives this for real")]
    [SerializeField] private ItemData testItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (lotController == null)
        {
            lotController = GetComponent<LotController>();
        }
    }

    private void Start()
    {
        SetState(GameState.Menu);

        if (testItem != null)
        {
            lotController.StartLot(testItem);
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Entered state: {newState}");

        switch (newState)
        {
            case GameState.Menu:
                break;
            case GameState.DayIntro:
                break;
            case GameState.LotLoop:
                break;
            case GameState.DayResults:
                break;
            case GameState.NextDay:
                currentDay++;
                SetState(GameState.DayIntro);
                break;
            case GameState.GameOver:
                break;
        }
    }
}
