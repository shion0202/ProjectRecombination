using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RadialButtonPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int partIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Managers.GUIManager.Instance.SelectedPartIndex = -1;
        Debug.Log(Managers.GUIManager.Instance.SelectedPartIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
