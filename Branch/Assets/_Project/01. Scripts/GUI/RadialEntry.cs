using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RadialEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    public void SetLabel(string name)
    {
        label.text = name;
    }
}
