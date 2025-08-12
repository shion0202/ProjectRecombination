using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventData : MonoBehaviour
{
    public abstract void Execute(GameObject requester = null);
}
