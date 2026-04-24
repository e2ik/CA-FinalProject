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

    [HideInInspector] public Vector3 lastPosition;
    [HideInInspector] public bool isLocked;
    [HideInInspector] public Vector3 lockedPosition;
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
        leftFootRot = Quaternion.identity;
        rightFootRot = Quaternion.identity;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!animator) return;

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

            footRot = Quaternion.Slerp(footRot, targetRot, Time.deltaTime * footRotationSpeed);

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

        float newY = Mathf.Lerp(lastPelvisY, targetY, Time.deltaTime * pelvisSpeed);
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

        animator.SetIKPositionWeight(goal, ik.positionWeight);
        animator.SetIKRotationWeight(goal, ik.rotationWeight);

        animator.SetIKPosition(goal, ik.target.position);
        animator.SetIKRotation(goal, ik.target.rotation);

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
}