using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Intensity Range")]
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.5f;

    [Header("Speed")]
    public float minDuration = 0.05f;
    public float maxDuration = 0.2f;

    private Light _light;
    private float _currentIntensity;
    private float _targetIntensity;
    private float _duration;
    private float _elapsed;

    void Start()
    {
        _light = GetComponent<Light>();
        _elapsed = Random.Range(0f, 1f);
        _currentIntensity = _light.intensity;
        PickNewTarget();
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, _elapsed / _duration);

        _light.intensity = Mathf.Lerp(_currentIntensity, _targetIntensity, t);

        if (_elapsed >= _duration)
        {
            _currentIntensity = _targetIntensity;
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        _elapsed = 0f;
        _targetIntensity = Random.Range(minIntensity, maxIntensity);
        _duration = Random.Range(minDuration, maxDuration);
    }
}