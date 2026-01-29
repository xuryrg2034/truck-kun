using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Collision.Systems
{
  /// <summary>
  /// Fallback distance-based collision detection.
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

        GameEntity hitEvent = _game.CreateEntity();
        hitEvent.AddHitEvent(pedestrian.pedestrianType.Value, pedestrian.id.Value);

        float estimatedForce = 5f;
        Vector3 impactPoint = pedPos;
        Vector3 impactNormal = (pedPos - heroPos).normalized;
        hitEvent.AddCollisionImpact(estimatedForce, impactPoint, impactNormal);
      }
    }
  }

  /// <summary>
  /// Destroys pedestrians that have been hit (non-ragdolled).
  /// </summary>
  public class DestroyHitPedestriansSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _hitPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public DestroyHitPedestriansSystem(GameContext game)
    {
      _hitPedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.Hit)
        .NoneOf(GameMatcher.Ragdolled));
    }

    public void Execute()
    {
      foreach (GameEntity pedestrian in _hitPedestrians.GetEntities(_buffer))
      {
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
}
