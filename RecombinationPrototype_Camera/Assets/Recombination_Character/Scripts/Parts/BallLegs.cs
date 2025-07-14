using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallLegs : MonoBehaviour, IPartAbility
{
    [SerializeField] private Transform modelPos;
    private Vector3 defaultMyPos;

    private float ValueA = 0.0f;
    private float ValueB = 0.0f;
    private bool isIncrease = false;
    private bool isJumping = false;

    private PlayerController owner;

    private void Awake()
    {
        defaultMyPos = new Vector3(0.0f, -0.9f, 0.0f);
    }

    private void Update()
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (isIncrease)
        {
            ValueA += Time.deltaTime * 40.0f;
            float yValue = Time.deltaTime * 0.4f;
            if (ValueA >= 100.0f)
            {
                ValueA = 100.0f;
            }
            if (modelPos.localPosition.y - yValue <= -1.0f)
            {
                yValue = 0.0f;
            }
            smr.SetBlendShapeWeight(0, ValueA);
            modelPos.localPosition = new Vector3(modelPos.localPosition.x, modelPos.localPosition.y - yValue, modelPos.localPosition.z);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + yValue, transform.localPosition.z);
        }

        if (isJumping)
        {
            ValueB += Time.deltaTime * 300.0f;
            float yValue = Time.deltaTime * 6.0f;
            if (ValueB >= 50.0f)
            {
                ValueB = 50.0f;
            }
            if (modelPos.localPosition.y - yValue >= 0.85f)
            {
                yValue = 0.0f;
            }
            smr.SetBlendShapeWeight(1, ValueB);
            modelPos.localPosition = new Vector3(modelPos.localPosition.x, modelPos.localPosition.y + yValue, modelPos.localPosition.z);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - yValue, transform.localPosition.z);
        }

        if (owner != null && owner.FallVelocity.y <= 0.0f)
        {
            isJumping = false;
            ValueB -= Time.deltaTime * 200.0f;
            float yValue = Time.deltaTime * 4.0f;
            if (ValueB <= 0.0f)
            {
                ValueB = 0.0f;
            }
            if (modelPos.localPosition.y - yValue <= 0.0f)
            {
                yValue = 0.0f;
            }
            smr.SetBlendShapeWeight(1, ValueB);
            modelPos.localPosition = new Vector3(modelPos.localPosition.x, modelPos.localPosition.y - yValue, modelPos.localPosition.z);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + yValue, transform.localPosition.z);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Jump(owner);
        }
    }

    public void UseAbility(PlayerController owner)
    {
        this.owner = owner;
        // Jump(owner);

        isIncrease = true;
    }

    private void Jump(PlayerController owner)
    {
        owner.PartJump(ValueA * 0.5f);

        isIncrease = false;
        isJumping = true;
        ValueA = 0.0f;
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        smr.SetBlendShapeWeight(0, ValueA);
        modelPos.localPosition = Vector3.zero;
        transform.localPosition = defaultMyPos;
    }
}
