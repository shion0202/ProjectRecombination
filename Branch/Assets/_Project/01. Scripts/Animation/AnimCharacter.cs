using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

public class AnimCharacter : MonoBehaviour
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject followCameraPrefab;
    private FollowCameraController _followCamera;

    [Header("Animations")]
    [SerializeField] private AnimationClip upperBodyAnim;
    [SerializeField] private AnimationClip lowerBodyAnim;
    [SerializeField, Range(0.0f, 5.0f)] private float upperSpeed = 1.0f;
    [SerializeField, Range(0.0f, 5.0f)] private float lowerSpeed = 1.0f;
    private AnimatorOverrideController _overrideController;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _followCamera = FindFirstObjectByType<FollowCameraController>();
        if (_followCamera == null)
        {
            GameObject cameraObject = Instantiate(followCameraPrefab);
            cameraObject.name = followCameraPrefab.name;
            _followCamera = cameraObject.GetComponent<FollowCameraController>();
        }
        _followCamera.InitFollowCamera(gameObject);

        _overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = _overrideController;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        _overrideController["UpperAnimation"] = upperBodyAnim;
        _overrideController["LowerAnimation"] = lowerBodyAnim;

        animator.SetFloat("upperSpeed", upperSpeed);
        animator.SetFloat("lowerSpeed", lowerSpeed);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
        }

        if (Input.GetMouseButtonDown(0))
        {
            
        }
    }

    private void LateUpdate()
    {
        _followCamera.UpdateFollowCamera();
    }
    #endregion
}
