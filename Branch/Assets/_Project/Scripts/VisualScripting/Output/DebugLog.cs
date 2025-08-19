using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class DebugLog : ProcessBase
    {
        public override void Execute()
        {
            Debug.Log("실행!");
        }
    }
}