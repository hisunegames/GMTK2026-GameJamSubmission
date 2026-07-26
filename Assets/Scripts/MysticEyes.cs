using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class MysticEyes : MonoBehaviour
{
    [SerializeField] Sprite mysticeyeactivesprite;
    [SerializeField] Sprite mysticeyeunactivesprite;
    [SerializeField] GameObject Background;
    [SerializeField] GameObject Character;
    [SerializeField] Material invertMaterial;
    [SerializeField] Material normalMaterial;
    [SerializeField] Image aritfactImage;
    [SerializeField] Button mysticeyesButton;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float invertStrength = 0.4f;

    private static readonly int InvertStrengthId = Shader.PropertyToID("_InvertStrength");

    private bool mysticeyeisActive = false;
    private Coroutine fadeRoutine;
    private Image eyesprite;

    private void Awake()
    {
        eyesprite = this.GetComponent<Image>();
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationStarted += OnConversationStarted;
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.hasInstance)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
        }
    }

    private void OnConversationStarted(Transform actor)
    {
        if (!mysticeyeisActive)
        {
            return;
        }

        mysticeyeisActive = false;
        Sprite eyeimage = mysticeyeisActive ? mysticeyeactivesprite : mysticeyeunactivesprite;
        eyesprite.sprite = eyeimage;

        ItemData item = GameManager.Instance != null ? GameManager.Instance.LotController.CurrentItem : null;
        if (item != null && aritfactImage != null)
        {
            aritfactImage.sprite = item.sprite;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(AnimateFade(0f));
    }

    void Start()
    {
        mysticeyeisActive = false;
        Debug.Log($"[MysticEyes] Start - mysticeyesButton={(mysticeyesButton != null ? mysticeyesButton.name : "NULL")}, interactable={(mysticeyesButton != null ? mysticeyesButton.interactable.ToString() : "N/A")}");
        if (mysticeyesButton != null)
        {
            mysticeyesButton.onClick.AddListener(OnClickToggleEyes);
        }

        if (invertMaterial != null)
        {
            invertMaterial.SetFloat(InvertStrengthId, 0f);
        }

        ApplyMaterial(invertMaterial);
    }

    private void ApplyMaterial(Material material)
    {
        Debug.Log($"[MysticEyes] ApplyMaterial - material={(material != null ? material.name : "NULL")}, Background={(Background != null ? "OK" : "NULL")}, Character={(Character != null ? "OK" : "NULL")}");

        if (material == null || Background == null || Character == null)
        {
            return;
        }

        SpriteRenderer[] bgRenderers = Background.GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer[] charRenderers = Character.GetComponentsInChildren<SpriteRenderer>(true);
        Debug.Log($"[MysticEyes] ApplyMaterial - found {bgRenderers.Length} background renderers, {charRenderers.Length} character renderers");

        foreach (SpriteRenderer bgSprite in bgRenderers)
        {
            bgSprite.material = material;
        }

        foreach (SpriteRenderer charSprite in charRenderers)
        {
            charSprite.material = material;
        }
    }

    public void OnClickToggleEyes()
    {
        ItemData item = GameManager.Instance != null ? GameManager.Instance.LotController.CurrentItem : null;
        if (item == null)
        {
            Debug.Log("Item is null");
            return;
        }

        mysticeyeisActive = !mysticeyeisActive;

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayMysticEye();
        }

        Sprite eyeimage = mysticeyeisActive ? mysticeyeactivesprite : mysticeyeunactivesprite;
        eyesprite.sprite = eyeimage;
        Sprite itemSprite = mysticeyeisActive && item.mysticTraitSprite != null ? item.mysticTraitSprite : item.sprite;
        if (item.mysticTraitSprite != null)
        {
            aritfactImage.sprite = itemSprite;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        float targetStrength = mysticeyeisActive ? invertStrength : 0f;
        Debug.Log($"[MysticEyes] OnClickToggleEyes - active={mysticeyeisActive}, hasMysticTraitSprite={item.mysticTraitSprite != null}, targetStrength={targetStrength}");
        fadeRoutine = StartCoroutine(AnimateFade(targetStrength));
    }

    private IEnumerator AnimateFade(float targetStrength)
    {
        if (invertMaterial == null)
        {
            yield break;
        }

        float startStrength = invertMaterial.GetFloat(InvertStrengthId);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            invertMaterial.SetFloat(InvertStrengthId, Mathf.Lerp(startStrength, targetStrength, t));
            yield return null;
        }

        invertMaterial.SetFloat(InvertStrengthId, targetStrength);
    }
}
