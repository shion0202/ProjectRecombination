using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public interface ICSVParsable
{
    void FromCSV(string[] values);
    int GetKey();
}

[System.Serializable]
public class CharacterParameters : ICSVParsable
{
    public int formID;
    public string formName;
    public string form;
    public float hp;
    public float attack;
    public float attackSpeed;
    public float range;
    public float defense;
    public float runSpeed;
    public float hpRecovery;

    public void FromCSV(string[] values)
    {
        formID = int.Parse(values[0]);
        formName = values[1];
        form = values[2];
        hp = float.Parse(values[3]);
        attack = float.Parse(values[4]);
        attackSpeed = float.Parse(values[5]);
        range = float.Parse(values[6]);
        defense = float.Parse(values[7]);
        runSpeed = float.Parse(values[8]);
        hpRecovery = float.Parse(values[9]);
    }

    public int GetKey()
    {
        return formID;
    }
}

public class CSVReader : MonoBehaviour
{
    private Dictionary<int, CharacterParameters> characterParamDict = new Dictionary<int, CharacterParameters>();

    private void Awake()
    {
        ReadCSV("CharacterParams.csv", characterParamDict);
    }

    public void ReadCSV<T>(string path, Dictionary<int, T> dict) where T : ICSVParsable, new()
    {
        StreamReader reader = new StreamReader($"{Application.dataPath}/_Project/Data/{path}");

        if (!reader.EndOfStream)
            reader.ReadLine();

        while (!reader.EndOfStream)
        {
            string data = reader.ReadLine();
            if (string.IsNullOrEmpty(data)) continue;

            var splitData = data.Split(',');

            T values = new T();
            values.FromCSV(splitData);

            dict.Add(values.GetKey(), values);
        }
    }
}
