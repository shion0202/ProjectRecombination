using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LoadSceneAction", menuName = "Events/Actions/Load Scene (Simple)")]
public class LoadSceneAction : EventData
{
    [Tooltip("로드할 씬의 이름. 빌드 설정(Build Settings)에 해당 씬이 포함되어 있어야 합니다.")]
    public string sceneName;

    public override void Execute(GameObject requester)
    {
        // 유효한 씬 이름이 있는지 간단히 확인
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 다른 효과 없이 즉시 씬을 로드합니다.
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("LoadSceneAction에 씬 이름이 지정되지 않았습니다.");
        }
    }
}