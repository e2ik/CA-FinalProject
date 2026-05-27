using UnityEngine;
using UnityEngine.UI;

public class UIAlphaPulse : MonoBehaviour
{
    [Header("Alpha Range")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    [Header("Speed")]
    public float minDuration = 0.5f;
    public float maxDuration = 1.5f;

    private Image _image;
    private float _currentAlpha;
    private float _targetAlpha;
    private float _duration;
    private float _elapsed;

    void Start()
    {
        _image = GetComponent<Image>();
        _currentAlpha = _image.color.a;
        PickNewTarget();
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, _elapsed / _duration);
        
        Color c = _image.color;
        c.a = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
        _image.color = c;

        if (_elapsed >= _duration)
        {
            _currentAlpha = _targetAlpha;
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        _elapsed = 0f;
        _targetAlpha = Random.Range(minAlpha, maxAlpha);
        _duration = Random.Range(minDuration, maxDuration);
    }
}