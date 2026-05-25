using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageUpdateTrigger : MonoBehaviour
{
    [SerializeField] private int stageIndex;
    [SerializeField] private GameObject respawnPosition;
    
    private int _previousStageIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        //DungeonManager.Instance.UpdateStage(stageIndex);
        _previousStageIndex = DungeonManager.Instance.CurrentPlayerStageIndex;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 플레이어가 뒤로 이동
        if (_previousStageIndex > stageIndex)
        {
            // 플레이어가 트리거를 왔다갔다하기만 해도 스테이지가 업데이트되는 문제가 존재
            // 현재 게임 구조상 이전 스테이지로 돌아가는 길 자체를 막고 있으므로, 이전 스테이지로 돌아가는 로직이 필요하지 않다고 판단되어 임시로 주석 처리
            //DungeonManager.Instance.UpdatePlayerStageIndex(stageIndex);
        }
        // 플레이어가 앞으로 이동
        else if (_previousStageIndex == stageIndex)
        {
            DungeonManager.Instance.UpdatePlayerStageIndex(stageIndex + 1);

            if (respawnPosition != null)
            {
                DungeonManager.Instance.RestartPosition = respawnPosition.transform.position;
            }
        }
    }
}
