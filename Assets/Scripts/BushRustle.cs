using UnityEngine;

public class BushRustle : MonoBehaviour
{
    [Header("Shake Settings")]
    public float positionStrength = 0.05f;
    public float rotationStrength = 3f;
    public float frequency = 20f;
    public float duration = 0.8f;

    [Header("Decay")]
    public AnimationCurve decayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _elapsed;
    private bool _shaking;

    void Start()
    {
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
    }

    public void Rustle()
    {
        StopAllCoroutines();
        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
        StartCoroutine(DoRustle());
    }

    private System.Collections.IEnumerator DoRustle()
    {
        _elapsed = 0f;
        _shaking = true;

        while (_elapsed < duration)
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / duration;
            float decay = decayCurve.Evaluate(t);

            float seed = _elapsed * frequency;
            float offsetX = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f * positionStrength * decay;
            float offsetZ = (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f * positionStrength * decay;
            float rotY    = (Mathf.PerlinNoise(seed + 99f, seed) - 0.5f) * 2f * rotationStrength * decay;
            float rotZ    = (Mathf.PerlinNoise(seed, seed + 99f) - 0.5f) * 2f * rotationStrength * decay;

            transform.localPosition = _startPosition + new Vector3(offsetX, 0f, offsetZ);
            transform.localRotation = _startRotation * Quaternion.Euler(0f, rotY, rotZ);

            yield return null;
        }

        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
        _shaking = false;
    }
}