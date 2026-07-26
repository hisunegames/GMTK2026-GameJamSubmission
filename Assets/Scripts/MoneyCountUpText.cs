using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyCountUpText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float duration = 1f;

    private Coroutine countRoutine;

    public void CountTo(float targetValue, Func<float, string> formatter)
    {
        if (countRoutine != null)
        {
            StopCoroutine(countRoutine);
        }

        countRoutine = StartCoroutine(CountRoutine(targetValue, formatter));
    }

    private IEnumerator CountRoutine(float targetValue, Func<float, string> formatter)
    {
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.StartCoinCollectLoop();
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(0f, targetValue, t);

            if (text != null)
            {
                text.text = formatter(value);
            }

            yield return null;
        }

        if (text != null)
        {
            text.text = formatter(targetValue);
        }

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.StopCoinCollectLoop();
            SfxManager.Instance.PlayCoinFinalize();
        }
    }
}
