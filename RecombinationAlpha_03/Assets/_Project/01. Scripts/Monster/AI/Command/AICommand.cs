using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public abstract class AICommand
    {
        public abstract IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete);
        protected static bool CheckAnimator(Blackboard.Blackboard blackboard, string animationName)
        {
            if (blackboard?.Animator == null)
                return false;
            if (!blackboard.Animator.HasState(0, Animator.StringToHash(animationName)))
                return false;
            // 현재 재생 중인 에니메이션이 animationName일 경우
            if (blackboard.Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
                return false;
            return true;
        }
    }
}