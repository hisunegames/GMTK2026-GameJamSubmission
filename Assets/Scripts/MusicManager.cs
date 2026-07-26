using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip conversationMusic;
    [SerializeField] private AudioClip biddingMusic;
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private Coroutine fadeRoutine;

    private bool isConversationActive;
    private bool isBiddingActive;
    private AudioClip currentClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        activeSource = gameObject.AddComponent<AudioSource>();
        inactiveSource = gameObject.AddComponent<AudioSource>();
        ConfigureSource(activeSource);
        ConfigureSource(inactiveSource);
    }

    private void ConfigureSource(AudioSource source)
    {
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    private void Start()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationStarted += OnConversationStarted;
            DialogueManager.instance.conversationEnded += OnConversationEnded;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;

            if (GameManager.Instance.LotController != null)
            {
                GameManager.Instance.LotController.OnBiddingStarted += HandleBiddingStarted;
                GameManager.Instance.LotController.OnBiddingEnded += HandleBiddingEnded;
            }
        }

        RefreshTrack();
    }

    private void OnDestroy()
    {
        if (DialogueManager.hasInstance)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
            DialogueManager.instance.conversationEnded -= OnConversationEnded;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;

            if (GameManager.Instance.LotController != null)
            {
                GameManager.Instance.LotController.OnBiddingStarted -= HandleBiddingStarted;
                GameManager.Instance.LotController.OnBiddingEnded -= HandleBiddingEnded;
            }
        }
    }

    private void HandleBiddingStarted()
    {
        isBiddingActive = true;
        RefreshTrack();
    }

    private void HandleBiddingEnded()
    {
        isBiddingActive = false;
        RefreshTrack();
    }

    private void OnConversationStarted(Transform actor)
    {
        isConversationActive = true;
        RefreshTrack();
    }

    private void OnConversationEnded(Transform actor)
    {
        isConversationActive = false;
        RefreshTrack();
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        RefreshTrack();
    }

    private void RefreshTrack()
    {
        AudioClip target = DetermineTrack();

        if (target == currentClip)
        {
            return;
        }

        currentClip = target;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(CrossfadeTo(target));
    }

    private AudioClip DetermineTrack()
    {
        if (isConversationActive && conversationMusic != null)
        {
            return conversationMusic;
        }

        if (isBiddingActive && biddingMusic != null)
        {
            return biddingMusic;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Menu)
        {
            return menuMusic;
        }

        return ambientMusic;
    }

    private IEnumerator CrossfadeTo(AudioClip clip)
    {
        AudioSource incoming = inactiveSource;
        AudioSource outgoing = activeSource;

        incoming.clip = clip;
        incoming.volume = 0f;

        if (clip != null)
        {
            incoming.Play();
        }

        float elapsed = 0f;
        float outgoingStartVolume = outgoing.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            incoming.volume = Mathf.Lerp(0f, musicVolume, t);
            outgoing.volume = Mathf.Lerp(outgoingStartVolume, 0f, t);
            yield return null;
        }

        incoming.volume = clip != null ? musicVolume : 0f;
        outgoing.volume = 0f;
        outgoing.Stop();

        activeSource = incoming;
        inactiveSource = outgoing;
    }
}
