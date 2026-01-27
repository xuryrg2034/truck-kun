using System;
using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Collision;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Ragdoll
{
  #region Components

  /// <summary>
  /// Marks a pedestrian as ragdolled (flying after being hit).
  /// Contains timestamp for cleanup timing.
  /// </summary>
  [Game]
  public class Ragdolled : IComponent
  {
    public float HitTime;
    public float DespawnTime;
  }

  #endregion

  #region Settings

  [Serializable]
  public class RagdollSettings
  {
    [Header("Force")]
    [Tooltip("Base force applied on hit")]
    public float HitForce = 800f;

    [Tooltip("Upward force component (makes NPC fly up)")]
    public float UpwardForce = 300f;

    [Tooltip("Torque for spin effect")]
    public float TorqueForce = 200f;

    [Header("Timing")]
    [Tooltip("Time before ragdolled NPC is removed")]
    public float DespawnDelay = 2.5f;

    [Tooltip("Time before fade starts (if enabled)")]
    public float FadeStartDelay = 1.5f;

    [Header("Limits")]
    [Tooltip("Maximum number of active ragdolls")]
    public int MaxActiveRagdolls = 5;

    [Header("Physics")]
    [Tooltip("Drag applied to ragdolled body")]
    public float RagdollDrag = 0.5f;

    [Tooltip("Angular drag for rotation damping")]
    public float RagdollAngularDrag = 0.5f;

    [Header("Visual")]
    [Tooltip("Enable fade out before despawn")]
    public bool EnableFadeOut = true;
  }

  #endregion

  #region Feature

  public sealed class RagdollFeature : Feature
  {
    public RagdollFeature(ISystemFactory systems)
    {
      Add(systems.Create<ApplyRagdollOnHitSystem>());
      Add(systems.Create<RagdollCleanupSystem>());
    }
  }

  #endregion

  #region Systems

  /// <summary>
  /// Converts hit pedestrians to ragdolls instead of destroying them.
  /// Applies physics force to make them fly away.
  /// </summary>
  public class ApplyRagdollOnHitSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly RagdollSettings _settings;
    private readonly IGroup<GameEntity> _hitPedestrians;
    private readonly IGroup<GameEntity> _activeRagdolls;
    private readonly IGroup<GameEntity> _heroes;
    private readonly List<GameEntity> _buffer = new(16);
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _ragdollBuffer = new(8);

    public ApplyRagdollOnHitSystem(GameContext game, RagdollSettings settings)
    {
      _game = game;
      _settings = settings;

      // Pedestrians that were just hit (have Hit but not yet Ragdolled)
      _hitPedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.Hit, GameMatcher.View)
        .NoneOf(GameMatcher.Ragdolled));

      // Currently active ragdolls (for limiting)
      _activeRagdolls = game.GetGroup(GameMatcher.AllOf(GameMatcher.Ragdolled));

      // Hero for direction calculation
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
    }

    public void Execute()
    {
      int hitCount = _hitPedestrians.count;
      if (hitCount > 0)
        Debug.Log($"<color=orange>[Ragdoll]</color> Found {hitCount} hit pedestrians to ragdoll");

      // Get hero position for force direction
      Vector3 heroPos = Vector3.zero;
      Vector3 heroVelocity = Vector3.forward * 5f; // Default velocity

      foreach (GameEntity hero in _heroes.GetEntities(_heroBuffer))
      {
        heroPos = hero.worldPosition.Value;
        if (hero.hasPhysicsVelocity)
          heroVelocity = hero.physicsVelocity.Value;
        break;
      }

      // Check ragdoll limit - remove oldest if needed
      EnforceRagdollLimit();

      foreach (GameEntity pedestrian in _hitPedestrians.GetEntities(_buffer))
      {
        ConvertToRagdoll(pedestrian, heroPos, heroVelocity);
      }
    }

    private void ConvertToRagdoll(GameEntity pedestrian, Vector3 heroPos, Vector3 heroVelocity)
    {
      if (!pedestrian.hasView)
        return;

      IEntityView view = pedestrian.view.Value;
      if (view is not Component component)
        return;

      GameObject go = component.gameObject;
      Vector3 pedPos = pedestrian.hasWorldPosition ? pedestrian.worldPosition.Value : go.transform.position;

      // Get or add Rigidbody
      Rigidbody rb = go.GetComponent<Rigidbody>();
      if (rb == null)
      {
        rb = go.AddComponent<Rigidbody>();
      }

      // Configure for ragdoll physics
      rb.isKinematic = false;
      rb.useGravity = true;
      rb.linearDamping = _settings.RagdollDrag;
      rb.angularDamping = _settings.RagdollAngularDrag;
      rb.mass = 70f; // Human mass
      rb.interpolation = RigidbodyInterpolation.Interpolate;

      // Remove constraints for free movement
      rb.constraints = RigidbodyConstraints.None;

      // Calculate force direction
      Vector3 hitDirection = (pedPos - heroPos).normalized;
      hitDirection.y = 0; // Flatten horizontal

      // If hero is moving, use velocity direction
      if (heroVelocity.sqrMagnitude > 0.1f)
      {
        hitDirection = heroVelocity.normalized;
        hitDirection.y = 0;
      }

      // Calculate impact force based on hero speed
      float speedMultiplier = Mathf.Clamp(heroVelocity.magnitude / 5f, 0.5f, 2f);
      float totalForce = _settings.HitForce * speedMultiplier;

      // Apply forces
      Vector3 force = hitDirection * totalForce;
      force.y = _settings.UpwardForce; // Add upward component

      rb.AddForce(force, ForceMode.Impulse);

      // Add random torque for tumbling effect
      Vector3 torque = new Vector3(
        UnityEngine.Random.Range(-1f, 1f),
        UnityEngine.Random.Range(-1f, 1f),
        UnityEngine.Random.Range(-1f, 1f)
      ) * _settings.TorqueForce;

      rb.AddTorque(torque, ForceMode.Impulse);

      // Mark as ragdolled
      float now = Time.time;
      pedestrian.AddRagdolled(now, now + _settings.DespawnDelay);

      // Remove from pedestrian movement systems
      if (pedestrian.hasMoveDirection)
        pedestrian.RemoveMoveDirection();
      if (pedestrian.hasMoveSpeed)
        pedestrian.RemoveMoveSpeed();

      Debug.Log($"[Ragdoll] Pedestrian {pedestrian.id.Value} launched with force {totalForce:F0}");
    }

    private void EnforceRagdollLimit()
    {
      int activeCount = _activeRagdolls.count;
      if (activeCount < _settings.MaxActiveRagdolls)
        return;

      // Find and remove oldest ragdoll
      float oldestTime = float.MaxValue;
      GameEntity oldestRagdoll = null;

      foreach (GameEntity ragdoll in _activeRagdolls.GetEntities(_ragdollBuffer))
      {
        if (ragdoll.ragdolled.HitTime < oldestTime)
        {
          oldestTime = ragdoll.ragdolled.HitTime;
          oldestRagdoll = ragdoll;
        }
      }

      if (oldestRagdoll != null)
      {
        DestroyRagdoll(oldestRagdoll);
        Debug.Log("[Ragdoll] Removed oldest ragdoll (limit reached)");
      }
    }

    private void DestroyRagdoll(GameEntity entity)
    {
      if (entity.hasView)
      {
        IEntityView view = entity.view.Value;
        view.ReleaseEntity();
        if (view is Component component)
          UnityEngine.Object.Destroy(component.gameObject);
      }
      entity.Destroy();
    }
  }

  /// <summary>
  /// Cleans up ragdolled pedestrians after delay.
  /// Optionally fades them out before removal.
  /// </summary>
  public class RagdollCleanupSystem : IExecuteSystem
  {
    private readonly RagdollSettings _settings;
    private readonly IGroup<GameEntity> _ragdolls;
    private readonly List<GameEntity> _buffer = new(8);

    public RagdollCleanupSystem(GameContext game, RagdollSettings settings)
    {
      _settings = settings;
      _ragdolls = game.GetGroup(GameMatcher.AllOf(GameMatcher.Ragdolled, GameMatcher.View));
    }

    public void Execute()
    {
      float now = Time.time;

      foreach (GameEntity ragdoll in _ragdolls.GetEntities(_buffer))
      {
        float despawnTime = ragdoll.ragdolled.DespawnTime;
        float hitTime = ragdoll.ragdolled.HitTime;

        // Check if should despawn
        if (now >= despawnTime)
        {
          DestroyRagdoll(ragdoll);
          continue;
        }

        // Apply fade effect
        if (_settings.EnableFadeOut && now >= hitTime + _settings.FadeStartDelay)
        {
          float fadeProgress = (now - hitTime - _settings.FadeStartDelay) /
                               (despawnTime - hitTime - _settings.FadeStartDelay);
          ApplyFade(ragdoll, 1f - fadeProgress);
        }
      }
    }

    private void ApplyFade(GameEntity entity, float alpha)
    {
      if (!entity.hasView)
        return;

      IEntityView view = entity.view.Value;
      if (view is not Component component)
        return;

      // Find all renderers and set alpha
      Renderer[] renderers = component.gameObject.GetComponentsInChildren<Renderer>();
      foreach (Renderer renderer in renderers)
      {
        foreach (Material mat in renderer.materials)
        {
          if (mat.HasProperty("_Color"))
          {
            Color color = mat.color;
            color.a = alpha;
            mat.color = color;
          }
          else if (mat.HasProperty("_BaseColor"))
          {
            Color color = mat.GetColor("_BaseColor");
            color.a = alpha;
            mat.SetColor("_BaseColor", color);
          }
        }
      }
    }

    private void DestroyRagdoll(GameEntity entity)
    {
      if (entity.hasView)
      {
        IEntityView view = entity.view.Value;
        view.ReleaseEntity();
        if (view is Component component)
          UnityEngine.Object.Destroy(component.gameObject);
      }
      entity.Destroy();
    }
  }

  #endregion
}
