using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Pause : MonoBehaviour
{
    [SerializeField] private List<Image> localizedImgs = new();

    private void Start()
    {
        EventManager.Instance.AddListener(EEventType.LanguageChange, OnLanguageChange);
    }

    private void UpdateAllLanguages()
    {
        if (localizedImgs.Count <= 0) return;

        foreach (Image img in localizedImgs)
        {
            if (img == null) return;

            LocalizedImage localizedUI = img.GetComponent<LocalizedImage>();
            if (localizedUI != null) localizedUI.UpdateImage();
        }
    }

    private void OnLanguageChange(EEventType eventType, Component sender, object param = null)
    {
        UpdateAllLanguages();
    }
}
