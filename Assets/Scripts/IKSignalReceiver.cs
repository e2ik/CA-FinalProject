using UnityEngine;
using System;

public class IKSignalReceiver : MonoBehaviour
{
    public CharacterIKController ik;

    [Header("Right Hand Targets")]
    public Transform[] rightHandTargets;

    [Header("Left Hand Targets")]
    public Transform[] leftHandTargets;

    [Header("Right Foot Targets")]
    public Transform[] rightFootTargets;

    [Header("Left Foot Targets")]
    public Transform[] leftFootTargets;

    [Header("Look At Target")]
    public Transform[] lookAtTarget;

    private int rightHandIndex;
    private int leftHandIndex;
    private int rightFootIndex;
    private int leftFootIndex;
    private int lookAtIndex;

    void ApplyLimb(Transform[] targets, int index, Action<Transform> setter)
    {
        if (targets == null || targets.Length == 0) return;

        index = Mathf.Clamp(index, 0, targets.Length - 1);
        setter(targets[index]);
    }


    // Note: Don't know a way around this but just make sure there's not more targets
    //       than what is coded here.

    // Look At Signal
    public void LookAt_Set_0()
    {
        lookAtIndex = 0;
        ApplyLimb(lookAtTarget, lookAtIndex, ik.SetLookAtTarget);
    }

    public void LookAt_Set_1()
    {
        lookAtIndex = 1;
        ApplyLimb(lookAtTarget, lookAtIndex, ik.SetLookAtTarget);
    }

    public void LookAt_Set_2()
    {
        lookAtIndex = 2;
        ApplyLimb(lookAtTarget, lookAtIndex, ik.SetLookAtTarget);
    }

    // right Hand Signal
    public void RightHand_Set_0()
    {
        rightHandIndex = 0;
        ApplyLimb(rightHandTargets, rightHandIndex, ik.SetRightHandTarget);
    }

    public void RightHand_Set_1()
    {
        rightHandIndex = 1;
        ApplyLimb(rightHandTargets, rightHandIndex, ik.SetRightHandTarget);
    }

    public void RightHand_Set_2()
    {
        rightHandIndex = 2;
        ApplyLimb(rightHandTargets, rightHandIndex, ik.SetRightHandTarget);
    }

    // left Hand Signal
    public void LeftHand_Set_0()
    {
        leftHandIndex = 0;
        ApplyLimb(leftHandTargets, leftHandIndex, ik.SetLeftHandTarget);
    }

    public void LeftHand_Set_1()
    {
        leftHandIndex = 1;
        ApplyLimb(leftHandTargets, leftHandIndex, ik.SetLeftHandTarget);
    }

    public void LeftHand_Set_2()
    {
        leftHandIndex = 2;
        ApplyLimb(leftHandTargets, leftHandIndex, ik.SetLeftHandTarget);
    }

    // right Foot Signal
    public void RightFoot_Set_0()
    {
        rightFootIndex = 0;
        ApplyLimb(rightFootTargets, rightFootIndex, ik.SetRightFootTarget);
    }

    public void RightFoot_Set_1()
    {
        rightFootIndex = 1;
        ApplyLimb(rightFootTargets, rightFootIndex, ik.SetRightFootTarget);
    }

    // left Foot Signal
    public void LeftFoot_Set_0()
    {
        leftFootIndex = 0;
        ApplyLimb(leftFootTargets, leftFootIndex, ik.SetLeftFootTarget);
    }

    public void LeftFoot_Set_1()
    {
        leftFootIndex = 1;
        ApplyLimb(leftFootTargets, leftFootIndex, ik.SetLeftFootTarget);
    }

    // weight control signals
    public void LookAt_On()     => ik.LookAt_On();
    public void LookAt_Off()    => ik.LookAt_Off();

    public void RightHand_On()  => ik.RightHand_On();
    public void RightHand_Off() => ik.RightHand_Off();

    public void LeftHand_On()   => ik.LeftHand_On();
    public void LeftHand_Off()  => ik.LeftHand_Off();

    public void RightFoot_On()  => ik.RightFoot_On();
    public void RightFoot_Off() => ik.RightFoot_Off();

    public void LeftFoot_On()   => ik.LeftFoot_On();
    public void LeftFoot_Off()  => ik.LeftFoot_Off();
}