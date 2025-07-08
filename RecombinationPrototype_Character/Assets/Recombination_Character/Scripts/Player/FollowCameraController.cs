using Cinemachine;
using UnityEditor;
using UnityEngine;

public enum ECameraState
{
    Normal,
    Zoom
}

public class FollowCameraController : MonoBehaviour
{
    [SerializeField] private ECameraState cameraState;
    public ECameraState CameraState { get { return cameraState; } }

    private void Awake()
    {
        GetComponent<CinemachineVirtualCamera>().m_LookAt = FindFirstObjectByType<PlayerController>().gameObject.GetComponentInChildren<CameraTarget>().transform;
        GetComponent<CinemachineVirtualCamera>().m_Follow = FindFirstObjectByType<PlayerController>().gameObject.GetComponentInChildren<CameraTarget>().transform;
    }

    
}
