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
    }

    public void SetEntity(GameEntity entity)
    {
      _entity = entity;

      // Core bindings
      _entity.AddView(this);
      _entity.Retain(this);
      _entity.AddTransform(transform);

      // Physics bindings
      BindRigidbody();
    }

    private void BindRigidbody()
    {
      // Check if entity needs physics
      if (_entity == null || !_entity.isPhysicsBody)
        return;

      // Get existing Rigidbody or setup new one
      _rigidbody = GetComponent<Rigidbody>();

      if (_rigidbody == null && _movementSettings != null)
      {
        // Setup physics body with proper configuration
        _rigidbody = HeroRigidbodySetup.SetupPhysicsBody(gameObject, _movementSettings);
      }
      else if (_rigidbody != null && _movementSettings != null)
      {
        // Configure existing Rigidbody
        HeroRigidbodySetup.ConfigureRigidbody(_rigidbody, _movementSettings);
      }

      // Bind Rigidbody to entity
      if (_rigidbody != null)
      {
        _entity.AddRigidbodyComponent(_rigidbody);
        Debug.Log($"[EntityBehaviour] Bound Rigidbody to entity {_entity.id.Value}");
      }
    }

    public void ReleaseEntity()
    {
      if (_entity == null)
        return;

      // Release physics component
      if (_entity.hasRigidbodyComponent)
        _entity.RemoveRigidbodyComponent();

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
