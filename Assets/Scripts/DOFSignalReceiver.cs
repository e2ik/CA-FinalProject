using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoFSignalReceiver : MonoBehaviour
{
    public float transitionDuration = 1f;

    private Volume _volume;
    private DepthOfField _dof;

    void Start()
    {
        _volume = GetComponent<Volume>();
        _volume.profile.TryGet(out _dof);
    }

    public void TriggerFocusShift()
    {
        Debug.Log("DOF Signal Received: Shifting Focus");
        StartCoroutine(ShiftFocus());
    }

    private System.Collections.IEnumerator ShiftFocus()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
            _dof.gaussianStart.value = Mathf.Lerp(1000f, 50f, t);
            yield return null;
        }

        _dof.gaussianStart.value = 50f;
    }
}