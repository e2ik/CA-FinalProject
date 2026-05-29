using UnityEngine;

public class GrassSway : MonoBehaviour
{
    [Header("Sway")]
    public float maxSwayAngle = 20f;
    public float swayFrequency = 1.2f;

    [Header("Variation")]
    public float frequencyVariance = 0.3f;
    public float idleWobbleAmount = 2f;

    private Quaternion _startRotation;
    private float _time;
    private float _rippleOffset;
    private float _actualFrequency;
    private float _randomPhase;

    void Start()
    {
        _startRotation = transform.localRotation;
        _actualFrequency = swayFrequency + Random.Range(-frequencyVariance, frequencyVariance);
        _randomPhase = Random.Range(0f, 100f);
        if (WindManager.Instance != null)
            _rippleOffset = WindManager.Instance.GetRippleOffset(transform.position);
    }

    void Update()
    {
        _time += Time.deltaTime;

        float windStrength = WindManager.Instance != null
            ? WindManager.Instance.CurrentWindStrength
            : 0.05f;

        float phase = (_time + _rippleOffset + _randomPhase) * _actualFrequency * Mathf.PI * 2f;
        float sway = maxSwayAngle * windStrength * Mathf.Sin(phase);
        float wobble = idleWobbleAmount * Mathf.Sin(phase * 1.7f + _randomPhase);

        transform.localRotation = _startRotation * Quaternion.Euler(0f, 0f, sway + wobble);
    }
}