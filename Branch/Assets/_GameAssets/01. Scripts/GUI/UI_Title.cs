using UnityEngine;
using Managers;
using UnityEngine.InputSystem;

public class UI_Title : MonoBehaviour, PlayerActions.IUIActionMapActions
{
    [SerializeField] private GameObject _optionPanel;
    private PlayerActions _uiActions;

    private void Awake()
    {
        if (_uiActions != null)
        {
            _uiActions.UIActionMap.Disable();
            _uiActions.Dispose();
        }
        _uiActions = new PlayerActions();
        _uiActions.UIActionMap.SetCallbacks(this);
        _uiActions.UIActionMap.Enable();
    }

    private void OnEnable()
    {
        _uiActions.UIActionMap.Enable();
    }

    private void OnDisable()
    {
        _uiActions.UIActionMap.Disable();
    }

    private void OnDestroy()
    {
        if (_uiActions != null)
        {
            _uiActions?.UIActionMap.Disable();
            _uiActions?.Dispose();
        }
    }

    void PlayerActions.IUIActionMapActions.OnNextDialogue(InputAction.CallbackContext context)
    {
        
    }

    void PlayerActions.IUIActionMapActions.OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnClickOptionExit();
        }
    }

    public void OnClickStart()
    {
        GameManager.Instance.EnterPrologue(EPlayMode.Normal);
    }

    /// <summary>
    /// 행사 출품용 체험 플레이 진입. 프롤로그는 본편과 공유하고, 이후 튜토리얼 씬으로 향한다.
    /// </summary>
    public void OnClickDemoStart()
    {
        GameManager.Instance.EnterPrologue(EPlayMode.Demo);
    }

    public void OnClickExit()
    {
        GameManager.Instance.ExitGame();
    }

    public void OnClickOption()
    {
        if (_optionPanel != null && !_optionPanel.activeSelf)
        {
            _optionPanel.SetActive(true);
        }
    }

    public void OnClickOptionExit()
    {
        if (_optionPanel != null && _optionPanel.activeSelf)
        {
            _optionPanel.SetActive(false);
        }
    }
}
