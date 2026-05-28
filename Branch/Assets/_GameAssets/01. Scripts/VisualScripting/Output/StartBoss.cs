using Managers;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class StartBoss : ProcessBase
    {
        [SerializeField] private string bossName;
        [SerializeField] private string enBossName = "";

        public override void Execute()
        {
            if (IsOn) return;

            Managers.GUIManager.Instance.GameUIController.SetBossName(LocalizationManager.IsKorean ? bossName : enBossName);
            Managers.GUIManager.Instance.GameUIController.ToggleBossHp(true);

            IsOn = true;
        }
    }
}