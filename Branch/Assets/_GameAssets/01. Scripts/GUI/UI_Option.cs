using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UI_Option : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI seVolumeText;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [Header("Camera")]
    [SerializeField] private TextMeshProUGUI sensitivityXText;
    [SerializeField] private TextMeshProUGUI sensitivityYText;
    [SerializeField] private Slider sensitivityXSlider;
    [SerializeField] private Slider sensitivityYSlider;

    [Header("HDR")]
    [SerializeField] private TextMeshProUGUI TxtStatus;
    [SerializeField] private Button HDRLeftArrow;
    [SerializeField] private Button HDRRightArrow;

    private void Start()
    {
        if (HDRLeftArrow != null) HDRLeftArrow.onClick.AddListener(OnClickHDRLeft);
        if (HDRRightArrow != null) HDRRightArrow.onClick.AddListener(OnClickHDRRight);

        float bgmVolume = PlayerPrefs.GetFloat("BGMParam", 0.8f);
        float seVolume = PlayerPrefs.GetFloat("SEParam", 0.8f);
        if (bgmSlider != null) bgmSlider.value = bgmVolume;
        if (seSlider != null) seSlider.value = seVolume;
        SetBGMVolume(bgmVolume);
        SetSEVolume(seVolume);

        float sensitivityX = PlayerPrefs.GetFloat("SensitivityX", 150.0f);
        float sensitivityY = PlayerPrefs.GetFloat("SensitivityY", 150.0f);
        if (sensitivityXSlider != null) sensitivityXSlider.value = sensitivityX;
        if (sensitivityYSlider != null) sensitivityYSlider.value = sensitivityY;
        SetSensitivityX(sensitivityX);

        UpdateHDRUI(PlayerPrefs.GetInt("HDREnabled", 0) == 1);        // UI 화살표 및 텍스트 갱신
    }

    public void SetBGMVolume(float value)
    {
        bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";

        value = Mathf.Clamp(value, 0.0001f, 1.0f);
        audioMixer.SetFloat("BGMParam", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("BGMParam", value);
    }

    public void SetSEVolume(float value)
    {
        seVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";

        value = Mathf.Clamp(value, 0.0001f, 1.0f);
        audioMixer.SetFloat("SEParam", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SEParam", value);
    }

    public void SetSensitivityX(float value)
    {
        sensitivityXText.text = $"{Mathf.RoundToInt(value)}%";
        PlayerPrefs.SetFloat("SensitivityX", value);

        EventManager.Instance.PostNotification(EEventType.SensitivityChangeX, this, null);
    }

    public void SetSensitivityY(float value)
    {
        sensitivityYText.text = $"{Mathf.RoundToInt(value)}%";
        PlayerPrefs.SetFloat("SensitivityY", value);

        EventManager.Instance.PostNotification(EEventType.SensitivityChangeY, this, null);
    }

    public void OnClickHDRLeft()
    {
        ApplyHDRChange(false);
    }

    public void OnClickHDRRight()
    {
        ApplyHDRChange(true);
    }

    private void ApplyHDRChange(bool enable)
    {
        GUIManager.Instance.SetHDR(enable);
        UpdateHDRUI(enable);
    }

    private void UpdateHDRUI(bool enable)
    {
        if (enable)
        {
            TxtStatus.text = "활성화";
            HDRLeftArrow.interactable = true;
            HDRRightArrow.interactable = false;
        }
        else
        {
            TxtStatus.text = "비활성화";
            HDRLeftArrow.interactable = false;
            HDRRightArrow.interactable = true;
        }
    }
}
