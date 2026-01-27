using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Infrastructure.View
{
  /// <summary>
  /// Handles physics collisions for entities with Rigidbody.
  /// Attach to Hero prefab to detect collisions with pedestrians.
  /// Creates HitEvent entities in ECS when collisions occur.
  /// </summary>
  public class PhysicsCollisionHandler : MonoBehaviour
  {
    private GameEntity _entity;
    private GameContext _gameContext;

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

    private void OnCollisionEnter(Collision collision)
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
      float impactForce = collision.relativeVelocity.magnitude;
      Vector3 impactPoint = collision.contacts.Length > 0
        ? collision.contacts[0].point
        : collision.transform.position;
      Vector3 impactNormal = collision.contacts.Length > 0
        ? collision.contacts[0].normal
        : Vector3.up;

      // Mark pedestrian as hit
      pedestrianEntity.isHit = true;

      // Create HitEvent with extended data
      CreateHitEvent(pedestrianEntity, impactForce, impactPoint, impactNormal);

      Debug.Log($"[PhysicsCollisionHandler] Hit pedestrian {pedestrianEntity.id.Value} " +
                $"(type: {pedestrianEntity.pedestrianType.Value}, force: {impactForce:F1})");
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
    }
  }
}
