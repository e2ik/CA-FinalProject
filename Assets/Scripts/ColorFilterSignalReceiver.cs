using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorFilterSignalReceiver : MonoBehaviour
{
    [Header("Settings")]
    public Color targetColor = Color.white;
    public float duration = 1f;

    private Volume _volume;
    private ColorAdjustments _colorAdjustments;

    void Start()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _colorAdjustments);
    }

    public void TriggerColorFilter()
    {
        StartCoroutine(LerpColorFilter());
    }

    private System.Collections.IEnumerator LerpColorFilter()
    {
        Color startColor = _colorAdjustments.colorFilter.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        _colorAdjustments.colorFilter.value = targetColor;
    }
}