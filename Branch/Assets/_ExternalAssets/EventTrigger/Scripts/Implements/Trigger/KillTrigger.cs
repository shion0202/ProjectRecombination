using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : BaseEventTrigger
{
    [Header("모두 처치해야 할 적 리스트")]
    public List<GameObject> enemies = new List<GameObject>();

    // 적이 죽었을 때 이 함수를 외부에서 호출
    public void OnEnemyKilled(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);

            // 모든 적이 제거되었는지 확인
            if (enemies.Count == 0)
            {
                Execute(this.gameObject);
            }
        }
    }
}