using Code.Gameplay.Features.Hero;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure.View
{
  public interface IEntityView
  {
    GameEntity Entity { get; }
    void SetEntity(GameEntity entity);
    void ReleaseEntity();
  }

  public class EntityBehaviour : MonoBehaviour, IEntityView
  {
    private GameEntity _entity;
    private Rigidbody _rigidbody;
    private RunnerMovementSettings _movementSettings;

    public GameEntity Entity => _entity;
    public Rigidbody Rigidbody => _rigidbody;

    /// <summary>
    /// Optional: Inject settings for Rigidbody configuration
    /// </summary>
    [Inject]
    public void Construct(RunnerMovementSettings movementSettings = null)
    {
      _movementSettings = movementSettings;
      Debug.Log($"[EntityBehaviour] Construct called, settings: {(_movementSettings != null ? "OK" : "NULL")}");
    }

    public void SetEntity(GameEntity entity)
    {
      _entity = entity;

      // Sync initial position from entity's WorldPosition
      // This is critical because View is instantiated at a temp position
      if (_entity.hasWorldPosition)
      {
        transform.position = _entity.worldPosition.Value;
      }

      // Core bindings
      _entity.AddView(this);
      _entity.Retain(this);
      _entity.AddTransform(transform);

      // Physics bindings (must be after position sync!)
      BindRigidbody();
    }

    private void BindRigidbody()
    {
      // Check if entity needs physics
      if (_entity == null || !_entity.isPhysicsBody)
      {
        Debug.Log($"[EntityBehaviour] BindRigidbody skipped: entity={_entity != null}, isPhysicsBody={_entity?.isPhysicsBody}");
        return;
      }

      Debug.Log($"[EntityBehaviour] BindRigidbody starting for physics body");

      // Get existing Rigidbody
      _rigidbody = GetComponent<Rigidbody>();

      if (_rigidbody == null)
      {
        // No Rigidbody on prefab - create one
        Debug.Log("[EntityBehaviour] No Rigidbody found, creating...");
        _rigidbody = CreateRigidbody();
      }
      else if (_movementSettings != null)
      {
        // Configure existing Rigidbody with injected settings
        Debug.Log("[EntityBehaviour] Configuring existing Rigidbody with settings");
        HeroRigidbodySetup.ConfigureRigidbody(_rigidbody, _movementSettings);
      }
      else
      {
        // Configure existing Rigidbody with defaults
        Debug.Log("[EntityBehaviour] Configuring existing Rigidbody with defaults");
        ConfigureRigidbodyDefaults(_rigidbody);
      }

      // Bind Rigidbody to entity
      if (_rigidbody != null)
      {
        // Ensure Rigidbody position matches WorldPosition
        if (_entity.hasWorldPosition)
        {
          _rigidbody.position = _entity.worldPosition.Value;
          _rigidbody.linearVelocity = Vector3.zero; // Reset any accumulated velocity
        }

        _entity.AddRigidbody(_rigidbody);
        Debug.Log($"[EntityBehaviour] SUCCESS: Bound Rigidbody to entity {_entity.id.Value} at {_rigidbody.position}");

        // Add collision handler for Hero
        if (_entity.isHero)
        {
          SetupCollisionHandler();
        }
      }
      else
      {
        Debug.LogError("[EntityBehaviour] FAILED: Could not create or find Rigidbody!");
      }
    }

    /// <summary>
    /// Setup PhysicsCollisionHandler for hero to detect collisions with pedestrians
    /// </summary>
    private void SetupCollisionHandler()
    {
      PhysicsCollisionHandler handler = GetComponent<PhysicsCollisionHandler>();
      if (handler == null)
      {
        handler = gameObject.AddComponent<PhysicsCollisionHandler>();
      }
      handler.Initialize(_entity);
      Debug.Log("[EntityBehaviour] PhysicsCollisionHandler added to hero");
    }

    /// <summary>
    /// Creates and configures a new Rigidbody for physics body
    /// </summary>
    private Rigidbody CreateRigidbody()
    {
      // Ensure collider exists
      Collider collider = GetComponent<Collider>();
      if (collider == null)
      {
        // Add default box collider
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(2f, 1.5f, 4f);  // Truck-sized
        box.center = new Vector3(0f, 0.75f, 0f);
        Debug.Log("[EntityBehaviour] Added default BoxCollider");
      }

      // Create Rigidbody
      Rigidbody rb = gameObject.AddComponent<Rigidbody>();

      if (_movementSettings != null)
      {
        // Use injected settings
        HeroRigidbodySetup.ConfigureRigidbody(rb, _movementSettings);
      }
      else
      {
        // Use defaults
        ConfigureRigidbodyDefaults(rb);
      }

      Debug.Log("[EntityBehaviour] Created new Rigidbody");
      return rb;
    }

    /// <summary>
    /// Configure Rigidbody with sensible defaults when settings aren't available
    /// </summary>
    private void ConfigureRigidbodyDefaults(Rigidbody rb)
    {
      rb.mass = 1000f;
      rb.linearDamping = 0f;
      rb.angularDamping = 0.05f;
      rb.useGravity = true;  // Enable gravity for ramps/jumps
      rb.isKinematic = false;
      rb.interpolation = RigidbodyInterpolation.Interpolate;
      rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

      // Freeze X/Z rotation only (allow Y movement for ramps)
      rb.constraints =
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationZ;

      Debug.Log("[EntityBehaviour] Rigidbody configured with defaults");
    }

    public void ReleaseEntity()
    {
      if (_entity == null)
        return;

      // Release physics component
      if (_entity.hasRigidbody)
        _entity.RemoveRigidbody();

      // Release core components
      if (_entity.hasTransform)
        _entity.RemoveTransform();
      if (_entity.hasView)
        _entity.RemoveView();

      _entity.Release(this);
      _entity = null;
      _rigidbody = null;
    }

    private void OnDestroy() => ReleaseEntity();

    #region Physics Events (optional collision handling)

    private void OnCollisionEnter(Collision collision)
    {
      if (_entity == null || !_entity.isPhysicsBody)
        return;

      // Calculate impact force
      float impactForce = collision.impulse.magnitude;
      Vector3 impactDirection = collision.contacts.Length > 0
        ? collision.contacts[0].normal
        : Vector3.zero;

      // Add impact component for physics system to process
      if (impactForce > 0.1f && _entity.isEnabled)
      {
        _entity.ReplacePhysicsImpact(
          impactDirection,
          impactForce,
          Time.time
        );
      }
    }

    #endregion
  }
}
