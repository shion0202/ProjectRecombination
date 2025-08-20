using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private PlayerController player;

        private void Start()
        {
            var hp = player.Stats.CurrentHealth;

            if (hp <= 0f)
                Debug.LogError("Player HP Stats Init Failed");
        }

        private void Update()
        {
            var hp = player.Stats.CurrentHealth;
            if (hp <= 0f)
            {
                // 게임 오버 처리
                Debug.Log("Player is Death");
                GUIManager.Instance.OnGameOverPanel();
                StartCoroutine(SceneClear());
            }
        }

        private IEnumerator SceneClear()
        {
            yield return new WaitForSeconds(5);
            // GameScene 다시 로드
            SceneManager.LoadScene("GameScene");
        }
    }
}
