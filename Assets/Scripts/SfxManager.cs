using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [SerializeField] private AudioClip hammerBid;
    [SerializeField] private AudioClip coinFinalize;
    [SerializeField] private AudioClip coinCollect;
    [SerializeField] private AudioClip scrollOpen;
    [SerializeField] private AudioClip outbid;
    [SerializeField] private AudioClip mysticEye;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip[] dialogueVariants;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private AudioSource oneShotSource;
    private AudioSource coinCollectLoopSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;

        coinCollectLoopSource = gameObject.AddComponent<AudioSource>();
        coinCollectLoopSource.playOnAwake = false;
        coinCollectLoopSource.loop = true;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
        {
            oneShotSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayHammerBid() => PlayOneShot(hammerBid);
    public void PlayCoinFinalize() => PlayOneShot(coinFinalize);
    public void PlayScrollOpen() => PlayOneShot(scrollOpen);
    public void PlayOutbid() => PlayOneShot(outbid);
    public void PlayMysticEye() => PlayOneShot(mysticEye);
    public void PlayGameOver() => PlayOneShot(gameOver);
    public void PlayButtonClick() => PlayOneShot(buttonClick);

    public void PlayDialogueVariant()
    {
        if (dialogueVariants != null && dialogueVariants.Length > 0)
        {
            PlayOneShot(dialogueVariants[Random.Range(0, dialogueVariants.Length)]);
        }
    }

    public void StartCoinCollectLoop()
    {
        if (coinCollect == null)
        {
            return;
        }

        coinCollectLoopSource.clip = coinCollect;
        coinCollectLoopSource.volume = sfxVolume;
        coinCollectLoopSource.Play();
    }

    public void StopCoinCollectLoop()
    {
        coinCollectLoopSource.Stop();
    }
}
