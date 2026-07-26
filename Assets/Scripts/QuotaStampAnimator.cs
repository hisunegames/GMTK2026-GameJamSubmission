using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuotaStampAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform stamper;
    [SerializeField] private Image stampImage;
    [SerializeField] private Sprite metStampSprite;
    [SerializeField] private Sprite notMetStampSprite;
    [SerializeField] private float raisedOffset = 300f;
    [SerializeField] private float delayBeforeStamp = 0.3f;
    [SerializeField] private float slamDuration = 0.12f;
    [SerializeField] private float retractDelay = 0.2f;
    [SerializeField] private float retractDuration = 0.25f;

    private Vector2 restingPosition;
    private Coroutine stampRoutine;

    private void Awake()
    {
        if (stamper != null)
        {
            restingPosition = stamper.anchoredPosition;
        }

        if (stampImage != null)
        {
            stampImage.enabled = false;
        }
    }

    public void PlayStamp(bool metQuota)
    {
        if (stampRoutine != null)
        {
            StopCoroutine(stampRoutine);
        }

        stampRoutine = StartCoroutine(StampSequence(metQuota));
    }

    private IEnumerator StampSequence(bool metQuota)
    {
        if (stampImage != null)
        {
            stampImage.enabled = false;
        }

        if (stamper == null)
        {
            if (stampImage != null)
            {
                stampImage.sprite = metQuota ? metStampSprite : notMetStampSprite;
                stampImage.enabled = true;
            }

            yield break;
        }

        Vector2 raisedPosition = restingPosition + (Vector2.up * raisedOffset);

        stamper.gameObject.SetActive(true);
        stamper.anchoredPosition = raisedPosition;

        yield return new WaitForSeconds(delayBeforeStamp);
        yield return Slide(stamper, raisedPosition, restingPosition, slamDuration);

        if (stampImage != null)
        {
            stampImage.sprite = metQuota ? metStampSprite : notMetStampSprite;
            stampImage.enabled = true;
        }

        yield return new WaitForSeconds(retractDelay);
        yield return Slide(stamper, restingPosition, raisedPosition, retractDuration);

        stamper.gameObject.SetActive(false);
    }

    private IEnumerator Slide(RectTransform target, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        target.anchoredPosition = to;
    }
}
