using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class HitCommand : AICommand
    {
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            yield return null;
        }
    }
}