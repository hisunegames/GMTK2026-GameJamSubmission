using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArcanaSurgeBoardPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private Image surgeImage;
    [SerializeField] private TMP_Text surgeNameText;
    [SerializeField] private TMP_Text surgeDescriptionText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float hiddenYOffset = 400f;

    private Vector2 shownPosition;
    private Coroutine slideRoutine;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (panel != null)
        {
            shownPosition = panel.anchoredPosition;
            panel.anchoredPosition = shownPosition + (Vector2.up * hiddenYOffset);
            panel.gameObject.SetActive(false);
        }
    }

    public void Show(ArcanaSurge.SurgeType surgeType)
    {
        if (panel == null)
        {
            Debug.LogWarning("[ArcanaSurgeBoardPanel] Panel RectTransform not assigned - skipping board and advancing to next week.");
            GameManager.Instance.AdvanceToNextWeek();
            return;
        }

        if (surgeNameText != null)
        {
            surgeNameText.text = ArcanaSurge.GetDisplayName(surgeType);
        }

        if (surgeDescriptionText != null)
        {
            surgeDescriptionText.text = ArcanaSurge.GetDescription(surgeType);
        }

        panel.gameObject.SetActive(true);
        panel.anchoredPosition = shownPosition + (Vector2.up * hiddenYOffset);

        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
        }

        slideRoutine = StartCoroutine(SlideTo(shownPosition));
    }

    private void OnContinueClicked()
    {
        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
        }

        slideRoutine = StartCoroutine(SlideOutAndAdvance());
    }

    private IEnumerator SlideOutAndAdvance()
    {
        yield return SlideTo(shownPosition + (Vector2.up * hiddenYOffset));
        panel.gameObject.SetActive(false);
        GameManager.Instance.AdvanceToNextWeek();
    }

    private IEnumerator SlideTo(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            panel.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        panel.anchoredPosition = target;
    }
}
