using System.Collections.Generic;
// using Jaeho.Monster;
using UnityEngine;

public class MonsterManager: ManagerBase<MonsterManager>
{
    // [SerializeField] private List<MonsterBase> monsters;        // TODO: 몬스터 베이스가 없다.
    [SerializeField] private GameObject player;

    // private void Start()
    // {
    //     if (monsters.Count == 0) return;
    //     
    //     foreach (var monster in monsters)
    //         monster.SetTarget(player);
    // }
    //
    // // 씬 진행 중 동적으로 몬스터 인스턴스 하거나 디스트로이 할 경우 호출 해서 타겟을 설정하고 리스트에서 지워라
    //
    // public void AddMonster(MonsterBase monster)
    // {
    //     monster.SetTarget(player);
    //     monsters.Add(monster);
    // }
    //
    // public void DeleteMonster(MonsterBase monster)
    // {
    //     monsters.Remove(monster);
    // }
    
    public GameObject Player => player;
}