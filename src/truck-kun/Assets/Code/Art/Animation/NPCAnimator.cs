using UnityEngine;

namespace Code.Art.Animation
{
  /// <summary>
  /// Simple procedural animation for NPC pedestrians.
  /// No Animator required - uses code-based animation.
  /// Attach to NPC prefab root.
  /// </summary>
  public class NPCAnimator : MonoBehaviour
  {
    [Header("Walk Animation")]
    [SerializeField] private float _walkCycleSpeed = 3f;
    [SerializeField] private float _bobHeight = 0.1f;
    [SerializeField] private float _swayAmount = 5f;
    [SerializeField] private float _armSwing = 30f;

    [Header("Idle Animation")]
    [SerializeField] private float _idleSwaySpeed = 1f;
    [SerializeField] private float _idleSwayAmount = 2f;
    [SerializeField] private float _breathingSpeed = 2f;
    [SerializeField] private float _breathingAmount = 0.02f;

    [Header("References")]
    [SerializeField] private Transform _body;
    [SerializeField] private Transform _leftArm;
    [SerializeField] private Transform _rightArm;
    [SerializeField] private Transform _leftLeg;
    [SerializeField] private Transform _rightLeg;

    private Vector3 _basePosition;
    private Quaternion _baseRotation;
    private float _animTime;
    private bool _isWalking;
    private float _currentSpeed;

    // Auto-find children
    private bool _autoSetup;

    private void Start()
    {
      _basePosition = transform.localPosition;
      _baseRotation = transform.localRotation;

      // Try to auto-find body parts if not assigned
      if (_body == null)
        AutoSetupReferences();
    }

    private void AutoSetupReferences()
    {
      _autoSetup = true;

      // If no explicit references, animate the whole object
      if (_body == null)
        _body = transform;

      // Try to find child transforms by name
      foreach (Transform child in transform)
      {
        string name = child.name.ToLower();

        if (name.Contains("body") || name.Contains("torso"))
          _body = child;
        else if (name.Contains("leftarm") || name.Contains("l_arm"))
          _leftArm = child;
        else if (name.Contains("rightarm") || name.Contains("r_arm"))
          _rightArm = child;
        else if (name.Contains("leftleg") || name.Contains("l_leg"))
          _leftLeg = child;
        else if (name.Contains("rightleg") || name.Contains("r_leg"))
          _rightLeg = child;
      }
    }

    private void Update()
    {
      _animTime += Time.deltaTime;

      if (_isWalking && _currentSpeed > 0.1f)
      {
        AnimateWalk();
      }
      else
      {
        AnimateIdle();
      }
    }

    /// <summary>
    /// Set walking state and speed
    /// </summary>
    public void SetWalking(bool walking, float speed = 1f)
    {
      _isWalking = walking;
      _currentSpeed = speed;
    }

    private void AnimateWalk()
    {
      float cycle = _animTime * _walkCycleSpeed * _currentSpeed;

      // Body bob
      if (_body != null)
      {
        float bob = Mathf.Abs(Mathf.Sin(cycle * 2f)) * _bobHeight;
        float sway = Mathf.Sin(cycle) * _swayAmount;

        if (_autoSetup)
        {
          // Animate position for simple models
          transform.localPosition = _basePosition + Vector3.up * bob;
          transform.localRotation = _baseRotation * Quaternion.Euler(0, 0, sway);
        }
        else
        {
          _body.localPosition = new Vector3(0, bob, 0);
          _body.localRotation = Quaternion.Euler(0, 0, sway);
        }
      }

      // Arm swing
      if (_leftArm != null)
      {
        float armAngle = Mathf.Sin(cycle) * _armSwing;
        _leftArm.localRotation = Quaternion.Euler(armAngle, 0, 0);
      }

      if (_rightArm != null)
      {
        float armAngle = -Mathf.Sin(cycle) * _armSwing;
        _rightArm.localRotation = Quaternion.Euler(armAngle, 0, 0);
      }

      // Leg swing
      if (_leftLeg != null)
      {
        float legAngle = -Mathf.Sin(cycle) * _armSwing * 0.8f;
        _leftLeg.localRotation = Quaternion.Euler(legAngle, 0, 0);
      }

      if (_rightLeg != null)
      {
        float legAngle = Mathf.Sin(cycle) * _armSwing * 0.8f;
        _rightLeg.localRotation = Quaternion.Euler(legAngle, 0, 0);
      }
    }

    private void AnimateIdle()
    {
      float time = _animTime;

      // Gentle sway
      float sway = Mathf.Sin(time * _idleSwaySpeed) * _idleSwayAmount;

      // Breathing (scale)
      float breath = 1f + Mathf.Sin(time * _breathingSpeed) * _breathingAmount;

      if (_body != null)
      {
        if (_autoSetup)
        {
          transform.localRotation = _baseRotation * Quaternion.Euler(0, 0, sway);
          transform.localScale = Vector3.one * breath;
        }
        else
        {
          _body.localRotation = Quaternion.Euler(0, 0, sway);
          _body.localScale = Vector3.one * breath;
        }
      }

      // Arms relaxed
      if (_leftArm != null)
        _leftArm.localRotation = Quaternion.Euler(5f + Mathf.Sin(time * 0.5f) * 2f, 0, 0);

      if (_rightArm != null)
        _rightArm.localRotation = Quaternion.Euler(5f + Mathf.Sin(time * 0.5f + 1f) * 2f, 0, 0);
    }

    /// <summary>
    /// Play hit reaction (before ragdoll)
    /// </summary>
    public void PlayHitReaction()
    {
      // Disable normal animation
      enabled = false;

      // Could add hit animation here
    }
  }
}
