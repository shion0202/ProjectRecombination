using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RadialUI : MonoBehaviour
{
    [SerializeField] private GameObject entryPrefab;
    private List<RadialEntry> entries;

    private void Awake()
    {
        entries = new List<RadialEntry>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenUI();
        }
    }

    public void OpenUI()
    {
        for (int i = 0; i < 5; ++i)
        {
            AddEntry($"i번째 파츠");
        }
    }

    private void AddEntry(string label)
    {
        GameObject entry = Instantiate(entryPrefab, transform);
        RadialEntry re = entry.GetComponent<RadialEntry>();
        re.SetLabel(label);
        entries.Add(re);
    }
}
