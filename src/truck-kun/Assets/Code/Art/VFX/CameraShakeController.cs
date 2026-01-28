using System.Collections;
using UnityEngine;

namespace Code.Art.VFX
{
  /// <summary>
  /// Simple camera shake effect for impacts.
  /// Attach to main camera or use as singleton.
  /// </summary>
  public class CameraShakeController : MonoBehaviour
  {
    [Header("Settings")]
    [SerializeField] private float _maxOffset = 0.5f;
    [SerializeField] private float _maxRotation = 3f;
    [SerializeField] private AnimationCurve _shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private static CameraShakeController _instance;
    public static CameraShakeController Instance => _instance;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Coroutine _shakeCoroutine;
    private Transform _cameraTransform;

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(this);
        return;
      }
      _instance = this;
    }

    private void Start()
    {
      // Find camera
      if (Camera.main != null)
      {
        _cameraTransform = Camera.main.transform;
      }
    }

    /// <summary>
    /// Trigger camera shake
    /// </summary>
    /// <param name="intensity">Shake intensity (0-1)</param>
    /// <param name="duration">Duration in seconds</param>
    public void Shake(float intensity, float duration)
    {
      if (_cameraTransform == null)
      {
        if (Camera.main != null)
          _cameraTransform = Camera.main.transform;
        else
          return;
      }

      intensity = Mathf.Clamp01(intensity);

      if (_shakeCoroutine != null)
        StopCoroutine(_shakeCoroutine);

      _shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
      _originalPosition = _cameraTransform.localPosition;
      _originalRotation = _cameraTransform.localRotation;

      float elapsed = 0f;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        float progress = elapsed / duration;
        float curveValue = _shakeCurve.Evaluate(progress);
        float currentIntensity = intensity * curveValue;

        // Random offset
        Vector3 offset = new Vector3(
          Random.Range(-1f, 1f) * _maxOffset * currentIntensity,
          Random.Range(-1f, 1f) * _maxOffset * currentIntensity,
          0
        );

        // Random rotation
        Vector3 rotationOffset = new Vector3(
          Random.Range(-1f, 1f) * _maxRotation * currentIntensity,
          Random.Range(-1f, 1f) * _maxRotation * currentIntensity,
          Random.Range(-1f, 1f) * _maxRotation * currentIntensity * 0.5f
        );

        _cameraTransform.localPosition = _originalPosition + offset;
        _cameraTransform.localRotation = _originalRotation * Quaternion.Euler(rotationOffset);

        yield return null;
      }

      // Reset to original
      _cameraTransform.localPosition = _originalPosition;
      _cameraTransform.localRotation = _originalRotation;

      _shakeCoroutine = null;
    }

    /// <summary>
    /// Stop any active shake
    /// </summary>
    public void StopShake()
    {
      if (_shakeCoroutine != null)
      {
        StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = null;

        if (_cameraTransform != null)
        {
          _cameraTransform.localPosition = _originalPosition;
          _cameraTransform.localRotation = _originalRotation;
        }
      }
    }

    private void OnDestroy()
    {
      if (_instance == this)
        _instance = null;
    }
  }
}
