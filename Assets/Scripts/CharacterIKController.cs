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
    public float lockThreshold = 0.15f;

    [Header("Pelvis Adjustment")]
    public float pelvisOffset = 0f;
    public float pelvisSpeed = 5f;

    [Header("LookAt")]
    public bool enableLookAt = true;
    public Transform lookAtTarget;

    [Range(0f, 1f)] public float lookAtWeight = 1f;
    public float lookAtSmoothSpeed = 8f;
    [HideInInspector] public float currentLookAtWeight;

    private Vector3 smoothLookAtPosition;
    private Animator animator;
    private float lastPelvisY;
    private bool initialized = false;
    private Quaternion leftFootRot, rightFootRot;

    private Transform leftFootBone;
    private Transform rightFootBone;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        {
            leftFootBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFootBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (leftFootBone == null)
                Debug.LogWarning("CharacterIKController: Could not find LeftFoot bone. Is this a Humanoid rig?");
            if (rightFootBone == null)
                Debug.LogWarning("CharacterIKController: Could not find RightFoot bone. Is this a Humanoid rig?");

            InitFootIK(leftFoot, leftFootBone);
            InitFootIK(rightFoot, rightFootBone);

            InitIK(leftHand);
            InitIK(rightHand);

            currentLookAtWeight = 0f;
            leftFootRot = Quaternion.identity;
            rightFootRot = Quaternion.identity;

            if (lookAtTarget != null)
            {
                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                smoothLookAtPosition = head != null ? head.position : transform.position;
            }
        }
    }

    void InitIK(IKTarget ik)
    {
        if (ik.target == null) return;
        
        ik.active = true;

        ik.active          = ik.enabled;
        ik.smoothPosition  = ik.target.position;
        ik.smoothRotation  = ik.target.rotation;
        ik.currentPositionWeight = ik.active ? ik.positionWeight : 0f;
        ik.currentRotationWeight = ik.active ? ik.rotationWeight : 0f;
        ik.lastPosition    = ik.target.position;
        ik.lockedPosition  = ik.target.position;
    }

    void InitFootIK(IKTarget ik, Transform bone)
    {
        if (bone == null) return;

        ik.active          = ik.enabled;
        ik.smoothPosition  = bone.position;
        ik.smoothRotation  = bone.rotation;
        ik.currentPositionWeight = ik.active ? ik.positionWeight : 0f;
        ik.currentRotationWeight = ik.active ? ik.rotationWeight : 0f;
        ik.lastPosition    = bone.position;
        ik.lockedPosition  = bone.position;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!animator) return;

    if (leftFootBone != null)
    {
        Vector3 origin = leftFootBone.position + Vector3.up * 0.5f;
        bool hit = Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, raycastDistance, groundLayer);
    }

        if (!initialized)
        {
            lastPelvisY = animator.bodyPosition.y;
            initialized = true;
        }

        SmoothLookAt();
        if (lookAtTarget != null)
        {
            animator.SetLookAtWeight(currentLookAtWeight);
            animator.SetLookAtPosition(smoothLookAtPosition);
        }
        else animator.SetLookAtWeight(0f);

        SmoothIK(leftHand);
        SmoothIK(rightHand);

        SmoothWeight(leftHand);
        SmoothWeight(rightHand);
        SmoothWeight(leftFoot);
        SmoothWeight(rightFoot);

        if (enableFootGrounding)
        {
            UpdateFoot(AvatarIKGoal.LeftFoot,  leftFoot,  leftFootBone,  ref leftFootRot);
            UpdateFoot(AvatarIKGoal.RightFoot, rightFoot, rightFootBone, ref rightFootRot);
            AdjustPelvisHeight();
        }
        else
        {
            SmoothIK(leftFoot);
            SmoothIK(rightFoot);
        }

        ApplyIK(AvatarIKGoal.LeftHand,  leftHand);
        ApplyIK(AvatarIKGoal.RightHand, rightHand);
        ApplyIK(AvatarIKGoal.LeftFoot,  leftFoot);
        ApplyIK(AvatarIKGoal.RightFoot, rightFoot);
    }

    void SmoothFootFromBone(IKTarget ik, Transform bone)
    {
        if (bone == null) return;
        float t = Time.deltaTime * ik.smoothSpeed;
        ik.smoothPosition = Vector3.Lerp(ik.smoothPosition, bone.position, t);
        ik.smoothRotation = Quaternion.Slerp(ik.smoothRotation, bone.rotation, t);
    }

    void SmoothIK(IKTarget ik)
    {
        if (ik == null || ik.target == null) return;
        float t = Time.deltaTime * ik.smoothSpeed;
        ik.smoothPosition = Vector3.Lerp(ik.smoothPosition, ik.target.position, t);
        ik.smoothRotation = Quaternion.Slerp(ik.smoothRotation, ik.target.rotation, t);
    }

    void SmoothWeight(IKTarget ik)
    {
        float targetWeight = ik.active ? ik.positionWeight : 0f;
        ik.currentPositionWeight = Mathf.Lerp(ik.currentPositionWeight, targetWeight, Time.deltaTime * ik.smoothSpeed);
        ik.currentRotationWeight = Mathf.Lerp(ik.currentRotationWeight, targetWeight, Time.deltaTime * ik.smoothSpeed);
    }

    void UpdateFoot(AvatarIKGoal goal, IKTarget ik, Transform bone, ref Quaternion footRot)
    {
        if (bone == null) return;

        Vector3 origin = bone.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Vector3 groundedPos = hit.point + Vector3.up * footOffset;

            bool shouldLock = (bone.position.y - hit.point.y) <= lockThreshold;
            if (shouldLock && !ik.isLocked)
            {
                ik.lockedPosition = groundedPos;
                ik.isLocked = true;
            }
            else if (!shouldLock)
            {
                ik.isLocked = false;
            }

            Vector3 targetPos = ik.isLocked ? ik.lockedPosition : groundedPos;
            ik.lastPosition = targetPos;

            Quaternion targetRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, hit.normal),
                hit.normal
            );

            footRot = Quaternion.Slerp(footRot, targetRot, Time.deltaTime * footRotationSpeed);

            if (groundedPos.y > bone.position.y)
            {
                ik.smoothPosition = targetPos;
                ik.smoothRotation = footRot;
            }
            else
            {
                ik.smoothPosition = bone.position;
                ik.smoothRotation = footRot;
            }

            Debug.DrawLine(origin, hit.point, Color.green);
        }
        else
        {
            SmoothFootFromBone(ik, bone);
            Debug.DrawRay(origin, Vector3.down * raycastDistance, Color.red);
        }
    }

    void SmoothLookAt()
    {
        float target = (enableLookAt && lookAtTarget != null) ? lookAtWeight : 0f;
        currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, target, Time.deltaTime * lookAtSmoothSpeed);

        if (lookAtTarget != null)
            smoothLookAtPosition = Vector3.Lerp(smoothLookAtPosition, lookAtTarget.position, Time.deltaTime * lookAtSmoothSpeed);
    }

    void AdjustPelvisHeight()
    {
        if (leftFootBone == null || rightFootBone == null) return;

        float leftOffset  = leftFoot.lastPosition.y  - transform.position.y;
        float rightOffset = rightFoot.lastPosition.y - transform.position.y;
        float lowest      = Mathf.Min(leftOffset, rightOffset);

        if (Mathf.Abs(lowest) < 0.02f) return;

        Vector3 pelvisPos = animator.bodyPosition;
        float targetY     = pelvisPos.y + lowest + pelvisOffset;
        pelvisPos.y       = Mathf.Lerp(pelvisPos.y, targetY, Time.deltaTime * pelvisSpeed);
        animator.bodyPosition = pelvisPos;
        lastPelvisY       = pelvisPos.y;
    }

    void ApplyIK(AvatarIKGoal goal, IKTarget ik)
    {
        bool isFoot = goal == AvatarIKGoal.LeftFoot || goal == AvatarIKGoal.RightFoot;

        if (!isFoot && (ik == null || !ik.enabled || ik.target == null))
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
            case AvatarIKGoal.LeftHand:  return AvatarIKHint.LeftElbow;
            case AvatarIKGoal.RightHand: return AvatarIKHint.RightElbow;
            case AvatarIKGoal.LeftFoot:  return AvatarIKHint.LeftKnee;
            case AvatarIKGoal.RightFoot: return AvatarIKHint.RightKnee;
            default:                     return AvatarIKHint.LeftElbow;
        }
    }

    void SetHintWeight(AvatarIKGoal goal, float weight)
    {
        animator.SetIKHintPositionWeight(GetHintType(goal), weight);
    }

    public void SetLeftHandTarget(Transform target)  => leftHand.target  = target;
    public void SetRightHandTarget(Transform target) => rightHand.target = target;

    public void SetLookAtTarget(Transform target)
    {
        if (target != null && lookAtTarget == null)
        {
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            smoothLookAtPosition = head != null ? head.position : transform.position;
        }
        lookAtTarget = target;
    }

    public void LookAt_On()    => enableLookAt = true;
    public void LookAt_Off()   => enableLookAt = false;

    public void LeftHand_On()  => leftHand.active  = true;
    public void LeftHand_Off() => leftHand.active  = false;

    public void RightHand_On()  => rightHand.active = true;
    public void RightHand_Off() => rightHand.active = false;

    public void LeftFoot_On()  => leftFoot.active  = true;
    public void LeftFoot_Off() => leftFoot.active  = false;

    public void RightFoot_On()  => rightFoot.active = true;
    public void RightFoot_Off() => rightFoot.active = false;
}