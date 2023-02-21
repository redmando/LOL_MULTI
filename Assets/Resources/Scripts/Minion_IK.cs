using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_IK : MonoBehaviour
{
    [Header("Bindings")]
    public Transform leftHandTarget = null;                 // 왼쪽 손
    public Transform rightHandTarget = null;                // 오른쪽 손

    [Header("Attachments")]
    public bool attachLeftHand = true;
    public bool attachRightHand = true;


    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim == null)
        {
            return;
        }

        if (attachLeftHand)
        {
            if (leftHandTarget != null)
            {
                AttachHandToHandle(AvatarIKGoal.LeftHand, leftHandTarget);
            }
            else
            {
                Debug.Log("Target Not Set");
                DetachHandFromHandle(AvatarIKGoal.LeftHand);
            }
        }

        if (attachRightHand)
        {
            if (rightHandTarget != null)
            {
                AttachHandToHandle(AvatarIKGoal.RightHand, rightHandTarget);
            }
            else
            {
                Debug.Log("Target Not Set");
                DetachHandFromHandle(AvatarIKGoal.RightHand);
            }
        }
    }

    private void AttachHandToHandle(AvatarIKGoal _hand, Transform _handle)
    {
        if (_hand == AvatarIKGoal.LeftHand)
        {
            _handle.position = anim.GetIKPosition(AvatarIKGoal.LeftHand);
        }
        else
        {
            _handle.position = anim.GetIKPosition(AvatarIKGoal.RightHand);
        }

        anim.SetIKPositionWeight(_hand, 1.0f);
        anim.SetIKPosition(_hand, _handle.position);
    }

    private void DetachHandFromHandle(AvatarIKGoal _hand)
    {
        anim.SetIKPositionWeight(_hand, 0);
    }
}
