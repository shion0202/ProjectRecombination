using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCheck : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip animationClip;
    [SerializeField, Range(0.0f, 5.0f)] private float animSpeed = 1.0f;
    [SerializeField] private bool isPlay = false;
    private AnimatorOverrideController _overrideController;

    [Header("Visual")]
    [SerializeField] private GameObject defaultObject;
    [SerializeField] private GameObject animObject;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
    }

    private void Update()
    {
        _overrideController["Take 001"] = animationClip;
        _animator.SetFloat("animSpeed", animSpeed);
        _animator.SetBool("isPlay", isPlay);

        if (defaultObject != null && animObject != null)
        {
            defaultObject.SetActive(!isPlay);
            animObject.SetActive(isPlay);
        }
    }

    public void Play()
    {
        isPlay = true;
    }
}
