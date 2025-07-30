using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : ManagerBase<GUIManager>
{
    [Header("Image Settings")]
    [SerializeField] private Image crosshead;                   // 조준점 이미지         
    [SerializeField] private Image fadeInOutPanel;              // 카메라 연출 이미지
    
    private Coroutine _runningFadeCoroutine;                    // 카메라 연출 코루틴 저장

    public void ToggleCrosshead()
    {
        // crosshead.SetActive(!crosshead.activeSelf);
        crosshead.enabled = !crosshead.enabled;
    }

    #region Fade In/Out

    private IEnumerator CoroutineFadeInOut(float startAlpha, float targetAlpha, float duration = 1f)
    {
        var timer = 0f;
        var panelColor = fadeInOutPanel.color;

        if (duration <= 0f)
        {
            panelColor.a = targetAlpha;
            fadeInOutPanel.color = panelColor;
            yield break;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            var progress = timer / duration;
            panelColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadeInOutPanel.color = panelColor;
            yield return null;
        }
        
        panelColor.a = targetAlpha;
        fadeInOutPanel.color = panelColor;
    }

    public void FadeIn(float duration = 1.0f)
    {
        if (fadeInOutPanel.color.a > 0)
        {
            if (_runningFadeCoroutine != null)
                StopCoroutine(_runningFadeCoroutine);
            _runningFadeCoroutine = StartCoroutine(CoroutineFadeInOut(1, 0, duration));
        }
        else
        {
            Debug.Log("Fade In Complete");
        }
    }

    public void FadeOut(float duration = 1.0f)
    {
        if (fadeInOutPanel.color.a < 1)
        {
            if (_runningFadeCoroutine != null)
                StopCoroutine(_runningFadeCoroutine);
            _runningFadeCoroutine = StartCoroutine(CoroutineFadeInOut(0, 1, duration));
        }
        else
        {
            Debug.Log("Fade In Complete");
        }
    }

    public void FadeTo(float targetAlpha, float duration = 1.0f)
    {
        if (_runningFadeCoroutine != null)
            StopCoroutine(_runningFadeCoroutine);
        _runningFadeCoroutine = StartCoroutine(CoroutineFadeInOut(fadeInOutPanel.color.a, targetAlpha, duration));
    }

    #endregion
}
