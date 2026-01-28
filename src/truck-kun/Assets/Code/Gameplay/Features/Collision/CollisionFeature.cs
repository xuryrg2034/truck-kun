using System;
using System.Collections.Generic;
using Code.Common.Components;
using Code.Gameplay.Features.Pedestrian;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Collision
{
  #region Components

  /// <summary>
  /// Flag marking an entity as hit (pedestrian was struck by hero)
  /// </summary>
  [Game] public class Hit : IComponent { }

  /// <summary>
  /// Event created when a collision occurs.
  /// Consumed by FeedbackFeature, QuestFeature, etc.
  /// </summary>
  [Game] public class HitEvent : IComponent
  {
    public PedestrianKind PedestrianType;
    public int PedestrianId;
  }

  /// <summary>
  /// Extended collision data for VFX and scoring.
  /// Attached to HitEvent entity.
  /// </summary>
  [Game] public class CollisionImpact : IComponent
  {
    /// <summary>Impact force magnitude (from relative velocity)</summary>
    public float Force;

    /// <summary>World position of impact point</summary>
    public Vector3 Point;

    /// <summary>Surface normal at impact point</summary>
    public Vector3 Normal;
  }

  #endregion

  #region Settings

  [Serializable]
  public class CollisionSettings
  {
    [Header("Legacy Distance Check (fallback)")]
    [Tooltip("Radius for distance-based collision detection (fallback if physics fails)")]
    public float HitRadius = 1.2f;

    [Header("Physics Collision")]
    [Tooltip("Use physics-based collision detection (recommended)")]
    public bool UsePhysicsCollision = true;

    [Header("Impact Thresholds")]
    [Tooltip("Minimum impact force to register a hit")]
    public float MinImpactForce = 0.5f;

    [Tooltip("Force considered a 'strong' hit (for VFX scaling)")]
    public float StrongImpactForce = 5f;

    [Header("Knockback Force")]
    [Tooltip("Multiplier for knockback force (impactSpeed * mass * multiplier)")]
    public float ForceMultiplier = 15f;

    [Tooltip("Minimum impact speed required for vertical lift")]
    public float MinSpeedForLift = 5f;

    [Tooltip("Maximum speed for lift calculation (caps the lift factor)")]
    public float MaxLiftSpeed = 20f;

    [Tooltip("Multiplier for vertical lift force (as fraction of horizontal)")]
    public float LiftMultiplier = 0.3f;
  }

  #endregion

  #region Feature

  public sealed class CollisionFeature : Feature
  {
    public CollisionFeature(ISystemFactory systems)
    {
      // Physics collision is handled by PhysicsCollisionHandler (MonoBehaviour)
      // This system is a fallback for non-physics scenarios
      Add(systems.Create<FallbackCollisionDetectionSystem>());

      // Process hit pedestrians
      Add(systems.Create<DestroyHitPedestriansSystem>());

      // Cleanup events at end of frame
      Add(systems.Create<CleanupHitEventsSystem>());
    }
  }

  #endregion

  #region Systems

  /// <summary>
  /// Fallback distance-based collision detection.
  /// Only active when UsePhysicsCollision is false or as backup.
  /// Primary collision detection is handled by PhysicsCollisionHandler.
  /// </summary>
  public class FallbackCollisionDetectionSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly CollisionSettings _settings;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _pedestrianBuffer = new(32);

    public FallbackCollisionDetectionSystem(
      GameContext game,
      CollisionSettings settings)
    {
      _game = game;
      _settings = settings;

      // Only check heroes WITHOUT Rigidbody (physics heroes use PhysicsCollisionHandler)
      _heroes = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.WorldPosition)
        .NoneOf(GameMatcher.Rigidbody));

      _pedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.WorldPosition, GameMatcher.PedestrianType)
        .NoneOf(GameMatcher.Hit));
    }

    public void Execute()
    {
      // Skip if physics collision is enabled and we have physics heroes
      if (_settings.UsePhysicsCollision)
        return;

      GameEntity hero = null;
      foreach (GameEntity h in _heroes.GetEntities(_heroBuffer))
      {
        hero = h;
        break;
      }

      if (hero == null)
        return;

      Vector3 heroPos = hero.worldPosition.Value;
      float radiusSq = _settings.HitRadius * _settings.HitRadius;

      foreach (GameEntity pedestrian in _pedestrians.GetEntities(_pedestrianBuffer))
      {
        Vector3 pedPos = pedestrian.worldPosition.Value;
        float distSq = (heroPos - pedPos).sqrMagnitude;

        if (distSq > radiusSq)
          continue;

        pedestrian.isHit = true;

        // Create hit event with estimated collision data
        GameEntity hitEvent = _game.CreateEntity();
        hitEvent.AddHitEvent(pedestrian.pedestrianType.Value, pedestrian.id.Value);

        // Add estimated collision impact
        float estimatedForce = 5f; // Default force for distance-based hits
        Vector3 impactPoint = pedPos;
        Vector3 impactNormal = (pedPos - heroPos).normalized;
        hitEvent.AddCollisionImpact(estimatedForce, impactPoint, impactNormal);
      }
    }
  }

  /// <summary>
  /// Destroys pedestrians that have been hit.
  /// Only destroys pedestrians that DON'T have Ragdolled component.
  /// Ragdolled pedestrians are handled by RagdollCleanupSystem.
  /// </summary>
  public class DestroyHitPedestriansSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _hitPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public DestroyHitPedestriansSystem(GameContext game)
    {
      // Only destroy hit pedestrians that are NOT ragdolled
      // Ragdolled pedestrians are cleaned up by RagdollCleanupSystem after delay
      _hitPedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.Hit)
        .NoneOf(GameMatcher.Ragdolled));
    }

    public void Execute()
    {
      foreach (GameEntity pedestrian in _hitPedestrians.GetEntities(_buffer))
      {
        // Skip if has View - RagdollFeature will handle it
        // Only destroy entities without view (shouldn't happen normally)
        if (pedestrian.hasView)
          continue;

        pedestrian.Destroy();
      }
    }
  }

  /// <summary>
  /// Cleans up HitEvent entities at end of frame.
  /// </summary>
  public class CleanupHitEventsSystem : ICleanupSystem
  {
    private readonly IGroup<GameEntity> _hitEvents;
    private readonly List<GameEntity> _buffer = new(16);

    public CleanupHitEventsSystem(GameContext game)
    {
      _hitEvents = game.GetGroup(GameMatcher.HitEvent);
    }

    public void Cleanup()
    {
      foreach (GameEntity entity in _hitEvents.GetEntities(_buffer))
        entity.Destroy();
    }
  }

  #endregion

  #region Extensions

  public static class CollisionExtensions
  {
    /// <summary>
    /// Check if this is a strong impact (for VFX scaling)
    /// </summary>
    public static bool IsStrongImpact(this GameEntity hitEvent, CollisionSettings settings)
    {
      if (!hitEvent.hasCollisionImpact)
        return false;

      return hitEvent.collisionImpact.Force >= settings.StrongImpactForce;
    }

    /// <summary>
    /// Get normalized impact strength (0-1 range for VFX)
    /// </summary>
    public static float GetImpactStrength(this GameEntity hitEvent, CollisionSettings settings)
    {
      if (!hitEvent.hasCollisionImpact)
        return 0.5f;

      float force = hitEvent.collisionImpact.Force;
      return Mathf.Clamp01(force / settings.StrongImpactForce);
    }
  }

  #endregion
}
