using UnityEngine;

public class EnableTrigger : BaseEventTrigger
{
    private void OnEnable()
    {
        Execute(this.gameObject);
    }
}