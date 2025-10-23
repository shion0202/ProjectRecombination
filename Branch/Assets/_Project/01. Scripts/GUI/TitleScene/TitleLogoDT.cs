using _Project.Scripts.GUI.TitleScene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleLogoDT : MonoBehaviour
{
    [Header("Title Logo")]
    public Image titleLogoImage;
    public RectTransform titleLogoRectTransform;
    
    public float animationDuration = 1.5f;
    public float startPositionX = -1200f;
    private float _endPositionX = 0f;
    
    public DTBase titleLogoIdleAnimation;
    
    [Header("Title Char")]
    public Image titleCharImage;
    public RectTransform titleCharRectTransform;
    
    public float animationDurationChar = 1.5f;
    public float startPositionXChar = 1200f;
    private float _endPositionXChar;
    
    public DTBase titleCharIdleAnimation;

    private void Awake()
    {
        DOTween.Init();

        if (titleLogoImage is null) return;
        if (titleLogoRectTransform is null) return;
        
        if (titleCharImage is null) return;
        if (titleCharRectTransform is null) return;
        
        _endPositionX = titleLogoRectTransform.anchoredPosition.x;
        _endPositionXChar = titleCharRectTransform.anchoredPosition.x;
    }
    void Start()
    {
        // 타이틀 로고 설정
        titleLogoImage.color = new Color(titleLogoImage.color.r, titleLogoImage.color.g, titleLogoImage.color.b, 0f);
        
        titleLogoRectTransform.anchoredPosition = new Vector2(startPositionX, titleLogoRectTransform.anchoredPosition.y);

        StartCoroutine(AnimationTitleLogo());
        
        // 타이틀 일러스트 설정
        titleCharImage.color = new Color(titleCharImage.color.r, titleCharImage.color.g, titleCharImage.color.b, 0f);
        titleCharRectTransform.anchoredPosition = new Vector2(startPositionXChar, titleCharRectTransform.anchoredPosition.y);

        StartCoroutine(AnimationTitleChar());
    }

    private IEnumerator AnimationTitleChar()
    {
        // --- 애니메이션 A: 화면 중앙으로 이동 ---
        // DOAnchorPos: RectTransform의 anchoredPosition 값을 변경합니다.
        titleCharRectTransform.DOAnchorPos(new Vector2(_endPositionXChar, titleCharRectTransform.anchoredPosition.y), animationDurationChar)
            .SetEase(Ease.OutExpo); // Ease 효과: 부드럽게 감속하며 도착 (OutExpo가 역동적)

        // --- 애니메이션 B: 서서히 나타나기 (Fade In) ---
        // DOFade: Image의 알파(color.a) 값을 변경합니다.
        titleCharImage.DOFade(1.0f, animationDurationChar) // 1.0f (불투명) 상태로 변경
            .SetEase(Ease.InSine); // Ease 효과: 서서히 나타나기 시작

        // titleCharIdleAnimation.isStarted = true;
        yield return new WaitForSeconds(animationDuration);
        titleCharIdleAnimation.AnimaStart();
    }

    private IEnumerator AnimationTitleLogo()
    {
        // --- 애니메이션 A: 화면 중앙으로 이동 ---
        // DOAnchorPos: RectTransform의 anchoredPosition 값을 변경합니다.
        titleLogoRectTransform.DOAnchorPos(new Vector2(_endPositionX, titleLogoRectTransform.anchoredPosition.y), animationDuration)
            .SetEase(Ease.OutExpo); // Ease 효과: 부드럽게 감속하며 도착 (OutExpo가 역동적)

        // --- 애니메이션 B: 서서히 나타나기 (Fade In) ---
        // DOFade: Image의 알파(color.a) 값을 변경합니다.
        titleLogoImage.DOFade(1.0f, animationDuration) // 1.0f (불투명) 상태로 변경
            .SetEase(Ease.InSine); // Ease 효과: 서서히 나타나기 시작
        
        // --- 애니메이션 C: 알파값을 적용해서 깜빡이기 ---
        // titleLogoImage.DOFade(0.2f, 1.5f)
        //     .SetEase(Ease.InOutSine)
        //     .SetLoops(-1, LoopType.Yoyo);
        yield return new WaitForSeconds(animationDuration);
        titleLogoIdleAnimation.AnimaStart();
    }
}
