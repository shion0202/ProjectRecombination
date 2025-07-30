using UnityEngine;

public class Deactivate : ProcessBase
{
    [SerializeField] private GameObject obj;

    public override void Execute()
    {
        IsOn = false;
        obj.SetActive(false);
    }
}