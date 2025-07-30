using UnityEngine;

public class Activate : ProcessBase
{
    [SerializeField] private GameObject obj;

    public override void Execute()
    {
        IsOn = true;
        obj.SetActive(true);
    }
}
