using UnityEngine;

public class WindManager : MonoBehaviour
{
    public static WindManager Instance { get; private set; }

    [Header("Gust Timing")]
    public float minGustInterval = 2f;
    public float maxGustInterval = 6f;
    public float gustDuration = 1.2f;

    [Header("Gust Strength")]
    public float idleStrength = 0.05f;
    public float gustStrength = 1f;

    [Header("Wind Direction")]
    public Vector3 windDirection = Vector3.right;
    public float rippleSpeed = 4f;

    public float CurrentWindStrength { get; private set; }

    private float _gustTimer;
    private float _nextGustIn;
    private bool _gusting;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => ScheduleNextGust();

    void Update()
    {
        _gustTimer += Time.deltaTime;

        if (!_gusting && _gustTimer >= _nextGustIn)
        {
            _gusting = true;
            _gustTimer = 0f;
        }

        if (_gusting)
        {
            float t = _gustTimer / gustDuration;
            CurrentWindStrength = Mathf.Lerp(idleStrength, gustStrength, Mathf.Sin(t * Mathf.PI));

            if (_gustTimer >= gustDuration)
            {
                _gusting = false;
                CurrentWindStrength = idleStrength;
                ScheduleNextGust();
            }
        }
        else
        {
            CurrentWindStrength = idleStrength;
        }
    }

    void ScheduleNextGust()
    {
        _gustTimer = 0f;
        _nextGustIn = Random.Range(minGustInterval, maxGustInterval);
    }

    public float GetRippleOffset(Vector3 worldPos)
    {
        Vector3 dir = windDirection.normalized;
        float projection = Vector3.Dot(worldPos, dir);
        return projection / rippleSpeed;
    }
}