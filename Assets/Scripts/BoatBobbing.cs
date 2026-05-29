using UnityEngine;

public class BoatBobbing : MonoBehaviour
{
    [Header("Bob (Vertical)")]
    public float bobHeight = 0.3f;
    public float bobFrequency = 0.8f;

    [Header("Roll (Z-axis tilt)")]
    public float rollAngle = 4f;
    public float rollFrequencyMultiplier = 0.9f;

    [Header("Pitch (X-axis tilt)")]
    public float pitchAngle = 2f;
    public float pitchFrequencyMultiplier = 1.1f;

    [Header("Phase Offsets")]
    public float rollPhaseOffset = 0.5f;
    public float pitchPhaseOffset = 1.2f;

    [Header("Wave Complexity")]
    [Range(0f, 1f)]
    public float secondaryWaveStrength = 0.3f;
    public float secondaryWaveFrequencyMultiplier = 2.3f;

    [Header("Playback")]
    public bool isAnimating = true;
    public float speedMultiplier = 1f;
    public float startTimeOffset = 0f;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _time;

    void Start()
    {
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        _time = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!isAnimating) return;

        _time += Time.deltaTime * speedMultiplier;

        float primaryBob   = Mathf.Sin(_time * bobFrequency * Mathf.PI * 2f);
        float secondaryBob = Mathf.Sin(_time * bobFrequency * secondaryWaveFrequencyMultiplier * Mathf.PI * 2f);
        float verticalOffset = bobHeight * Mathf.Lerp(primaryBob, secondaryBob, secondaryWaveStrength);

        float roll  = rollAngle  * Mathf.Sin(_time * bobFrequency * rollFrequencyMultiplier  * Mathf.PI * 2f + rollPhaseOffset);

        float pitch = pitchAngle * Mathf.Sin(_time * bobFrequency * pitchFrequencyMultiplier * Mathf.PI * 2f + pitchPhaseOffset);

        transform.localPosition = _startPosition + Vector3.up * verticalOffset;
        transform.localRotation = _startRotation * Quaternion.Euler(pitch, 0f, roll);
    }

    public void ResetAnimation()
    {
        _time = 0f;
        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
    }

    public void SetCalmPreset()
    {
        bobHeight    = 0.08f;
        bobFrequency = 0.4f;
        rollAngle    = 1.5f;
        pitchAngle   = 0.8f;
        speedMultiplier = 0.7f;
    }

    public void SetStormyPreset()
    {
        bobHeight    = 0.7f;
        bobFrequency = 1.4f;
        rollAngle    = 12f;
        pitchAngle   = 6f;
        speedMultiplier = 1.5f;
    }
}