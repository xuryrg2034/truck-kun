using Code.Infrastructure.View;
using UnityEngine;

namespace Code.Gameplay.Features.Obstacle
{
  /// <summary>
  /// Trigger zone that applies boost to entities passing through.
  /// Attach to a GameObject with a trigger collider.
  /// </summary>
  public class BoostZoneBehaviour : MonoBehaviour
  {
    [Header("Boost Settings")]
    [Tooltip("Additional forward speed boost")]
    [SerializeField] private float _forwardBoost = 5f;

    [Tooltip("Upward boost force")]
    [SerializeField] private float _upwardBoost = 8f;

    [Tooltip("Duration of launch state (prevents velocity override)")]
    [SerializeField] private float _launchDuration = 1.0f;

    public float ForwardBoost => _forwardBoost;
    public float UpwardBoost => _upwardBoost;
    public float LaunchDuration => _launchDuration;

    private void OnTriggerEnter(Collider other)
    {
      Rigidbody rb = other.attachedRigidbody;
      if (rb == null)
        return;

      ApplyBoost(rb, other.gameObject);
    }

    private void ApplyBoost(Rigidbody rb, GameObject target)
    {
      // Calculate boost velocity
      Vector3 boostVelocity = new Vector3(0f, _upwardBoost, _forwardBoost);

      // Apply as impulse
      rb.AddForce(boostVelocity, ForceMode.VelocityChange);

      // Try to add LaunchBoost to ECS entity
      EntityBehaviour entityBehaviour = target.GetComponentInParent<EntityBehaviour>();
      if (entityBehaviour == null)
        entityBehaviour = rb.GetComponent<EntityBehaviour>();

      if (entityBehaviour != null && entityBehaviour.Entity != null)
      {
        GameEntity entity = entityBehaviour.Entity;
        if (entity.isEnabled && _launchDuration > 0f)
        {
          entity.ReplaceLaunchBoost(boostVelocity, Time.time, _launchDuration);
          Debug.Log($"[BoostZone] {target.name} boosted: forward={_forwardBoost}, up={_upwardBoost}");
        }
      }
      else
      {
        Debug.Log($"[BoostZone] {target.name} boosted (physics only)");
      }

      // TODO: Play sound via AudioService
      // TODO: Spawn particles
    }
  }
}
