using System;
using System.Collections.Generic;
using Code.Common.Components;
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
    [Header("Force (legacy - now handled by PhysicsCollisionHandler)")]
    [Tooltip("Base force applied on hit (legacy, kept for compatibility)")]
    public float HitForce = 800f;

    [Tooltip("Upward force component (legacy)")]
    public float UpwardForce = 300f;

    [Tooltip("Torque for spin effect (legacy)")]
    public float TorqueForce = 200f;

    [Header("Timing")]
    [Tooltip("Time before ragdolled NPC starts fading/is removed")]
    public float DespawnAfterHitDelay = 3f;

    [Tooltip("Duration of fade out animation")]
    public float FadeDuration = 0.5f;

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

    // Legacy property for backward compatibility
    public float DespawnDelay => DespawnAfterHitDelay;
    public float FadeStartDelay => DespawnAfterHitDelay - FadeDuration;
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
  /// Converts hit pedestrians to ragdolls.
  /// Note: Knockback force is now applied by PhysicsCollisionHandler.
  /// This system only configures ragdoll state and timing.
  /// </summary>
  public class ApplyRagdollOnHitSystem : IExecuteSystem
  {
    private readonly RagdollSettings _settings;
    private readonly IGroup<GameEntity> _hitPedestrians;
    private readonly IGroup<GameEntity> _activeRagdolls;
    private readonly List<GameEntity> _buffer = new(16);
    private readonly List<GameEntity> _ragdollBuffer = new(8);

    public ApplyRagdollOnHitSystem(GameContext game, RagdollSettings settings)
    {
      _settings = settings;

      // Pedestrians that were just hit (have Hit but not yet Ragdolled)
      _hitPedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.Hit, GameMatcher.View)
        .NoneOf(GameMatcher.Ragdolled));

      // Currently active ragdolls (for limiting)
      _activeRagdolls = game.GetGroup(GameMatcher.AllOf(GameMatcher.Ragdolled));
    }

    public void Execute()
    {
      // Check ragdoll limit - remove oldest if needed
      EnforceRagdollLimit();

      foreach (GameEntity pedestrian in _hitPedestrians.GetEntities(_buffer))
      {
        ConvertToRagdoll(pedestrian);
      }
    }

    private void ConvertToRagdoll(GameEntity pedestrian)
    {
      if (!pedestrian.hasView)
        return;

      IEntityView view = pedestrian.view.Value;
      if (view is not Component component)
        return;

      GameObject go = component.gameObject;

      // Get Rigidbody (should already exist from PedestrianFactory)
      Rigidbody rb = go.GetComponent<Rigidbody>();
      if (rb != null)
      {
        // Configure for ragdoll physics (free movement)
        rb.linearDamping = _settings.RagdollDrag;
        rb.angularDamping = _settings.RagdollAngularDrag;
        rb.constraints = RigidbodyConstraints.None;
      }

      // Disable animator if present
      Animator animator = go.GetComponent<Animator>();
      if (animator != null)
        animator.enabled = false;

      // Mark as ragdolled with despawn timing
      float now = Time.time;
      float totalTime = _settings.DespawnAfterHitDelay + _settings.FadeDuration;
      pedestrian.AddRagdolled(now, now + totalTime);

      // Remove from pedestrian movement systems
      if (pedestrian.hasCrossingPedestrian)
        pedestrian.RemoveCrossingPedestrian();
      if (pedestrian.hasMoveDirection)
        pedestrian.RemoveMoveDirection();
      if (pedestrian.hasMoveSpeed)
        pedestrian.RemoveMoveSpeed();

      Debug.Log($"[Ragdoll] Pedestrian {pedestrian.id.Value} converted to ragdoll");
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
  /// Fades them out before removal if enabled.
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

        // Apply fade effect during FadeDuration before despawn
        if (_settings.EnableFadeOut)
        {
          float fadeStartTime = despawnTime - _settings.FadeDuration;
          if (now >= fadeStartTime)
          {
            float fadeProgress = (now - fadeStartTime) / _settings.FadeDuration;
            ApplyFade(ragdoll, 1f - fadeProgress);
          }
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
