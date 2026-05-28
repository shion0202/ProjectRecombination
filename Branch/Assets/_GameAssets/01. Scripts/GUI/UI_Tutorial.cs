using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UI_Tutorial : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI pageIndicatorText; // "1/3" 처럼 표시할 텍스트
    [SerializeField] private Image exampleImage;
    [SerializeField] private string defaultKey;

    [Header("Navigation Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private Dictionary<string, TutorialDataSO> tutorialDict;
    private TutorialDataSO currentData;
    private int currentPageIndex = 0;

    // 현재 언어 상태에 맞춰 캐싱할 변수들
    private string currentTitle;
    private string[] currentDescriptions;

    private void Awake()
    {
        LoadTutorialData();

        // 버튼 이벤트 바인딩
        if (prevButton != null) prevButton.onClick.AddListener(OnClickPrev);
        if (nextButton != null) nextButton.onClick.AddListener(OnClickNext);
    }

    // 설정 창에서 언어가 바뀐 후, 튜토리얼 창이 새로 열릴 때(SetActive(true))마다 실행됩니다.
    private void OnEnable()
    {
        if (currentData == null) return;

        // 변경된 최신 언어 상태를 반영하여 타이틀과 설명 배열을 다시 세팅합니다.
        currentPageIndex = 0;
        SetupCurrentLanguageData();
        UpdateUI();
    }

    public void ShowTutorialByKey(string key)
    {
        if (!tutorialDict.TryGetValue(key, out var data))
        {
            Debug.LogWarning($"Tutorial key not found: {key}");
            return;
        }

        currentData = data;
        currentPageIndex = 0; // 튜토리얼을 열 때는 항상 1페이지부터
        SetupCurrentLanguageData(); // 열리는 순간의 최신 언어 데이터 캐싱
        UpdateUI();
    }

    private void SetupCurrentLanguageData()
    {
        if (currentData == null) return;

        currentTitle = LocalizationManager.IsKorean ? currentData.title : currentData.enTitle;
        currentDescriptions = LocalizationManager.IsKorean ? currentData.descriptions : currentData.enDescriptions;
    }

    public void OnClickNext()
    {
        if (currentData == null || currentData.descriptions == null) return;

        if (currentPageIndex < currentData.descriptions.Length - 1)
        {
            currentPageIndex++;
            UpdateUI();
        }
    }

    public void OnClickPrev()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (currentData == null) return;

        titleText.text = currentTitle;
        exampleImage.sprite = currentData.exampleImage;

        if (currentDescriptions.Length > 0)
        {
            descriptionText.text = currentDescriptions[currentPageIndex];

            if (pageIndicatorText != null)
                pageIndicatorText.text = $"{currentPageIndex + 1} / {currentDescriptions.Length}";
        }
        else
        {
            descriptionText.text = string.Empty;
            if (pageIndicatorText != null) pageIndicatorText.text = "0 / 0";
        }

        // 버튼 활성화/비활성화 제어 (첫 페이지면 '이전' 비활성화 등)
        if (prevButton != null) prevButton.interactable = currentPageIndex > 0;
        if (nextButton != null) nextButton.interactable = currentPageIndex < currentData.descriptions.Length - 1;
    }

    private void LoadTutorialData()
    {
        TutorialDataSO[] datas = Resources.LoadAll<TutorialDataSO>("Tutorial");
        tutorialDict = new Dictionary<string, TutorialDataSO>();

        foreach (TutorialDataSO data in datas)
        {
            if (string.IsNullOrEmpty(data.key)) continue;
            if (tutorialDict.ContainsKey(data.key)) continue;
            tutorialDict.Add(data.key, data);
        }

        foreach (TutorialDataSO data in datas)
        {
            if (string.IsNullOrEmpty(data.key))
            {
                Debug.LogWarning($"{data.name} has empty key");
                continue;
            }

            if (tutorialDict.ContainsKey(data.key))
            {
                Debug.LogWarning($"Duplicate tutorial key: {data.key}");
                continue;
            }

            tutorialDict.Add(data.key, data);
        }

        // 디폴트 표시
        ShowTutorialByKey(defaultKey);
    }
}
