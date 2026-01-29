using Code.Art.VFX;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Infrastructure.View
{
  /// <summary>
  /// Handles physics collisions for entities with Rigidbody.
  /// Attach to Hero prefab to detect collisions with pedestrians.
  /// Creates HitEvent entities in ECS when collisions occur.
  /// Applies knockback force to pedestrians using new physics formula.
  /// </summary>
  public class PhysicsCollisionHandler : MonoBehaviour
  {
    private GameEntity _entity;
    private GameContext _gameContext;
    private CollisionSettings _collisionSettings;

    /// <summary>
    /// Initialize with entity reference.
    /// Called from EntityBehaviour after binding.
    /// </summary>
    public void Initialize(GameEntity entity)
    {
      _entity = entity;
      _gameContext = Contexts.sharedInstance.game;
      Debug.Log($"[PhysicsCollisionHandler] Initialized for entity {_entity.id.Value}");
    }

    /// <summary>
    /// Set collision settings for knockback calculation.
    /// Called from EntityBehaviour or Zenject injection.
    /// </summary>
    public void SetCollisionSettings(CollisionSettings settings)
    {
      _collisionSettings = settings;
    }

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
      if (_entity == null || _gameContext == null)
        return;

      // Only Hero should process collisions
      if (!_entity.isHero)
        return;

      // Try to get pedestrian entity from collision
      GameEntity pedestrianEntity = GetPedestrianEntity(collision.gameObject);
      if (pedestrianEntity == null)
        return;

      // Skip if already hit
      if (pedestrianEntity.isHit)
        return;

      // Calculate impact data
      float impactSpeed = collision.relativeVelocity.magnitude;
      Vector3 impactPoint = collision.contacts.Length > 0
        ? collision.contacts[0].point
        : collision.transform.position;
      Vector3 impactNormal = collision.contacts.Length > 0
        ? collision.contacts[0].normal
        : Vector3.up;

      // Mark pedestrian as hit
      pedestrianEntity.isHit = true;

      // Apply knockback force to pedestrian
      ApplyKnockbackForce(pedestrianEntity, collision, impactSpeed, impactPoint);

      // Create HitEvent with extended data
      CreateHitEvent(pedestrianEntity, impactSpeed, impactPoint, impactNormal);

      // Play hit VFX (particles + camera shake)
      HitEffectController.Instance?.PlayHitEffect(impactPoint, impactNormal, impactSpeed);

      // Notify debug visualizer
#if UNITY_EDITOR || DEVELOPMENT_BUILD
      if (Code.Debugging.DebugPhysicsController.Instance?.Visualizer != null)
      {
        Code.Debugging.DebugPhysicsController.Instance.Visualizer.RegisterCollision(
          impactPoint, impactSpeed, pedestrianEntity.pedestrianType.Value.ToString());
      }
#endif

      Debug.Log($"<color=red>[COLLISION]</color> Hit {pedestrianEntity.pedestrianType.Value} " +
                $"at velocity {impactSpeed:F1} m/s (id: {pedestrianEntity.id.Value})");
    }

    /// <summary>
    /// Apply knockback force to pedestrian based on impact.
    /// Uses new physics formula: horizontalForce + liftForce.
    /// </summary>
    private void ApplyKnockbackForce(GameEntity pedestrian, UnityEngine.Collision collision, float impactSpeed, Vector3 impactPoint)
    {
      // Get pedestrian Rigidbody
      Rigidbody pedestrianRb = collision.rigidbody;
      if (pedestrianRb == null)
      {
        // Try to get from GameObject
        pedestrianRb = collision.gameObject.GetComponent<Rigidbody>();
        if (pedestrianRb == null)
          return;
      }

      // Get settings (use defaults if not set)
      float forceMultiplier = _collisionSettings?.ForceMultiplier ?? 15f;
      float minSpeedForLift = _collisionSettings?.MinSpeedForLift ?? 5f;
      float maxLiftSpeed = _collisionSettings?.MaxLiftSpeed ?? 20f;
      float liftMultiplier = _collisionSettings?.LiftMultiplier ?? 0.3f;

      // Get pedestrian mass
      float mass = pedestrianRb.mass;

      // Calculate hit direction (from hero to pedestrian)
      Vector3 heroPos = transform.position;
      Vector3 pedPos = collision.transform.position;
      Vector3 hitDirection = (pedPos - heroPos).normalized;
      hitDirection.y = 0f; // Flatten to horizontal
      if (hitDirection.sqrMagnitude < 0.01f)
        hitDirection = Vector3.forward; // Fallback

      hitDirection.Normalize();

      // Calculate horizontal force
      float horizontalForce = impactSpeed * mass * forceMultiplier;

      // Calculate vertical lift force (based on speed)
      float liftForce = 0f;
      if (impactSpeed >= minSpeedForLift)
      {
        float liftFactor = Mathf.InverseLerp(minSpeedForLift, maxLiftSpeed, impactSpeed);
        liftForce = horizontalForce * liftMultiplier * liftFactor;
      }

      // Combine forces
      Vector3 totalForce = hitDirection * horizontalForce + Vector3.up * liftForce;

      // Apply impulse force
      pedestrianRb.AddForce(totalForce, ForceMode.Impulse);

      // Add random torque for tumbling effect
      Vector3 torque = new Vector3(
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f),
        Random.Range(-1f, 1f)
      ) * (horizontalForce * 0.1f);
      pedestrianRb.AddTorque(torque, ForceMode.Impulse);

      Debug.Log($"[Knockback] Applied force={totalForce.magnitude:F0}N (horizontal={horizontalForce:F0}, lift={liftForce:F0}) to {pedestrian.pedestrianType.Value}");
    }

    /// <summary>
    /// Find GameEntity from collision GameObject.
    /// Checks for EntityBehaviour component on the collided object.
    /// </summary>
    private GameEntity GetPedestrianEntity(GameObject collisionObject)
    {
      // Try to get EntityBehaviour from the object or its parent
      EntityBehaviour entityView = collisionObject.GetComponent<EntityBehaviour>();
      if (entityView == null)
        entityView = collisionObject.GetComponentInParent<EntityBehaviour>();

      if (entityView == null || entityView.Entity == null)
        return null;

      GameEntity entity = entityView.Entity;

      // Verify it's a pedestrian
      if (!entity.isPedestrian || !entity.hasPedestrianType)
        return null;

      return entity;
    }

    /// <summary>
    /// Create HitEvent entity with collision data.
    /// </summary>
    private void CreateHitEvent(GameEntity pedestrian, float impactForce, Vector3 impactPoint, Vector3 impactNormal)
    {
      GameEntity hitEvent = _gameContext.CreateEntity();

      // Standard hit event data
      hitEvent.AddHitEvent(
        pedestrian.pedestrianType.Value,
        pedestrian.id.Value
      );

      // Extended collision data for VFX and scoring
      hitEvent.AddCollisionImpact(
        impactForce,
        impactPoint,
        impactNormal
      );
    }

    private void OnDestroy()
    {
      _entity = null;
      _gameContext = null;
      _collisionSettings = null;
    }
  }
}
