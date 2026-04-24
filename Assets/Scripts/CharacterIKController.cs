using UnityEngine;

[System.Serializable]
public class IKTarget
{
    public bool enabled = true;

    [Header("Target")]
    public Transform target;
    [Range(0f, 1f)] public float positionWeight = 1f;
    [Range(0f, 1f)] public float rotationWeight = 1f;

    [Header("Hint")]
    public Transform hint;
    [Range(0f, 1f)] public float hintWeight = 0f;

    [Header("Smoothing")]
    public float smoothSpeed = 10f;

    [HideInInspector] public Vector3 lastPosition;
    [HideInInspector] public bool isLocked;
    [HideInInspector] public Vector3 lockedPosition;
    [HideInInspector] public Vector3 smoothPosition;
    [HideInInspector] public Quaternion smoothRotation;
    [HideInInspector] public float currentPositionWeight;
    [HideInInspector] public float currentRotationWeight;
    [HideInInspector] public bool active;
}

public class CharacterIKController : MonoBehaviour
{
    [Header("IK Targets")]
    public IKTarget leftHand;
    public IKTarget rightHand;
    public IKTarget leftFoot;
    public IKTarget rightFoot;

    [Header("Foot Grounding")]
    public bool enableFootGrounding = true;
    public LayerMask groundLayer;
    public float raycastDistance = 1.5f;
    public float footOffset = 0.05f;
    public float footRotationSpeed = 10f;

    [Header("Foot Locking")]
    public float lockThreshold = 0.8f;

    [Header("Pelvis Adjustment")]
    public float pelvisOffset = 0f;
    public float pelvisSpeed = 5f;

    private Animator animator;

    private float lastPelvisY;
    private Quaternion leftFootRot, rightFootRot;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        InitIK(leftHand);
        InitIK(rightHand);
        InitIK(leftFoot);
        InitIK(rightFoot);

        lastPelvisY = animator.bodyPosition.y;
        leftFootRot = Quaternion.identity;
        rightFootRot = Quaternion.identity;
    }

    void InitIK(IKTarget ik)
    {
        if (ik.target == null) return;

        ik.smoothPosition = ik.target.position;
        ik.smoothRotation = ik.target.rotation;

        ik.currentPositionWeight = ik.active ? ik.positionWeight : 0f;
        ik.currentRotationWeight = ik.active ? ik.rotationWeight : 0f;

        ik.lastPosition = ik.target.position;
        ik.lockedPosition = ik.target.position;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!animator) return;

        SmoothIK(leftHand);
        SmoothIK(rightHand);
        SmoothIK(leftFoot);
        SmoothIK(rightFoot);

        SmoothWeight(leftHand);
        SmoothWeight(rightHand);
        SmoothWeight(leftFoot);
        SmoothWeight(rightFoot);

        if (enableFootGrounding)
        {
            UpdateFoot(AvatarIKGoal.LeftFoot, leftFoot, ref leftFootRot);
            UpdateFoot(AvatarIKGoal.RightFoot, rightFoot, ref rightFootRot);

            AdjustPelvisHeight();
        }

        ApplyIK(AvatarIKGoal.LeftHand, leftHand);
        ApplyIK(AvatarIKGoal.RightHand, rightHand);
        ApplyIK(AvatarIKGoal.LeftFoot, leftFoot);
        ApplyIK(AvatarIKGoal.RightFoot, rightFoot);
    }

    void SmoothIK(IKTarget ik)
    {
        if (ik == null || ik.target == null) return;

        float t = Time.deltaTime * ik.smoothSpeed;

        ik.smoothPosition = Vector3.Lerp(
            ik.smoothPosition,
            ik.target.position,
            t
        );

        ik.smoothRotation = Quaternion.Slerp(
            ik.smoothRotation,
            ik.target.rotation,
            t
        );
    }

    void SmoothWeight(IKTarget ik)
    {
        float targetWeight = ik.active ? ik.positionWeight : 0f;

        ik.currentPositionWeight = Mathf.Lerp(
            ik.currentPositionWeight,
            targetWeight,
            Time.deltaTime * ik.smoothSpeed
        );

        ik.currentRotationWeight = Mathf.Lerp(
            ik.currentRotationWeight,
            targetWeight,
            Time.deltaTime * ik.smoothSpeed
        );
    }

    // not tested
    void UpdateFoot(AvatarIKGoal goal, IKTarget ik, ref Quaternion footRot)
    {
        if (ik == null || ik.target == null) return;

        bool shouldLock = ik.positionWeight > lockThreshold;

        if (shouldLock && !ik.isLocked)
        {
            ik.lockedPosition = ik.target.position;
            ik.isLocked = true;
        }
        else if (!shouldLock) ik.isLocked = false;

        Vector3 origin = ik.target.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Vector3 targetPos = hit.point + Vector3.up * footOffset;

            if (ik.isLocked) targetPos = ik.lockedPosition;
            else ik.lockedPosition = targetPos;

            ik.lastPosition = targetPos;

            Quaternion targetRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, hit.normal),
                hit.normal
            );

            footRot = Quaternion.Slerp(
                footRot,
                targetRot,
                Time.deltaTime * footRotationSpeed
            );

            ik.target.position = targetPos;
            ik.target.rotation = footRot;
        }
    }

    void AdjustPelvisHeight()
    {
        if (leftFoot.target == null || rightFoot.target == null) return;

        float leftOffset = leftFoot.lastPosition.y - transform.position.y;
        float rightOffset = rightFoot.lastPosition.y - transform.position.y;

        float lowest = Mathf.Min(leftOffset, rightOffset);

        Vector3 pelvisPos = animator.bodyPosition;

        float targetY = pelvisPos.y + lowest + pelvisOffset;

        float newY = Mathf.Lerp(
            pelvisPos.y,
            targetY,
            Time.deltaTime * pelvisSpeed
        );

        pelvisPos.y = newY;
        animator.bodyPosition = pelvisPos;

        lastPelvisY = newY;
    }

    void ApplyIK(AvatarIKGoal goal, IKTarget ik)
    {
        if (ik == null || !ik.enabled || ik.target == null)
        {
            animator.SetIKPositionWeight(goal, 0f);
            animator.SetIKRotationWeight(goal, 0f);
            SetHintWeight(goal, 0f);
            return;
        }

        animator.SetIKPositionWeight(goal, ik.currentPositionWeight);
        animator.SetIKRotationWeight(goal, ik.currentRotationWeight);

        animator.SetIKPosition(goal, ik.smoothPosition);
        animator.SetIKRotation(goal, ik.smoothRotation);

        if (ik.hint != null && ik.hintWeight > 0f)
        {
            AvatarIKHint hintType = GetHintType(goal);
            animator.SetIKHintPositionWeight(hintType, ik.hintWeight);
            animator.SetIKHintPosition(hintType, ik.hint.position);
        }
        else SetHintWeight(goal, 0f);
    }

    AvatarIKHint GetHintType(AvatarIKGoal goal)
    {
        switch (goal)
        {
            case AvatarIKGoal.LeftHand: return AvatarIKHint.LeftElbow;
            case AvatarIKGoal.RightHand: return AvatarIKHint.RightElbow;
            case AvatarIKGoal.LeftFoot: return AvatarIKHint.LeftKnee;
            case AvatarIKGoal.RightFoot: return AvatarIKHint.RightKnee;
            default: return AvatarIKHint.LeftElbow;
        }
    }

    void SetHintWeight(AvatarIKGoal goal, float weight)
    {
        animator.SetIKHintPositionWeight(GetHintType(goal), weight);
    }

    // target controls
    public void SetLeftFootTarget(Transform target) => leftFoot.target = target;
    public void SetRightFootTarget(Transform target) => rightFoot.target = target;
    public void SetLeftHandTarget(Transform target) => leftHand.target = target;
    public void SetRightHandTarget(Transform target) => rightHand.target = target;

    // weight controls
    public void LeftHand_On() => leftHand.active = true;
    public void LeftHand_Off() => leftHand.active = false;

    public void RightHand_On() => rightHand.active = true;
    public void RightHand_Off() => rightHand.active = false;

    public void LeftFoot_On() => leftFoot.active = true;
    public void LeftFoot_Off() => leftFoot.active = false;

    public void RightFoot_On() => rightFoot.active = true;
    public void RightFoot_Off() => rightFoot.active = false;
}