using UnityEngine;
using UnityEngine.UI;
using Managers; // LocalizationManager 네임스페이스

[RequireComponent(typeof(Image))]
public class LocalizedImage : MonoBehaviour
{
    [SerializeField] private Sprite krSprite; // 한국어 이미지
    [SerializeField] private Sprite enSprite; // 영어 이미지
    [SerializeField] private Vector2 enPos = Vector2.zero;

    private Image targetImage;
    private RectTransform rectTransform;
    private Vector2 originalPos;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        if (enPos == Vector2.zero) enPos = originalPos;
    }

    // 오브젝트가 활성화될 때(창이 열릴 때) 현재 언어에 맞게 이미지를 교체합니다.
    private void OnEnable()
    {
        UpdateImage();
    }

    public void UpdateImage()
    {
        if (targetImage == null) return;

        if (LocalizationManager.IsKorean)
        {
            if (krSprite != null) targetImage.sprite = krSprite;
            rectTransform.anchoredPosition = originalPos;
        }
        else
        {
            // 만약 영어 이미지가 누락되었다면 백업용으로 한국어 이미지를 띄웁니다.
            targetImage.sprite = (enSprite != null) ? enSprite : krSprite;
            rectTransform.anchoredPosition = enPos;
        }

        targetImage.SetNativeSize();
    }
}
