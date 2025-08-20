using _Project.Scripts.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AnimCheck 스크립트 연동
public class PlayAnimation : ProcessBase
{
    [SerializeField] private List<AnimCheck> objects = new();

    public override void Execute()
    {
        if (IsOn) return;
        if (CheckNull()) return;

        foreach (AnimCheck obj in objects)
        {
            obj.Play();
        }
    }

    private bool CheckNull()
    {
        return objects.Count <= 0;
    }
}
