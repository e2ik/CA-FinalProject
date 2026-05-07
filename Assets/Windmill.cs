using UnityEngine;

public class Windmill : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 0f, 1f);

    [Header("Speed Range")]
    [SerializeField] private float minRotationSpeed = 120f;
    [SerializeField] private float maxRotationSpeed = 360f;

    [Header("Random Change Timing")]
    [SerializeField] private float minChangeTime = 1f;
    [SerializeField] private float maxChangeTime = 3f;

    [Header("Smoothing")]
    [SerializeField] private float speedLerpSpeed = 2f;

    [SerializeField] private bool useUnscaledTime = false;

    private float currentSpeed;
    private float targetSpeed;
    private float changeTimer;

    private void Awake()
    {
        currentSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        PickNewTargetSpeed();
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // Smoothly move toward new random speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedLerpSpeed * dt);

        // Rotate object
        transform.Rotate(rotationAxis.normalized * currentSpeed * dt);

        // Count down to next random speed change
        changeTimer -= dt;

        if (changeTimer <= 0f)
        {
            PickNewTargetSpeed();
        }
    }

    private void PickNewTargetSpeed()
    {
        targetSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        changeTimer = Random.Range(minChangeTime, maxChangeTime);
    }
}