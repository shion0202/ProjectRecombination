using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraTrigger : MonoBehaviour
{
    private bool _isInit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isInit) return;

        if (other.transform.CompareTag("Camera"))
        {
            FollowMinimap minimap = other.GetComponent<FollowMinimap>();
            if (minimap)
            {
                minimap.SetMinimapOffset(new Vector3(0.0f, 30.0f, 0.0f)); // 트리거에 진입하면 미니맵 카메라의 높이를 30으로 조정
            }

            _isInit = true;
        }
    }
}
