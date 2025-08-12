using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class OnEvent : ProcessBase
    {
        public override void Execute()
        {
            IsOn = true;
        }
    }
}
