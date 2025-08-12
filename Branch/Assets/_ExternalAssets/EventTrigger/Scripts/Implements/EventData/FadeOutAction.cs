using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "FadeOutAction", menuName = "Events/Actions/Fade Out")]
public class FadeOutAction : EventData
{
    public float duration = 1.0f;

    public override void Execute(GameObject requester)
    {
        if (requester != null)
            requester.GetComponent<MonoBehaviour>().StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        // 이전과 동일하게 페이드용 UI를 동적으로 생성
        GameObject canvasGo = new GameObject("FadeCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        Image fadeImage = new GameObject("FadeImage").AddComponent<Image>();
        fadeImage.transform.SetParent(canvasGo.transform, false);
        fadeImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height); // 임시 크기 설정
        fadeImage.color = new Color(0, 0, 0, 0);

        // 페이드 아웃 로직
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, time / duration));
            yield return null;
        }
        // 페이드 UI를 다음 액션에서 재사용할 수 있도록 파괴하지 않음
    }
}