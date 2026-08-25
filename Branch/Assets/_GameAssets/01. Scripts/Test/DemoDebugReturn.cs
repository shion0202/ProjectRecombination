using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [임시] F9 키로 체험 플레이를 즉시 종료하고 타이틀로 복귀한다.
/// 정식 결과 화면(Task 6)이 붙기 전까지 세션 리셋을 반복 검증하기 위한 개발용 컴포넌트다.
/// Task 6 완료 시 이 파일과 씬 배치를 함께 제거한다.
/// </summary>
public class DemoDebugReturn : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.f9Key.wasPressedThisFrame) return;

        Debug.Log("[DemoDebugReturn] F9 입력 감지, 타이틀 복귀 요청");
        GameManager.Instance.ReturnToTitleFromDemo();
    }
}
