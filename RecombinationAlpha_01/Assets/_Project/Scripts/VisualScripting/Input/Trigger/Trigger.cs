using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
public class Trigger : ProcessBase
{
    [SerializeField] private string selectedTag = "";
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(selectedTag))
            Execute();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(selectedTag))
            Execute();
    }

    public override void Execute()
    {
        IsOn = !IsOn;
    }
}
    
}