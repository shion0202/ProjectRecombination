using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : ProcessBase
{
    [SerializeField] private float seconds;
    public override void Execute()
    {
        StartCoroutine(WaitForSeconds());
    }

    private IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(seconds);
        IsOn = true;
    }
}
