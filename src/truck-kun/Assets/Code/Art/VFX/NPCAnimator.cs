using System.Collections.Generic;
using UnityEngine;

namespace Code.Art.VFX
{
  /// <summary>
  /// NPC animation system - walk cycle and idle animations using procedural animation
  /// </summary>
  public class NPCAnimator : MonoBehaviour
  {
    [Header("Idle Animation")]
    [SerializeField] private float _idleSwayAmount = 0.05f;
    [SerializeField] private float _idleSwaySpeed = 2f;
    [SerializeField] private float _idleBreathAmount = 0.02f;
    [SerializeField] private float _idleBreathSpeed = 1.5f;

    [Header("Walk Animation")]
    [SerializeField] private float _walkBobAmount = 0.08f;
    [SerializeField] private float _walkBobSpeed = 8f;
    [SerializeField] private float _walkSwayAmount = 0.03f;
    [SerializeField] private float _walkLeanAmount = 5f;
    [SerializeField] private float _walkCycleTime = 2f;

    [Header("State")]
    [SerializeField] private bool _isWalking;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;
    private float _animationTime;
    private float _walkPhase;

    private void Start()
    {
      _originalPosition = transform.localPosition;
      _originalRotation = transform.localRotation;
      _originalScale = transform.localScale;
    }

    private void Update()
    {
      _animationTime += Time.deltaTime;

      if (_isWalking)
        ApplyWalkAnimation();
      else
        ApplyIdleAnimation();
    }

    private void ApplyIdleAnimation()
    {
      // Subtle side-to-side sway
      float swayX = Mathf.Sin(_animationTime * _idleSwaySpeed) * _idleSwayAmount;

      // Breathing - subtle Y scale change
      float breathScale = 1f + Mathf.Sin(_animationTime * _idleBreathSpeed) * _idleBreathAmount;

      // Apply position offset
      Vector3 idleOffset = new Vector3(swayX, 0f, 0f);
      transform.localPosition = _originalPosition + idleOffset;

      // Apply subtle rotation (very gentle sway)
      float swayRotation = Mathf.Sin(_animationTime * _idleSwaySpeed * 0.5f) * 1.5f;
      transform.localRotation = _originalRotation * Quaternion.Euler(0f, 0f, swayRotation);

      // Apply breathing scale
      Vector3 scale = _originalScale;
      scale.y *= breathScale;
      transform.localScale = scale;
    }

    private void ApplyWalkAnimation()
    {
      _walkPhase += Time.deltaTime / _walkCycleTime;
      if (_walkPhase > 1f) _walkPhase -= 1f;

      float phase = _walkPhase * Mathf.PI * 2f;

      // Vertical bob (up-down motion) - two bobs per cycle (each foot)
      float bob = Mathf.Abs(Mathf.Sin(phase * 2f)) * _walkBobAmount;

      // Side-to-side sway (shift weight between feet)
      float sway = Mathf.Sin(phase) * _walkSwayAmount;

      // Forward lean while walking
      float lean = Mathf.Sin(phase * 2f) * _walkLeanAmount;

      // Apply transforms
      Vector3 walkOffset = new Vector3(sway, bob, 0f);
      transform.localPosition = _originalPosition + walkOffset;

      // Rotation: subtle roll and pitch
      float roll = Mathf.Sin(phase) * 3f;  // Side tilt
      transform.localRotation = _originalRotation * Quaternion.Euler(lean * 0.5f, 0f, roll);
    }

    /// <summary>
    /// Set walking state
    /// </summary>
    public void SetWalking(bool isWalking)
    {
      _isWalking = isWalking;

      if (!isWalking)
      {
        // Reset to original when stopping
        _walkPhase = 0f;
      }
    }

    /// <summary>
    /// Configure animation parameters
    /// </summary>
    public void Configure(NPCAnimationSettings settings)
    {
      _idleSwayAmount = settings.IdleSwayAmount;
      _idleSwaySpeed = settings.IdleSwaySpeed;
      _idleBreathAmount = settings.IdleBreathAmount;
      _idleBreathSpeed = settings.IdleBreathSpeed;
      _walkBobAmount = settings.WalkBobAmount;
      _walkBobSpeed = settings.WalkBobSpeed;
      _walkSwayAmount = settings.WalkSwayAmount;
      _walkLeanAmount = settings.WalkLeanAmount;
      _walkCycleTime = settings.WalkCycleTime;
    }

    /// <summary>
    /// Reset to original pose
    /// </summary>
    public void ResetPose()
    {
      transform.localPosition = _originalPosition;
      transform.localRotation = _originalRotation;
      transform.localScale = _originalScale;
      _animationTime = 0f;
      _walkPhase = 0f;
    }
  }

  /// <summary>
  /// Animation settings for NPCs
  /// </summary>
  [System.Serializable]
  public class NPCAnimationSettings
  {
    [Header("Idle Animation")]
    public float IdleSwayAmount = 0.05f;
    public float IdleSwaySpeed = 2f;
    public float IdleBreathAmount = 0.02f;
    public float IdleBreathSpeed = 1.5f;

    [Header("Walk Animation")]
    public float WalkBobAmount = 0.08f;
    public float WalkBobSpeed = 8f;
    public float WalkSwayAmount = 0.03f;
    public float WalkLeanAmount = 5f;
    public float WalkCycleTime = 2f;

    public static NPCAnimationSettings Default => new NPCAnimationSettings();
  }

  /// <summary>
  /// Manager for NPC animations - attaches and manages NPCAnimator components
  /// </summary>
  public static class NPCAnimationManager
  {
    private static readonly Dictionary<GameObject, NPCAnimator> _animators = new();
    private static NPCAnimationSettings _defaultSettings = new();

    /// <summary>
    /// Configure default animation settings
    /// </summary>
    public static void SetDefaultSettings(NPCAnimationSettings settings)
    {
      _defaultSettings = settings ?? new NPCAnimationSettings();
    }

    /// <summary>
    /// Add animator to NPC GameObject
    /// </summary>
    public static NPCAnimator AttachAnimator(GameObject npc, bool isWalking = false)
    {
      if (npc == null)
        return null;

      // Check if already has animator
      if (_animators.TryGetValue(npc, out NPCAnimator existing))
      {
        existing.SetWalking(isWalking);
        return existing;
      }

      // Add new animator
      NPCAnimator animator = npc.AddComponent<NPCAnimator>();
      animator.Configure(_defaultSettings);
      animator.SetWalking(isWalking);

      _animators[npc] = animator;
      return animator;
    }

    /// <summary>
    /// Remove animator from NPC
    /// </summary>
    public static void RemoveAnimator(GameObject npc)
    {
      if (npc == null)
        return;

      if (_animators.TryGetValue(npc, out NPCAnimator animator))
      {
        if (animator != null)
          Object.Destroy(animator);

        _animators.Remove(npc);
      }
    }

    /// <summary>
    /// Set walking state for NPC
    /// </summary>
    public static void SetWalking(GameObject npc, bool isWalking)
    {
      if (npc == null)
        return;

      if (_animators.TryGetValue(npc, out NPCAnimator animator) && animator != null)
        animator.SetWalking(isWalking);
    }

    /// <summary>
    /// Clean up destroyed NPCs from cache
    /// </summary>
    public static void Cleanup()
    {
      List<GameObject> toRemove = new();

      foreach (var kvp in _animators)
      {
        if (kvp.Key == null || kvp.Value == null)
          toRemove.Add(kvp.Key);
      }

      foreach (GameObject go in toRemove)
        _animators.Remove(go);
    }
  }
}
