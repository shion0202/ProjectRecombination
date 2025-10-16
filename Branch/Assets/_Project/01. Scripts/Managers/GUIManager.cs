using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class GUIManager : Singleton<GUIManager>
    {
        [Header("Image Settings")] [SerializeField]
        private Image crosshead; // 조준점 이미지         

        [SerializeField] private Image fadeInOutPanel; // 카메라 연출 이미지

        [Header("Test Part System UI")] [SerializeField]
        private TextMeshProUGUI testPartSystemUILeft;

        [SerializeField] private TextMeshProUGUI testPartSystemUIRight;
        [SerializeField] private TextMeshProUGUI testPartSystemUILegs;

        [Header("Test HP Bar UI")] [SerializeField]
        private Slider testHpBar;

        [Header("Test GameOver UI")] [SerializeField]
        private Image testGameOverPanel;

        private Coroutine _runningFadeCoroutine; // 카메라 연출 코루틴 저장

        [Header("Test Crosshair UI")]
        [SerializeField] private Slider ammoLeftBar;
        [SerializeField] private Slider ammoRightBar;

        [Header("Test Skill UI")]
        [SerializeField] private Image legsSkillCooldownImage;
        [SerializeField] private Image backSkillCooldownImage;
        [SerializeField] private TextMeshProUGUI legsSkillCooldownText;
        [SerializeField] private TextMeshProUGUI backSkillCooldownText;

        [Header("Interaction UI")]
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private TextMeshProUGUI interactionText;

        [Header("Radial UI")]
        // 꽤 급하게 작업하였으므로 리팩토링 필요
        [SerializeField] private GameObject radialUI;
        [SerializeField] private List<Image> selectedCircles = new();
        [SerializeField] private List<Image> partIcons = new();
        [SerializeField] private Image baseIcon;
        [SerializeField] private List<Button> laserParts = new();
        [SerializeField] private List<Button> rapidParts = new();
        [SerializeField] private List<Button> heavyParts = new();
        private int _selectedIndex = -1;
        private Color originalColor = Color.white;
        private Color selectedColor = new Color(0.09411765f, 0.1411765f, 0.1411765f);

        public GameObject InteractionUI
        {
            get => interactionUI;
            set => interactionUI = value;
        }

        public TextMeshProUGUI InteractionText
        {
            get => interactionText;
            set => interactionText = value;
        }

        public void ToggleCrosshead()
        {
            // crosshead.SetActive(!crosshead.activeSelf);
            crosshead.enabled = !crosshead.enabled;
        }

        public void SetLeftPartText(string leftPart)
        {
            //testPartSystemUILeft.text = leftPart;
        }

        public void SetRightPartText(string rightPart)
        {
            //testPartSystemUIRight.text = rightPart;
        }

        public void SetLegsText(string legs)
        {
            //testPartSystemUILegs.text = legs;
        }

        public void SetHpSlider(float currentHealth, float maxHealth)
        {
            float rate = currentHealth / maxHealth;

            testHpBar.value = rate;
        }

        public void OnGameOverPanel()
        {
            testGameOverPanel.enabled = true;
        }

        public void CloseGameOverPanel()
        {
            testGameOverPanel.enabled = false;
        }

        public void SetAmmoLeftSlider(float current, float max)
        {
            float rate = current / max;
            ammoLeftBar.value = rate;
        }

        public void SetAmmoRightSlider(float current, float max)
        {
            float rate = current / max;
            ammoRightBar.value = rate;
        }

        public void SetAmmoColor(EPartType type, bool isOverheat)
        {
            Image image = null;
            if (type == EPartType.ArmL)
            {
                image = ammoLeftBar.GetComponentInChildren<Image>();
            }
            else
            {
                image = ammoRightBar.GetComponentInChildren<Image>();
            }
            if (image == null) return;

            if (isOverheat)
            {
                image.color = Color.white;
            }
            else
            {
                image.color = Color.black;
            }
        }

        public void SetLegsSkillIcon(bool isOn)
        {
            legsSkillCooldownImage.gameObject.SetActive(isOn);
        }

        public void SetLegsSkillCooldown(bool isOn)
        {
            legsSkillCooldownText.gameObject.SetActive(isOn);
        }

        public void SetBackSkillIcon(bool isOn)
        {
            backSkillCooldownImage.gameObject.SetActive(isOn);
        }

        public void SetBackSkillCooldown(bool isOn)
        {
            backSkillCooldownText.gameObject.SetActive(isOn);
        }

        public void SetLegsSkillCooldown(float cooldownTime)
        {
            legsSkillCooldownText.text = cooldownTime.ToString("F0");
        }

        public void SetBackSkillCooldown(float cooldownTime)
        {
            backSkillCooldownText.text = cooldownTime.ToString("F0");
        }

        public void ResetSkillCooldown()
        {
            SetLegsSkillIcon(false);
            SetLegsSkillCooldown(false);
            SetBackSkillIcon(false);
            SetBackSkillCooldown(false);
        }

        public void ToggleRadialUI(bool isOpen)
        {
            if (isOpen) radialUI.gameObject.SetActive(true);
            else radialUI.gameObject.SetActive(false);

            if (_selectedIndex >= 0 && _selectedIndex < selectedCircles.Count)
            {
                selectedCircles[_selectedIndex].gameObject.SetActive(false);
                partIcons[_selectedIndex].color = originalColor;
            }
            _selectedIndex = -1;
            ToggleBasePartButton(false);
        }

        public void SelectPartPosition(int type)
        {
            if (_selectedIndex >= 0 && _selectedIndex < selectedCircles.Count)
            {
                selectedCircles[_selectedIndex].gameObject.SetActive(false);
                partIcons[_selectedIndex].color = originalColor;
            }

            _selectedIndex = type;
            selectedCircles[_selectedIndex].gameObject.SetActive(true);
            partIcons[_selectedIndex].color = selectedColor;

            // 왼팔 기본 파츠 X
            if (_selectedIndex != 3)
            {
                ToggleBasePartButton(true);
            }
            else
            {
                ToggleBasePartButton(false);
            }
        }

        public void SelectShoulderPartType(int attackType)
        {
            PlayerController player = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
            if (player)
            {
                player.Inven.EquipItem(EPartType.Back, (EAttackType)(1 << attackType));
                player.Inven.EquipItem(EPartType.Shoulder, (EAttackType)(1 << attackType));
                player.Inven.EquipItem(EPartType.Mask, (EAttackType)(1 << attackType));
            }

            ToggleRadialUI(false);
        }

        public void SelectLegsPartType(int attackType)
        {
            PlayerController player = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
            if (player)
            {
                player.Inven.EquipItem(EPartType.Legs, (EAttackType)(1 << attackType));
            }

            ToggleRadialUI(false);
        }

        public void SelectArmLPartType(int attackType)
        {
            PlayerController player = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
            if (player)
            {
                player.Inven.EquipItem(EPartType.ArmL, (EAttackType)(1 << attackType));
            }

            ToggleRadialUI(false);
        }

        public void SelectArmRPartType(int attackType)
        {
            PlayerController player = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
            if (player)
            {
                player.Inven.EquipItem(EPartType.ArmR, (EAttackType)(1 << attackType));
            }

            ToggleRadialUI(false);
        }

        public void ToggleBasePartButton(bool isActivate)
        {
            if (isActivate) baseIcon.gameObject.SetActive(true);
            else baseIcon.gameObject.SetActive(false);
        }

        public void SelectBasePart()
        {
            // 현재 selectedIndex에 맞는 부위를 기본 파츠로 교체 (오른팔 제외)
            // 0: 등/어깨, 1: 다리, 2: 왼팔, 3: 오른팔
            PlayerController player = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
            if (player)
            {
                switch (_selectedIndex)
                {
                    case 0:
                        player.Inven.EquipItem(EPartType.Back, (EAttackType)(1 << 0));
                        player.Inven.EquipItem(EPartType.Shoulder, (EAttackType)(1 << 0));
                        player.Inven.EquipItem(EPartType.Mask, (EAttackType)(1 << 0));
                        break;
                    case 1:
                        player.Inven.EquipItem(EPartType.Legs, (EAttackType)(1 << 0));
                        break;
                    case 2:
                        player.Inven.EquipItem(EPartType.ArmL, (EAttackType)(1 << 0));
                        break;
                    case 3:
                        break;
                }
            }

            ToggleRadialUI(false);
        }

        public void UnlockParts(int index)
        {
            if (index < 0 || index > 2) return;

            switch (index)
            {
                case 0:
                    foreach (var button in laserParts)
                    {
                        button.interactable = true;
                    }
                    break;
                case 1:
                    foreach (var button in rapidParts)
                    {
                        button.interactable = true;
                    }
                    break;
                case 2:
                    foreach (var button in heavyParts)
                    {
                        button.interactable = true;
                    }
                    break;
            }
        }

        #region Fade In/Out

        private IEnumerator CoroutineFadeInOut(float startAlpha, float targetAlpha, float duration = 1f)
        {
            var timer = 0f;
            var panelColor = fadeInOutPanel.color;

            if (duration <= 0f)
            {
                panelColor.a = targetAlpha;
                fadeInOutPanel.color = panelColor;
                yield break;
            }

            while (timer < duration)
            {
                timer += Time.deltaTime;
                var progress = timer / duration;
                panelColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
                fadeInOutPanel.color = panelColor;
                yield return null;
            }

            panelColor.a = targetAlpha;
            fadeInOutPanel.color = panelColor;
        }

        public void FadeIn(float duration = 1.0f)
        {
            if (fadeInOutPanel.color.a > 0)
            {
                if (_runningFadeCoroutine != null)
                    StopCoroutine(_runningFadeCoroutine);
                _runningFadeCoroutine = StartCoroutine(CoroutineFadeInOut(1, 0, duration));
            }
            else
            {
                Debug.Log("Fade In Complete");
            }
        }

        public void FadeOut(float duration = 1.0f)
        {
            if (fadeInOutPanel.color.a < 1)
            {
                if (_runningFadeCoroutine != null)
                    StopCoroutine(_runningFadeCoroutine);
                _runningFadeCoroutine = StartCoroutine(CoroutineFadeInOut(0, 1, duration));
            }
            else
            {
                Debug.Log("Fade In Complete");
            }
        }

        public void FadeTo(float targetAlpha, float duration = 1.0f)
        {
            if (_runningFadeCoroutine != null)
                StopCoroutine(_runningFadeCoroutine);
            _runningFadeCoroutine = StartCoroutine(CoroutineFadeInOut(fadeInOutPanel.color.a, targetAlpha, duration));
        }

        #endregion
    }
}
