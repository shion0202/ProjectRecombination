using UnityEngine;
using TMPro;
using Managers;

public class LocalizedUI : MonoBehaviour
{
    private TextMeshProUGUI _textComponent;
    private string _originKoreanText;
    private bool _isInitialized = false;

    private void Awake()
    {
        EnsureInitialization();
    }

    private void OnEnable()
    {
        EnsureInitialization();
        ApplyLanguage();
    }

    public void ApplyLanguage()
    {
        if (_textComponent == null || string.IsNullOrEmpty(_originKoreanText)) return;
        _textComponent.text = LocalizationManager.GetLocalizedString(_originKoreanText);
    }

    private void EnsureInitialization()
    {
        if (_isInitialized) return;

        _textComponent = GetComponent<TextMeshProUGUI>();
        if (_textComponent != null)
        {
            _originKoreanText = _textComponent.text;
        }

        _isInitialized = true;
    }
}
